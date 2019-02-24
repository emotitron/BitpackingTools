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
	/// A mini-bitpacker (up to 40 bytes) used for storing compressed transforms. Contains methods for basic Serialization.
	/// </summary>
	public unsafe struct Bitstream1024
	{
	
		/// <summary>
		/// The number of fixed ulong fragments acting as the backing array for the Bitstream struct.
		/// </summary>
		public const int ULONG_COUNT = 128;
		private fixed ulong fragments[ULONG_COUNT];

		private int _writePtr, _readPtr;
		/// <summary>
		/// The current bit position for writes to this bitstream. The next write will begin at this bit.
		/// </summary>
		public int WritePtr { get { return _writePtr; } }
		/// <summary>
		/// The current bit position for reads from this bitstream. The next read will begin at this bit.
		/// </summary>
		public int ReadPtr { get { return _readPtr; } }

		/// <summary>
		/// The writePtr minus the readPtr, This is how many bits are left to Write() before reaching the end of the bitstream.
		/// </summary>
		public int RemainingBits { get { return _writePtr - _readPtr; } }

		/// <summary>
		/// Sets the write bit position to 0;
		/// </summary>
		public void ResetWritePtr()
		{
			_writePtr = 0;
		}
		/// <summary>
		/// Sets the read bit position to 0;
		/// </summary>
		public void ResetReadPtr()
		{
			_readPtr = 0;
		}

		/// <summary>
		/// Reset the bitstream to an empty state.
		/// </summary>
		public void Reset()
		{
			_writePtr = 0;
			_readPtr = 0;
			fixed (ulong* ulongPtr = fragments)
			{
				for (int i = 0; i < ULONG_COUNT; ++i)
					ulongPtr[i] = 0;
			}
		}

		/// <summary>
		/// Returns the rounded up number of bytes currently used, based on the position of the WritePtr.
		/// </summary>
		public int WritePtrBytes
		{
			get
			{
				return (_writePtr + 7) >> 3;
			}
		}

		/// <summary>
		/// Returns how many bytes have been read, rounded up to the nearest byte. If 0 bits have been read, this will be zero. If 9 bits have been read, this will be 2.
		/// </summary>
		public int ReadPtrBytes
		{
			get
			{
				return (_readPtr + 7) >> 3;
			}
		}

		/// <summary>
		/// Returns how many bits of the fragment index are used given the total bits
		/// </summary>
		/// <param name="fragment"></param>
		/// <param name="totalbits"></param>
		/// <returns></returns>
		public static int BitsUsedByFragment(int fragment, int totalbits)
		{
			return System.Math.Max(0, System.Math.Min(64, totalbits - fragment * 64));
		}

		public unsafe ulong this[int i]
		{
			get
			{
				fixed (ulong* ulongPtr = fragments)
				{
					if (i < ULONG_COUNT)
						return ulongPtr[i];
					else
						return 0;
				}
			}
		}

		/// <summary>
		/// Read out the byte[] index equivalent. Interally the bitstream is Fixed ulong[5].
		/// </summary>
		/// <param name="arrayIndex"></param>
		/// <returns></returns>
		public unsafe byte GetByte(int arrayIndex)
		{
			fixed (ulong* ulongPtr = fragments)
			{
				return ((byte*)ulongPtr)[arrayIndex];
			}
		}
		public unsafe ushort GetInt16(int arrayIndex)
		{
			fixed (ulong* ulongPtr = fragments)
			{
				return ((ushort*)ulongPtr)[arrayIndex];
			}
		}
		public unsafe uint GetInt32(int arrayIndex)
		{
			fixed (ulong* ulongPtr = fragments)
			{
				return ((uint*)ulongPtr)[arrayIndex];
			}
		}
		public unsafe ulong GetUint64(int arrayIndex)
		{
			fixed (ulong* ulongPtr = fragments)
				return ulongPtr[arrayIndex];
		}

		#region Constructors

		// Constructor
		public unsafe Bitstream1024(byte[] bytes) : this()
		{
			int count = bytes.Length;
			_writePtr = count * 8;

			fixed (ulong* ulongPtr = fragments)
			{
				byte* arrayPtr = (byte*)ulongPtr;
				for (int i = 0; i < count; ++i)
					arrayPtr[i] = bytes[i];
			}
		}

		// Constructor
		public unsafe Bitstream1024(byte[] bytes, int count) : this()
		{
			_writePtr = count * 8;

			fixed (ulong* ulongPtr = fragments)
			{
				byte* arrayPtr = (byte*)ulongPtr;
				for (int i = 0; i < count; ++i)
					arrayPtr[i] = bytes[i];
			}
		}

		// Constructor
		public unsafe Bitstream1024(ushort[] array) : this()
		{
			int count = array.Length;
			_writePtr = count * 16;

			fixed (ulong* ulongPtr = fragments)
			{
				ushort* arrayPtr = (ushort*)ulongPtr;
				for (int i = 0; i < count; ++i)
					arrayPtr[i] = array[i];
			}
		}

		// Constructor
		public unsafe Bitstream1024(ushort[] array, int count) : this()
		{
			_writePtr = count * 16;

			fixed (ulong* ulongPtr = fragments)
			{
				ushort* arrayPtr = (ushort*)ulongPtr;
				for (int i = 0; i < count; ++i)
					arrayPtr[i] = array[i];
			}
		}

		// Constructor
		public unsafe Bitstream1024(uint[] array) : this()
		{
			int count = array.Length;
			_writePtr = count * 32;

			fixed (ulong* ulongPtr = fragments)
			{
				uint* arrayPtr = (uint*)ulongPtr;
				for (int i = 0; i < count; ++i)
					arrayPtr[i] = array[i];
			}
		}

		// Constructor
		public unsafe Bitstream1024(uint[] array, int count) : this()
		{
			_writePtr = count * 32;

			fixed (ulong* ulongPtr = fragments)
			{
				uint* arrayPtr = (uint*)ulongPtr;
				for (int i = 0; i < count; ++i)
					arrayPtr[i] = array[i];
			}
		}

		// Constructor
		public Bitstream1024(ulong A, int aBits) : this()
		{
			fixed (ulong* ulongPtr = fragments)
				ulongPtr[0] = A;

			_writePtr = aBits;
		}

		#endregion

		/// <summary>
		/// Writes the bits of src bitstream (from 0 to WritePtr) into this bitstream. NOTE this method is the BEST choice. Passing bitstream as a ref when possible
		/// avoids a 48 byte struct allocation.
		/// </summary>
		/// <param name="src"></param>
		public void Write(ref Bitstream1024 src)
		{
			int bits = src._writePtr;
			int pos = 0;

			fixed (ulong* int64Ptr = src.fragments)
			{
				for (int i = 0; i < 128; i++)
				{
					if (bits > pos)
						Write(int64Ptr[i], System.Math.Min(64, System.Math.Max(0, bits - pos)));
					else return;
					pos += 64;
				}
			}
		}


		private const string bufferoverrunerr = "Bitstream write would exceed index limitation of 5 ulongs (40 bytes). Aborting write. Data corruption very likely.";
		/// <summary>
		/// The primary write method. All other write methods lead to this one.
		/// </summary>
		/// <param name="value">Value to write to bitstream.</param>
		/// <param name="bits">Number of lower order bits to write.</param>
		public void Write(ulong value, int bits)
		{
			int index = _writePtr >> 6;
			int offset = _writePtr % 64;
			int endpos = _writePtr + bits;
			int endindex = ((endpos - 1) >> 6);

			_writePtr += bits;

			ulong offsetcomp = value << offset;

			bool overrun = endindex >= 5;

			System.Diagnostics.Debug.Assert(!overrun, bufferoverrunerr);

			if (overrun)
				return;

			fixed (ulong* int64Ptr = fragments)
			{
				int64Ptr[index] |= offsetcomp;

				if (index == endindex)
					return;

				offset = 64 - offset;
				offsetcomp = value >> (offset);

				int64Ptr[endindex] |= offsetcomp;
			}
		}

		/// <summary>
		/// Write the contents of a byte[] to this bitstream.
		/// </summary>
		/// <param name="src"></param>
		/// <param name="bitcount">Number of bits to copy.</param>
		public void WriteFromByteBuffer(byte[] src, int bitcount = -1)
		{
			if (bitcount == -1)
				bitcount = src.Length * 8;

			int i = 0;
			while (bitcount > 0)
			{
				if (bitcount > 8)
				{
					Write(src[i], 8);
					bitcount -= 8;
				}
				else
				{
					Write(src[i], bitcount);
					break;
				}

				i++;
				if (i == src.Length)
					break;
			}
		}

		public void WriteByte(byte value, int bits = 8) { Write(value, bits); }
		public void WriteSByte(sbyte value, int bits = 8) { Write((ulong)value, bits); }
		public void WriteUShort(ushort value, int bits = 16) { Write(value, bits); }
		public void WriteShort(short value, int bits = 16) { Write((ulong)value, bits); }
		public void WriteUInt(uint value, int bits = 32) { Write(value, bits); }
		public void WriteInt(int value, int bits = 32) { Write((ulong)value, bits); }
		public void WriteULong(ulong value, int bits = 64) { Write(value, bits); }
		public void WriteBool(bool value) { Write((value ? (ulong)1 : 0), 1); }

		public byte ReadByte(int bits = 8) { return (byte)Read(bits); }
		public ushort ReadShort(int bits = 16) { return (ushort)Read(bits); }
		public uint ReadUint32(int bits = 32) { return (uint)Read(bits); }
		public ulong ReadUInt64(int bits = 64) { return Read(bits); }
		public bool ReadBool() { return (Read(1) == 1); }

		/// <summary>
		/// The primary Read method. All other Read method overloads lead to this one.
		/// </summary>
		/// <param name="bits"></param>
		/// <returns></returns>
		public unsafe ulong Read(int bits)
		{
			int index = _readPtr >> 6;
			int offset = _readPtr % 64;
			int endpos = _readPtr + bits;
			int endindex = ((endpos - 1) >> 6);

			_readPtr += bits;

			ulong mask = ulong.MaxValue >> (64 - bits);
			ulong line;

			ulong value = 0;
			fixed (ulong* fragptr = fragments)
			{
				line = (fragptr[index] >> offset);

				value |= (line & mask);

				// End here if this value doesn't span two ulongs
				if (index == endindex)
					return value;

				offset = 64 - offset;

				line = (fragptr[endindex] << offset);
			}

			value |= (line & mask);
			return value;
		}

		/// <summary>
		/// Read this entire bitstream (from bit 0 to the current writePtr position) to the supplied byte[]. Bitposition will be incremented accordingly.
		/// </summary>
		/// <param name="target">Target array buffer.</param>
		/// <param name="bitposition">Reference to the bitpointer of the target array buffer.</param>
		public void ReadOut(byte[] target, ref int bitposition)
		{
			int remainingbits = _writePtr;
			int index = 0;

			fixed (ulong* ulongPtr = fragments)
			{
				while (remainingbits > 0)
				{
					ulong frag = ulongPtr[index];
					for (int i = 0; i < 64; i += 8)
					{
						int bits = remainingbits > 8 ? 8 : remainingbits;
						target.Write(frag >> i, ref bitposition, bits);
						remainingbits -= bits;

						if (remainingbits == 0)
							return;
					}
					index++;
				}
			}
		}

		/// <summary>
		/// Read this entire bitstream (from bit 0 to the current writePtr position) to the supplied byte[].
		/// </summary>
		/// <param name="target">Target array buffer.</param>
		/// <returns>Returns the number of bits that were written.</returns>
		public int ReadOut(byte[] target)
		{
			int bitsused = 0;
			ReadOut(target, ref bitsused);
			return bitsused;
		}

		/// <summary>
		/// When possible, use Compare(ref Bitstream a, ref Bitstream b). Bitstream is a 40byte struct, so it is best to pass by ref when possible.
		/// </summary>
		/// <returns>true if the bitstreams match.</returns>
		public static bool Compare(ref Bitstream1024 a, ref Bitstream1024 b)
		{
			if (a._writePtr != b._writePtr)
				return false;

			fixed (ulong* aPtr = a.fragments)
			fixed (ulong* bPtr = b.fragments)
			{
				if (aPtr[0] == bPtr[0] &&
					aPtr[1] == bPtr[1] &&
					aPtr[2] == bPtr[2] &&
					aPtr[3] == bPtr[3] &&
					aPtr[4] == bPtr[4])
					return true;

				return false;
			}
		}

		public bool Equals(ref Bitstream1024 other)
		{
			if (_writePtr != other._writePtr)
				return false;

			fixed (ulong* aPtr = fragments)
			fixed (ulong* bPtr = other.fragments)
			{
				if (
					aPtr[0] == bPtr[0] &&
					aPtr[1] == bPtr[1] &&
					aPtr[2] == bPtr[2] &&
					aPtr[3] == bPtr[3] &&
					aPtr[4] == bPtr[4])
					return true;

				return false;
			}
		}

		public override int GetHashCode()
		{
			var hashCode = 298898743;
			hashCode = hashCode * -1521134295 + _writePtr.GetHashCode();
			fixed (ulong* Ptr = fragments)
			{
				hashCode = hashCode * -1521134295 + Ptr[0].GetHashCode();
				hashCode = hashCode * -1521134295 + Ptr[1].GetHashCode();
				hashCode = hashCode * -1521134295 + Ptr[2].GetHashCode();
				hashCode = hashCode * -1521134295 + Ptr[3].GetHashCode();
				hashCode = hashCode * -1521134295 + Ptr[4].GetHashCode();
			}

			return hashCode;
		}


		public override string ToString()
		{
			return "CompositeBuffer <b>len:" + WritePtr + "</b> [frag0: " + this[0] + " frag1: " + this[1] + " frag2: " + this[2] + " frag3: " + this[3] + " frag4: " + this[4] + "]";
		}

	}

}

