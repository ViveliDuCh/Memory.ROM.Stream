using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Libraries;
using Memory.ROMStream.Libraries;
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

Console.WriteLine($"ReadOnlyMemory<T> Stream tests\n");
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


// Memory<T>
Console.WriteLine($"\nMemory<T> Stream tests\n");
// Baseline
// Performance test: StreamFromData_base - Small input READ
byte[] smallBytesDataBaseArray = Encoding.UTF8.GetBytes("Small test content");
Memory<byte> smallBytesDataBase = smallBytesDataBaseArray;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_base(smallBytesDataBase, iswritable: false))
{
    for (int i = 0; i < smallBytesDataBase.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"\n(BASELINE) Time taken with Unmanaged/MemoryStream READ (small): {sw.ElapsedMilliseconds}ms");

// Performance test: StreamFromData_base - Big input READ
byte[] bigBytesDataBaseArray = new byte[1_000_000];
Array.Fill(bigBytesDataBaseArray, (byte)'B');
Memory<byte> bigBytesDataBase = bigBytesDataBaseArray;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_base(bigBytesDataBase, iswritable: false))
{
    for (int i = 0; i < bigBytesDataBase.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"(BASELINE) Time taken with Unmanaged/MemoryStream READ (big): {sw.ElapsedMilliseconds}ms");

// Performance test: StreamFromData_base - Small input WRITE
byte[] smallWriteBufferBase = new byte[100];
Memory<byte> smallWriteDataBase = smallWriteBufferBase;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_base(smallWriteDataBase, iswritable: true))
{
    for (int i = 0; i < smallWriteDataBase.Length; i++)
        stream.WriteByte((byte)(i % 256));
}
sw.Stop();
Console.WriteLine($"(BASELINE) Time taken with Unmanaged/MemoryStream WRITE (small): {sw.ElapsedMilliseconds}ms");

// Performance test: StreamFromData_base - Big input WRITE
byte[] bigWriteBufferBase = new byte[1_000_000];
Memory<byte> bigWriteDataBase = bigWriteBufferBase;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_base(bigWriteDataBase, iswritable: true))
{
    for (int i = 0; i < bigWriteDataBase.Length; i++)
        stream.WriteByte((byte)'C');
}
sw.Stop();
Console.WriteLine($"(BASELINE) Time taken with Unmanaged/MemoryStream WRITE (big): {sw.ElapsedMilliseconds}ms");

