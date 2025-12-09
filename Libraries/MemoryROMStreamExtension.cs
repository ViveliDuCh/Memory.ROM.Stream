using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Libraries;

namespace Memory.ROMStream.Libraries;

public class MemoryROMStreamExtension
{
    public static Stream StreamFromText(ReadOnlyMemory<char> text, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        // Return MemoryStream wrapping the bytes
        return new MemoryStream(encoding.GetBytes(text.ToArray()), writable: false);
    }
    public static Stream StreamFromUnifiedROMText(ReadOnlyMemory<char> text, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return new UnifiedReadOnlyMemoryStream(text,encoding);
    }
    public static Stream StreamFromUnifiedROMData(ReadOnlyMemory<byte> memory)
    {
        return new UnifiedReadOnlyMemoryStream(memory);
    }
    public static Stream StreamFromInternalReadOnlyMemory(ReadOnlyMemory<byte> memory)
    {
        return new IntReadOnlyMemoryStream(memory);
    }
    
    public static Stream StreamFromReadOnlyMemory(ReadOnlyMemory<byte> memory)
    {
        return new ReadOnlyMemoryStream(memory);
    }
}
