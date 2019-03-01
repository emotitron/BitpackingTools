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
	public static class ArrayPackedBitsExt
	{
		public enum PackBitCount { UInt8 = 3, UInt16 = 4, UInt32 = 5, UInt64 = 6 }

		#region Primary Write Packed

		/// <summary>
		/// EXPERIMENTAL: Primary WritePacked Method
		/// </summary>
		/// <param name="countbits"></param>
		public static void WritePackedBits(this byte[] buffer, ulong value, ref int bitposition, PackBitCount countbits)
		{
			int cnt = value.UsedBitCount();

			buffer.Write((uint)(cnt - 1), ref bitposition, (int)countbits);
			buffer.Write(value, ref bitposition, cnt);

			UnityEngine.Debug.Log(value + " = ones : " + cnt + " / " + (int)countbits + "  total bits: " + ((int)countbits + cnt));
		}
		/// <summary>
		/// EXPERIMENTAL: Primary WritePacked Method
		/// </summary>
		/// <param name="countbits"></param>
		public static void WritePackedBits(this uint[] buffer, ulong value, ref int bitposition, PackBitCount countbits)
		{
			int cnt = value.UsedBitCount();

			buffer.Write((uint)(cnt - 1), ref bitposition, (int)countbits);
			buffer.Write(value, ref bitposition, cnt);

			UnityEngine.Debug.Log(value + " = ones : " + cnt + " / " + (int)countbits + "  total bits: " + ((int)countbits + cnt));
		}
		/// <summary>
		/// EXPERIMENTAL: Primary WritePacked Method
		/// </summary>
		/// <param name="countbits"></param>
		public static void WritePackedBits(this ulong[] buffer, ulong value, ref int bitposition, PackBitCount countbits)
		{
			int cnt = value.UsedBitCount();

			buffer.Write((uint)(cnt - 1), ref bitposition, (int)countbits);
			buffer.Write(value, ref bitposition, cnt);

			UnityEngine.Debug.Log(value + " = ones : " + cnt + " / " + (int)countbits + "  total bits: " + ((int)countbits + cnt));
		}

		#endregion

		#region Secondary WritePacked

		/// <summary>
		/// EXPERIMENTAL: Write packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WritePackedBits(this ulong[] buffer, ulong value, ref int bitposition)
		{
			WritePackedBits(buffer, value, ref bitposition, PackBitCount.UInt64);
		}
		/// <summary>
		/// EXPERIMENTAL: Write packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WritePackedBits(this ulong[] buffer, uint value, ref int bitposition)
		{
			WritePackedBits(buffer, value, ref bitposition, PackBitCount.UInt32);
		}
		/// <summary>
		/// EXPERIMENTAL: Write packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WritePackedBits(this ulong[] buffer, ushort value, ref int bitposition)
		{
			WritePackedBits(buffer, value, ref bitposition, PackBitCount.UInt16);
		}
		/// <summary>
		/// EXPERIMENTAL: Write packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WritePackedBits(this ulong[] buffer, byte value, ref int bitposition)
		{
			WritePackedBits(buffer, value, ref bitposition, PackBitCount.UInt8);
		}


		/// <summary>
		/// EXPERIMENTAL: Write packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WritePackedBits(this uint[] buffer, ulong value, ref int bitposition)
		{
			WritePackedBits(buffer, value, ref bitposition, PackBitCount.UInt64);
		}
		/// <summary>
		/// EXPERIMENTAL: Write packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WritePackedBits(this uint[] buffer, uint value, ref int bitposition)
		{
			WritePackedBits(buffer, value, ref bitposition, PackBitCount.UInt32);
		}
		/// <summary>
		/// EXPERIMENTAL: Write packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WritePackedBits(this uint[] buffer, ushort value, ref int bitposition)
		{
			WritePackedBits(buffer, value, ref bitposition, PackBitCount.UInt16);
		}
		/// <summary>
		/// EXPERIMENTAL: Write packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WritePackedBits(this uint[] buffer, byte value, ref int bitposition)
		{
			WritePackedBits(buffer, value, ref bitposition, PackBitCount.UInt8);
		}


		/// <summary>
		/// EXPERIMENTAL: Write packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WritePackedBits(this byte[] buffer, ulong value, ref int bitposition)
		{
			WritePackedBits(buffer, value, ref bitposition, PackBitCount.UInt64);
		}
		/// <summary>
		/// EXPERIMENTAL: Write packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WritePackedBits(this byte[] buffer, uint value, ref int bitposition)
		{
			WritePackedBits(buffer, value, ref bitposition, PackBitCount.UInt32);
		}
		/// <summary>
		/// EXPERIMENTAL: Write packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WritePackedBits(this byte[] buffer, ushort value, ref int bitposition)
		{
			WritePackedBits(buffer, value, ref bitposition, PackBitCount.UInt16);
		}
		/// <summary>
		/// EXPERIMENTAL: Write packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WritePackedBits(this byte[] buffer, byte value, ref int bitposition)
		{
			WritePackedBits(buffer, value, ref bitposition, PackBitCount.UInt8);
		}


		#endregion

		#region Primary Read Packed


		/// <summary>
		/// Primary Reader for PackedBits.
		/// </summary>
		public static ulong ReadPackedBits(this ulong[] buffer, ref int bitposition, PackBitCount packcount)
		{
			int cnt = (int)buffer.Read(ref bitposition, (int)packcount) + 1;
			return buffer.Read(ref bitposition, cnt);
		}
		/// <summary>
		/// Primary Reader for PackedBits.
		/// </summary>
		public static ulong ReadPackedBits(this uint[] buffer, ref int bitposition, PackBitCount packcount)
		{
			int cnt = (int)buffer.Read(ref bitposition, (int)packcount) + 1;
			return buffer.Read(ref bitposition, cnt);
		}
		/// <summary>
		/// Primary Reader for PackedBits.
		/// </summary>
		public static ulong ReadPackedBits(this byte[] buffer, ref int bitposition, PackBitCount packcount)
		{
			int cnt = (int)buffer.Read(ref bitposition, (int)packcount) + 1;
			return buffer.Read(ref bitposition, cnt);
		}

		#endregion

		#region Secondary Read Packed

		// ulong[]
		/// <summary>
		/// EXPERIMENTAL: Read packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover near or at zero in value.
		/// </summary>
		public static ulong ReadPackedUInt64(this ulong[] buffer, ref int bitposition)
		{
			return ReadPackedBits(buffer, ref bitposition, PackBitCount.UInt64);
		}
		/// <summary>
		/// EXPERIMENTAL: Read packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover near or at zero in value.
		/// </summary>
		public static uint ReadPackedUInt32(this ulong[] buffer, ref int bitposition)
		{
			return (uint)ReadPackedBits(buffer, ref bitposition, PackBitCount.UInt32);
		}
		/// <summary>
		/// EXPERIMENTAL: Read packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover near or at zero in value.
		/// </summary>
		public static ushort ReadPackedUInt16(this ulong[] buffer, ref int bitposition)
		{
			return (byte)ReadPackedBits(buffer, ref bitposition, PackBitCount.UInt16);
		}
		/// <summary>
		/// EXPERIMENTAL: Read packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover near or at zero in value.
		/// </summary>
		public static byte ReadPackedUInt8(this ulong[] buffer, ref int bitposition)
		{
			return (byte)ReadPackedBits(buffer, ref bitposition, PackBitCount.UInt8);
		}

		// uint[]
		/// <summary>
		/// EXPERIMENTAL: Read packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover near or at zero in value.
		/// </summary>
		public static ulong ReadPackedUInt64(this uint[] buffer, ref int bitposition)
		{
			return ReadPackedBits(buffer, ref bitposition, PackBitCount.UInt64);
		}
		/// <summary>
		/// EXPERIMENTAL: Read packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover near or at zero in value.
		/// </summary>
		public static uint ReadPackedUInt32(this uint[] buffer, ref int bitposition)
		{
			return (uint)ReadPackedBits(buffer, ref bitposition, PackBitCount.UInt32);
		}
		/// <summary>
		/// EXPERIMENTAL: Read packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover near or at zero in value.
		/// </summary>
		public static ushort ReadPackedUInt16(this uint[] buffer, ref int bitposition)
		{
			return (ushort)ReadPackedBits(buffer, ref bitposition, PackBitCount.UInt16);
		}
		/// <summary>
		/// EXPERIMENTAL: Read packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover near or at zero in value.
		/// </summary>
		public static byte ReadPackedUInt8(this uint[] buffer, ref int bitposition)
		{
			return (byte)ReadPackedBits(buffer, ref bitposition, PackBitCount.UInt8);
		}

		// byte[]
		/// <summary>
		/// EXPERIMENTAL: Read packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover near or at zero in value.
		/// </summary>
		public static ulong ReadPackedUInt64(this byte[] buffer, ref int bitposition)
		{
			return (ulong)ReadPackedBits(buffer, ref bitposition, PackBitCount.UInt64);
		}
		/// <summary>
		/// EXPERIMENTAL: Read packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover near or at zero in value.
		/// </summary>
		public static uint ReadPackedUInt32(this byte[] buffer, ref int bitposition)
		{
			return (uint)ReadPackedBits(buffer, ref bitposition, PackBitCount.UInt32);
		}
		/// <summary>
		/// EXPERIMENTAL: Read packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover near or at zero in value.
		/// </summary>
		public static ushort ReadPackedUInt16(this byte[] buffer, ref int bitposition)
		{
			return (ushort)ReadPackedBits(buffer, ref bitposition, PackBitCount.UInt16);
		}
		/// <summary>
		/// EXPERIMENTAL: Read packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover near or at zero in value.
		/// </summary>
		public static byte ReadPackedUInt8(this byte[] buffer, ref int bitposition)
		{
			return (byte)ReadPackedBits(buffer, ref bitposition, PackBitCount.UInt8);
		}

		#endregion

		#region Packed Signed

		/// <summary>
		/// EXPERIMENTAL: Write packed value. Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static void WritePackedBitsSigned(this ulong[] buffer, int value, ref int bitposition)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBits(zigzag, ref bitposition, PackBitCount.UInt32);
		}

		public static int ReadPackedBitsSignedInt32(this ulong[] buffer, ref int bitposition)
		{
			uint value = (uint)buffer.ReadPackedBits(ref bitposition, PackBitCount.UInt32);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}


		public static void WritePackedBitsSigned(this uint[] buffer, int value, ref int bitposition)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBits(zigzag, ref bitposition, PackBitCount.UInt32);
		}

		public static int ReadPackedBitsSignedInt32(this uint[] buffer, ref int bitposition)
		{
			uint value = (uint)buffer.ReadPackedBits(ref bitposition, PackBitCount.UInt32);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}

		public static void WritePackedBitsSigned(this byte[] buffer, int value, ref int bitposition)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBits(zigzag, ref bitposition, PackBitCount.UInt32);
		}

		public static int ReadPackedBitsSignedInt32(this byte[] buffer, ref int bitposition)
		{
			uint value = (uint)buffer.ReadPackedBits(ref bitposition, PackBitCount.UInt32);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}




		#endregion

	}
}