// Performance test: CommunityToolkit.AsStream() with Memory<byte> - Small input READ
byte[] smallBytesMemoryAsStreamArray = Encoding.UTF8.GetBytes("Small test content");
Memory<byte> smallBytesMemoryAsStream = smallBytesMemoryAsStreamArray;
sw = Stopwatch.StartNew();
using (var stream = smallBytesMemoryAsStream.AsStream())
{
    for (int i = 0; i < smallBytesMemoryAsStream.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"Time taken with CommunityToolkit.AsStream() Memory<byte> READ (small): {sw.ElapsedMilliseconds}ms");

// Performance test: CommunityToolkit.AsStream() with Memory<byte> - Big input READ
Memory<byte> bigBytesMemoryAsStream = testData;
sw = Stopwatch.StartNew();
using (var stream = bigBytesMemoryAsStream.AsStream())
{
    for (int i = 0; i < bigBytesMemoryAsStream.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"Time taken with CommunityToolkit.AsStream() Memory<byte> READ (big): {sw.ElapsedMilliseconds}ms");

// Performance test: CommunityToolkit.AsStream() with Memory<byte> - Small input WRITE
byte[] smallWriteBufferMemoryAsStream = new byte[100];
Memory<byte> smallWriteDataMemoryAsStream = smallWriteBufferMemoryAsStream;
sw = Stopwatch.StartNew();
using (var stream = smallWriteDataMemoryAsStream.AsStream())
{
    for (int i = 0; i < smallWriteDataMemoryAsStream.Length; i++)
        stream.WriteByte((byte)(i % 256));
}
sw.Stop();
Console.WriteLine($"Time taken with CommunityToolkit.AsStream() Memory<byte> WRITE (small): {sw.ElapsedMilliseconds}ms");

// Performance test: CommunityToolkit.AsStream() with Memory<byte> - Big input WRITE
byte[] bigWriteBufferMemoryAsStream = new byte[1_000_000];
Memory<byte> bigWriteDataMemoryAsStream = bigWriteBufferMemoryAsStream;
sw = Stopwatch.StartNew();
using (var stream = bigWriteDataMemoryAsStream.AsStream())
{
    for (int i = 0; i < bigWriteDataMemoryAsStream.Length; i++)
        stream.WriteByte((byte)'C');
}
sw.Stop();
Console.WriteLine($"Time taken with CommunityToolkit.AsStream() Memory<byte> WRITE (big): {sw.ElapsedMilliseconds}ms");

// Fixed MemoryTStream tests
// Performance test: MemoryTStream (StreamFromData) - Small input READ
byte[] smallBytesDataArray = Encoding.UTF8.GetBytes("Small test content");
Memory<byte> smallBytesData = smallBytesDataArray;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData(smallBytesData))
{
    for (int i = 0; i < smallBytesData.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"(byte-FIXED) Time taken with MemoryTStream READ (small): {sw.ElapsedMilliseconds}ms");

// Performance test: MemoryTStream (StreamFromData) - Big input READ
byte[] bigBytesDataArray = new byte[1_000_000];
Array.Fill(bigBytesDataArray, (byte)'B');
Memory<byte> bigBytesData = bigBytesDataArray;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData(bigBytesData))
{
    for (int i = 0; i < bigBytesData.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"(byte-FIXED) Time taken with MemoryTStream READ (big): {sw.ElapsedMilliseconds}ms");

// Performance test: MemoryTStream (StreamFromData) - Small input WRITE
byte[] smallWriteBuffer = new byte[100];
Memory<byte> smallWriteData = smallWriteBuffer;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData(smallWriteData))
{
    for (int i = 0; i < smallWriteData.Length; i++)
        stream.WriteByte((byte)(i % 256));
}
sw.Stop();
Console.WriteLine($"(byte-FIXED) Time taken with MemoryTStream WRITE (small): {sw.ElapsedMilliseconds}ms");

// Performance test: MemoryTStream (StreamFromData) - Big input WRITE
byte[] bigWriteBuffer = new byte[1_000_000];
Memory<byte> bigWriteData = bigWriteBuffer;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData(bigWriteData))
{
    for (int i = 0; i < bigWriteData.Length; i++)
        stream.WriteByte((byte)'C');
}
sw.Stop();
Console.WriteLine($"(byte-FIXED) Time taken with MemoryTStream WRITE (big): {sw.ElapsedMilliseconds}ms");

// Generic TMemoryTStream<T> tests
Console.WriteLine($"\nGeneric TMemoryTStream<T> tests");

