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

namespace emotitron.Compression
{
	/// <summary>
	/// Experimental packers, that counts number of used bits for serialization. Effective for values that hover close to zero.
	/// </summary>
	public static class ArrayPackBitsExt
	{

		#region Primary Write Packed

		/// <summary>
		/// EXPERIMENTAL: Primary WritePacked Method
		/// </summary>
		/// <param name="countbits"></param>
		public static void WritePackedBits(this byte[] buffer, ulong value, ref int bitposition, int bits)
		{
			int cnt = value.UsedBitCount();
			int countbits = bits.UsedBitCount();
			buffer.Write((uint)(cnt), ref bitposition, (int)countbits);
			buffer.Write(value, ref bitposition, cnt);

			UnityEngine.Debug.Log(value + " = ones : " + cnt + " / " + (int)countbits + "  total bits: " + ((int)countbits + cnt));
		}
		/// <summary>
		/// EXPERIMENTAL: Primary WritePacked Method
		/// </summary>
		/// <param name="countbits"></param>
		public static void WritePackedBits(this uint[] buffer, ulong value, ref int bitposition, int bits)
		{
			int cnt = value.UsedBitCount();
			int countbits = bits.UsedBitCount();
			buffer.Write((uint)(cnt), ref bitposition, (int)countbits);
			buffer.Write(value, ref bitposition, cnt);

			UnityEngine.Debug.Log(value + " = ones : " + cnt + " / " + (int)countbits + "  total bits: " + ((int)countbits + cnt));
		}
		/// <summary>
		/// EXPERIMENTAL: Primary WritePacked Method
		/// </summary>
		/// <param name="countbits"></param>
		public static void WritePackedBits(this ulong[] buffer, ulong value, ref int bitposition, int bits)
		{
			int cnt = value.UsedBitCount();
			int countbits = bits.UsedBitCount();
			buffer.Write((uint)(cnt), ref bitposition, (int)countbits);
			buffer.Write(value, ref bitposition, cnt);

			UnityEngine.Debug.Log(value + " = ones : " + cnt + " / " + (int)countbits + "  total bits: " + ((int)countbits + cnt));
		}

		#endregion

		#region Primary Read Packed

		/// <summary>
		/// Primary Reader for PackedBits.
		/// </summary>
		public static ulong ReadPackedBits(this ulong[] buffer, ref int bitposition, int bits)
		{
			int countbits = bits.UsedBitCount();
			int cnt = (int)buffer.Read(ref bitposition, countbits);
			return buffer.Read(ref bitposition, cnt);
		}
		/// <summary>
		/// Primary Reader for PackedBits.
		/// </summary>
		public static ulong ReadPackedBits(this uint[] buffer, ref int bitposition, int bits)
		{
			int countbits = bits.UsedBitCount();
			int cnt = (int)buffer.Read(ref bitposition, countbits);
			return buffer.Read(ref bitposition, cnt);
		}
		/// <summary>
		/// Primary Reader for PackedBits.
		/// </summary>
		public static ulong ReadPackedBits(this byte[] buffer, ref int bitposition, int bits)
		{
			int countbits = bits.UsedBitCount();
			int cnt = (int)buffer.Read(ref bitposition, countbits);
			return buffer.Read(ref bitposition, cnt);
		}

		#endregion

		#region Packed Signed

		// ulong[]

		/// <summary>
		/// EXPERIMENTAL: Primary Write packed signed value. ZigZag is employed to move the sign to the rightmost position.
		/// Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WriteSignedPackedBits(this ulong[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBits(zigzag, ref bitposition, bits);
		}
		/// <summary>
		/// EXPERIMENTAL: Primary Read packed signed value. ZigZag is employed to move the sign to the rightmost position.
		/// Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static int ReadSignedPackedBits(this ulong[] buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.ReadPackedBits(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}

		//uint[]

		/// <summary>
		/// EXPERIMENTAL: Primary Write packed signed value. ZigZag is employed to move the sign to the rightmost position.
		/// Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WriteSignedPackedBits(this uint[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBits(zigzag, ref bitposition, bits);
		}
		/// <summary>
		/// EXPERIMENTAL: Primary Read packed signed value. ZigZag is employed to move the sign to the rightmost position.
		/// Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static int ReadSignedPackedBits(this uint[] buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.ReadPackedBits(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}

		// byte[]

		/// <summary>
		/// EXPERIMENTAL: Primary Write packed signed value. ZigZag is employed to move the sign to the rightmost position.
		/// Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WriteSignedPackedBits(this byte[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBits(zigzag, ref bitposition, bits);
		}
		/// <summary>
		/// EXPERIMENTAL: Primary Read packed signed value. ZigZag is employed to move the sign to the rightmost position.
		/// Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static int ReadSignedPackedBits(this byte[] buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.ReadPackedBits(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}
		
		#endregion
	}
}

