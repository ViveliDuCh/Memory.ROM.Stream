namespace Libraries;

public class IntReadOnlyMemoryStream : Stream
{
    private ReadOnlyMemory<byte> _content;
    private int _position;
    private bool _isOpen;

    public IntReadOnlyMemoryStream(ReadOnlyMemory<byte> content)
    {
        _content = content;
        _isOpen = true;
    }

    public override bool CanRead => _isOpen;
    public override bool CanSeek => _isOpen;
    public override bool CanWrite => false;

    private void EnsureNotClosed()
    {
        if (!_isOpen)
        {
            throw new ObjectDisposedException(null, "Cannot access a closed stream.");
        }
    }

    public override long Length
    {
        get
        {
            EnsureNotClosed();
            return _content.Length;
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
            if (value < 0 || value > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _position = (int)value;
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        EnsureNotClosed();

        long pos =
            origin == SeekOrigin.Begin ? offset :
            origin == SeekOrigin.Current ? _position + offset :
            origin == SeekOrigin.End ? _content.Length + offset :
            throw new ArgumentOutOfRangeException(nameof(origin));

        if (pos > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }
        else if (pos < 0)
        {
            throw new IOException("An attempt was made to move the position before the beginning of the stream.");
        }

        _position = (int)pos;
        return _position;
    }

    public override int ReadByte()
    {
        EnsureNotClosed();

        ReadOnlySpan<byte> s = _content.Span;
        return _position < s.Length ? s[_position++] : -1;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Non-negative number required.");
        }

        if ((uint)count > buffer.Length - offset)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Offset and length were out of bounds for the array.");
        }

        EnsureNotClosed();

        int remaining = _content.Length - _position;

        if (remaining <= 0 || count == 0)
        {
            return 0;
        }
        else if (remaining <= count)
        {
            _content.Span.Slice(_position).CopyTo(new Span<byte>(buffer, offset, remaining));
            _position = _content.Length;
            return remaining;
        }
        else
        {
            _content.Span.Slice(_position, count).CopyTo(new Span<byte>(buffer, offset, count));
            _position += count;
            return count;
        }
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Non-negative number required.");
        }

        if ((uint)count > buffer.Length - offset)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Offset and length were out of bounds for the array.");
        }

        EnsureNotClosed();

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<int>(cancellationToken);
        }

        return Task.FromResult(Read(buffer, offset, count));
    }

    public override void Flush() { }

    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        _isOpen = false;
        _content = default;
        base.Dispose(disposing);
    }
}
