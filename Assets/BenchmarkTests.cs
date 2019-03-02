using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using emotitron.Compression;

public class BenchmarkTests : MonoBehaviour
{
	public const int BYTE_CNT = 128;
	public const int LOOP = 1000000;
	public static byte[] buffer = new byte[4800];
	public static uint[] ibuffer = new uint[128];
	public static ulong[] ubuffer = new ulong[128];
	public static ulong[] ubuffer2 = new ulong[128];

	//[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	//static void Test()
	//{
	//	TestAsArray();

	//	ArrayCopy();
	//}

	private void Start()
	{
		TestWriterIntegrity();

		ArrayCopy();
		ArrayCopySafe();
	}


	public static void TestWriterIntegrity()
	{
		int wpos = 1;
		int rpos = 1;

		ubuffer.Write(ulong.MaxValue, ref wpos, 64);
		if (ubuffer.Read(ref rpos, 64) != ulong.MaxValue)
			Debug.Log("Error writing with maxulong");

		for (int i = 0; i < 3000; ++i)
		{
			wpos = Random.Range(0, 200);
			rpos = wpos;
			int holdpos = wpos;
			int size = Random.Range(1, 64);
			int val = Random.Range(-(1 << (size - 1)), (1 << (size - 1)) - 1);
			ubuffer.WriteSigned(val, ref wpos, size);
			ubuffer.WriteSigned(val, ref wpos, size);
			ubuffer.WriteSigned(val, ref wpos, size);
			if (ubuffer.ReadSigned(ref rpos, size) != val)
				Debug.Log("Error writing " + val + " to pos " + holdpos + " with size " + size);
			if (ubuffer.ReadSigned(ref rpos, size) != val)
				Debug.Log("Error writing " + val + " to pos " + holdpos + " with size " + size);
			if (ubuffer.ReadSigned(ref rpos, size) != val)
				Debug.Log("Error writing " + val + " to pos " + holdpos + " with size " + size);


			ulong val2 = (ulong)Random.Range(0, ((ulong)1 << (size)) - 1);
			ubuffer.Write(val2, ref wpos, size);
			ubuffer.Write(val2, ref wpos, size);
			ubuffer.Write(val2, ref wpos, size);
			if (ubuffer.Read(ref rpos, size) != val2)
				Debug.Log("Error writing " + val2 + " to pos " + holdpos + " with size " + size);
			if (ubuffer.Read(ref rpos, size) != val2)
				Debug.Log("Error writing " + val2 + " to pos " + holdpos + " with size " + size);
			if (ubuffer.Read(ref rpos, size) != val2)
				Debug.Log("Error writing " + val2 + " to pos " + holdpos + " with size " + size);
		}

		Debug.Log("Integrity check complete.");

	}

	//static void ArrayCopySafe()
	//{
	//	var watch = System.Diagnostics.Stopwatch.StartNew();

	//	for (int loop = 0; loop < LOOP; ++loop)
	//	{
	//		int pos2 = 0;
	//		ubuffer.ReadArrayOutSafe(0, buffer, ref pos2, 120 * 8);
	//	}

	//	watch.Stop();

	//	Debug.Log("Array Copy Safe: time=" + watch.ElapsedMilliseconds + " ms");
	//}

	static void TestLog2()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		uint i = 0;
		while (i <= uint.MaxValue)
		{
			i.UsedBitCount();
			i.UsedBitCount();
			i.UsedBitCount();
			i.UsedBitCount();
			i.UsedBitCount();

			if ((uint.MaxValue - i) < 4000)
				break;

			i += 3000;
		}

		watch.Stop();

