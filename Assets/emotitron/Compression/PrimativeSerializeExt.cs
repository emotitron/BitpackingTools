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

using System;

namespace emotitron.Compression
{
	/// <summary>
	/// Extension methods for writing bits to primitive buffers.
	/// </summary>
	public static class PrimitiveSerializeExt
	{
		const string overrunerror = "Write buffer overrun. writepos + bitcount exceeds target length. Data loss will occur.";

		#region Obsolete Inject

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		[System.Obsolete("Argument order changed")]
		public static void Inject(this ulong src, ref uint buffer, int bitcount, ref int bitposition)
		{
			Inject(src, ref buffer, ref bitposition, bitcount);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		[System.Obsolete("Argument order changed")]
		public static void Inject(this ulong src, ref ulong buffer, int bitcount, ref int bitposition)
		{
			Inject(src, ref buffer, ref bitposition, bitcount);
		}
		#endregion

		#region	Inject ByteConverted

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ByteConverter src, ref ulong buffer, ref int bitposition, int bitcount)
		{
			((ulong)src).Inject(ref buffer, ref bitposition, bitcount);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ByteConverter src, ref uint buffer, ref int bitposition, int bitcount)
		{
			((ulong)src).Inject(ref buffer, ref bitposition, bitcount);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ByteConverter src, ref ushort buffer, ref int bitposition, int bitcount)
		{
			((ulong)src).Inject(ref buffer, ref bitposition, bitcount);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ByteConverter src, ref byte buffer, ref int bitposition, int bitcount)
		{
			((ulong)src).Inject(ref buffer, ref bitposition, bitcount);
		}

		#endregion

