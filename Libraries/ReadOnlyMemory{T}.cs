using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Memory.ROMStream.Libraries.Generic;

public class ReadOnlyMemoryStream<T> : Stream 
    where T : unmanaged
{
    private ReadOnlyMemory<T> _buffer;
    private int _position;
    private bool _isOpen;
    private readonly bool _publiclyVisible;

    public ReadOnlyMemoryStream(ReadOnlyMemory<T> buffer)
        : this(buffer, publiclyVisible: true)
    {
    }

    public ReadOnlyMemoryStream(ReadOnlyMemory<T> buffer, bool publiclyVisible)
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
            return _buffer.Length * Unsafe.SizeOf<T>();
        }
    }

    // 
    public override long Position
    {
        get
        {
            EnsureNotClosed();
            return _position * Unsafe.SizeOf<T>();
        }
        set
        {
            EnsureNotClosed();
            int sizeOfT = Unsafe.SizeOf<T>();
            if (value < 0 || value > _buffer.Length * sizeOfT)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (value % sizeOfT != 0)
                throw new ArgumentException("Position must be aligned to element boundary.", nameof(value));
            _position = (int)(value / sizeOfT);
        }
    }

    // Attempts to get the underlying buffer.
    public bool TryGetBuffer(out ReadOnlyMemory<T> buffer)
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

        // Added a constraint to T to satisfy MemoryMarshal.AsBytes<T>,
        // which requires at least 'where T : struct'
        // Used 'unmanaged' instead because it implies 'struct' and is more restrictive
        ReadOnlySpan<byte> sourceBytes = MemoryMarshal.AsBytes(_buffer.Span);
        // Taking size of T into account so cases like T = char(2 bytes) work correctly
        int bytesAvailable = (_buffer.Length - _position) * Unsafe.SizeOf<T>();
        int bytesToRead = Math.Min(bytesAvailable, count);

        if (bytesToRead > 0)
        {
            int elementsToRead = bytesToRead / Unsafe.SizeOf<T>();
            ReadOnlySpan<byte> span = MemoryMarshal.AsBytes(_buffer.Span.Slice(_position, elementsToRead));
            span.CopyTo(buffer.AsSpan(offset, bytesToRead));
            _position += elementsToRead;
        }

        return bytesToRead;
    }

    // Maybe there should be a ReadElement or ReadT method
    public override int ReadByte()
    {
        EnsureNotClosed();

        ReadOnlySpan<byte> sourceBytes = MemoryMarshal.AsBytes(_buffer.Span);
        // Taking size of T into account so cases like T = char(2 bytes) work correctly
        int bytePosition = _position * Unsafe.SizeOf<T>();
        if (bytePosition >= sourceBytes.Length)
            return -1;

        byte result = sourceBytes[bytePosition];
        
        // Advance position by one byte
        if ((bytePosition + 1) % Unsafe.SizeOf<T>() == 0)
            _position++; 
        // Checks if completed an entire element of type T
        // before advancing the Position counter

        return result;
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

        int sizeOfT = Unsafe.SizeOf<T>();
        long lengthInBytes = _buffer.Length * sizeOfT;
        
        long newPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position * sizeOfT + offset, //Position in bytes + byte offset
            SeekOrigin.End => lengthInBytes + offset,
            _ => throw new ArgumentException("Invalid seek origin.", nameof(origin))
        };

        if (newPosition < 0 || newPosition > lengthInBytes)
            throw new IOException("Seek position out of range.");
        
        if (newPosition % sizeOfT != 0)
            throw new IOException("Seek position must be aligned to element boundary.");

        _position = (int)(newPosition / sizeOfT); // Advance _position in terms of T
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

