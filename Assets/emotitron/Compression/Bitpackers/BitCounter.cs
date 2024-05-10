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
using System.Runtime.CompilerServices;

namespace emotitron.Compression
{

	public enum PackedBitsSize { UInt8 = 4, UInt16 = 5, UInt32 = 6, UInt64 = 7 }
	public enum PackedBytesSize { UInt8 = 1, UInt16 = 2, UInt32 = 3, UInt64 = 4 }

	/// <summary>
	/// Experimental packers, that counts number of used bits for serialization. Effective for values that hover close to zero.
	/// </summary>
	public static class BitCounter
	{
		#region Count Used Bit Utils

		public static readonly int[] bitPatternToLog2 = new int[128] {
			0, // change to 1 if you want bitSize(0) = 1
			48, -1, -1, 31, -1, 15, 51, -1, 63, 5, -1, -1, -1, 19, -1,
			23, 28, -1, -1, -1, 40, 36, 46, -1, 13, -1, -1, -1, 34, -1, 58,
			-1, 60, 2, 43, 55, -1, -1, -1, 50, 62, 4, -1, 18, 27, -1, 39,
			45, -1, -1, 33, 57, -1, 1, 54, -1, 49, -1, 17, -1, -1, 32, -1,
			53, -1, 16, -1, -1, 52, -1, -1, -1, 64, 6, 7, 8, -1, 9, -1,
			-1, -1, 20, 10, -1, -1, 24, -1, 29, -1, -1, 21, -1, 11, -1, -1,
			41, -1, 25, 37, -1, 47, -1, 30, 14, -1, -1, -1, -1, 22, -1, -1,
			35, 12, -1, -1, -1, 59, 42, -1, -1, 61, 3, 26, 38, 44, -1, 56
		};
		public const ulong MULTIPLICATOR = 0x6c04f118e9966f6bUL;

		/// <summary>
		/// Number of bits used (ie. position of the first non-zero bit from left to right).
		/// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int UsedBitCount(this ulong val)
		{
			val |= val >> 1;
			val |= val >> 2;
			val |= val >> 4;
			val |= val >> 8;
			val |= val >> 16;
			val |= val >> 32;
			return bitPatternToLog2[(ulong)(val * MULTIPLICATOR) >> 57];
		}

		/// <summary>
		/// Number of bits used (ie. position of the first non-zero bit from left to right).
		/// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int UsedBitCount(this uint val)
		{
			val |= val >> 1;
			val |= val >> 2;
			val |= val >> 4;
			val |= val >> 8;
			val |= val >> 16;
			//v |= v >> 32;
			return bitPatternToLog2[(ulong)(val * MULTIPLICATOR) >> 57];
		}

		/// <summary>
		/// Number of bits used (ie. position of the first non-zero bit from left to right).
		/// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int UsedBitCount(this int val) {
      
      val |= val >> 1;
      val |= val >> 2;
      val |= val >> 4;
      val |= val >> 8;
      val |= val >> 16;
      //v |= v >> 32;
      return bitPatternToLog2[((ulong)val * MULTIPLICATOR) >> 57];
      
      // Alternative method, not sure which is faster here.
      
      // if (val == 0) return 0;
      //
      // if ((val & 0xFFFF0000) != 0) {
      //   if ((val & 0xFF000000) != 0) {
      //     if ((val & 0xF0000000) != 0) {
      //       if ((val & 0x80000000) == 0x80000000) return 32;
      //       if ((val & 0x40000000) == 0x40000000) return 31;
      //       if ((val & 0x20000000) == 0x20000000) return 30;
      //       return 29;            
      //     } else {
      //       if ((val & 0x08000000) == 0x08000000) return 28;
      //       if ((val & 0x04000000) == 0x04000000) return 27;
      //       if ((val & 0x02000000) == 0x02000000) return 26;
      //       return 25;            
      //     }
      //
      //   } else { // 0x00FF0000
      //     if ((val & 0x00F00000) != 0) {
      //       if ((val & 0x00800000) == 0x00800000) return 24;
      //       if ((val & 0x00400000) == 0x00400000) return 23;
      //       if ((val & 0x00200000) == 0x00200000) return 22;
      //       return 21;
      //     } else {
      //       if ((val & 0x00080000) == 0x00080000) return 20;
      //       if ((val & 0x00040000) == 0x00040000) return 19;
      //       if ((val & 0x00020000) == 0x00020000) return 18;
      //       return 17;              
      //     }
      //   }
      //
      // } else {
      //
      //   if ((val & 0x0000FF00) != 0) {
      //     if ((val & 0x0000F000) != 0) {
      //       if ((val & 0x00008000) == 0x00008000) return 16;
      //       if ((val & 0x00004000) == 0x00004000) return 15;
      //       if ((val & 0x00002000) == 0x00002000) return 14;
      //       return 13;            
      //     } else {
      //       if ((val & 0x00000800) == 0x00000800) return 12;
      //       if ((val & 0x00000400) == 0x00000400) return 11;
      //       if ((val & 0x00000200) == 0x00000200) return 10;
      //       return 09;              
      //     }
      //   
      //   } else { // 0x000000FF
      //     if ((val & 0x000000F0) != 0) {
      //       if ((val & 0x00000080) == 0x00000080) return 08;
      //       if ((val & 0x00000040) == 0x00000040) return 07;
      //       if ((val & 0x00000020) == 0x00000020) return 06;
      //       return 05;            
      //     } else {
      //       if ((val & 0x00000008) == 0x00000008) return 04;
      //       if ((val & 0x00000004) == 0x00000004) return 03;
      //       if ((val & 0x00000002) == 0x00000002) return 02;
      //       return 01;               
      //     }
      //   }
      // }
    }

		/// <summary>
		/// Number of bits used (ie. position of the first non-zero bit from left to right).
		/// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int UsedBitCount(this ushort val)
		{
			uint v = val;
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			//val |= val >> 16;
			//v |= v >> 32;
			return bitPatternToLog2[(ulong)(v * MULTIPLICATOR) >> 57];
		}

		/// <summary>
		/// Number of bits used (ie. position of the first non-zero bit from left to right).
		/// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int UsedBitCount(this byte val)
		{
			uint v = val;
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			//v |= v >> 8;
			//v |= v >> 16;
			//v |= v >> 32;
			return bitPatternToLog2[(ulong)(v * MULTIPLICATOR) >> 57];
		}

		#endregion

		#region Count Used Bytes Utils

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int UsedByteCount(this ulong val) {
      if (val == 0) return 0;
      
      if ((val & 0xFFFFFFFF00000000) != 0) {
        // one of the left 4 was used
        if ((val & 0xFFFF000000000000) != 0) {
          return ((val & 0xFF00000000000000) != 0) ? 8 : 7;
        } else {
          return ((val & 0x0000FF0000000000) != 0) ? 6 : 5;
        }
        
      } else {
        // None of the left 4 were used
        if ((val & 0x00000000FFFF0000) != 0) {
          return ((val & 0x00000000FF000000) != 0) ? 4 : 3;
        } else {
          return ((val & 0x000000000000FF00) != 0) ? 2 : 1;
        }
      }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int UsedByteCount(this uint val)
    {
      if (val == 0) return 0;
      
      // None of the left 4 were used
      if ((val & 0xFFFF0000) != 0) {
        return ((val & 0xFF000000) != 0) ? 4 : 3;
      } else {
        return ((val & 0x0000FF00) != 0) ? 2 : 1;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int UsedByteCount(this ushort val)
		{
      if (val == 0) return 0;
      return ((val & 0xFF00) != 0) ? 2 : 1;
		}
		#endregion
	}
}

