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

	//[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	//static void Test()
	//{
	//	TestAsArray();

	//	ArrayCopy();
	//}

	private void Start()
	{
		TestAsArray();

		ArrayCopy();
	}


	public unsafe static void TestAsArray()
	{
		int writep = 0;
		int readp = 0;


		ulong pbuffer = 0;
		int posi = 0;

		pbuffer = pbuffer.WritePackedBytes(666, ref posi, 25);
		posi = 0;
		ulong r5 = pbuffer.ReadPackedBytes(ref posi, 25);
		Debug.Log("<b>PackByte </b>" + 666 + " " + r5);
		Debug.Log("------");

		posi = 0;
		pbuffer = pbuffer.WriteSignedPackedBits(-129, ref posi, 32);
		posi = 0;
		int r7 = pbuffer.ReadSignedPackedBits(ref posi, 32);
		Debug.Log("<b>Packed Signed </b>" + -129 + " " + r7);
		Debug.Log("------");

		for (int i = -70; i < 70; ++i)
		{
			
			buffer.WriteSignedPackedBits(i, ref writep, 31);
			int r2 = buffer.ReadSignedPackedBits(ref readp, 31);
			Debug.Log(i + " " + r2);
		}

		//byte[] myBuffer = new byte[64];

		//int writepos = 0;
		//myBuffer.WriteBool(true, ref writepos);
		//myBuffer.WriteSigned(-666, ref writepos, 10);
		//myBuffer.Write(999, ref writepos, 10);

		//int readpos = 0;
		//bool restoredbool = myBuffer.ReadBool(ref readpos);
		//int restoredval1 = myBuffer.ReadSigned(ref readpos, 10);
		//uint restoredval2 = (uint)myBuffer.Read(ref readpos, 10);

		return;


		const int size = 63;
		ulong val1 = 1;
		int val2 = -2;
		ulong val3 = 3;
		ulong val4 = 4;
		ulong val5 = 5;
		ulong val6 = 6;
		ulong val7 = 7;
		ulong val8 = 8;
		ulong val9 = 9;

		var watch2 = System.Diagnostics.Stopwatch.StartNew();
		for (int loop = 0; loop < LOOP; ++loop)
		{
			int pos = 0;
			buffer.Write(val1, ref pos, size);
			buffer.Write((ulong)val2, ref pos, size);
			buffer.Write(val3, ref pos, size);
			buffer.Write(val4, ref pos, size);
			buffer.Write(val5, ref pos, size);
			buffer.Write(val6, ref pos, size);
			buffer.Write(val7, ref pos, size);
			buffer.Write(val8, ref pos, size);
			buffer.Write(val9, ref pos, size);

			pos = 0;
			buffer.Read(ref pos, size);
			buffer.Read(ref pos, size);
			buffer.Read(ref pos, size);
			buffer.Read(ref pos, size);
			buffer.Read(ref pos, size);
			buffer.Read(ref pos, size);
			buffer.Read(ref pos, size);
			buffer.Read(ref pos, size);
			buffer.Read(ref pos, size);
		}
		watch2.Stop();
		Debug.Log("Safe " + watch2.ElapsedMilliseconds);

		var watch1 = System.Diagnostics.Stopwatch.StartNew();
		fixed (byte* bPtr = buffer)
		{
			ulong* uPtr = (ulong*)bPtr;
			for (int loop = 0; loop < LOOP; ++loop)
			{
				int pos = 0;
				
				val1.InjectUnsafe(uPtr, ref pos, size);
				ArraySerializerUnsafe.WriteSigned(uPtr, val2, ref pos, size);
				val3.InjectUnsafe(uPtr, ref pos, size);
				val4.InjectUnsafe(uPtr, ref pos, size);
				val5.InjectUnsafe(uPtr, ref pos, size);
				val6.InjectUnsafe(uPtr, ref pos, size);
				val7.InjectUnsafe(uPtr, ref pos, size);
				val8.InjectUnsafe(uPtr, ref pos, size);
				val9.InjectUnsafe(uPtr, ref pos, size);


				pos = 0;
				ArraySerializerUnsafe.ReadUnsafe(uPtr, ref pos, size);
				ArraySerializerUnsafe.ReadSigned(uPtr, ref pos, size);
				ArraySerializerUnsafe.ReadUnsafe(uPtr, ref pos, size);
				ArraySerializerUnsafe.ReadUnsafe(uPtr, ref pos, size);
				ArraySerializerUnsafe.ReadUnsafe(uPtr, ref pos, size);
				ArraySerializerUnsafe.ReadUnsafe(uPtr, ref pos, size);
				ArraySerializerUnsafe.ReadUnsafe(uPtr, ref pos, size);
				ArraySerializerUnsafe.ReadUnsafe(uPtr, ref pos, size);
				ArraySerializerUnsafe.ReadUnsafe(uPtr, ref pos, size);
			}
		}
		
		watch1.Stop();
		Debug.Log("UnSafe " + watch1.ElapsedMilliseconds);

		return;
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
