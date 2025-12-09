using System;
using System.Buffers;
using System.Text;

namespace Memory.ROMStream.Libraries;

public sealed class UnifiedReadOnlyMemoryStream : Stream
{
    private readonly byte[] _encodedBytes;
    private readonly ReadOnlyMemory<byte> _byteBuffer;
    private readonly bool _ownsEncodedBytes;
    private int _position;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance with ReadOnlyMemory&lt;byte&gt;.
    /// </summary>
    public UnifiedReadOnlyMemoryStream(ReadOnlyMemory<byte> buffer)
    {
        _byteBuffer = buffer;
        _encodedBytes = Array.Empty<byte>();
        _ownsEncodedBytes = false;
    }

    /// <summary>
    /// Initializes a new instance with ReadOnlyMemory&lt;char&gt;, encoding to bytes.
    /// </summary>
    public UnifiedReadOnlyMemoryStream(ReadOnlyMemory<char> buffer, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;

        // Pre-encode the entire char buffer to bytes
        int maxByteCount = encoding.GetMaxByteCount(buffer.Length);
        byte[] tempBuffer = ArrayPool<byte>.Shared.Rent(maxByteCount);

        try
        {
            int actualByteCount = encoding.GetBytes(buffer.Span, tempBuffer);
            _encodedBytes = new byte[actualByteCount];
            Array.Copy(tempBuffer, _encodedBytes, actualByteCount);
            _byteBuffer = _encodedBytes;
            _ownsEncodedBytes = true;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(tempBuffer);
        }
    }

    public override bool CanRead => !_disposed;
    public override bool CanSeek => !_disposed;
    public override bool CanWrite => false;

    public override long Length
    {
        get
        {
            ThrowIfDisposed();
            return _byteBuffer.Length;
        }
    }

    public override long Position
    {
        get
        {
            ThrowIfDisposed();
            return _position;
        }
        set
        {
            ThrowIfDisposed();
            if (value < 0 || value > _byteBuffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _position = (int)value;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);
        ThrowIfDisposed();

        int bytesAvailable = _byteBuffer.Length - _position;
        int bytesToRead = Math.Min(bytesAvailable, count);

        _byteBuffer.Span.Slice(_position, bytesToRead).CopyTo(buffer.AsSpan(offset));
        _position += bytesToRead;

        return bytesToRead;
    }

    public override int Read(Span<byte> buffer)
    {
        ThrowIfDisposed();

        int bytesAvailable = _byteBuffer.Length - _position;
        int bytesToRead = Math.Min(bytesAvailable, buffer.Length);

        _byteBuffer.Span.Slice(_position, bytesToRead).CopyTo(buffer);
        _position += bytesToRead;

        return bytesToRead;
    }

    public override int ReadByte()
    {
        ThrowIfDisposed();

        if (_position >= _byteBuffer.Length)
        {
            return -1;
        }

        return _byteBuffer.Span[_position++];
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<int>(cancellationToken);
        }

        try
        {
            int bytesRead = Read(buffer.Span);
            return ValueTask.FromResult(bytesRead);
        }
        catch (Exception ex)
        {
            return ValueTask.FromException<int>(ex);
        }
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<int>(cancellationToken);
        }

        try
        {
            int bytesRead = Read(buffer, offset, count);
            return Task.FromResult(bytesRead);
        }
        catch (Exception ex)
        {
            return Task.FromException<int>(ex);
        }
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
        ValidateCopyToArguments(destination, bufferSize);
        ThrowIfDisposed();

        ReadOnlySpan<byte> source = _byteBuffer.Span.Slice(_position);
        _position = _byteBuffer.Length;

        // Write the remaining bytes
        byte[] temp = ArrayPool<byte>.Shared.Rent(Math.Min(bufferSize, source.Length));
        try
        {
            while (source.Length > 0)
            {
                int bytesToCopy = Math.Min(source.Length, temp.Length);
                source.Slice(0, bytesToCopy).CopyTo(temp);
                destination.Write(temp, 0, bytesToCopy);
                source = source.Slice(bytesToCopy);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(temp);
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        ThrowIfDisposed();

        long newPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => _byteBuffer.Length + offset,
            _ => throw new ArgumentException("Invalid seek origin.", nameof(origin))
        };

        if (newPosition < 0 || newPosition > _byteBuffer.Length)
        {
            throw new IOException("Seek position is out of range.");
        }

        _position = (int)newPosition;
        return newPosition;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("Cannot write to a read-only stream.");
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        throw new NotSupportedException("Cannot write to a read-only stream.");
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Cannot write to a read-only stream.");
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Cannot write to a read-only stream.");
    }

    public override void WriteByte(byte value)
    {
        throw new NotSupportedException("Cannot write to a read-only stream.");
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("Cannot resize a read-only stream.");
    }

    public override void Flush()
    {
        // No-op for read-only streams
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        return Task.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
            // Note: We don't clear _encodedBytes since it's just a reference
            // The GC will handle it when this object is collected
        }

        base.Dispose(disposing);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    private static void ValidateBufferArguments(byte[] buffer, int offset, int count)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");
        }

        if (buffer.Length - offset < count)
        {
            throw new ArgumentException("Buffer offset and count are invalid.");
        }
    }

    private static void ValidateCopyToArguments(Stream destination, int bufferSize)
    {
        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        if (!destination.CanWrite)
        {
            throw new NotSupportedException("Destination stream does not support writing.");
        }

        if (bufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size must be positive.");
        }
    }
}