		#region Inject UInt64 Buffer

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong src, ref ulong buffer, ref int bitposition, int bitcount = 64)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 64, overrunerror);

			ulong offsetsrc = src << bitposition;
			ulong mask = ulong.MaxValue >> (64 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong src, ref ulong buffer, int bitposition, int bitcount = 64)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 64, overrunerror);

			ulong offsetsrc = src << bitposition;
			ulong mask = ulong.MaxValue >> (64 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);
		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint src, ref ulong buffer, ref int bitposition, int bitcount = 32)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 64, overrunerror);

			ulong offsetsrc = ((ulong)src << bitposition);
			ulong mask = ulong.MaxValue >> (64 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint src, ref ulong buffer, int bitposition, int bitcount = 32)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 64, overrunerror);

			ulong offsetsrc = ((ulong)src << bitposition);
			ulong mask = ulong.MaxValue >> (64 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort src, ref ulong buffer, ref int bitposition, int bitcount = 16)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 64, overrunerror);

			ulong offsetsrc = ((ulong)src << bitposition);
			ulong mask = ulong.MaxValue >> (64 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort src, ref ulong buffer, int bitposition, int bitcount = 16)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 64, overrunerror);

			ulong offsetsrc = ((ulong)src << bitposition);
			ulong mask = ulong.MaxValue >> (64 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte src, ref ulong buffer, ref int bitposition, int bitcount = 8)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 64, overrunerror);

			ulong offsetsrc = ((ulong)src << bitposition);
			ulong mask = ulong.MaxValue >> (64 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte src, ref ulong buffer, int bitposition, int bitcount = 8)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 64, overrunerror);

			ulong offsetsrc = ((ulong)src << bitposition);
			ulong mask = ulong.MaxValue >> (64 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}

		#endregion

		#region Inject UInt32 Buffer

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong src, ref uint buffer, ref int bitposition, int bitcount = 64)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 32, overrunerror);

			uint offsetsrc = (uint)src << bitposition;
			uint mask = uint.MaxValue >> (32 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong src, ref uint buffer, int bitposition, int bitcount = 64)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 32, overrunerror);

			uint offsetsrc = (uint)src << bitposition;
			uint mask = uint.MaxValue >> (32 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint src, ref uint buffer, ref int bitposition, int bitcount = 32)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 32, overrunerror);

			uint offsetsrc = (uint)src << bitposition;
			uint mask = uint.MaxValue >> (32 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint src, ref uint buffer, int bitposition, int bitcount = 32)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 32, overrunerror);

			uint offsetsrc = (uint)src << bitposition;
			uint mask = uint.MaxValue >> (32 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort src, ref uint buffer, ref int bitposition, int bitcount = 16)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 32, overrunerror);

			uint offsetsrc = (uint)src << bitposition;
			uint mask = uint.MaxValue >> (32 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort src, ref uint buffer, int bitposition, int bitcount = 16)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 32, overrunerror);

			uint offsetsrc = (uint)src << bitposition;
			uint mask = uint.MaxValue >> (32 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte src, ref uint buffer, ref int bitposition, int bitcount = 8)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 32, overrunerror);

			uint offsetsrc = (uint)src << bitposition;
			uint mask = uint.MaxValue >> (32 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte src, ref uint buffer, int bitposition, int bitcount = 8)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 32, overrunerror);

			uint offsetsrc = (uint)src << bitposition;
			uint mask = uint.MaxValue >> (32 - bitcount) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetsrc);

			bitposition += bitcount;
		}
		#endregion

		#region Inject UInt16 Buffer

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong src, ref ushort buffer, ref int bitposition, int bitcount = 16)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 16, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)ushort.MaxValue >> (16 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (ushort)_target;

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong src, ref ushort buffer, int bitposition, int bitcount = 16)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 16, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)ushort.MaxValue >> (16 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (ushort)_target;

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint src, ref ushort buffer, ref int bitposition, int bitcount = 16)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 16, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)ushort.MaxValue >> (16 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (ushort)_target;

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint src, ref ushort buffer, int bitposition, int bitcount = 16)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 16, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)ushort.MaxValue >> (16 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (ushort)_target;

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort src, ref ushort buffer, ref int bitposition, int bitcount = 16)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 16, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)ushort.MaxValue >> (16 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (ushort)_target;

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort src, ref ushort buffer, int bitposition, int bitcount = 16)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 16, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)ushort.MaxValue >> (16 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (ushort)_target;

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte src, ref ushort buffer, ref int bitposition, int bitcount = 8)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 16, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)ushort.MaxValue >> (16 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (ushort)_target;

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte src, ref ushort buffer, int bitposition, int bitcount = 8)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 16, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)ushort.MaxValue >> (16 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (ushort)_target;

			bitposition += bitcount;
		}

		#endregion

		#region Inject UInt8 Buffer

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong src, ref byte buffer, ref int bitposition, int bitcount = 8)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 8, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)byte.MaxValue >> (8 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (byte)_target;

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong src, ref byte buffer, int bitposition, int bitcount = 8)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 8, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)byte.MaxValue >> (8 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (byte)_target;

			bitposition += bitcount;
		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint src, ref byte buffer, ref int bitposition, int bitcount = 8)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 8, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)byte.MaxValue >> (8 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (byte)_target;

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint src, ref byte buffer, int bitposition, int bitcount = 8)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 8, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)byte.MaxValue >> (8 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (byte)_target;

			bitposition += bitcount;
		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort src, ref byte buffer, ref int bitposition, int bitcount = 8)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 8, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)byte.MaxValue >> (8 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (byte)_target;

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort src, ref byte buffer, int bitposition, int bitcount = 8)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 8, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)byte.MaxValue >> (8 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (byte)_target;

			bitposition += bitcount;
		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bitcount added to it.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte src, ref byte buffer, ref int bitposition, int bitcount = 8)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 8, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)byte.MaxValue >> (8 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (byte)_target;

			bitposition += bitcount;
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="src">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte src, ref byte buffer, int bitposition, int bitcount = 8)
		{
			System.Diagnostics.Debug.Assert(bitposition + bitcount <= 8, overrunerror);

			uint offsetsrc = ((uint)src << bitposition);
			uint mask = ((uint)byte.MaxValue >> (8 - bitcount) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetsrc);
			buffer = (byte)_target;

			bitposition += bitcount;
		}
		#endregion

		#region Obsolete Extract

		/// <summary>
		/// Extract (read/deserialize) a value from a source primitive (the buffer) by reading x bits starting at the bitposition, and return the reconstructed value. 
		/// </summary>
		/// <param name="src">Source primitive buffer to read from.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to return value.</param>
		/// <param name="bitposition">Auto-incremented reference to the src read bit pointer. Extraction starts at this point in src.</param>
		/// <returns>Downcast this ulong return value to the desired type.</returns>
		[System.Obsolete("Argument order changed")]
		public static ulong Extract(this ulong src, int bitcount, ref int bitposition)
		{
			return Extract(src, bitcount, ref bitposition);
		}

		#endregion


		#region Extract - Uint64 Buffer


		/// <summary>
		/// Extract (read/deserialize) a value from a source primitive (the buffer) by reading x bits starting at the bitposition, and return the reconstructed value. 
		/// </summary>
		/// <param name="src">Source primitive buffer to read from.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to return value.</param>
		/// <param name="bitposition">Auto-incremented reference to the src read bit pointer. Extraction starts at this point in src.</param>
		/// <returns>Downcast this ulong return value to the desired type.</returns>
		public static ulong Extract(this ulong src, ref int bitposition, int bitcount)
		{
			ulong mask = (ulong.MaxValue >> (64 - bitcount));
			ulong fragment = (((ulong)src >> bitposition) & mask);

			bitposition += bitcount;
			return fragment;
		}

		/// <summary>
		/// Extract and return bits from src. 
		/// </summary>
		/// <param name="src">Source primitive.</param>
		/// <param name="bitcount">How many lower order bits to read.</param>
		/// <returns>Cast the return value to the desired type.</returns>
		[System.Obsolete("Always include the [ref int bitposition] argument. Extracting from position 0 would be better handled with a mask operation.")]
		public static ulong Extract(this ulong src, int bitcount)
		{
			ulong mask = (ulong.MaxValue >> (64 - bitcount));
			ulong fragment = ((ulong)src  & mask);

			return fragment;
		}

		#endregion

		#region Extract - Uint32 Buffer


		/// <summary>
		/// Extract (read/deserialize) a value from a source primitive (the buffer) by reading x bits starting at the bitposition, and return the reconstructed value. 
		/// </summary>
		/// <param name="src">Source primitive buffer to read from.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to return value.</param>
		/// <param name="bitposition">Auto-incremented reference to the src read bit pointer. Extraction starts at this point in src.</param>
		/// <returns>Cast this uint return value to the desired type.</returns>
		public static uint Extract(this uint src, ref int bitposition, int bitcount)
		{
			uint mask = (uint.MaxValue >> (32 - bitcount));
			uint fragment = (((uint)src >> bitposition) & mask);

			bitposition += bitcount;
			return fragment;
		}
		/// <summary>
		/// Extract and return bits from src. 
		/// </summary>
		/// <param name="src">Source primitive.</param>
		/// <param name="bitcount">How many lower order bits to read.</param>
		/// <returns>Cast the return value to the desired type.</returns>
		[System.Obsolete("Always include the [ref int bitposition] argument. Extracting from position 0 would be better handled with a mask operation.")]
		public static uint Extract(this uint src, int bitcount)
		{
			uint mask = (uint.MaxValue >> (32 - bitcount));
			uint fragment = ((uint)src & mask);

			return fragment;
		}

		#endregion

		#region Extract - Uint16 Buffer

		/// <summary>
		/// Extract (read/deserialize) a value from a source primitive (the buffer) by reading x bits starting at the bitposition, and return the reconstructed value. 
		/// </summary>
		/// <param name="src">Source primitive buffer to read from.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to return value.</param>
		/// <param name="bitposition">Auto-incremented reference to the src read bit pointer. Extraction starts at this point in src.</param>
		/// <returns>Cast this ushort return value to the desired type.</returns>
		public static uint Extract(this ushort src, ref int bitposition, int bitcount)
		{
			uint mask = ((uint)ushort.MaxValue >> (16 - bitcount));
			uint fragment = (((uint)src >> bitposition) & mask);

			bitposition += bitcount;
			return fragment;
		}

		#endregion

		#region Extract - Uint8 Buffer

		/// <summary>
		/// Extract (read/deserialize) a value from a source primitive (the buffer) by reading x bits starting at the bitposition, and return the reconstructed value. 
		/// </summary>
		/// <param name="src">Source primitive buffer to read from.</param>
		/// <param name="bitcount">Number of lower order bits to copy from source to return value.</param>
		/// <param name="bitposition">Auto-incremented reference to the src read bit pointer. Extraction starts at this point in src.</param>
		/// <returns>Downcast this uint return value to the desired type.</returns>
		public static uint Extract(this byte src, ref int bitposition, int bitcount)
		{
			uint mask = ((uint)byte.MaxValue >> (8 - bitcount));
			uint fragment = (((uint)src >> bitposition) & mask);

			bitposition += bitcount;
			return fragment;
		}
		/// <summary>
		/// Extract and return bits from src. 
		/// </summary>
		/// <param name="src">Source primitive.</param>
		/// <param name="bitcount">How many lower order bits to read.</param>
		/// <returns>Cast the return value to the desired type.</returns>
		[System.Obsolete("Always include the [ref int bitposition] argument. Extracting from position 0 would be better handled with a mask operation.")]
		public static byte Extract(this byte src, int bitcount)
		{
			uint mask = ((uint)byte.MaxValue >> (8 - bitcount));
			uint fragment = ((uint)src & mask);

			return (byte)fragment;
		}

		#endregion


		#region Float

		/// <summary>
		/// Inject (serialize/write) a float into a primitive buffer at the bitposition. No compression occurs, this is a full 32bit write.
		/// </summary>
		/// <param name="f">Float to compress and write.</param>
		/// <param name="buffer">Target buffer for write.</param>
		/// <param name="bitposition">Auto-incremented read position for the buffer (in bits)</param>
		public static void Inject(this float f, ref ulong buffer, ref int bitposition)
		{
			((ulong)(ByteConverter)f).Inject(ref buffer, ref bitposition, 32);
		}

		/// <summary>
		/// Extract a float from a bitpacked primitive(src) starting at bitposition.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="bitposition">Auto-incremented read position for the buffer (in bits)</param>
		/// <returns></returns>
		public static float ExtractFloat(this ulong buffer, ref int bitposition)
		{
			return (ByteConverter)Extract(buffer, ref bitposition, 32);
		}

		#endregion

		#region HalfFloat

		/// <summary>
		/// Inject (serialize/write) a compressed float into a primitive buffer at the bitposition.
		/// </summary>
		/// <param name="f">Float to compress and write.</param>
		/// <param name="buffer">Target buffer for write.</param>
		/// <param name="bitposition">Auto-incremented read position for the buffer (in bits)</param>
		public static ushort InjectAsHalfFloat(this float f, ref ulong buffer, ref int bitposition)
		{
			ushort c = HalfUtilities.Pack(f);
			c.Inject(ref buffer, ref bitposition, 16);
			return c;
		}
		/// <summary>
		/// Inject (serialize/write) a compressed float into a primitive buffer at the bitposition.
		/// </summary>
		/// <param name="f">Float to compress and write.</param>
		/// <param name="buffer">Target buffer for write.</param>
		/// <param name="bitposition">Auto-incremented read position for the buffer (in bits)</param>
		public static ushort InjectAsHalfFloat(this float f, ref uint buffer, ref int bitposition)
		{
			ushort c = HalfUtilities.Pack(f);
			c.Inject(ref buffer, ref bitposition, 16);
			return c;
		}

		/// <summary>
		/// Extract a float from a bitpacked primitive(src) starting at bitposition.
		/// </summary>
		/// <param name="buffer">Source buffer to extract from.</param>
		/// <param name="bitposition">Auto-incremented read position for the buffer (in bits)</param>
		/// <returns></returns>
		public static float ExtractHalfFloat(this ulong buffer, ref int bitposition)
		{
			ushort c = (ushort)Extract(buffer, ref bitposition, 16);
			return HalfUtilities.Unpack(c);
		}
		/// <summary>
		/// Extract a float from a bitpacked primitive(src) starting at bitposition.
		/// </summary>
		/// <param name="buffer">Source buffer to extract from.</param>
		/// <param name="bitposition">Auto-incremented read position for the buffer (in bits)</param>
		/// <returns></returns>
		public static float ExtractHalfFloat(this uint buffer, ref int bitposition)
		{
			ushort c = (ushort)Extract(buffer, ref bitposition, 16);
			return HalfUtilities.Unpack(c);
		}

		#endregion

	}
}
