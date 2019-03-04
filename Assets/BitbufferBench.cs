#if DEVELOPMENT_BUILD
#define UNITY_ASSERTIONS
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using NetStack.Serialization;
using emotitron.Compression;
using UnityEngine.UI;


public class BitbufferBench : MonoBehaviour {


	public Text text;

	//static BitBuffer bb = new BitBuffer(256);
	static byte[] buffer = new byte[1024];
	static uint[] ibuffer = new uint[256];
	static ulong[] ubuffer = new ulong[128];

	public unsafe void IntegrityCheck()
	{
		int writepos = 0;
		int readpos = 0;


		//writepos = 0;
		//fixed (byte* bPtr = buffer)
		//{
		//	ulong* uPtr = (ulong*)bPtr;
		//	ArraySerializerUnsafe.AppendUnsafe(uPtr, 1, ref writepos, 1);
		//	ArraySerializerUnsafe.AppendUnsafe(uPtr, 1, ref writepos, 0);
		//	ArraySerializerUnsafe.AppendUnsafe(uPtr, ulong.MinValue, ref writepos, 64);
		//	ArraySerializerUnsafe.AppendUnsafe(uPtr, 666, ref writepos, 64);
		//	ArraySerializerUnsafe.AppendUnsafe(uPtr, ulong.MaxValue, ref writepos, 64);
		//}

		for (int i = 0; i < ubuffer.Length; ++i)
			ubuffer[i] = ulong.MaxValue;

		//writepos = 0;
		//ubuffer.WriteVerified(1, ref writepos, 1);
		//ubuffer.WriteVerified(1, ref writepos, 0);
		//ubuffer.WriteSignedPackedBits(-17, ref writepos, 17);
		//ubuffer.WriteVerified(ulong.MinValue, ref writepos, 64);
		//ubuffer.WriteVerified((ulong)666, ref writepos, 64);
		//ubuffer.WriteVerified(ulong.MaxValue, ref writepos, 64);


		writepos = 0;
		fixed (ulong* bPtr = ubuffer)
		{
			ulong* uPtr = (ulong*)bPtr;
			ArraySerializerUnsafe.Write(uPtr, 1, ref writepos, 1);
			ArraySerializerUnsafe.Write(uPtr, 1, ref writepos, 0);
			ArrayPackBitsExt.WriteSignedPackedBits(uPtr, -17, ref writepos, 17);
			ArraySerializerUnsafe.Write(uPtr, ulong.MinValue, ref writepos, 64);
			ArraySerializerUnsafe.Write(uPtr, 666, ref writepos, 64);
			ArraySerializerUnsafe.Write(uPtr, ulong.MaxValue, ref writepos, 64);
		}

		writepos = 0;
		buffer.Write(1, ref writepos, 1);
		buffer.Write(1, ref writepos, 0);
		buffer.WriteSignedPackedBits(-17, ref writepos, 17);
		buffer.Write(ulong.MinValue, ref writepos, 64);
		buffer.Write((ulong)666, ref writepos, 64);
		buffer.Write(ulong.MaxValue, ref writepos, 64);

		for (int i = 0; i < ibuffer.Length; ++i)
			ibuffer[i] = uint.MaxValue;

		writepos = 0;
		ibuffer.Write((ulong)1, ref writepos, 1);
		ibuffer.Write((ulong)1, ref writepos, 0);
		ibuffer.WriteSignedPackedBits(-17, ref writepos, 17);
		ibuffer.Write(ulong.MinValue, ref writepos, 64);
		ibuffer.Write((ulong)666, ref writepos, 64);
		ibuffer.Write(ulong.MaxValue, ref writepos, 64);

		//writepos = 0;
		//ubuffer.AppendVerified(1, ref writepos, 1);
		//ubuffer.AppendVerified(1, ref writepos, 0);
		//ubuffer.AppendVerified(ulong.MinValue, ref writepos, 64);
		//ubuffer.AppendVerified((ulong)666, ref writepos, 64);
		//ubuffer.AppendVerified(ulong.MaxValue, ref writepos, 64);

		//bb.Add(32, uint.MinValue);
		//bb.Add(32, 666);
		//bb.Add(32, uint.MaxValue);

		//readpos = 0;
		//writepos = 0;
		//fixed (uint* bPtr = ibuffer)
		//{
		//	ulong* uPtr = (ulong*)bPtr;
		//	str.Append(
		//		"Emo ReadUnsafe: : " +
		//		ArraySerializerUnsafe.ReadUnsafe(uPtr, ref readpos, 1) + " " +
		//		ArraySerializerUnsafe.ReadUnsafe(uPtr, ref readpos, 0) + " " +
		//		ArraySerializerUnsafe.ReadUnsafe(uPtr, ref readpos, 64) + " " +
		//		ArraySerializerUnsafe.ReadUnsafe(uPtr, ref readpos, 64) + " " +
		//		ArraySerializerUnsafe.ReadUnsafe(uPtr, ref readpos, 64) + "  :  \n"
		//		);
		//}


		readpos = 0;
		fixed (byte* bPtr = buffer)
		{
			ulong* uPtr = (ulong*)bPtr;
			str.Append(
				"Emo Read byte[] UnSafe : " +
				ArraySerializerUnsafe.Read(uPtr, ref readpos, 1) + " " +
				ArraySerializerUnsafe.Read(uPtr, ref readpos, 0) + " " +
				ArrayPackBitsExt.ReadSignedPackedBits(uPtr, ref readpos, 17) + " " +
				ArraySerializerUnsafe.Read(uPtr, ref readpos, 64) + " " +
				ArraySerializerUnsafe.Read(uPtr, ref readpos, 64) + " " +
				ArraySerializerUnsafe.Read(uPtr, ref readpos, 64) + "  :  \n"
				);
		}

		readpos = 0;
		str.Append(
			"Emo Uint[] Read: : " +
			ibuffer.Read(ref readpos, 1) + " " +
			ibuffer.Read(ref readpos, 0) + " " +
			ibuffer.ReadSignedPackedBits(ref readpos, 17) + " " +
			ibuffer.Read(ref readpos, 64) + " " +
			ibuffer.Read(ref readpos, 64) + " " +
			ibuffer.Read(ref readpos, 64) + "  :  \n"
			);

		readpos = 0;
		str.Append(
			"Emo Ulong[] Read: : " +
			ubuffer.Read(ref readpos, 1) + " " +
			ubuffer.Read(ref readpos, 0) + " " +
			ubuffer.ReadSignedPackedBits(ref readpos, 17) + " " +
			ubuffer.Read(ref readpos, 64) + " " +
			ubuffer.Read(ref readpos, 64) + " " +
			ubuffer.Read(ref readpos, 64) + "  :  \n"
			);

		readpos = 0;
		str.Append(
			"Emo byte[] Read: : " +
			buffer.Read(ref readpos, 1) + " " +
			buffer.Read(ref readpos, 0) + " " +
			buffer.ReadSignedPackedBits(ref readpos, 17) + " " +
			buffer.Read(ref readpos, 64) + " " +
			buffer.Read(ref readpos, 64) + " " +
			buffer.Read(ref readpos, 64) + "  :  \n"
			);

		//str.Append(
		//	"BB : " +
		//	bb.Read(32) + " " +
		//	bb.Read(32) + " " +
		//	bb.Read(32) + "  : \n "
		//	);

		writepos = 1;
		readpos = 1;
		//bb.Clear();

		buffer.Append(1, ref writepos, 1);
		buffer.Append(1, ref writepos, 0);
		buffer.WriteSignedPackedBits(-17, ref writepos, 17);
		buffer.Append(ulong.MinValue, ref writepos, 64);
		buffer.Append(666, ref writepos, 64);
		buffer.Append(ulong.MaxValue, ref writepos, 64);

		//bb.Add(2, 1);
		//bb.Add(32, uint.MinValue);
		//bb.Add(32, 666);
		//bb.Add(32, uint.MaxValue);

		str.Append(
			"Emo : " +
			buffer.Read(ref readpos, 1) + " " +
			buffer.Read(ref readpos, 0) + " " +
			buffer.ReadSignedPackedBits(ref readpos, 17) + " " +
			buffer.Read(ref readpos, 64) + " " +
			buffer.Read(ref readpos, 64) + " " +
			buffer.Read(ref readpos, 64) + " : \n"
			);

		//str.Append(
		//	"BB : " +
		//	bb.Read(2) + " >>: " +
		//	bb.Read(32) + " " +
		//	bb.Read(32) + " " +
		//	bb.Read(32) + "  : \n "
		//	);

		return;
	}

