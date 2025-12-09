namespace Memory.ROMStream.Libraries;

public class ReadOnlyMemoryStream : Stream //ReadOnlyBufferStream from usecasesExtension project
{
    private ReadOnlyMemory<byte> _buffer;
    private int _position;
    private bool _isOpen;
    private readonly bool _publiclyVisible;

    public ReadOnlyMemoryStream(ReadOnlyMemory<byte> buffer)
        : this(buffer, publiclyVisible: true)
    {
    }

    public ReadOnlyMemoryStream(ReadOnlyMemory<byte> buffer, bool publiclyVisible)
    {
        _buffer = buffer;
        _publiclyVisible = publiclyVisible;
        _isOpen = true;
        _position = 0;
    }

    public override bool CanRead => _isOpen;
    public override bool CanSeek => _isOpen;
    public override bool CanWrite => false;

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
    public bool TryGetBuffer(out ReadOnlyMemory<byte> buffer)
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
        throw new NotSupportedException("Stream does not support writing.");
    }

    public override void WriteByte(byte value)
    {
        throw new NotSupportedException("Stream does not support writing.");
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
        throw new NotSupportedException("Cannot resize ReadOnlyBufferStream.");
    }

    public override void Flush()
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _isOpen)
        {
            _isOpen = false;
            // Don't set buffer to null - allow TryGetBuffer, GetBuffer & ToArray to work.
            // That the stream should no longer be used for I/O
            // doesn’t mean the underlying memory should be invalidated.
        }
        base.Dispose(disposing);
    }

    private void EnsureNotClosed()
    {
        if (!_isOpen)
            throw new ObjectDisposedException(GetType().Name);
    }
}
