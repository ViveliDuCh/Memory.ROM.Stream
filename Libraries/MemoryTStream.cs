using System.Diagnostics;

namespace Memory.ROMStream.Libraries;

public class MemoryTStream : Stream //For Memory<byte> backing
{
    private Memory<byte> _buffer;
    private int _position;
    private bool _isOpen;
    private bool _writable; // For read-only support
    private readonly bool _publiclyVisible;

    // Writeable and publicly visible by default
    public MemoryTStream(Memory<byte> buffer)
    : this(buffer, publiclyVisible: true)
    {
    }

    public MemoryTStream(Memory<byte> buffer, bool publiclyVisible)
    : this(buffer, publiclyVisible: true, writable: true)
    {
    }

    public MemoryTStream(Memory<byte> buffer, bool publiclyVisible, bool writable)
    {
        _buffer = buffer;
        _writable = writable;
        _publiclyVisible = publiclyVisible;
        _isOpen = true;
        _position = 0;
    }

    public override bool CanRead => _isOpen;
    public override bool CanSeek => _isOpen;
    public override bool CanWrite => _writable && _isOpen;

    public override long Length
    {
        get
        {
            EnsureNotClosed();
            return _buffer.Length;
        }
    }

    public override long Position
    {
        get
        {
            EnsureNotClosed();
            return _position;
        }
        set
        {
            EnsureNotClosed();
            if (value < 0 || value > _buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(value));
            _position = (int)value;
        }
    }

    // Attempts to get the underlying buffer.
    public bool TryGetBuffer(out Memory<byte> buffer)
    {
        if (!_publiclyVisible)
        {
            buffer = default;
            return false;
        }

        buffer = _buffer;
        return true;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        if (offset < 0 || count < 0)
            throw new ArgumentOutOfRangeException();
        if (buffer.Length - offset < count)
            throw new ArgumentException("Invalid offset or count.");

        EnsureNotClosed();

        int bytesAvailable = _buffer.Length - _position;
        int bytesToRead = Math.Min(bytesAvailable, count);

        if (bytesToRead > 0)
        {
            _buffer.Span.Slice(_position, bytesToRead).CopyTo(buffer.AsSpan(offset));
            _position += bytesToRead;
        }

        return bytesToRead;
    }

    public override int ReadByte()
    {
        EnsureNotClosed();

        if (_position >= _buffer.Length)
            return -1;

        return _buffer.Span[_position++];
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        if (offset < 0 || count < 0)
            throw new ArgumentOutOfRangeException();
        if (buffer.Length - offset < count)
            throw new ArgumentException("Invalid offset or count.");

        EnsureNotClosed();
        EnsureWriteable();

        if (_position + count > _buffer.Length)
            throw new NotSupportedException("Cannot expand buffer.  Write would exceed capacity.");

        buffer.AsSpan(offset, count).CopyTo(_buffer.Span.Slice(_position));
        _position += count;
    }

    public override void WriteByte(byte value)
    {
        EnsureNotClosed();
        EnsureWriteable();

        if (_position >= _buffer.Length)
            throw new NotSupportedException("Cannot expand buffer. Write would exceed capacity.");

        _buffer.Span[_position++] = value;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        EnsureNotClosed();

        long newPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => _buffer.Length + offset,
            _ => throw new ArgumentException("Invalid seek origin.", nameof(origin))
        };

        if (newPosition < 0 || newPosition > _buffer.Length)
            throw new IOException("Seek position out of range.");

        _position = (int)newPosition;
        return newPosition;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("Cannot resize MemoryTStream.");
    }

    public override void Flush()
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _isOpen)
        {
            _isOpen = false;
            _writable = false;
            // Don't set buffer to null - allow TryGetBuffer, GetBuffer & ToArray to work.
            // That the stream should no longer be used for I/O
            // doesn't mean the underlying memory should be invalidated.
        }
        base.Dispose(disposing);
    }

    private void EnsureNotClosed()
    {
        if (!_isOpen)
            throw new ObjectDisposedException(GetType().Name);
    }
    
    private void EnsureWriteable()
    {
        if (!CanWrite)
            throw new NotSupportedException();
    }
}
