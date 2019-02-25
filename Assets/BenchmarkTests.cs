using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using emotitron.Compression;

public class BenchmarkTests : MonoBehaviour
{
	public const int BYTE_CNT = 40;
	public const int LOOP = 10000;
	public static byte[] buffer = new byte[BYTE_CNT];
	//private static Bitstream bs = new Bitstream((ulong)222, (ulong)222, (ulong)222, (ulong)222, (uint)222);
	//private static Bitstream1024 bs1024 = new Bitstream1024();
	private static Bitstream<Buffer1024> bsvar = new Bitstream<Buffer1024>();
	private static Bitstream<Buffer40> bs40 = new Bitstream<Buffer40>();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static void Test()
	{
		Debug.Log("Testing <b>" + BYTE_CNT * LOOP + "</b> Byte Read/Writes");

		ByteForByteWrite();
		BitpackBytesEven();
		BitpackBytesUnEven();

		BitstreamTest();
		BitstreamIndirectTest();

		Debug.Log("--------");
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
				byte b = buffer.Read(ref bitpos, 8);
			}
		}
		
		watch.Stop();

		Debug.Log("Even Bitpack byte: time=" + watch.ElapsedMilliseconds + " ms");
	}


	public static void BitstreamTest()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (int loop = 0; loop < LOOP; ++loop)
		{

			bs40.Reset();

			/// First 1 bit write is to ensure all following byte writes don't align with a single byte in the byte[], 
			/// forcing worst case split across two byte[] indexs
			bs40.WriteBool(true);

			for (int i = 0; i < 40 - 1; ++i)
				bs40.Write(255, 8);

			bool ob = bs40.ReadBool();

			for (int i = 0; i < 40 - 1; ++i)
			{
				byte b = (byte)bs40.Read(8);
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
			byte ob = buffer.Read(ref bitpos, 1);

			for (int i = 0; i < BYTE_CNT - 1; ++i)
			{
				byte b = buffer.Read(ref bitpos, 8);
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
