using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using emotitron.Compression;

public class BenchmarkTests : MonoBehaviour
{
	public const int BYTE_CNT = 1024;
	public const int LOOP = 1000;
	public static byte[] buffer = new byte[BYTE_CNT];

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static void Test()
	{
		Debug.Log("Testing " + BYTE_CNT * LOOP + " Byte Read/Writes");

		ByteForByteWrite();
		BitpackBytesEven();
		BitpackBytesUnEven();
		ByteForByteWrite();
		BitpackBytesEven();
		BitpackBytesUnEven();
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

	public static void BitpackBytesUnEven()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();
		
		for (int loop = 0; loop < LOOP; ++loop)
		{
			int bitpos = 0;
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
	// Update is called once per frame
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