	const int CNT = 200000;

	public unsafe void Bench()
	{
		int writepos = 0;
		int readpos = 0;

		
		// BitBuffer

		//var BitBuffWatch = System.Diagnostics.Stopwatch.StartNew();

		//for (int cnt = 0; cnt < CNT; ++cnt)
		//{
		//	bb.Clear();
		//	for (int i = 1; i <= 32; ++i)
		//	{
		//		bb.Add(i, 1);
		//		bb.Add(i, 1);
		//		bb.Add(i, 1);
		//		bb.Add(i, 1);
		//	}
		//	bb.bitposition = 500;
		//	bb.readPosition = 0;
		//	for (int i = 1; i <= 32; ++i)
		//	{
		//		bb.Read(i);
		//		bb.Read(i);
		//		bb.Read(i);
		//		bb.Read(i);
		//	}
		//}
		//BitBuffWatch.Stop();
		//str.Append("Bitbuffer Append: " + BitBuffWatch.ElapsedMilliseconds + " ms\n");

		// Emo unsafe
		var EmoUnsafeWriteWatch = System.Diagnostics.Stopwatch.StartNew();

		fixed (uint* iPtr = ibuffer)
		{
			ulong* lPtr = (ulong*)iPtr;
			for (int cnt = 0; cnt < CNT; ++cnt)
			{
				writepos = 0;
				for (int i = 1; i <= 32; ++i)
				{
					ArraySerializerUnsafe.Write(lPtr, 1, ref writepos, i);
					ArraySerializerUnsafe.Write(lPtr, 1, ref writepos, i);
					ArraySerializerUnsafe.Write(lPtr, 1, ref writepos, i);
					ArraySerializerUnsafe.Write(lPtr, 1, ref writepos, i);
				}

				readpos = 0;
				for (int i = 1; i <= 32; ++i)
				{
					ArraySerializerUnsafe.Read(lPtr, ref readpos, i);
					ArraySerializerUnsafe.Read(lPtr, ref readpos, i);
					ArraySerializerUnsafe.Read(lPtr, ref readpos, i);
					ArraySerializerUnsafe.Read(lPtr, ref readpos, i);
				}
			}
		}
		EmoUnsafeWriteWatch.Stop();
		str.Append("Emo Uint[] Unsafe Write: " + EmoUnsafeWriteWatch.ElapsedMilliseconds + " ms\n");


		// Emo Unsafe Append
		var EmoUnsafeAppendWatch = System.Diagnostics.Stopwatch.StartNew();

		
		for (int cnt = 0; cnt < CNT; ++cnt)
		{
			fixed (uint* iPtr = ibuffer)
			{
				ulong* lPtr = (ulong*)iPtr;
				writepos = 0;
				for (int i = 1; i <= 32; ++i)
				{
					ArraySerializerUnsafe.Append(lPtr, 1, ref writepos, i);
					ArraySerializerUnsafe.Append(lPtr, 1, ref writepos, i);
					ArraySerializerUnsafe.Append(lPtr, 1, ref writepos, i);
					ArraySerializerUnsafe.Append(lPtr, 1, ref writepos, i);
				}

				readpos = 0;
				for (int i = 1; i <= 32; ++i)
				{
					ArraySerializerUnsafe.Read(lPtr, ref readpos, i);
					ArraySerializerUnsafe.Read(lPtr, ref readpos, i);
					ArraySerializerUnsafe.Read(lPtr, ref readpos, i);
					ArraySerializerUnsafe.Read(lPtr, ref readpos, i);
				}
			}
		}
		EmoUnsafeAppendWatch.Stop();
		str.Append("Emo Uint Unsafe Append: " + EmoUnsafeAppendWatch.ElapsedMilliseconds + " ms\n");

		// Emo ByteAppend Safe
		var EmoUintWriteWatch = System.Diagnostics.Stopwatch.StartNew();
		for (int cnt = 0; cnt < CNT; ++cnt)
		{
			writepos = 0;
			for (int i = 1; i <= 32; ++i)
			{
				ibuffer.Append((ulong)1, ref writepos, i);
				ibuffer.Append((ulong)1, ref writepos, i);
				ibuffer.Append((ulong)1, ref writepos, i);
				ibuffer.Append((ulong)1, ref writepos, i);

				//ibuffer.Append(1, ref writepos, i);
				//ibuffer.Append(1, ref writepos, i);
				//ibuffer.Append(1, ref writepos, i);
				//ibuffer.Append(1, ref writepos, i);
			}

			readpos = 0;
			for (int i = 1; i <= 32; ++i)
			{
				ibuffer.Read(ref readpos, i);
				ibuffer.Read(ref readpos, i);
				ibuffer.Read(ref readpos, i);
				ibuffer.Read(ref readpos, i);
			}
		}
		EmoUintWriteWatch.Stop();
		str.Append("Emo UInt[] Safe Append: " + EmoUintWriteWatch.ElapsedMilliseconds + " ms\n");

		// Emo UintAppend Safe
		var EmoUintAppendWatch = System.Diagnostics.Stopwatch.StartNew();
		for (int cnt = 0; cnt < CNT; ++cnt)
		{
			writepos = 0;
			for (int i = 1; i <= 32; ++i)
			{
				ibuffer.Append((ulong)1, ref writepos, i);
				ibuffer.Append((ulong)1, ref writepos, i);
				ibuffer.Append((ulong)1, ref writepos, i);
				ibuffer.Append((ulong)1, ref writepos, i);

				//ibuffer.Append(1, ref writepos, i);
				//ibuffer.Append(1, ref writepos, i);
				//ibuffer.Append(1, ref writepos, i);
				//ibuffer.Append(1, ref writepos, i);
			}

			readpos = 0;
			for (int i = 1; i <= 32; ++i)
			{
				ibuffer.Read(ref readpos, i);
				ibuffer.Read(ref readpos, i);
				ibuffer.Read(ref readpos, i);
				ibuffer.Read(ref readpos, i);
			}
		}
		EmoUintAppendWatch.Stop();
		str.Append("Emo uint[] Safe Append Ulong: " + EmoUintAppendWatch.ElapsedMilliseconds + " ms\n");

		// Repeat BitBuffer


		//var BitBuffWatch2 = System.Diagnostics.Stopwatch.StartNew();

		//for (int cnt = 0; cnt < CNT; ++cnt)
		//{
		//	bb.Clear();
		//	for (int i = 1; i <= 32; ++i)
		//	{
		//		bb.Add(i, 1);
		//		bb.Add(i, 1);
		//		bb.Add(i, 1);
		//		bb.Add(i, 1);
		//	}
		//	bb.bitposition = 500;
		//	bb.readPosition = 0;
		//	for (int i = 1; i <= 32; ++i)
		//	{
		//		bb.Read(i);
		//		bb.Read(i);
		//		bb.Read(i);
		//		bb.Read(i);
		//	}
		//}
		//BitBuffWatch2.Stop();
		//str.Append("Bitbuffer Append: " + BitBuffWatch2.ElapsedMilliseconds + " ms\n");


		str.Append("\n");
	}



	private void Awake()
	{
		text = GetComponent<Text>();
	}

	System.Text.StringBuilder str = new System.Text.StringBuilder(3000);
	// Use this for initialization
	void Start ()
	{
		
		//IntegrityCheck();

		Bench();
		text.text = str.ToString();
		str.Length = 0;
	}

	// Update is called once per frame

	float timer = 10f;
	void Update ()
	{
		if (timer < 15f)
		{
			timer += Time.deltaTime;
			return;
		}

		Bench();
		Bench();

		text.text = str.ToString();
		str.Length = 0;
		timer = 0;

	}
}
