using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using emotitron.Compression;

public class BenchmarkTests : MonoBehaviour
{
	public const int BYTE_CNT = 128;
	public const int LOOP = 1000000;
	public static byte[] buffer = new byte[BYTE_CNT * 8];
	public static uint[] ibuffer = new uint[128];
	public static ulong[] ubuffer = new ulong[128];
	public static ulong[] ubuffer2 = new ulong[128];
	//private static Bitstream bs = new Bitstream((ulong)222, (ulong)222, (ulong)222, (ulong)222, (uint)222);
	//private static Bitstream1024 bs1024 = new Bitstream1024();
	private static Bitstream<Buffer1024> bs1024 = new Bitstream<Buffer1024>();
	private static Bitstream<Buffer40> bs40 = new Bitstream<Buffer40>();
	private static Bitstream bs = new Bitstream();
	private static Bitstream bs2 = new Bitstream();

	public static void TestAsArray()
	{
		//int pos = 0;
		//buffer.Write(1, ref pos, 8);
		//buffer.Write(3, ref pos, 8);
		//buffer.Write(7, ref pos, 8);
		//buffer.Write(15, ref pos, 8);

		//Debug.Log(buffer.IndexAsUInt64(0));
		//Debug.Log(buffer.GetIndexAsUlongUnsafe(0));

		int pos = 0;
		ubuffer.Write(1, ref pos, 32);
		ubuffer.Write(3, ref pos, 32);
		ubuffer.Write(7, ref pos, 32);
		ubuffer.Write(15, ref pos, 32);
		pos = 0;
		Debug.Log(ubuffer.Read(ref pos, 32));
		Debug.Log(ubuffer.Read(ref pos, 32));
		Debug.Log(ubuffer.Read(ref pos, 32));
		Debug.Log(ubuffer.Read(ref pos, 32));

		Debug.Log(ubuffer.IndexAsUInt32(0));
		Debug.Log(ubuffer.IndexAsUInt32(1));
		Debug.Log(ubuffer.IndexAsUInt32(2));
		Debug.Log(ubuffer.IndexAsUInt32(3));
		return;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static void Test()
	{
		TestAsArray();

		return;

		ArrayCopy();
		ArrayCopySafe();


		bs.Buffer = ubuffer;
		bs2.Buffer = ubuffer2;

		int pos = 0;
		for (int i = 0; i < ubuffer.Length; ++i)
			ubuffer.Write(88, ref pos, 60);

		int pos2 = 0;
		ubuffer.ReadOutUnsafe(0, buffer, ref pos2, pos);

		pos = 0;
		for (int i = 0; i < ubuffer.Length; ++i)
			Debug.Log(i + ": " + buffer.Read(ref pos, 60));

		Debug.Log("Testing <b>" + BYTE_CNT * LOOP + "</b> Byte Read/Writes");

		//var watch = System.Diagnostics.Stopwatch.StartNew();
		//for (int loop = 0; loop < LOOP; ++loop)
		//	for (int i = 0; i < 20; ++i)
		//	{
		//		int pos = 0;
		//		ubuffer.WriteFast(2, ref pos, 60);
		//	}
		//watch.Stop();
		//Debug.Log("Fast " + watch.ElapsedMilliseconds);


		var watch2 = System.Diagnostics.Stopwatch.StartNew();
		for (int loop = 0; loop < LOOP; ++loop)
			for (int i = 0; i < 20; ++i)
			{
				int posit = 0;
				ubuffer.Write(2, ref posit, 60);
			}
		watch2.Stop();
		Debug.Log("Slow " + watch2.ElapsedMilliseconds);

		ByteForByteWrite();

		BitpackBytesEven();
		BitpackBytesUnEven();

		BitpackBytesToULongUneven();

		BitstreamTest();
		//BitstreamIndirectTest();
		BitstreamSafeArrayTest();


		Debug.Log("--------");
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

	}

	static void ArrayCopySafe()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (int loop = 0; loop < LOOP; ++loop)
		{
			int pos2 = 0;
			ubuffer.ReadArrayOutSafe(0, buffer, ref pos2, 120 * 8);
		}

		watch.Stop();

		Debug.Log("Array Copy Safe: time=" + watch.ElapsedMilliseconds + " ms");
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
				byte b = BasicWriter.BasicRead(buffer);
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
				byte b = (byte)buffer.Read(ref bitpos, 8);
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
				byte b = (byte)ubuffer.Read(ref bitpos, 33);
			}
		}

		watch.Stop();

		Debug.Log("Uneven Bitpack ulong[]: time=" + watch.ElapsedMilliseconds + " ms");
	}


	public static void BitstreamTest()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (int loop = 0; loop < LOOP; ++loop)
		{

			bs1024.Reset();

			/// First 1 bit write is to ensure all following byte writes don't align with a single byte in the byte[], 
			/// forcing worst case split across two byte[] indexs
			bs1024.WriteBool(true);

			for (int i = 0; i < BYTE_CNT - 1; ++i)
				bs1024.Write(255, 8);

			bool ob = bs1024.ReadBool();

			for (int i = 0; i < BYTE_CNT -1; ++i)
			{
				byte b = (byte)bs1024.Read(8);
			}
		}

		watch.Stop();

		Debug.Log("Unsafe Bitstream: time=" + watch.ElapsedMilliseconds + " ms");
	}

	public static void BitstreamIndirectTest()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (int loop = 0; loop < LOOP; ++loop)
		{

			bs40.Reset();

			/// First 1 bit write is to ensure all following byte writes don't align with a single byte in the byte[], 
			/// forcing worst case split across two byte[] indexs
			bs40.WriteBool(true);

			for (int i = 0; i < 40 - 1; ++i)
				bs40.WriteByte(255);

			bool ob = bs40.ReadBool();

			for (int i = 0; i < 40 - 1; ++i)
			{
				byte b = bs40.ReadByte();
			}
		}

		watch.Stop();

		Debug.Log("Unsafe Bitstream w/ Indirect Calls: time=" + watch.ElapsedMilliseconds + " ms");
	}
	

	public static void BitstreamSafeArrayTest()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (int loop = 0; loop < LOOP; ++loop)
		{
			bs.Reset();

			/// First 1 bit write is to ensure all following byte writes don't align with a single byte in the byte[], 
			/// forcing worst case split across two byte[] indexs
			bs.WriteBool(true);

			for (int i = 0; i < BYTE_CNT - 1; ++i)
					bs.Overwrite(255,  33);

			bool ob = bs.ReadBool();

			for (int i = 0; i < BYTE_CNT - 1; ++i)
			{
				byte b = (byte)bs.Read(33);
			}
		}

		watch.Stop();

		Debug.Log("Safe Bitstream w/ Array: time=" + watch.ElapsedMilliseconds + " ms");
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
			byte ob = (byte)buffer.Read(ref bitpos, 1);

			for (int i = 0; i < BYTE_CNT - 1; ++i)
			{
				byte b = (byte)buffer.Read(ref bitpos, 8);
			}
		}

		watch.Stop();

		Debug.Log("Uneven Bitpack byte: time=" + watch.ElapsedMilliseconds + " ms");
	}

	static float interval = 0;

	void Update()
	{
		interval += Time.deltaTime;
		if (interval > 3)
		{
			Test();
			interval = 0;
		}
	}
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
