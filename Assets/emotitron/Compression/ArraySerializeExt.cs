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

using System.Runtime.InteropServices;
using System;


namespace emotitron.Compression
{

	/// <summary>
	/// A Utility class that gives all byte[], uint[] and ulong[] buffers bitpacking/serialization methods.
	/// </summary>
	public static class ArraySerializeExt
	{
		private const string bufferOverrunMsg = "Byte buffer overrun. Dataloss will occur.";

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

		#region Read/Write Signed Value

		public static byte[] WriteSigned(this byte[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.Write(zigzag, ref bitposition, bits);
			return buffer;
		}
		public static uint[] WriteSigned(this uint[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.Write(zigzag, ref bitposition, bits);
			return buffer;
		}
		public static ulong[] WriteSigned(this ulong[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.Write(zigzag, ref bitposition, bits);
			return buffer;
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

		#endregion

		#region Float Reader/Writer

		/// <summary>
		/// Converts a float to a 32bit uint with ByteConverter, then writes those 32 bits to the buffer.
		/// </summary>
		/// <param name="buffer">The array we are reading from.</param>
		/// <param name="value">The float value to write.</param>
		/// <param name="bitposition">The bit position in the array we start the read at. Will be incremented by 32 bits.</param>
		public static byte[] WriteFloat(this byte[] buffer, float value, ref int bitposition)
		{
			Write(buffer, ((ByteConverter)value).uint32, ref bitposition, 32);
			return buffer;
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

			System.Diagnostics.Debug.Assert((endindex < buffer.Length), bufferOverrunMsg);

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

		/// <summary>
		/// This is an untested version of primary byte[].Write() method. Includes auto array resizing.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="value"></param>
		/// <param name="bitposition"></param>
		/// <param name="bits"></param>
		/// <param name="allowResize">Allows the buffer to be doubled in size if it is too small for this write. YOU MUST GET THE RETURN BYTE[] REFERENCE. The supplied buffer becomes invalid.</param>
		/// <returns>The actual byte[] used. Will be a new array if a resize occured.</returns>
		[System.Obsolete("Technically not obsolete... experimental code that may or may not become legit.")]
		public static void Write(this byte[] buffer, ulong value, ref int bitposition, int bits, bool allowResize)
		{
			if (bits == 0)
				return;

			const int MAXBITS = 8;
			const int MODULUS = MAXBITS - 1;
			int offset = -(bitposition & MODULUS); // this is just a modulus
			int index = bitposition >> 3;
			int endpos = bitposition + bits;
			int endindex = ((endpos - 1) >> 3);

			//System.Diagnostics.Debug.Assert((endindex < buffer.Length), bufferOverrunMsg);
			
			if (allowResize && endindex >= buffer.Length)
				System.Array.Resize(ref buffer, buffer.Length * 2);

			// Offset both the mask and the compressed value using the remainder as the offset
			ulong mask = ulong.MaxValue >> (64 - bits);

			ulong offsetmask = mask << -offset;
			ulong offsetval = (ulong)value << -offset;

			while (true)
			{
				// set the unwritten bits in byte[] to zero as a safety
				buffer[index] &= (byte)~offsetmask; // set bits to zero
				buffer[index] |= (byte)(offsetval & offsetmask);

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

		public static void Write(this uint[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			const int MAXBITS = 32;
			const int MODULUS = MAXBITS - 1;
			int offset = -(bitposition & MODULUS); // this is just a modulus
			int index = bitposition  >> 5;
			int endpos = bitposition + bits;
			int endindex = ((endpos - 1) >> 5);

			ulong mask = ulong.MaxValue >> (64 - bits);

			ulong offsetmask = mask << -offset;
			ulong offsetval = (ulong)value << -offset;

			//System.Diagnostics.Debug.Assert(endpos <= (buffer.Length << 3), bufferOverrunMsg);

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

			while (true)
			{
				// set the unwritten bits in byte[] to zero as a safety
				buffer[index] &= ~offsetmask ; // set bits to zero
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

		#endregion



		#region UINT32 Array Readers

		/// <summary>
		/// Read a bitcrushed uint out of an array starting at the indicated bit postion.
		/// </summary>
		/// <param name="buffer">The array we are deserializing from.</param>
		/// <param name="bitposition">The position in the array (in bits) where we will begin reading.</param>
		/// <param name="bits">The number of bits to read.</param>
		public static uint ReadUInt32(this byte[] buffer, ref int bitposition, int bits = 32)
		{
			return (uint)Read(buffer, ref bitposition, bits);
		}

		/// <summary>
		/// Read a bitcrushed uint out of an array starting at the indicated bit postion.
		/// </summary>
		/// <param name="buffer">The array we are deserializing from.</param>
		/// <param name="bitposition">The position in the array (in bits) where we will begin reading.</param>
		/// <param name="bits">The number of bits to read.</param>
		public static uint ReadUInt32(this uint[] buffer, ref int bitposition, int bits = 32)
		{
			return (uint)Read(buffer, ref bitposition, bits);
		}

		/// <summary>
		/// Read a bitcrushed uint out of an array starting at the indicated bit postion.
		/// </summary>
		/// <param name="buffer">The array we are deserializing from.</param>
		/// <param name="bitposition">The position in the array (in bits) where we will begin reading.</param>
		/// <param name="bits">The number of bits to read.</param>
		public static uint ReadUInt32(this ulong[] buffer, ref int bitposition, int bits)
		{
			return (uint)Read(buffer, ref bitposition, bits);
		}



		#endregion

		#region UINT64 Array Readers

		/// <summary>
		/// Read a bitcrushed uint out of a array starting at the indicated bit postion.
		/// </summary>
		/// <param name="buffer">The array we are deserializing from.</param>
		/// <param name="bitposition">The position in the array (in bits) where we will begin reading.</param>
		/// <param name="bits">The number of bits to read.</param>
		[System.Obsolete("When reading out UInt64 values, just use the base Read() method instead to eliminate this extra method call.")]
		public static ulong ReadUInt64(this byte[] buffer, ref int bitposition, int bits = 64)
		{
			return Read(buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Read a bitcrushed uint out of a array starting at the indicated bit postion.
		/// </summary>
		/// <param name="buffer">The array we are deserializing from.</param>
		/// <param name="bitposition">The position in the array (in bits) where we will begin reading.</param>
		/// <param name="bits">The number of bits to read.</param>
		[System.Obsolete("When reading out UInt64 values, just use the base Read() method instead to eliminate this extra method call.")]
		public static ulong ReadUInt64(this uint[] buffer, ref int bitposition, int bits = 64)
		{
			return Read(buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Read a bitcrushed uint out of a array starting at the indicated bit postion.
		/// </summary>
		/// <param name="buffer">The array we are deserializing from.</param>
		/// <param name="bitposition">The position in the array (in bits) where we will begin reading.</param>
		/// <param name="bits">The number of bits to read.</param>
		[System.Obsolete("When reading out UInt64 values, just use the base Read() method instead to eliminate this extra method call.")]
		public static ulong ReadUInt64(this ulong[] buffer, ref int bitposition, int bits = 64)
		{
			return Read(buffer, ref bitposition, bits);
		}
		#endregion

		/// <summary>
		/// Copy bits from one array to another.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="srcbuffer"></param>
		/// <param name="readpos"></param>
		/// <param name="writepos"></param>
		/// <param name="bits"></param>
		/// <returns>Returns the target buffer.</returns>
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
	}
}

