﻿/*
* The MIT License (MIT)
* 
* Copyright (c) 2018-2019 Davin Carten (emotitron) (davincarten@gmail.com)
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/


namespace emotitron.Compression
{

	/// <summary>
	/// A Utility class that gives all byte[], uint[] and ulong[] buffers bitpacking/serialization methods.
	/// </summary>
	public static class ArraySerializeExt
	{
		private const string bufferOverrunMsg = "Byte buffer overrun. Dataloss will occur.";

		#region Read/Write Signed Value

		public static void WriteSigned(this byte[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.Write(zigzag, ref bitposition, bits);
		}
		public static void WriteSigned(this uint[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.Write(zigzag, ref bitposition, bits);
		}
		public static void WriteSigned(this ulong[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.Write(zigzag, ref bitposition, bits);
		}
		public unsafe static void WriteSigned(ulong* buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			WriteUnsafe(buffer, zigzag, ref bitposition, bits);
		}
		public unsafe static void InjectSigned(this int value, ulong* buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			WriteUnsafe(buffer, zigzag, ref bitposition, bits);
		}

		public static int ReadSigned(this byte[] buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.Read(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}
		public static int ReadSigned(this uint[] buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.Read(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}
		public static int ReadSigned(this ulong[] buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.Read(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}
		public unsafe static int ReadSigned(ulong* uPtr, ref int bitposition, int bits)
		{
			uint value = (uint)ReadUnsafe(uPtr, ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}
		//public unsafe static int ExtractSigned(ulong* uPtr, ref int bitposition, int bits)
		//{
		//	uint value = (uint)ReadUnsafe(uPtr, ref bitposition, bits);
		//	int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
		//	return zagzig;
		//}

		#endregion

		#region Float Reader/Writer

		/// <summary>
		/// Converts a float to a 32bit uint with ByteConverter, then writes those 32 bits to the buffer.
		/// </summary>
		/// <param name="buffer">The array we are reading from.</param>
		/// <param name="value">The float value to write.</param>
		/// <param name="bitposition">The bit position in the array we start the read at. Will be incremented by 32 bits.</param>
		public static void WriteFloat(this byte[] buffer, float value, ref int bitposition)
		{
			Write(buffer, ((ByteConverter)value).uint32, ref bitposition, 32);
		}

		/// <summary>
		/// Reads a uint32 from the buffer, and converts that back to a float with a ByteConverter cast. If performance is a concern, you can call the primary (ByteConverter)byte[].Read())
		/// </summary>
		/// <param name="buffer">The array we are reading from.</param>
		/// <param name="bitposition">The bit position in the array we start the read at. Will be incremented by 32 bits.</param>
		public static float ReadFloat(this byte[] buffer, ref int bitposition)
		{
			return ((ByteConverter)Read(buffer, ref bitposition, 32));
		}

		#endregion

		#region Primary Writers

		/// <summary>
		/// This is the primary byte[].Write() method. All other byte[].Write methods lead to this one, so when performance matters, cast using (ByteConverter)value and use this method.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="value"></param>
		/// <param name="bitposition"></param>
		/// <param name="bits"></param>
		/// <returns></returns>
		public static void Write(this byte[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			const int MAXBITS = 8;
			const int MODULUS = MAXBITS - 1;
			int offset = -(bitposition & MODULUS); // this is just a modulus
			int index = bitposition >> 3;
			int endpos = bitposition + bits;
			int endindex = ((endpos - 1) >> 3);

			//System.Diagnostics.Debug.Assert((endpos <= buffer.Length << 3), bufferOverrunMsg);

			// Offset both the mask and the compressed value using the remainder as the offset
			ulong mask = ulong.MaxValue >> (64 - bits);

			ulong offsetmask = mask << -offset;
			ulong offsetcomp = (ulong)value << -offset;

			while (true)
			{
				// set the unwritten bits in byte[] to zero as a safety
				buffer[index] &= (byte)~offsetmask; // set bits to zero
				buffer[index] |= (byte)(offsetcomp & offsetmask);

				if (index == endindex)
					break;

				// Push the compressed value and the mask to align with the next array element
				offset += MAXBITS;
				offsetmask = mask >> offset;
				offsetcomp = (ulong)value >> offset;
				index++;
			}

			if (endpos > bitposition)
				bitposition = endpos;

			return;
		}

		//public unsafe static void WriteUnsafe(this byte[] buffer, ulong value, ref int bitposition, int bits)
		//{
		//	if (bits == 0)
		//		return;

		//	const int MAXBITS = 64;
		//	const int MODULUS = MAXBITS - 1;
		//	int offset = -(bitposition & MODULUS); // this is just a modulus
		//	int index = bitposition >> 6;
		//	int endpos = bitposition + bits;
		//	int endindex = ((endpos - 1) >> 6);

		//	System.Diagnostics.Debug.Assert((endpos <= buffer.Length << 3), bufferOverrunMsg);

		//	// Offset both the mask and the compressed value using the remainder as the offset
		//	ulong mask = ulong.MaxValue >> (64 - bits);

		//	ulong offsetmask = mask << -offset;
		//	ulong offsetcomp = (ulong)value << -offset;

		//	fixed (byte* bPtr = buffer)
		//	{
		//		ulong* uPtr = (ulong*)bPtr;
		//		while (true)
		//		{

		//			// set the unwritten bits in byte[] to zero as a safety
		//			uPtr[index] &= (ulong)~offsetmask; // set bits to zero
		//			uPtr[index] |= (ulong)(offsetcomp & offsetmask);

		//			if (index == endindex)
		//				break;

		//			// Push the compressed value and the mask to align with the next array element
		//			offset += MAXBITS;
		//			offsetmask = mask >> offset;
		//			offsetcomp = (ulong)value >> offset;
		//			index++;
		//		}
		//	}


		//	if (endpos > bitposition)
		//		bitposition = endpos;

		//	return;
		//}

		/// <summary>
		/// Primary Unsafe writer. Faster method for writing to byte[] or uint[] buffers. Uses unsafe to treat them as ulong[].
		/// WARNING: There is no bounds checking on this. If you write too far, you will crash.
		/// </summary>
		/// <param name="uPtr">Cast your byte* or uint* to ulong*</param>
		/// <param name="value"></param>
		/// <param name="bitposition"></param>
		/// <param name="bits"></param>
		public unsafe static void WriteUnsafe(ulong* uPtr, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			const int MAXBITS = 64;
			const int MODULUS = MAXBITS - 1;
			int offset = -(bitposition & MODULUS); // this is just a modulus
			int index = bitposition >> 6;
			int endpos = bitposition + bits;
			int endindex = ((endpos - 1) >> 6);

			//System.Diagnostics.Debug.Assert((endpos <= buffer.Length << 3), bufferOverrunMsg);

			// Offset both the mask and the compressed value using the remainder as the offset
			ulong mask = ulong.MaxValue >> (64 - bits);

			ulong offsetmask = mask << -offset;
			ulong offsetcomp = (ulong)value << -offset;

			while (true)
			{
				// set the unwritten bits in byte[] to zero as a safety
				uPtr[index] &= (ulong)~offsetmask; // set bits to zero
				uPtr[index] |= (ulong)(offsetcomp & offsetmask);

				if (index == endindex)
					break;

				// Push the compressed value and the mask to align with the next array element
				offset += MAXBITS;
				offsetmask = mask >> offset;
				offsetcomp = (ulong)value >> offset;
				index++;
			}

			if (endpos > bitposition)
				bitposition = endpos;

			return;
		}

		/// <summary>
		/// Primary Unsafe writer. Faster method for writing to byte[] or uint[] buffers. Uses unsafe to treat them as ulong[].
		/// WARNING: There is no bounds checking on this. If you write too far, you will crash.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="uPtr">Cast your byte* or uint* to ulong*</param>
		/// <param name="bitposition"></param>
		/// <param name="bits"></param>
		public unsafe static void InjectUnsafe(this ulong value, ulong* uPtr, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			const int MAXBITS = 64;
			const int MODULUS = MAXBITS - 1;
			int offset = -(bitposition & MODULUS); // this is just a modulus
			int index = bitposition >> 6;
			int endpos = bitposition + bits;
			int endindex = ((endpos - 1) >> 6);

			//System.Diagnostics.Debug.Assert((endpos <= buffer.Length << 3), bufferOverrunMsg);

			// Offset both the mask and the compressed value using the remainder as the offset
			ulong mask = ulong.MaxValue >> (64 - bits);

			ulong offsetmask = mask << -offset;
			ulong offsetcomp = (ulong)value << -offset;

			while (true)
			{
				// set the unwritten bits in byte[] to zero as a safety
				uPtr[index] &= (ulong)~offsetmask; // set bits to zero
				uPtr[index] |= (ulong)(offsetcomp & offsetmask);

				if (index == endindex)
					break;

				// Push the compressed value and the mask to align with the next array element
				offset += MAXBITS;
				offsetmask = mask >> offset;
				offsetcomp = (ulong)value >> offset;
				index++;
			}

			if (endpos > bitposition)
				bitposition = endpos;

			return;
		}


		public static void Write(this uint[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			const int MAXBITS = 32;
			const int MODULUS = MAXBITS - 1;
			int offset = -(bitposition & MODULUS); // this is just a modulus
			int index = bitposition >> 5;
			int endpos = bitposition + bits;
			int endindex = ((endpos - 1) >> 5);

			ulong mask = ulong.MaxValue >> (64 - bits);

			ulong offsetmask = mask << -offset;
			ulong offsetval = (ulong)value << -offset;

			//System.Diagnostics.Debug.Assert((endpos <= buffer.Length << 5), bufferOverrunMsg);

			while (true)
			{
				// set the unwritten bits in byte[] to zero as a safety
				buffer[index] &= (uint)(~offsetmask); // set bits to zero
				buffer[index] |= (uint)(offsetval & offsetmask);

				if (index == endindex)
					break;

				// Push the compressed value and the mask to align with the next array element
				offset += MAXBITS;
				offsetmask = mask >> offset;
				offsetval = (ulong)value >> offset;
				index++;
			}

			if (endpos > bitposition)
				bitposition = endpos;

			return;
		}

		public static void Write(this ulong[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			const int MAXBITS = 64;
			const int MODULUS = MAXBITS - 1;
			int offset = -(bitposition & MODULUS); // this is just a modulus
			int index = bitposition >> 6;
			int endpos = bitposition + bits;
			int endindex = ((endpos - 1) >> 6);

			ulong mask = ulong.MaxValue >> (64 - bits);

			ulong offsetmask = (mask << -offset);
			ulong offsetval = (value & mask) << -offset;

			//System.Diagnostics.Debug.Assert((endpos <= buffer.Length << 6), bufferOverrunMsg);

			while (true)
			{
				// set the unwritten bits in byte[] to zero as a safety
				buffer[index] &= ~offsetmask; // set bits to zero
				buffer[index] |= (offsetval & offsetmask);

				if (index == endindex)
					break;

				// Push the compressed value and the mask to align with this array element
				offset += MAXBITS;
				offsetmask = mask >> offset;
				offsetval = value >> offset;
				index++;
			}

			if (endpos > bitposition)
				bitposition = endpos;

		}

		#endregion

		#region Secondary Readers

		public static void WriteBool(this ulong[] buffer, bool b, ref int bitposition)
		{
			buffer.Write((ulong)(b ? 1 : 0), ref bitposition, 1);
		}
		public static void WriteBool(this uint[] buffer, bool b, ref int bitposition)
		{
			buffer.Write((ulong)(b ? 1 : 0), ref bitposition, 1);
		}
		public static void WriteBool(this byte[] buffer, bool b, ref int bitposition)
		{
			buffer.Write((ulong)(b ? 1 : 0), ref bitposition, 1);
		}

		#endregion

		#region Primary Readers

		/// <summary>
		/// This is the Primary byte[].Read() method. All other byte[].ReadXXX() methods lead here. For maximum performance use this for all Read() calls and cast accordingly.
		/// </summary>
		/// <param name="buffer">The array we are deserializing from.</param>
		/// <param name="bitposition">The position in the array (in bits) where we will begin reading.</param>
		/// <param name="bits">The number of bits to read.</param>
		/// <returns>UInt64 read value. Cast this to the intended type.</returns>
		public static ulong Read(this byte[] buffer, ref int bitposition, int bits)
		{
			const int MAXBITS = 8;
			const int MODULUS = MAXBITS - 1;
			int offset = -(bitposition & MODULUS); // this is just a modulus
			int index = bitposition >> 3;
			int endpos = bitposition + bits;
			int endindex = ((endpos - 1) >> 3);

			//System.Diagnostics.Debug.Assert(endpos <= (buffer.Length << 3), bufferOverrunMsg);

			ulong mask = ulong.MaxValue >> (64 - bits);
			ulong line = ((ulong)buffer[index] >> -offset);
			ulong value = 0;

			while (true)
			{
				value |= (line & mask);

				if (index == endindex)
					break;

				offset += MAXBITS;
				index++;
				line = ((ulong)buffer[index] << offset);
			}

			bitposition = bitposition + bits;
			return value;

		}

		/// <summary>
		/// This is the Primary uint[].Read() method. All other uint[].ReadXXX methods lead here. For maximum performance use this for all Read() calls and cast accordingly.
		/// </summary>
		/// <param name="buffer">The array we are deserializing from.</param>
		/// <param name="bitposition">The position in the array (in bits) where we will begin reading.</param>
		/// <param name="bits">The number of bits to read.</param>
		/// <returns>UInt64 read value. Cast this to the intended type.</returns>
		public static ulong Read(this uint[] buffer, ref int bitposition, int bits)
		{
			const int MAXBITS = 32;
			const int MODULUS = MAXBITS - 1;
			int offset = -(bitposition & MODULUS); // this is just a modulus
			int index = bitposition >> 5;
			int endpos = bitposition + bits;
			int endindex = ((endpos - 1) >> 5);

			//System.Diagnostics.Debug.Assert(endpos <= (buffer.Length << 3), bufferOverrunMsg);

			ulong mask = ulong.MaxValue >> (64 - bits);
			ulong line = ((ulong)buffer[index] >> -offset);
			ulong value = 0;

			while (true)
			{
				value |= (line & mask);

				if (index == endindex)
					break;

				offset += MAXBITS;
				index++;
				line = ((ulong)buffer[index] << offset);
			}

			bitposition = bitposition + bits;
			return value;
		}

		/// <summary>
		/// This is the Primary ulong[].Read() method. All other ulong[].ReadXXX methods lead here. For maximum performance use this for all Read() calls and cast accordingly.
		/// </summary>
		/// <param name="buffer">The array we are deserializing from.</param>
		/// <param name="bitposition">The position in the array (in bits) where we will begin reading.</param>
		/// <param name="bits">The number of bits to read.</param>
		/// <returns>UInt64 read value. Cast this to the intended type.</returns>
		public static ulong Read(this ulong[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
				return 0;

			const int MAXBITS = 64;
			const int MODULUS = MAXBITS - 1;
			int offset = -(bitposition & MODULUS); // this is just a modulus
			int index = bitposition >> 6;
			int endpos = bitposition + bits;
			int endindex = ((endpos - 1) >> 6);

			//System.Diagnostics.Debug.Assert(endpos <= (buffer.Length << 3), bufferOverrunMsg);

			ulong mask = ulong.MaxValue >> (64 - bits);
			ulong line = ((ulong)buffer[index] >> -offset);
			ulong value = 0;

			while (true)
			{
				value |= (line & mask);

				if (index == endindex)
					break;

				offset += MAXBITS;
				index++;
				line = ((ulong)buffer[index] << offset);
			}

			bitposition = bitposition + bits;
			return value;
		}

		/// <summary>
		/// Primary Unsafe Read. Fast read for byte[] and unit[] by treating them as ulong[].
		/// WARNING: There is no bounds checking on this method!
		/// </summary>
		/// <param name="uPtr">Cast your byte* or uint* to ulong*</param>
		/// <param name="bitposition"></param>
		/// <param name="bits"></param>
		/// <returns>Returns the read value.</returns>
		public unsafe static ulong ReadUnsafe(ulong* uPtr, ref int bitposition, int bits)
		{
			if (bits == 0)
				return 0;

			const int MAXBITS = 64;
			const int MODULUS = MAXBITS - 1;
			int offset = -(bitposition & MODULUS); // this is just a modulus
			int index = bitposition >> 6;
			int endpos = bitposition + bits;
			int endindex = ((endpos - 1) >> 6);

			//System.Diagnostics.Debug.Assert(endpos <= (buffer.Length << 3), bufferOverrunMsg);

			ulong mask = ulong.MaxValue >> (64 - bits);
			ulong line = ((ulong)uPtr[index] >> -offset);
			ulong value = 0;

			while (true)
			{
				value |= (line & mask);

				if (index == endindex)
					break;

				offset += MAXBITS;
				index++;
				line = ((ulong)uPtr[index] << offset);
			}

			bitposition = bitposition + bits;
			return value;
		}

		#endregion

		#region Secondary Readers

		public static ulong ReadUInt64(this byte[] buffer, ref int bitposition, int bits = 64)
		{
			return Read(buffer, ref bitposition, bits);
		}

		public static ulong ReadUInt64(this uint[] buffer, ref int bitposition, int bits = 64)
		{
			return Read(buffer, ref bitposition, bits);
		}

		public static ulong ReadUInt64(this ulong[] buffer, ref int bitposition, int bits = 64)
		{
			return Read(buffer, ref bitposition, bits);
		}

		public static uint ReadUInt32(this byte[] buffer, ref int bitposition, int bits = 32)
		{
			return (uint)Read(buffer, ref bitposition, bits);
		}

		public static uint ReadUInt32(this uint[] buffer, ref int bitposition, int bits = 32)
		{
			return (uint)Read(buffer, ref bitposition, bits);
		}

		public static uint ReadUInt32(this ulong[] buffer, ref int bitposition, int bits)
		{
			return (uint)Read(buffer, ref bitposition, bits);
		}

		public static bool ReadBool(this ulong[] buffer, ref int bitposition)
		{
			return Read(buffer, ref bitposition, 1) == 1 ? true : false;
		}
		public static bool ReadBool(this uint[] buffer, ref int bitposition)
		{
			return Read(buffer, ref bitposition, 1) == 1 ? true : false;
		}
		public static bool ReadBool(this byte[] buffer, ref int bitposition)
		{
			return Read(buffer, ref bitposition, 1) == 1 ? true : false;
		}

		#endregion

		#region ReadOut UInt64[] To Array


		/// <summary>
		/// Primary ReadOutUnsafe method. WARNING: No bounds checking. Use with caution. Cast array pointers to ulong*.
		/// </summary>
		/// <param name="sourcePtr"></param>
		/// <param name="sourcePos"></param>
		/// <param name="targetPtr"></param>
		/// <param name="targetPos"></param>
		/// <param name="bits"></param>
		public unsafe static void ReadOutUnsafe(ulong* sourcePtr, int sourcePos, ulong* targetPtr, ref int targetPos, int bits)
		{
			if (bits == 0)
				return;

			int readpos = sourcePos;
			int remaining = bits;

			while (remaining > 0)
			{
				int cnt = remaining > 64 ? 64 : remaining;
				ulong val = ReadUnsafe(sourcePtr, ref readpos, cnt);
				WriteUnsafe(targetPtr, val, ref targetPos, cnt);

				remaining -= cnt;
			}

			targetPos += bits;
		}

		/// <summary>
		/// Read the contents of one bitpacked array to another using Unsafe. This generally requires arrays to have a total byte count divisible by 8,
		/// as they will be treated as ulong[] in unsafe.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="sourcePos">Bitpos of the source array to start read from.</param>
		/// <param name="target"></param>
		/// <param name="targetPos">The target bitposition (that will be incremented with this write).</param>
		/// <param name="bits">Number of bits to copy. This should be the current bitpos of the source.</param>
		public unsafe static void ReadOutUnsafe(this ulong[] source, int sourcePos, byte[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
				return;
			int readpos = sourcePos;
			int remaining = bits;

			System.Diagnostics.Debug.Assert((targetPos + bits) <= (target.Length << 3), bufferOverrunMsg);
			System.Diagnostics.Debug.Assert((sourcePos + bits) <= (source.Length << 3), bufferOverrunMsg);

			fixed (ulong* sPtr = source)
			fixed (byte* _tPtr = target)
			{
				ulong* tPtr = (ulong*)_tPtr;

				while (remaining > 0)
				{
					int cnt = remaining > 64 ? 64 : remaining;
					ulong val = ReadUnsafe(sPtr, ref readpos, cnt);
					WriteUnsafe(tPtr, val, ref targetPos, cnt);

					remaining -= cnt;
				}
			}

			targetPos += bits;
		}
		/// <summary>
		/// Read the contents of one bitpacked array to another using Unsafe. This generally requires arrays to have a total byte count divisible by 8,
		/// as they will be treated as ulong[] in unsafe.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="sourcePos">Bitpos of the source array to start read from.</param>
		/// <param name="target"></param>
		/// <param name="targetPos">The target bitposition (that will be incremented with this write).</param>
		/// <param name="bits">Number of bits to copy. This should be the current bitpos of the source.</param>
		public unsafe static void ReadOutUnsafe(this ulong[] source, int sourcePos, uint[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
				return;
			int readpos = sourcePos;
			int remaining = bits;

			System.Diagnostics.Debug.Assert((targetPos + bits) <= (target.Length << 3), bufferOverrunMsg);
			System.Diagnostics.Debug.Assert((sourcePos + bits) <= (source.Length << 3), bufferOverrunMsg);

			fixed (ulong* sPtr = source)
			fixed (uint* _tPtr = target)
			{
				ulong* tPtr = (ulong*)_tPtr;

				while (remaining > 0)
				{
					int cnt = remaining > 64 ? 64 : remaining;
					ulong val = ReadUnsafe(sPtr, ref readpos, cnt);
					WriteUnsafe(tPtr, val, ref targetPos, cnt);

					remaining -= cnt;
				}
			}

			targetPos += bits;
		}
		/// <summary>
		/// Read the contents of one bitpacked array to another using Unsafe. This generally requires arrays to have a total byte count divisible by 8,
		/// as they will be treated as ulong[] in unsafe.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="sourcePos">Bitpos of the source array to start read from.</param>
		/// <param name="target"></param>
		/// <param name="targetPos">The target bitposition (that will be incremented with this write).</param>
		/// <param name="bits">Number of bits to copy. This should be the current bitpos of the source.</param>
		public unsafe static void ReadOutUnsafe(this ulong[] source, int sourcePos, ulong[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
				return;
			int readpos = sourcePos;
			int remaining = bits;

			System.Diagnostics.Debug.Assert((targetPos + bits) <= (target.Length << 3), bufferOverrunMsg);
			System.Diagnostics.Debug.Assert((sourcePos + bits) <= (source.Length << 3), bufferOverrunMsg);

			fixed (ulong* sPtr = source)
			fixed (ulong* tPtr = target)
			{
				while (remaining > 0)
				{
					int cnt = remaining > 64 ? 64 : remaining;
					ulong val = ReadUnsafe(sPtr, ref readpos, cnt);
					WriteUnsafe(tPtr, val, ref targetPos, cnt);

					remaining -= cnt;
				}
			}

			targetPos += bits;
		}

		#endregion

		#region ReadOut UInt32[] To Array

		/// <summary>
		/// Read the contents of one bitpacked array to another using Unsafe. This generally requires arrays to have a total byte count divisible by 8,
		/// as they will be treated as ulong[] in unsafe.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="sourcePos">Bitpos of the source array to start read from.</param>
		/// <param name="target"></param>
		/// <param name="targetPos">The target bitposition (that will be incremented with this write).</param>
		/// <param name="bits">Number of bits to copy. This should be the current bitpos of the source.</param>
		public unsafe static void ReadOutUnsafe(this uint[] source, int sourcePos, byte[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
				return;
			int readpos = sourcePos;
			int remaining = bits;

			System.Diagnostics.Debug.Assert((targetPos + bits) <= (target.Length << 3), bufferOverrunMsg);
			System.Diagnostics.Debug.Assert((sourcePos + bits) <= (source.Length << 3), bufferOverrunMsg);

			fixed (uint* _sPtr = source)
			fixed (byte* _tPtr = target)
			{
				ulong* sPtr = (ulong*)_sPtr;
				ulong* tPtr = (ulong*)_tPtr;

				while (remaining > 0)
				{
					int cnt = remaining > 64 ? 64 : remaining;
					ulong val = ReadUnsafe(sPtr, ref readpos, cnt);
					WriteUnsafe(tPtr, val, ref targetPos, cnt);

					remaining -= cnt;
				}
			}

			targetPos += bits;
		}
		/// <summary>
		/// Read the contents of one bitpacked array to another using Unsafe. This generally requires arrays to have a total byte count divisible by 8,
		/// as they will be treated as ulong[] in unsafe.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="sourcePos">Bitpos of the source array to start read from.</param>
		/// <param name="target"></param>
		/// <param name="targetPos">The target bitposition (that will be incremented with this write).</param>
		/// <param name="bits">Number of bits to copy. This should be the current bitpos of the source.</param>
		public unsafe static void ReadOutUnsafe(this uint[] source, int sourcePos, uint[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
				return;

			int readpos = sourcePos;
			int remaining = bits;

			System.Diagnostics.Debug.Assert((targetPos + bits) <= (target.Length << 3), bufferOverrunMsg);
			System.Diagnostics.Debug.Assert((sourcePos + bits) <= (source.Length << 3), bufferOverrunMsg);

			fixed (uint* _sPtr = source)
			fixed (uint* _tPtr = target)
			{
				ulong* sPtr = (ulong*)_sPtr;
				ulong* tPtr = (ulong*)_tPtr;

				while (remaining > 0)
				{
					int cnt = remaining > 64 ? 64 : remaining;
					ulong val = ReadUnsafe(sPtr, ref readpos, cnt);
					WriteUnsafe(tPtr, val, ref targetPos, cnt);

					remaining -= cnt;
				}
			}

			targetPos += bits;
		}
		/// <summary>
		/// Read the contents of one bitpacked array to another using Unsafe. This generally requires arrays to have a total byte count divisible by 8,
		/// as they will be treated as ulong[] in unsafe.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="sourcePos">Bitpos of the source array to start read from.</param>
		/// <param name="target"></param>
		/// <param name="targetPos">The target bitposition (that will be incremented with this write).</param>
		/// <param name="bits">Number of bits to copy. This should be the current bitpos of the source.</param>
		public unsafe static void ReadOutUnsafe(this uint[] source, int sourcePos, ulong[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
				return;
			int readpos = sourcePos;
			int remaining = bits;

			System.Diagnostics.Debug.Assert((targetPos + bits) <= (target.Length << 3), bufferOverrunMsg);
			System.Diagnostics.Debug.Assert((sourcePos + bits) <= (source.Length << 3), bufferOverrunMsg);

			fixed (uint* _sPtr = source)
			fixed (ulong* tPtr = target)
			{
				ulong* sPtr = (ulong*)_sPtr;

				while (remaining > 0)
				{
					int cnt = remaining > 64 ? 64 : remaining;
					ulong val = ReadUnsafe(sPtr, ref readpos, cnt);
					WriteUnsafe(tPtr, val, ref targetPos, cnt);

					remaining -= cnt;
				}
			}

			targetPos += bits;
		}

		#endregion

		#region ReadOut UInt8[] to Array

		/// <summary>
		/// Read the contents of one bitpacked array to another using Unsafe. This generally requires arrays to have a total byte count divisible by 8,
		/// as they will be treated as ulong[] in unsafe.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="sourcePos">Bitpos of the source array to start read from.</param>
		/// <param name="target"></param>
		/// <param name="targetPos">The target bitposition (that will be incremented with this write).</param>
		/// <param name="bits">Number of bits to copy. This should be the current bitpos of the source.</param>
		public unsafe static void ReadOutUnsafe(this byte[] source, int sourcePos, ulong[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
				return;

			int readpos = sourcePos;
			int remaining = bits;

			System.Diagnostics.Debug.Assert((targetPos + bits) <= (target.Length << 3), bufferOverrunMsg);
			System.Diagnostics.Debug.Assert((sourcePos + bits) <= (source.Length << 3), bufferOverrunMsg);

			fixed (byte* _sPtr = source)
			fixed (ulong* tPtr = target)
			{
				ulong* sPtr = (ulong*)_sPtr;

				while (remaining > 0)
				{
					int cnt = remaining > 64 ? 64 : remaining;
					ulong val = ReadUnsafe(sPtr, ref readpos, cnt);
					WriteUnsafe(tPtr, val, ref targetPos, cnt);

					remaining -= cnt;
				}
			}
			targetPos += bits;
		}

		/// <summary>
		/// Read the contents of one bitpacked array to another using Unsafe. This generally requires arrays to have a total byte count divisible by 8,
		/// as they will be treated as ulong[] in unsafe.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="sourcePos">Bitpos of the source array to start read from.</param>
		/// <param name="target"></param>
		/// <param name="targetPos">The target bitposition (that will be incremented with this write).</param>
		/// <param name="bits">Number of bits to copy. This should be the current bitpos of the source.</param>
		public unsafe static void ReadOutUnsafe(this byte[] source, int sourcePos, uint[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
				return;

			int readpos = sourcePos;
			int remaining = bits;

			System.Diagnostics.Debug.Assert((targetPos + bits) <= (target.Length << 3), bufferOverrunMsg);
			System.Diagnostics.Debug.Assert((sourcePos + bits) <= (source.Length << 3), bufferOverrunMsg);

			fixed (byte* _sPtr = source)
			fixed (uint* _tPtr = target)
			{
				ulong* sPtr = (ulong*)_sPtr;
				ulong* tPtr = (ulong*)_tPtr;

				while (remaining > 0)
				{
					int cnt = remaining > 64 ? 64 : remaining;
					ulong val = ReadUnsafe(sPtr, ref readpos, cnt);
					WriteUnsafe(tPtr, val, ref targetPos, cnt);

					remaining -= cnt;
				}
			}
			targetPos += bits;
		}
		/// <summary>
		/// Read the contents of one bitpacked array to another using Unsafe. This generally requires arrays to have a total byte count divisible by 8,
		/// as they will be treated as ulong[] in unsafe.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="sourcePos">Bitpos of the source array to start read from.</param>
		/// <param name="target"></param>
		/// <param name="targetPos">The target bitposition (that will be incremented with this write).</param>
		/// <param name="bits">Number of bits to copy. This should be the current bitpos of the source.</param>
		public unsafe static void ReadOutUnsafe(this byte[] source, int sourcePos, byte[] target, ref int targetPos, int bits)
		{
			if (bits == 0)
				return;

			int readpos = sourcePos;
			int remaining = bits;

			System.Diagnostics.Debug.Assert((targetPos + bits) <= (target.Length << 3), bufferOverrunMsg);
			System.Diagnostics.Debug.Assert((sourcePos + bits) <= (source.Length << 3), bufferOverrunMsg);

			fixed (byte* _sPtr = source)
			fixed (byte* _tPtr = target)
			{
				ulong* sPtr = (ulong*)_sPtr;
				ulong* tPtr = (ulong*)_tPtr;

				while (remaining > 0)
				{
					int cnt = remaining > 64 ? 64 : remaining;
					ulong val = ReadUnsafe(sPtr, ref readpos, cnt);
					WriteUnsafe(tPtr, val, ref targetPos, cnt);

					remaining -= cnt;
				}
			}
			targetPos += bits;
		}

		#endregion

		/// <summary>
		/// Treats buffer as ulong[] and returns the index value of that virtual ulong[]
		/// </summary>
		/// <param name="index">The index of the virtual ulong[]</param>
		public static ulong IndexAsUInt64(this byte[] buffer, int index)
		{
			int i = index << 3;
			return (ulong)(
				(ulong)buffer[i] |
				(ulong)buffer[i + 1] << 8 |
				(ulong)buffer[i + 2] << 16 |
				(ulong)buffer[i + 3] << 24 |
				(ulong)buffer[i + 4] << 32 |
				(ulong)buffer[i + 5] << 40 |
				(ulong)buffer[i + 6] << 48 |
				(ulong)buffer[i + 7] << 56);
		}
		/// <summary>
		/// Treats buffer as ulong[] and returns the index value of that virtual ulong[]
		/// </summary>
		/// <param name="index">The index of the virtual ulong[]</param>
		public static ulong IndexAsUInt64(this uint[] buffer, int index)
		{
			int i = index << 1;
			return (ulong)(
				(ulong)buffer[i] |
				(ulong)buffer[i + 1] << 32);
		}

		/// <summary>
		/// Treats buffer as uint[] and returns the index value of that virtual uint[]
		/// </summary>
		/// <param name="index">The index of the virtual uint[]</param>
		public static uint IndexAsUInt32(this byte[] buffer, int index)
		{
			int i = index << 3;
			return (uint)(
				(uint)buffer[i] |
				(uint)buffer[i + 1] << 8 |
				(uint)buffer[i + 2] << 16 |
				(uint)buffer[i + 3] << 24);
		}
		/// <summary>
		/// Treats buffer as uint[] and returns the index value of that virtual uint[]
		/// </summary>
		/// <param name="index">The index of the virtual uint[]</param>
		public static uint IndexAsUInt32(this ulong[] buffer, int index)
		{
			const int MODULUS = 1;
			int i = index >> 1;
			int offset = (index & MODULUS) << 5; // modulus * 8
			ulong element = buffer[i];
			return (byte)((element >> offset));
		}
		/// <summary>
		/// Treats buffer as byte[] and returns the index value of that virtual byte[]
		/// </summary>
		/// <param name="index">The index of the virtual byte[]</param>
		public static byte IndexAsUInt8(this ulong[] buffer, int index)
		{
			const int MODULUS = 7;
			int i = index >> 3;
			int offset = (index & MODULUS) << 3; // modulus * 8
			ulong element = buffer[i];
			return (byte)((element >> offset));
		}
		/// <summary>
		/// Treats buffer as byte[] and returns the index value of that virtual byte[]
		/// </summary>
		/// <param name="index">The index of the virtual byte[]</param>
		public static byte IndexAsUInt8(this uint[] buffer, int index)
		{
			const int MODULUS = 3;
			int i = index >> 3;
			int offset = (index & MODULUS) << 3; // modulus * 8
			ulong element = buffer[i];
			return (byte)((element >> offset));
		}

		#region Obsolete Writers

		[System.Obsolete("Argument order has changed.")]
		public static byte[] Write(this byte[] buffer, ulong value, int bits, ref int bitposition)
		{
			Write(buffer, value, ref bitposition, bits);
			return buffer;
		}
		[System.Obsolete("Argument order has changed.")]
		public static uint[] Write(this uint[] buffer, ulong value, int bits, ref int bitposition)
		{
			Write(buffer, value, ref bitposition, bits);
			return buffer;
		}
		[System.Obsolete("Argument order has changed.")]
		public static ulong[] Write(this ulong[] buffer, ulong value, int bits, ref int bitposition)
		{
			Write(buffer, value, ref bitposition, bits);
			return buffer;
		}
		[System.Obsolete("Argument order has changed.")]
		public static byte[] Write(this byte[] buffer, float value, ref int bitposition)
		{
			Write(buffer, ((ByteConverter)value).uint32, ref bitposition, 32);
			return buffer;
		}
		[System.Obsolete("Argument order has changed.")]
		public static float Read(this byte[] buffer, ref int bitposition)
		{
			return Read(buffer, ref bitposition, 32);
		}

		#endregion

		#region Obsolete Readers

		[System.Obsolete("Argument order has changed.")]
		public static ulong Read(this byte[] buffer, int bits, ref int bitposition)
		{
			return Read(buffer, ref bitposition, bits);
		}
		[System.Obsolete("Argument order has changed.")]
		public static ulong Read(this uint[] buffer, int bits, ref int bitposition)
		{
			return Read(buffer, ref bitposition, bits);
		}
		[System.Obsolete("Argument order has changed.")]
		public static ulong Read(this ulong[] buffer, int bits, ref int bitposition)
		{
			return Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Argument order has changed.")]
		public static byte ReadUInt8(this ulong[] buffer, int bits, ref int bitposition)
		{
			return (byte)Read(buffer, ref bitposition, bits);
		}
		[System.Obsolete("Argument order has changed.")]
		public static uint ReadUInt32(this ulong[] buffer, int bits, ref int bitposition)
		{
			return (uint)Read(buffer, ref bitposition, bits);
		}
		[System.Obsolete("Argument order has changed.")]
		public static ulong ReadUInt64(this ulong[] buffer, int bits, ref int bitposition)
		{
			return Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Argument order has changed.")]
		public static byte ReadUInt8(this uint[] buffer, int bits, ref int bitposition)
		{
			return (byte)Read(buffer, ref bitposition, bits);
		}
		[System.Obsolete("Argument order has changed.")]
		public static uint ReadUInt32(this uint[] buffer, int bits, ref int bitposition)
		{
			return (uint)Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Argument order has changed.")]
		public static ulong ReadUInt64(this uint[] buffer, int bits, ref int bitposition)
		{
			return Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Argument order has changed.")]
		public static byte ReadUInt8(this byte[] buffer, int bits, ref int bitposition)
		{
			return (byte)Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Argument order has changed.")]
		public static uint ReadUInt32(this byte[] buffer, int bits, ref int bitposition)
		{
			return (byte)Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Argument order has changed.")]
		public static ulong ReadUInt64(this byte[] buffer, int bits, ref int bitposition)
		{
			return Read(buffer, ref bitposition, bits);
		}



		[System.Obsolete("Instead use ReadOutUnsafe. They are much faster.")]
		public static byte[] Write(this byte[] buffer, byte[] srcbuffer, ref int readpos, ref int writepos, int bits)
		{
			while (bits > 0)
			{
				int fragbits = (bits > 64) ? 64 : bits;
				ulong frag = srcbuffer.Read(ref readpos, fragbits);
				buffer.Write(frag, ref writepos, fragbits);
				bits -= fragbits;
			}

			return buffer;
		}

		[System.Obsolete("Do not use, this is for benchmarking comparisons only.")]
		public static void ReadArrayOutSafe(this ulong[] source, int srcStartPos, byte[] target, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;
			int readpos = srcStartPos;
			int remaining = bits;

			// TODO: Add len checks

			while (remaining > 0)
			{
				int cnt = remaining > 64 ? 64 : remaining;
				ulong val = source.Read(ref readpos, cnt);
				target.Write(val, ref bitposition, cnt);

				remaining -= cnt;
			}
			bitposition += bits;
		}

		#endregion

	}
}