// Performance test: TMemoryTStream<byte> (StreamFromData_T) - Small input READ
byte[] smallBytesDataTArray = Encoding.UTF8.GetBytes("Small test content");
Memory<byte> smallBytesDataT = smallBytesDataTArray;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(smallBytesDataT))
{
    for (int i = 0; i < smallBytesDataT.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<byte> READ (small): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<byte> (StreamFromData_T) - Big input READ
byte[] bigBytesDataTArray = new byte[1_000_000];
Array.Fill(bigBytesDataTArray, (byte)'B');
Memory<byte> bigBytesDataT = bigBytesDataTArray;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(bigBytesDataT))
{
    for (int i = 0; i < bigBytesDataT.Length; i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<byte> READ (big): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<byte> (StreamFromData_T) - Small input WRITE
byte[] smallWriteBufferT = new byte[100];
Memory<byte> smallWriteDataT = smallWriteBufferT;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(smallWriteDataT))
{
    for (int i = 0; i < smallWriteDataT.Length; i++)
        stream.WriteByte((byte)(i % 256));
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<byte> WRITE (small): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<byte> (StreamFromData_T) - Big input WRITE
byte[] bigWriteBufferT = new byte[1_000_000];
Memory<byte> bigWriteDataT = bigWriteBufferT;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(bigWriteDataT))
{
    for (int i = 0; i < bigWriteDataT.Length; i++)
        stream.WriteByte((byte)'C');
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<byte> WRITE (big): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<int> (StreamFromData_T) - Small input READ
int[] smallIntsDataT = Enumerable.Range(1, 100).ToArray();
Memory<int> smallIntsMemoryT = smallIntsDataT;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(smallIntsMemoryT))
{
    // Read as bytes - 4 bytes per int, 100 ints = 400 bytes
    for (int i = 0; i < smallIntsMemoryT.Length * sizeof(int); i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<int> READ (small): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<int> (StreamFromData_T) - Big input READ
int[] bigIntsDataT = Enumerable.Range(1, 250_000).ToArray();
Memory<int> bigIntsMemoryT = bigIntsDataT;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(bigIntsMemoryT))
{
    // Read as bytes - 4 bytes per int, 250,000 ints = 1,000,000 bytes
    for (int i = 0; i < bigIntsMemoryT.Length * sizeof(int); i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<int> READ (big): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<int> (StreamFromData_T) - Small input WRITE
int[] smallIntsWriteT = new int[100];
Memory<int> smallIntsWriteMemoryT = smallIntsWriteT;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(smallIntsWriteMemoryT))
{
    // Write as bytes - 4 bytes per int, 100 ints = 400 bytes
    for (int i = 0; i < smallIntsWriteMemoryT.Length * sizeof(int); i++)
        stream.WriteByte((byte)(i % 256));
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<int> WRITE (small): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<int> (StreamFromData_T) - Big input WRITE
int[] bigIntsWriteT = new int[250_000];
Memory<int> bigIntsWriteMemoryT = bigIntsWriteT;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(bigIntsWriteMemoryT))
{
    // Write as bytes - 4 bytes per int, 250,000 ints = 1,000,000 bytes
    for (int i = 0; i < bigIntsWriteMemoryT.Length * sizeof(int); i++)
        stream.WriteByte((byte)'C');
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<int> WRITE (big): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<float> (StreamFromData_T) - Small input READ
float[] smallFloatsDataT = Enumerable.Range(1, 100).Select(x => (float)x * 1.5f).ToArray();
Memory<float> smallFloatsMemoryT = smallFloatsDataT;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(smallFloatsMemoryT))
{
    // Read as bytes - 4 bytes per float, 100 floats = 400 bytes
    for (int i = 0; i < smallFloatsMemoryT.Length * sizeof(float); i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<float> READ (small): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<float> (StreamFromData_T) - Big input READ
float[] bigFloatsDataT = Enumerable.Range(1, 250_000).Select(x => (float)x * 1.5f).ToArray();
Memory<float> bigFloatsMemoryT = bigFloatsDataT;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(bigFloatsMemoryT))
{
    // Read as bytes - 4 bytes per float, 250,000 floats = 1,000,000 bytes
    for (int i = 0; i < bigFloatsMemoryT.Length * sizeof(float); i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<float> READ (big): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<float> (StreamFromData_T) - Small input WRITE
float[] smallFloatsWriteT = new float[100];
Memory<float> smallFloatsWriteMemoryT = smallFloatsWriteT;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(smallFloatsWriteMemoryT))
{
    // Write as bytes - 4 bytes per float, 100 floats = 400 bytes
    for (int i = 0; i < smallFloatsWriteMemoryT.Length * sizeof(float); i++)
        stream.WriteByte((byte)(i % 256));
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<float> WRITE (small): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<float> (StreamFromData_T) - Big input WRITE
float[] bigFloatsWriteT = new float[250_000];
Memory<float> bigFloatsWriteMemoryT = bigFloatsWriteT;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(bigFloatsWriteMemoryT))
{
    // Write as bytes - 4 bytes per float, 250,000 floats = 1,000,000 bytes
    for (int i = 0; i < bigFloatsWriteMemoryT.Length * sizeof(float); i++)
        stream.WriteByte((byte)'C');
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<float> WRITE (big): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<char> (StreamFromData_T) - Small input READ
char[] smallCharsDataT = "Small test content".ToCharArray();
Memory<char> smallCharsMemoryT = smallCharsDataT;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(smallCharsMemoryT))
{
    // Read as bytes - 2 bytes per char
    for (int i = 0; i < smallCharsMemoryT.Length * sizeof(char); i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<char> READ (small): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<char> (StreamFromData_T) - Big input READ
char[] bigCharsDataT = new string('A', 1_000_000).ToCharArray();
Memory<char> bigCharsMemoryT = bigCharsDataT;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(bigCharsMemoryT))
{
    // Read as bytes - 2 bytes per char, 1,000,000 chars = 2,000,000 bytes
    for (int i = 0; i < bigCharsMemoryT.Length * sizeof(char); i++)
        _ = stream.ReadByte();
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<char> READ (big): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<char> (StreamFromData_T) - Small input WRITE
char[] smallCharsWriteT = new char[100];
Memory<char> smallCharsWriteMemoryT = smallCharsWriteT;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(smallCharsWriteMemoryT))
{
    // Write as bytes - 2 bytes per char, 100 chars = 200 bytes
    for (int i = 0; i < smallCharsWriteMemoryT.Length * sizeof(char); i++)
        stream.WriteByte((byte)(i % 256));
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<char> WRITE (small): {sw.ElapsedMilliseconds}ms");

// Performance test: TMemoryTStream<char> (StreamFromData_T) - Big input WRITE
char[] bigCharsWriteT = new char[1_000_000];
Memory<char> bigCharsWriteMemoryT = bigCharsWriteT;
sw = Stopwatch.StartNew();
using (var stream = MemoryROMStreamExtension.StreamFromData_T(bigCharsWriteMemoryT))
{
    // Write as bytes - 2 bytes per char, 1,000,000 chars = 2,000,000 bytes
    for (int i = 0; i < bigCharsWriteMemoryT.Length * sizeof(char); i++)
        stream.WriteByte((byte)'C');
}
sw.Stop();
Console.WriteLine($"GENERIC: Time taken with TMemoryTStream<char> WRITE (big): {sw.ElapsedMilliseconds}ms");




// ---- Correctness Minimal Tests ----
Console.WriteLine($"\n\nGeneric ReadOnlyMemory Stream Correctness Tests");

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

// ---- Correctness Minimal Tests for Memory<T> ----
Console.WriteLine($"\n\nGeneric Memory<T> Stream Correctness Tests");

// --- Test byte ---
var bytesMemory = new byte[] { 10, 20, 30 };
Console.WriteLine("\nBYTE Original: " + string.Join(",", bytesMemory));

using (var s = MemoryROMStreamExtension.StreamFromData_T((Memory<byte>)bytesMemory))
{
    var buffer = new byte[3];
    int read = s.Read(buffer, 0, buffer.Length);
    Console.WriteLine("BYTE test: " + string.Join(",", buffer));
}

// --- Test char ---
var charsMemory = "ABC".ToCharArray(); // chars = ['A','B','C']
ReadOnlySpan<byte> charsMemory_bytes = MemoryMarshal.AsBytes<char>(charsMemory.AsSpan());
Console.WriteLine("\nCHAR original bytes: " + BitConverter.ToString(charsMemory_bytes.ToArray()));
Console.WriteLine("CHAR original chars: " + string.Join("", charsMemory));

using (var s = MemoryROMStreamExtension.StreamFromData_T((Memory<char>)charsMemory))
{
    // chars are 2 bytes each (UTF-16)
    var buffer = new byte[charsMemory.Length * 2];
    _ = s.Read(buffer, 0, buffer.Length);

    Console.WriteLine("CHAR test bytes:     " + BitConverter.ToString(buffer));

    string reconstructed = Encoding.Unicode.GetString(buffer);
    Console.WriteLine("CHAR test chars:     " + reconstructed); // reconstructed
}

// --- Test int ---
var intsMemory = new int[] { 100, 200, 300 };

ReadOnlySpan<byte> intsMemory_bytes = MemoryMarshal.AsBytes<int>(intsMemory.AsSpan());
Console.WriteLine("\nINT original bytes: " + BitConverter.ToString(intsMemory_bytes.ToArray()));
Console.WriteLine("INT original ints:  " + string.Join(",", intsMemory));

using (var s = MemoryROMStreamExtension.StreamFromData_T((Memory<int>)intsMemory))
{
    var buffer = new byte[intsMemory.Length * 4];
    _ = s.Read(buffer, 0, buffer.Length);

    Console.WriteLine("INT test bytes:     " + BitConverter.ToString(buffer));

    // Convert back to ints
    int[] back = new int[3];
    Buffer.BlockCopy(buffer, 0, back, 0, buffer.Length);

    Console.WriteLine("INT test ints:      " + string.Join(",", back)); //reconstructed
}

// --- Test float ---
var floatsMemory = new float[] { 1.5f, 2.5f, 3.5f };

ReadOnlySpan<byte> floatsMemory_bytes = MemoryMarshal.AsBytes<float>(floatsMemory.AsSpan());
Console.WriteLine("\nFLOAT original bytes: " + BitConverter.ToString(floatsMemory_bytes.ToArray()));
Console.WriteLine("FLOAT original floats:  " + string.Join(",", floatsMemory));

using (var s = MemoryROMStreamExtension.StreamFromData_T((Memory<float>)floatsMemory))
{
    var buffer = new byte[floatsMemory.Length * 4];
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



