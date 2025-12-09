using Libraries;
using Memory.ROMStream.Libraries.Generic;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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
    public static Stream StreamFromReadOnlyMemory_T<T>(ReadOnlyMemory<T> memory)
        where T : unmanaged
    {
        return new ReadOnlyMemoryStream<T>(memory);
    }

    // Memory <T> 
    public static unsafe Stream StreamFromData_base(Memory<byte> memory, bool iswritable)
    {
        // Try to get the underlying array segment (zero-copy for array-backed Memory)
        if (MemoryMarshal.TryGetArray<byte>(memory, out ArraySegment<byte> segment))
        {
            // Array-backed: Create writable MemoryStream over the existing array segment
            return new MemoryStream(segment.Array!, segment.Offset, segment.Count, iswritable);
        }
        else
        {
            // Non-array-backed: Pin and use UnmanagedMemoryStream
            return new PinnedMemoryStream(memory, iswritable);
        }
    }

    public static Stream StreamFromData(Memory<byte> memory)
    {
        return new MemoryTStream(memory);
    }

    public static Stream StreamFromData_T<T>(Memory<T> memory)
        where T : unmanaged
    {
        return new TMemoryTStream<T>(memory);
    }

    // Manual cases - Helpers

    // Note: Had to enable unsafe code usage for the project
    // Internal helper class: Hook into UnmanagedMemoryStream's disposal creating a wrapper or derived class.
    // Manages the lifetime of the memory pin.
    private sealed unsafe class PinnedMemoryStream : UnmanagedMemoryStream
    {
        private readonly MemoryHandle _handle;

        public PinnedMemoryStream(Memory<byte> memory, bool writable = false)
        {
            _handle = memory.Pin();
            byte* ptr = (byte*)_handle.Pointer;

            if (writable)
            {
                Initialize(ptr, memory.Length, memory.Length, FileAccess.ReadWrite);
            }
            else
            {
                Initialize(ptr, memory.Length, memory.Length, FileAccess.Read);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _handle.Dispose(); // Unpin the memory
            }
        }
    }
}
