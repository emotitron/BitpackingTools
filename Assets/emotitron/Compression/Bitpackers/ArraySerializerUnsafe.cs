/*
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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace emotitron.Compression
{

	public static class ArraySerializerUnsafe
	{

		private const string bufferOverrunMsg = "Byte buffer overrun. Dataloss will occur.";

		#region Read/Write Signed Value

		public unsafe static void AppendSigned(ulong* buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			AppendUnsafe(buffer, zigzag, ref bitposition, bits);
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

		public unsafe static int ReadSigned(ulong* uPtr, ref int bitposition, int bits)
		{
			uint value = (uint)ReadUnsafe(uPtr, ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}

		#endregion

		/// <summary>
		/// Primary Append writer. Faster method for writing to byte[] or uint[] buffers. Uses unsafe to treat them as ulong[].
		/// Append does not preserve existing buffer data past the write point in exchange for a faster write.
		/// WARNING: There is no bounds checking on this. If you write too far, you will crash.
		/// </summary>
		/// <param name="uPtr">Cast your byte* or uint* to ulong*</param>
		/// <param name="value"></param>
		/// <param name="bitposition"></param>
		/// <param name="bits"></param>
		public unsafe static void AppendUnsafe(ulong* uPtr, ulong value, ref int bitposition, int bits)
		{
			const int MAXBITS = 64;
			const int MODULUS = MAXBITS - 1;
			int offset = -(bitposition & MODULUS); // this is just a modulus
			int index = bitposition >> 6;
			int endpos = bitposition + bits;
			int endindex = ((endpos - 1) >> 6);

			// Offset both the mask and the compressed value using the remainder as the offset
			ulong offsetmask = ((1UL << offset) - 1);

			//System.Diagnostics.Debug.Assert((endpos <= buffer.Length << 5), bufferOverrunMsg);

			ulong comp = uPtr[index] & offsetmask;
			ulong result = comp | (value << offset);
			uPtr[index] = (uint)result;
			uPtr[index + 1] = (uint)(result >> MAXBITS);

			bitposition += bits;
		}

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

			bitposition = endpos;
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
	}
}