		Debug.Log("Log2 nifty: time=" + watch.ElapsedMilliseconds + " ms");
	}

	static void ArrayCopy()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (int loop = 0; loop < LOOP; ++loop)
		{
			int pos2 = 0;
			ubuffer.ReadOutUnsafe(0, buffer, ref pos2, 120 * 8);
		}

		watch.Stop();

		Debug.Log("Array Copy Unsafe: time=" + watch.ElapsedMilliseconds + " ms");
	}

	static void ArrayCopySafe()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (int loop = 0; loop < LOOP; ++loop)
		{
			int pos2 = 0;
			ubuffer.ReadOutSafe(0, buffer, ref pos2, 120 * 8);
		}

		watch.Stop();

		Debug.Log("Array Copy Safe: time=" + watch.ElapsedMilliseconds + " ms");
	}

	public static void ByteForByteWrite()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (int loop = 0; loop < LOOP; ++loop)
		{
			BasicWriter.Reset();
			for (int i = 0; i < BYTE_CNT; ++i)
				BasicWriter.BasicWrite(buffer, 255);

			BasicWriter.Reset();
			for (int i = 0; i < BYTE_CNT; ++i)
			{
				BasicWriter.BasicRead(buffer);
			}
		}

		watch.Stop();

		Debug.Log("Byte For Byte: time=" + watch.ElapsedMilliseconds + " ms");
	}

	public static void BitpackBytesEven()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (int loop = 0; loop < LOOP; ++loop)
		{
			/// First 1 bit write is to ensure all following byte writes don't align with a single byte in the byte[], 
			/// forcing worst case split across two byte[] indexs
			int bitpos = 0;
			for (int i = 0; i < BYTE_CNT; ++i)
				buffer.Write(255, ref bitpos, 8);

			bitpos = 0;
			for (int i = 0; i < BYTE_CNT - 1; ++i)
			{
				buffer.Read(ref bitpos, 8);
			}
		}
		
		watch.Stop();

		Debug.Log("Even Bitpack byte: time=" + watch.ElapsedMilliseconds + " ms");
	}

	public static void BitpackBytesToULongUneven()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (int loop = 0; loop < LOOP; ++loop)
		{
			/// First 1 bit write is to ensure all following byte writes don't align with a single byte in the byte[], 
			/// forcing worst case split across two byte[] indexs
			int bitpos = 0;
			ubuffer.Write(1, ref bitpos, 1);
			for (int i = 0; i < BYTE_CNT - 1; ++i)
				ubuffer.Write(255, ref bitpos, 33);

			bitpos = 0;
			ubuffer.Read(ref bitpos, 1);
			for (int i = 0; i < BYTE_CNT - 1; ++i)
			{
				ubuffer.Read(ref bitpos, 33);
			}
		}

		watch.Stop();

		Debug.Log("Uneven Bitpack ulong[]: time=" + watch.ElapsedMilliseconds + " ms");
	}

	
	public static void BitpackBytesUnEven()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();
		
		for (int loop = 0; loop < LOOP; ++loop)
		{
			int bitpos = 0;

			/// First 1 bit write is to ensure all following byte writes don't align with a single byte in the byte[], 
			/// forcing worst case split across two byte[] indexs
			buffer.Write(1, ref bitpos, 1);

			for (int i = 0; i < BYTE_CNT - 1; ++i)
				buffer.Write(255, ref bitpos, 8);

			bitpos = 0;
			buffer.Read(ref bitpos, 1);

			for (int i = 0; i < BYTE_CNT - 1; ++i)
			{
				buffer.Read(ref bitpos, 8);
			}
		}

		watch.Stop();

		Debug.Log("Uneven Bitpack byte: time=" + watch.ElapsedMilliseconds + " ms");
	}

	static float interval = 0;

	//void Update()
	//{
	//	interval += Time.deltaTime;
	//	if (interval > 3)
	//	{
	//		Test();
	//		interval = 0;
	//	}
	//}
}

/// <summary>
/// Simulate a VERY basic byte writer. This is just to make the test more fair than inlining byte writes, as this would never be inline.
/// </summary>
public class BasicWriter
{
	public static int pos;

	public static void Reset()
	{
		pos = 0;
	}
	public static byte[] BasicWrite(byte[] buffer, byte value)
	{
		//UnityEngine.Profiling.Profiler.BeginSample("Basic Write");

		buffer[pos] = value;
		pos++;

		//UnityEngine.Profiling.Profiler.EndSample();
		return buffer;

	}

	public static byte BasicRead(byte[] buffer)
	{
		//UnityEngine.Profiling.Profiler.BeginSample("Basic Write");

		byte b = buffer[pos];
		pos++;
		return b;

		//UnityEngine.Profiling.Profiler.EndSample();

	}

}
