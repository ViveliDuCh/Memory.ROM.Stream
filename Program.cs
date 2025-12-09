using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Libraries;
using Memory.ROMStream.Libraries;
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

// Byte only
byte[] testData = new byte[1_000_000];
Array.Fill(testData, (byte)'A');

Console.WriteLine($"From Data");

// Performance test: MemoryStream - Small input
ReadOnlyMemory<char> basesmallInfo = "Small test content".AsMemory();
var sw = Stopwatch.StartNew();
using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(basesmallInfo.ToArray()))) {
    for (int i = 0; i < testData.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"(baseline - with in-front encoding) Time taken with MemoryStream (small): {sw.ElapsedMilliseconds}ms");

// Desired input but not being able to use it directly
char[] testTextData = new char[1_000_000];
Array.Fill(testData, (byte)'A');
ReadOnlyMemory<char> baseBigInfo = testTextData;

sw = Stopwatch.StartNew();
using (var stream = new MemoryStream(testData)) // Baseline but not really because it doesn't accept char[], ROM or text
{
    for (int i = 0; i < testData.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"(byte[]) Time taken with MemoryStream (big): {sw.ElapsedMilliseconds}ms");

// Performance test: ReadOnlyMemoryStream - Small input
ReadOnlyMemory<byte> smallBytes = Encoding.UTF8.GetBytes("Small test content");
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromReadOnlyMemory(smallBytes))
{
    for (int i = 0; i < smallBytes.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"FIXED TO BYTE ROM:: Time taken with ReadOnlyMemoryStream (small): {sw.ElapsedMilliseconds}ms");

// Performance test: ReadOnlyMemoryStream - Big input
ReadOnlyMemory<byte> bigBytes = testData;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromReadOnlyMemory(bigBytes))
{
    for (int i = 0; i < bigBytes.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"FIXED TO BYTE ROM: Time taken with ReadOnlyMemoryStream (big): {sw.ElapsedMilliseconds}ms");

// Performance test: CommunityToolkit.AsStream() - Small input
ReadOnlyMemory<byte> smallBytesAsStream = Encoding.UTF8.GetBytes("Small test content");
sw = Stopwatch.StartNew();
using (var stream = smallBytesAsStream.AsStream())
{
    for (int i = 0; i < smallBytesAsStream.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"Time taken with CommunityToolkit.AsStream() (small): {sw.ElapsedMilliseconds}ms");

// Performance test: CommunityToolkit.AsStream() - Big input
ReadOnlyMemory<byte> bigBytesAsStream = testData;
sw = Stopwatch.StartNew();
using (var stream = bigBytesAsStream.AsStream())
{
    for (int i = 0; i < bigBytesAsStream.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"Time taken with CommunityToolkit.AsStream() (big): {sw.ElapsedMilliseconds}ms");

// Performance test: IntReadOnlyMemoryStream - Small input
ReadOnlyMemory<byte> smallBytesInternal = Encoding.UTF8.GetBytes("Small test content");
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromInternalReadOnlyMemory(smallBytesInternal))
{
    for (int i = 0; i < smallBytesInternal.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"Time taken with Internal ReadOnlyMemoryStream (small): {sw.ElapsedMilliseconds}ms");

// Performance test: IntReadOnlyMemoryStream - Big input
ReadOnlyMemory<byte> bigBytesInternal = testData;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromInternalReadOnlyMemory(bigBytesInternal))
{
    for (int i = 0; i < bigBytesInternal.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"Time taken with Internal ReadOnlyMemoryStream (big): {sw.ElapsedMilliseconds}ms");

// Performance test: UnifiedReadOnlyMemoryStream - Small input
ReadOnlyMemory<byte> smallBytesUnified = Encoding.UTF8.GetBytes("Small test content");
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromUnifiedROMData(smallBytesUnified))
{
    for (int i = 0; i < smallBytesUnified.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"Time taken with UnifiedReadOnlyMemoryStream (small): {sw.ElapsedMilliseconds}ms");

// Performance test: UnifiedReadOnlyMemoryStream - Big input
ReadOnlyMemory<byte> bigBytesUnified = testData;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromUnifiedROMData(bigBytesUnified))
{
    for (int i = 0; i < bigBytesUnified.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"Time taken with UnifiedReadOnlyMemoryStream (big): {sw.ElapsedMilliseconds}ms");

// Generic ReadOnlyMemoryStream<T> tests
// Performance test: ReadOnlyMemoryStream<int> - Small input
int[] smallInts = Enumerable.Range(1, 100).ToArray();
ReadOnlyMemory<int> smallIntsGeneric = smallInts;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromReadOnlyMemory_T(smallIntsGeneric))
{
    for (int i = 0; i < smallIntsGeneric.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with ReadOnlyMemoryStream<int> (small): {sw.ElapsedMilliseconds}ms");

// Performance test: ReadOnlyMemoryStream<int> - Big input
int[] bigInts = Enumerable.Range(1, 1_000_000).ToArray();
ReadOnlyMemory<int> bigIntsGeneric = bigInts;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromReadOnlyMemory_T(bigIntsGeneric))
{
    //byte[] buffer = new byte[sizeof(int)];
    //while (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
    //{
    //    _ = BitConverter.ToInt32(buffer, 0);
    //}
    for (int i = 0; i < bigIntsGeneric.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with ReadOnlyMemoryStream<int> (big): {sw.ElapsedMilliseconds}ms");

// Performance test: ReadOnlyMemoryStream<byte> - Small input
byte[] smallBytesGeneric = Encoding.UTF8.GetBytes("Small test content");
ReadOnlyMemory<byte> smallBytesGenericROM = smallBytesGeneric;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromReadOnlyMemory_T(smallBytesGenericROM))
{
    for (int i = 0; i < smallBytesGenericROM.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with ReadOnlyMemoryStream<byte> (small): {sw.ElapsedMilliseconds}ms");

// Performance test: ReadOnlyMemoryStream<byte> - Big input
byte[] bigBytesGeneric = testData;
ReadOnlyMemory<byte> bigBytesGenericROM = bigBytesGeneric;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromReadOnlyMemory_T(bigBytesGenericROM))
{
    for (int i = 0; i < bigBytesGenericROM.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with ReadOnlyMemoryStream<byte> (big): {sw.ElapsedMilliseconds}ms");

// Performance test: ReadOnlyMemoryStream<float> - Small input
float[] smallFloats = Enumerable.Range(1, 100).Select(x => (float)x * 1.5f).ToArray();
ReadOnlyMemory<float> smallFloatsGeneric = smallFloats;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromReadOnlyMemory_T(smallFloatsGeneric))
{
    //byte[] buffer = new byte[sizeof(float)];
    //while (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
    //{
    //    _ = BitConverter.ToSingle(buffer, 0);
    //}
    for (int i = 0; i < smallFloatsGeneric.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with ReadOnlyMemoryStream<float> (small): {sw.ElapsedMilliseconds}ms");

// Performance test: ReadOnlyMemoryStream<float> - Big input
// Materializes the sequence of floats scaled by 1.5 into a float array in memory.
float[] bigFloats = Enumerable.Range(1, 1_000_000).Select(x => (float)x * 1.5f).ToArray();
ReadOnlyMemory<float> bigFloatsGeneric = bigFloats;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromReadOnlyMemory_T(bigFloatsGeneric))
{
    for (int i = 0; i < bigFloatsGeneric.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with ReadOnlyMemoryStream<float> (big): {sw.ElapsedMilliseconds}ms");

// ---- Char input ----
Console.WriteLine($"From Text");

// Performance test: MemoryStream - Small input
ReadOnlyMemory<char> smallText = "Small test content".AsMemory();
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromText(smallText))
{
    using var reader = new StreamReader(stream);
    _ = reader.ReadToEnd();
}
sw.Stop();
Console.WriteLine($"(baseline - With helper static method) Time taken with MemoryStream (small): {sw.ElapsedMilliseconds}ms");

// Performance test: MemoryStream - Big input
ReadOnlyMemory<char> bigText = new string('A', 10_000_000).AsMemory();
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromText(bigText))
{
    using var reader = new StreamReader(stream);
    _ = reader.ReadToEnd();
}
sw.Stop();
Console.WriteLine($"Time taken with MemoryStream (big): {sw.ElapsedMilliseconds}ms");

// Performance test: UnifiedReadOnlyMemoryStream with chars - Small input
ReadOnlyMemory<char> smallCharsUnified = "Small test content".AsMemory();
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromUnifiedROMText(smallCharsUnified))
{
    using var reader = new StreamReader(stream);
    _ = reader.ReadToEnd();
}
sw.Stop();
Console.WriteLine($"Time taken with UnifiedReadOnlyMemoryStream chars (small): {sw.ElapsedMilliseconds}ms");

// Performance test: UnifiedReadOnlyMemoryStream with chars - Big input
ReadOnlyMemory<char> bigCharsUnified = new string('A', 10_000_000).AsMemory();
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromUnifiedROMText(bigCharsUnified))
{
    using var reader = new StreamReader(stream);
    _ = reader.ReadToEnd();
}
sw.Stop();
Console.WriteLine($"Time taken with UnifiedReadOnlyMemoryStream chars (big): {sw.ElapsedMilliseconds}ms");

// Generic ReadOnlyMemoryStream<T> tests 
// Performance test: ReadOnlyMemoryStream<char> - Small input
ReadOnlyMemory<char> smallCharsGeneric = "Small test content".AsMemory();
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromReadOnlyMemory_T(smallCharsGeneric))
{
    using var reader = new StreamReader(stream);
    _ = reader.ReadToEnd();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with ReadOnlyMemoryStream<char> (small): {sw.ElapsedMilliseconds}ms");

// Performance test: ReadOnlyMemoryStream<char> - Big input
ReadOnlyMemory<char> bigCharsGeneric = new string('A', 10_000_000).AsMemory();
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromReadOnlyMemory_T(bigCharsGeneric))
{
    using var reader = new StreamReader(stream);
    _ = reader.ReadToEnd();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with ReadOnlyMemoryStream<char> (big): {sw.ElapsedMilliseconds}ms");




// ---- Correctness Minimal Tests ----
Console.WriteLine($"\n\nGeneric ROM Correctness Tests");

// --- Test byte ---
var bytes = new byte[] { 10, 20, 30 };
Console.WriteLine("\nBYTE Original: " + string.Join(",", bytes));

using (var s = MemoryROMStreamExtension.StreamFromReadOnlyMemory_T((ReadOnlyMemory<byte>)bytes))
{
    var buffer = new byte[3];
    int read = s.Read(buffer, 0, buffer.Length);
    Console.WriteLine("BYTE test: " + string.Join(",", buffer));  
}

// --- Test char ---
var chars = "ABC".ToCharArray(); // chars = ['A','B','C']
ReadOnlySpan<byte> chars_bytes = MemoryMarshal.AsBytes<char>(chars);
Console.WriteLine("\nCHAR original bytes: " + BitConverter.ToString(chars_bytes.ToArray()));
Console.WriteLine("CHAR original chars: " + string.Join("", chars));

using (var s = MemoryROMStreamExtension.StreamFromReadOnlyMemory_T((ReadOnlyMemory<char>)chars))
{
    // chars are 2 bytes each (UTF-16)
    var buffer = new byte[chars.Length * 2];
    _ = s.Read(buffer, 0, buffer.Length);

    Console.WriteLine("CHAR test bytes:     " + BitConverter.ToString(buffer));

    string reconstructed = Encoding.Unicode.GetString(buffer);
    Console.WriteLine("CHAR test chars:     " + reconstructed); // reconstructed
}

// --- Test int ---
var ints = new int[] { 100, 200, 300 };

ReadOnlySpan<byte> ints_bytes = MemoryMarshal.AsBytes<int>(ints);
Console.WriteLine("\nINT original bytes: " + BitConverter.ToString(ints_bytes.ToArray()));
Console.WriteLine("INT original ints:  " + string.Join(",", ints));

using (var s = MemoryROMStreamExtension.StreamFromReadOnlyMemory_T((ReadOnlyMemory<int>)ints))
{
    var buffer = new byte[ints.Length * 4];
    _ = s.Read(buffer, 0, buffer.Length);

    Console.WriteLine("INT test bytes:     " + BitConverter.ToString(buffer));

    // Convert back to ints
    int[] back = new int[3];
    Buffer.BlockCopy(buffer, 0, back, 0, buffer.Length);

    Console.WriteLine("INT test ints:      " + string.Join(",", back)); //reconstructed
}

// --- Test float ---
var floats = new float[] { 1.5f, 2.5f, 3.5f };

ReadOnlySpan<byte> floats_bytes = MemoryMarshal.AsBytes<float>(floats);
Console.WriteLine("\nFLOAT original bytes: " + BitConverter.ToString(floats_bytes.ToArray()));
Console.WriteLine("FLOAT original floats:  " + string.Join(",", floats));

using (var s = MemoryROMStreamExtension.StreamFromReadOnlyMemory_T((ReadOnlyMemory<float>)floats))
{
    var buffer = new byte[floats.Length * 4];
    _ = s.Read(buffer, 0, buffer.Length);

    Console.WriteLine("FLOAT test bytes:     " + BitConverter.ToString(buffer));

    // Convert back to floats
    float[] back = new float[3];
    Buffer.BlockCopy(buffer, 0, back, 0, buffer.Length);

    Console.WriteLine("FLOAT test floats:      " + string.Join(",", back)); //reconstructed
}

//// ---- Primitive types for T in Memory<T> + Stream ----
//int value = 0x12345678;

//// Little Endian
//byte[] littleEndianBytes = BitConverter.GetBytes(value); // BitConverter uses system endianness
//Console.WriteLine("Little Endian: " + BitConverter.ToString(littleEndianBytes));

//// Big Endian (manual)
//byte[] bigEndianBytes = new byte[4];
//BinaryPrimitives.WriteInt32BigEndian(bigEndianBytes, value);
//Console.WriteLine("Big Endian: " + BitConverter.ToString(bigEndianBytes));

//// Write to stream
//using var ms = new MemoryStream();
//ms.Write(bigEndianBytes);
//Console.WriteLine("Stream length: " + ms.Length);



