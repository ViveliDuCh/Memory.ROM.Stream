using CommunityToolkit.HighPerformance;
using Libraries;
using System.Buffers.Binary;
using Memory.ROMStream.Libraries;
using System.Diagnostics;
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
Console.WriteLine($"Time taken with ReadOnlyMemoryStream (small): {sw.ElapsedMilliseconds}ms");

// Performance test: ReadOnlyMemoryStream - Big input
ReadOnlyMemory<byte> bigBytes = testData;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromReadOnlyMemory(bigBytes))
{
    for (int i = 0; i < bigBytes.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"Time taken with ReadOnlyMemoryStream (big): {sw.ElapsedMilliseconds}ms");

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
ReadOnlyMemory<byte> bigBytesAsStream = Encoding.UTF8.GetBytes(new string('A', 10_000_000));
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
ReadOnlyMemory<byte> bigBytesInternal = Encoding.UTF8.GetBytes(new string('A', 10_000_000));
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
ReadOnlyMemory<byte> bigBytesUnified = Encoding.UTF8.GetBytes(new string('A', 10_000_000));
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromUnifiedROMData(bigBytesUnified))
{
    for (int i = 0; i < bigBytesUnified.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"Time taken with UnifiedReadOnlyMemoryStream (big): {sw.ElapsedMilliseconds}ms");

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


// ---- Primitive types for T in Memory<T> + Stream ----
int value = 0x12345678;

// Little Endian
byte[] littleEndianBytes = BitConverter.GetBytes(value); // BitConverter uses system endianness
Console.WriteLine("Little Endian: " + BitConverter.ToString(littleEndianBytes));

// Big Endian (manual)
byte[] bigEndianBytes = new byte[4];
BinaryPrimitives.WriteInt32BigEndian(bigEndianBytes, value);
Console.WriteLine("Big Endian: " + BitConverter.ToString(bigEndianBytes));

// Write to stream
using var ms = new MemoryStream();
ms.Write(bigEndianBytes);
Console.WriteLine("Stream length: " + ms.Length);



