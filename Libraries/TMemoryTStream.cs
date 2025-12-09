using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Memory.ROMStream.Libraries;

public class TMemoryTStream<T> : Stream where T : unmanaged
{
    private Memory<T> _buffer;
    private int _position;  // Position in terms of T elements
    private int _length;    // Length in terms of T elements
    private bool _isOpen;

    public TMemoryTStream(Memory<T> buffer)
    {
        _buffer = buffer;
        _length = buffer.Length;
        _isOpen = true;
    }

    public override bool CanRead => _isOpen;
    public override bool CanSeek => _isOpen;
    public override bool CanWrite => _isOpen;

    public override long Length
    {
        get
        {
            EnsureNotClosed();
            return _length * Unsafe.SizeOf<T>();
        }
    }

    // 5. Position - Required property
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
            if (value < 0 || value > _length * sizeOfT)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (value % sizeOfT != 0)
                throw new ArgumentException("Position must be aligned to element boundary.");
            _position = (int)(value / sizeOfT);
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (offset < 0 || count < 0) throw new ArgumentOutOfRangeException();
        if (buffer.Length - offset < count) throw new ArgumentException();

        EnsureNotClosed();

        ReadOnlySpan<byte> sourceBytes = MemoryMarshal.AsBytes(_buffer.Span);
        int bytePosition = _position * Unsafe.SizeOf<T>();
        int bytesAvailable = (_length * Unsafe.SizeOf<T>()) - bytePosition;
        int bytesToRead = Math.Min(bytesAvailable, count);

        if (bytesToRead <= 0)
            return 0;

        sourceBytes.Slice(bytePosition, bytesToRead).CopyTo(buffer.AsSpan(offset));
        
        int newBytePosition = bytePosition + bytesToRead;
        // Position is the index of the element containing the last byte read
        _position = (newBytePosition - 1) / Unsafe.SizeOf<T>();
        
        return bytesToRead;
    }
    
// //For doing encoding and serialization on-the-fly I was thinking on having overloads
// //for the methods and constructors involved
    public override void Write(byte[] buffer, int offset, int count)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (offset < 0 || count < 0) throw new ArgumentOutOfRangeException();
        if (buffer.Length - offset < count) throw new ArgumentException();

        EnsureNotClosed();

        int sizeOfT = Unsafe.SizeOf<T>();
        int bytePosition = _position * sizeOfT;
        int requiredLength = (bytePosition + count + sizeOfT - 1) / sizeOfT;

        if (requiredLength > _buffer.Length)
            throw new NotSupportedException("Cannot expand fixed buffer.");

        Span<byte> targetSpan = MemoryMarshal.AsBytes(_buffer.Span);
        buffer.AsSpan(offset, count).CopyTo(targetSpan.Slice(bytePosition));
        
        int newBytePosition = bytePosition + count;
        // Position is the index of the element containing the last byte written
        _position = (newBytePosition - 1) / sizeOfT;

        if (_position > _length)
            _length = _position;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        EnsureNotClosed();

        int sizeOfT = Unsafe.SizeOf<T>();
        long lengthInBytes = _length * sizeOfT;

        long newPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position * sizeOfT + offset,
            SeekOrigin.End => lengthInBytes + offset,
            _ => throw new ArgumentException("Invalid seek origin.")
        };

        if (newPosition < 0 || newPosition > lengthInBytes)
            throw new IOException("Seek position out of range.");

        _position = (int)(newPosition / sizeOfT);
        return newPosition;
    }

    public override void SetLength(long value)
    {
        if (value < 0 || value > _buffer.Length * Unsafe.SizeOf<T>())
            throw new ArgumentOutOfRangeException(nameof(value));

        EnsureNotClosed();

        int sizeOfT = Unsafe.SizeOf<T>();
        int newLength = (int)(value / sizeOfT);
        
        _length = newLength;
        if (_position > newLength)
            _position = newLength;
    }

    public override void Flush()
    {
        // No-op for memory streams
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _isOpen = false;
        base.Dispose(disposing);
    }

    private void EnsureNotClosed()
    {
        if (!_isOpen)
            throw new ObjectDisposedException(GetType().Name);
    }

    public bool TryGetBuffer(out Memory<T> buffer)
    {
        buffer = _buffer.Slice(0, _length);
        return true;
    }
}
