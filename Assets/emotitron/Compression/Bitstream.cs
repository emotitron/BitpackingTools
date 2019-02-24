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
	public unsafe struct Bitstream : IEquatable<Bitstream>
	{
		/// <summary>
		/// The number of fixed ulong fragments acting as the backing array for the Bitstream struct.
		/// </summary>
		public const int ULONG_COUNT = 5;
		private fixed ulong fragments[ULONG_COUNT];

		private int _writePtr;
		private int _readPtr;

		/// <summary>
		/// Implicit conversion of Bitstream to ulong extracts just the first fragment.
		/// </summary>
		/// <param name="bs"></param>
		public static implicit operator ulong(Bitstream bs)
		{
				return bs.fragments[0];
		}
		/// <summary>
		/// Implicit conversion of Bitstream to uint extracts just the first 32 bits of the first fragment.
		/// </summary>
		/// <param name="bs"></param>
		public static implicit operator uint(Bitstream bs)
		{
			return (uint)bs.fragments[0];
		}
		/// <summary>
		/// Implicit conversion of Bitstream to uint extracts just the first 16 bits of the first fragment.
		/// </summary>
		/// <param name="bs"></param>
		public static implicit operator ushort(Bitstream bs)
		{
			return (ushort)bs.fragments[0];
		}
		/// <summary>
		/// Implicit conversion of Bitstream to uint extracts just the first 8 bits of the first fragment.
		/// </summary>
		/// <param name="bs"></param>
		public static implicit operator byte(Bitstream bs)
		{
			return (byte)bs.fragments[0];
		}

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
				ulongPtr[0] = 0;
				ulongPtr[1] = 0;
				ulongPtr[2] = 0;
				ulongPtr[3] = 0;
				ulongPtr[4] = 0;
			}
		}

		/// <summary>
		/// Returns the rounded up number of bytes currently used, based on the position of the WritePtr.
		/// </summary>
		public int BytesUsed
		{
			get
			{
				return (_writePtr + 7) >> 3;
			}
		}
		/// <summary>
		/// Returns how many bytes have been read, rounded up to the nearest byte. If 0 bits have been read, this will be zero. If 9 bits have been read, this will be 2.
		/// </summary>
		public int ReadPtrBytePosition
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
		public unsafe Bitstream(ulong fragment0, ulong fragment1 = 0, ulong fragment2 = 0, ulong fragment3 = 0, uint fragment4 = 0) : this()
		{
			// TODO: This constructor doesn't set the WritePtr... should it?
			fixed (ulong* ulongPtr = fragments)
			{
				ulongPtr[0] = fragment0;
				ulongPtr[1] = fragment1;
				ulongPtr[2] = fragment2;
				ulongPtr[3] = fragment3;
				ulongPtr[4] = fragment4;
			}
		}

		// Constructor
		public Bitstream(ref Bitstream A, ref Bitstream B, ref Bitstream C) : this()
		{
			Write(ref A);
			Write(ref B);
			Write(ref C);
		}

		// Constructor
		public unsafe Bitstream(uint A, int aBits, uint B, int bBits, uint C, int cBits) : this()
		{
			fixed (ulong* ulongPtr = fragments)
				ulongPtr[0] = A;
			_writePtr = aBits;

			Write(B, bBits);
			Write(C, cBits);

		}

		// Constructor
		public unsafe Bitstream(PackedValue a, PackedValue b, PackedValue c) : this()
		{
			fixed (ulong* ulongPtr = fragments)
				ulongPtr[0] = a;
			_writePtr = a.bits;

			Write(b.uint64, b.bits);
			Write(c.uint64, c.bits);
		}

		// Constructor
		public unsafe Bitstream(byte[] bytes) : this()
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
		public unsafe Bitstream(byte[] bytes, int count) : this()
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
		public unsafe Bitstream(ushort[] array) : this()
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
		public unsafe Bitstream(ushort[] array, int count) : this()
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
		public unsafe Bitstream(uint[] array) : this()
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
		public unsafe Bitstream(uint[] array, int count) : this()
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
		public Bitstream(ulong A, int aBits) : this()
		{
			fixed (ulong* ulongPtr = fragments)
				ulongPtr[0] = A;

			_writePtr = aBits;
		}

		#endregion

		/// <summary>
		/// Writes the bits of src bitstream (from 0 to WritePtr) into this bitstream. NOTE this method is the worst choice. When possible use 'Write(ref src)'
		/// or 'crusher.Write(value, ref bitstream)'. 
		/// </summary>
		/// <param name="src"></param>
		public void Write(Bitstream src)
		{
			int bits = src._writePtr;
			{
				if (bits > 0)
					Write(src.fragments[0], System.Math.Min(64, System.Math.Max(0, bits)));
				else return;

				if (bits > 64)
					Write(src.fragments[1], System.Math.Min(64, System.Math.Max(0, bits - 64)));
				else return;

				if (bits > 128)
					Write(src.fragments[2], System.Math.Min(64, System.Math.Max(0, bits - 128)));
				else return;

				if (bits > 192)
					Write(src.fragments[3], System.Math.Min(64, System.Math.Max(0, bits - 192)));
				else return;

				if (bits > 256)
					Write(src.fragments[4], System.Math.Min(64, System.Math.Max(0, bits - 256)));
				else return;
			}
		}

		/// <summary>
		/// Writes the bits of src bitstream (from 0 to WritePtr) into this bitstream. NOTE this method is the BEST choice. Passing bitstream as a ref when possible
		/// avoids a 48 byte struct allocation.
		/// </summary>
		/// <param name="src"></param>
		public void Write(ref Bitstream src)
		{
			int bits = src._writePtr;
			fixed (ulong* int64Ptr = src.fragments)
			{
				if (bits > 0)
					Write(int64Ptr[0], System.Math.Min(64, System.Math.Max(0, bits)));
				else return;

				if (bits > 64)
					Write(int64Ptr[1], System.Math.Min(64, System.Math.Max(0, bits - 64)));
				else return;

				if (bits > 128)
					Write(int64Ptr[2], System.Math.Min(64, System.Math.Max(0, bits - 128)));
				else return;

				if (bits > 192)
					Write(int64Ptr[3], System.Math.Min(64, System.Math.Max(0, bits - 192)));
				else return;

				if (bits > 256)
					Write(int64Ptr[4], System.Math.Min(64, System.Math.Max(0, bits - 256)));
				else return;
			}
		}

		//public void Write(ref Bitstream src)
		//{
		//	int bits = src._writePtr;
		//	fixed (ulong* int64Ptr = src.fragments)
		//	{
		//		fixed (ulong* int4Ptr = fragments)
		//		{
		//			if (bits >= 64)



		//		else if (bits > 0)
		//			{
		//				Write(int64Ptr[0], System.Math.Min(64, System.Math.Max(0, bits)));
		//				return;
		//			}

		//			if (bits > 64)
		//				Write(int64Ptr[1], System.Math.Min(64, System.Math.Max(0, bits - 64)));
		//			else return;

		//			if (bits > 128)
		//				Write(int64Ptr[2], System.Math.Min(64, System.Math.Max(0, bits - 128)));
		//			else return;

		//			if (bits > 192)
		//				Write(int64Ptr[3], System.Math.Min(64, System.Math.Max(0, bits - 192)));
		//			else return;

		//			if (bits > 256)
		//				Write(int64Ptr[4], System.Math.Min(64, System.Math.Max(0, bits - 256)));
		//			else return;
		//		}
		//	}
		//}

		/// <summary>
		/// The primary write method. All other write methods lead to this one.
		/// </summary>
		/// <param name="value">Value to write to bitstream.</param>
		/// <param name="bits">Number of lower order bits to write.</param>
		public void Write(ulong value, int bits)
		{
			int index = _writePtr / 64;
			int offset = _writePtr % 64;
			int endpos = _writePtr + bits;
			int endindex = (endpos % 64 == 0) ? endpos / 64 - 1 : endpos / 64;

			_writePtr += bits;

			ulong offsetcomp = value << offset;

			bool overrun = endindex >= 5;

			System.Diagnostics.Debug.Assert(!overrun,
				"Bitstream write would exceed index limitation of 5 ulongs (40 bytes). Aborting write. Data corruption very likely.");

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

		[System.Obsolete("Confusing name. Use ReadOut() or Write()")]
		public void WriteBytes(byte[] target, ref int bitposition)
		{
			int remainingbits = _writePtr;
			int index = 0;
			while (remainingbits > 0)
			{
				ulong frag = this[index];
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

		//		public static byte[] Write(byte[] array, ulong buffer, int bits, ref int bitposition)
		//		{
		//			const int MAXBITS = 8;
		//			int offset = -(bitposition % MAXBITS);
		//			int index = bitposition / MAXBITS;
		//			int endpos = bitposition + bits;
		//			int endindex = (endpos % MAXBITS == 0) ? endpos / MAXBITS - 1 : endpos / MAXBITS;

		//			//DebugX.LogError("Buffer byte[] too is short. You are attempting a write to index " + endindex + ", but the byte[] buffer length is only " + buffer.Length + ".", endindex >= buffer.Length, true);
		//#if !DEVELOPMENT_BUILD
		//			if (endindex >= array.Length)
		//				System.Console.Error.Write("Buffer byte[] too is short.You are attempting a write to index " + endindex + ", but the byte[] buffer length is only " + array.Length + ".");
		//#endif

		//			// Offset both the mask and the compressed value using the remainder as the offset
		//			ulong mask = ulong.MaxValue >> (64 - bits);

		//			ulong offsetmask = mask << -offset;
		//			ulong offsetcomp = buffer << -offset;

		//			while (true)
		//			{
		//				// set the unwritten bits in byte[] to zero as a safety
		//				array[index] &= (byte)~offsetmask; // set bits to zero
		//				array[index] |= (byte)(offsetcomp & offsetmask);

		//				if (index == endindex)
		//					break;

		//				// Push the compressed value and the mask to align with the next array element
		//				offset += MAXBITS;
		//				offsetmask = mask >> offset;
		//				offsetcomp = buffer >> offset;
		//				index++;
		//			}

		//			bitposition += bits;
		//			return array;
		//}

		///// <summary>
		///// Write the contents of the this bitstream to the supplied byte[].
		///// </summary>
		///// <param name="array"></param>
		///// <param name="startBytePosition">Which byte of the target array to begin writing at.</param>
		///// <returns></returns>
		//public int WriteBytes(byte[] array, int startBytePosition = 0)
		//{
		//	int index = _writePtr / 64;
		//	int offset = _writePtr % 64;
		//	int endpos = _writePtr + bits;
		//	int endindex = (endpos % 64 == 0) ? endpos / 64 - 1 : endpos / 64;

		//	_writePtr += bits;

		//	ulong offsetcomp = value << offset;
		//	unsafe
		//	{
		//		fixed (ulong* int64Ptr = fragments)
		//		{
		//			int64Ptr[index] |= offsetcomp;

		//			if (index == endindex)
		//				return;

		//			offset = 64 - offset;
		//			offsetcomp = value >> (offset);

		//			int64Ptr[endindex] |= offsetcomp;
		//		}
		//	}
		//}

		public void WriteByte(byte value, int bits = 8) { Write(value, bits); }
		public void WriteSByte(sbyte value, int bits = 8) { Write((ulong)value, bits); }
		public void WriteUShort(ushort value, int bits = 16) { Write(value, bits); }
		public void WriteShort(short value, int bits = 16) { Write((ulong)value, bits); }
		public void WriteUInt(uint value, int bits = 32) { Write(value, bits); }
		public void WriteInt(int value, int bits = 32) { Write((ulong)value, bits); }
		public void WriteULong(ulong value, int bits = 64) { Write(value, bits); }

		public void WriteBool(bool value) { Write((value ? (ulong)1 : 0), 1); }
		public void Write(PackedValue pv) { Write(pv.uint64, pv.bits); }


		///// <summary>
		///// Write bitstream contents (rounded up to the nearest byte) UNET NetworkWriter.
		///// </summary>
		///// <param name="writer"></param>
		//[System.Obsolete("UnityEngine writers inside of Bitstream are deprecated, use the extension Write() methods instead.")]
		//public void Write(UnityEngine.Networking.NetworkWriter writer)
		//{
		//	// Write the packed bytes from the bitstream into the UNET writer.
		//	int count = BytesUsed;
		//	for (int i = 0; i < count; ++i)
		//	{
		//		writer.Write(ReadByte());
		//	}
		//}

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
			int offset = _readPtr % 64;
			int index = _readPtr / 64;
			int endpos = _readPtr + bits;
			int endindex = (endpos % 64 == 0) ? endpos / 64 - 1 : endpos / 64;
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
			while (remainingbits > 0)
			{
				ulong frag = this[index];
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
		/// Extracts out the 5 fixed ulong fragments that are backing the Bistream.
		/// </summary>
		/// <param name="fragment0"></param>
		/// <param name="fragment1"></param>
		/// <param name="fragment2"></param>
		/// <param name="fragment3"></param>
		/// <param name="fragment4"></param>
		public void ReadOut(ref ulong fragment0, ref ulong fragment1, ref ulong fragment2, ref ulong fragment3, ref ulong fragment4)
		{
			fixed (ulong* ulongPtr = fragments)
			{
					fragment0 = ulongPtr[0];
					fragment1 = ulongPtr[1];
					fragment2 = ulongPtr[2];
					fragment3 = ulongPtr[3];
					fragment4 = ulongPtr[4];
			}
		}
		/// <summary>
		/// Extracts out the first 4 fixed ulong fragments that are backing the Bistream.
		/// </summary>
		/// <param name="fragment0"></param>
		/// <param name="fragment1"></param>
		/// <param name="fragment2"></param>
		/// <param name="fragment3"></param>
		public void ReadOut(ref ulong fragment0, ref ulong fragment1, ref ulong fragment2, ref ulong fragment3)
		{
			fixed (ulong* ulongPtr = fragments)
			{
				fragment0 = ulongPtr[0];
				fragment1 = ulongPtr[1];
				fragment2 = ulongPtr[2];
				fragment3 = ulongPtr[3];
			}
		}
		/// <summary>
		/// Extracts out the first 3 fixed ulong fragments that are backing the Bistream.
		/// </summary>
		/// <param name="fragment0"></param>
		/// <param name="fragment1"></param>
		/// <param name="fragment2"></param>
		public void ReadOut(ref ulong fragment0, ref ulong fragment1, ref ulong fragment2)
		{
			fixed (ulong* ulongPtr = fragments)
			{
				fragment0 = ulongPtr[0];
				fragment1 = ulongPtr[1];
				fragment2 = ulongPtr[2];
			}
		}
		/// <summary>
		/// Extracts out the first 2 fixed ulong fragments that are backing the Bistream.
		/// </summary>
		/// <param name="fragment0"></param>
		/// <param name="fragment1"></param>
		public void ReadOut(ref ulong fragment0, ref ulong fragment1)
		{
			fixed (ulong* ulongPtr = fragments)
			{
				fragment0 = ulongPtr[0];
				fragment1 = ulongPtr[1];
			}
		}
		/// <summary>
		/// Extracts out the first fixed ulong fragment that are backing the Bistream.
		/// </summary>
		/// <param name="fragment0"></param>
		public void ReadOut(ref ulong fragment0)
		{
			fixed (ulong* ulongPtr = fragments)
			{
				fragment0 = ulongPtr[0];
			}
		}

		/// <summary>
		/// When possible, use Compare(ref Bitstream a, ref Bitstream b). Bitstream is a 40byte struct, so it is best to pass by ref when possible.
		/// </summary>
		/// <returns>true if the bitstreams match.</returns>
		public static bool Compare(Bitstream a, Bitstream b)
		{
			Bitstream aPtr = a;
			Bitstream bPtr = b;
			{
				if (aPtr[0] != bPtr[0] ||
					aPtr[1] != bPtr[1] ||
					aPtr[2] != bPtr[2] ||
					aPtr[3] != bPtr[3] ||
					aPtr[4] != bPtr[4])
					return false;

				return true;
			}
		}
		
		[System.Obsolete("Just use Equals().")]
		public static bool Compare(ref Bitstream a, ref Bitstream b)
		{

			fixed (ulong* aPtr = a.fragments)
			fixed (ulong* bPtr = b.fragments)
			{
				if (aPtr[0] != bPtr[0] ||
					aPtr[1] != bPtr[1] ||
					aPtr[2] != bPtr[2] ||
					aPtr[3] != bPtr[3] ||
					aPtr[4] != bPtr[4])
					return false;

				return true;
			}
		}

		public override string ToString()
		{
			return "CompositeBuffer <b>len:" + WritePtr + "</b> [frag0: " + this[0] + " frag1: " + this[1] + " frag2: " + this[2] + " frag3: " + this[3] + " frag4: " + this[4] + "]";
		}

		public override bool Equals(object obj)
		{
			return obj is Bitstream && Equals((Bitstream)obj);
		}

		public bool Equals(Bitstream other)
		{
			if (_writePtr == other._writePtr &&
				this[0] == other[0] &&
				this[1] == other[1] &&
				this[2] == other[2] &&
				this[3] == other[3] &&
				this[4] == other[4])
				return true;

			return false;
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
	}

	/// <summary>
	/// A simple wrapper for unsigned ints, which also contains how many bits that value is packed down to.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct PackedValue
	{
		[FieldOffset(0)]
		public System.Byte int8;
		[FieldOffset(0)]
		public System.SByte uint8;
		[FieldOffset(0)]
		public System.Int16 int16;
		[FieldOffset(0)]
		public System.UInt16 uint16;
		[FieldOffset(0)]
		public System.Int32 int32;
		[FieldOffset(0)]
		public System.UInt32 uint32;
		[FieldOffset(0)]
		public System.Int64 int64;
		[FieldOffset(0)]
		public System.UInt64 uint64;
		[FieldOffset(0)]
		public System.Single float32;
		[FieldOffset(0)]
		public System.Double float64;
		[FieldOffset(0)]
		public System.Boolean boolean;
		[FieldOffset(0)]
		public System.Char character;

		[FieldOffset(8)]
		public int bits;

		public PackedValue(System.Byte int8, int bits = 8) : this()
		{
			this.int8 = int8;
			this.bits = bits;
		}
		public PackedValue(System.SByte uint8, int bits = 8) : this()
		{
			this.uint8 = uint8;
			this.bits = bits;
		}
		public PackedValue(System.Int16 int16, int bits = 16) : this()
		{
			this.int16 = int16;
			this.bits = bits;
		}
		public PackedValue(System.UInt16 uint16, int bits = 16) : this()
		{
			this.uint16 = uint16;
			this.bits = bits;
		}
		public PackedValue(System.Int32 int32, int bits = 32) : this()
		{
			this.int32 = int32;
			this.bits = bits;
		}
		public PackedValue(System.UInt32 uint32, int bits = 32) : this()
		{
			this.uint32 = uint32;
			this.bits = bits;
		}
		public PackedValue(System.Int64 int64, int bits = 64) : this()
		{
			this.int64 = int64;
			this.bits = bits;
		}
		public PackedValue(System.UInt64 uint64, int bits = 64) : this()
		{
			this.uint64 = uint64;
			this.bits = bits;
		}
		public PackedValue(System.Single float32) : this()
		{
			this.float32 = float32;
			this.bits = 32;
		}
		public PackedValue(System.Double float64) : this()
		{
			this.float64 = float64;
			this.bits = 64;
		}
		public PackedValue(System.Boolean boolean) : this()
		{
			this.boolean = boolean;
			this.bits = 1;
		}
		public PackedValue(System.Char character, int bits = 16) : this()
		{
			this.character = character;
			this.bits = bits;
		}

		
		public static implicit operator System.Byte(PackedValue pv) { return pv.int8; }
		public static implicit operator System.SByte(PackedValue pv) { return pv.uint8; }

		public static implicit operator System.Int16(PackedValue pv) { return pv.int16; }
		public static implicit operator System.UInt16(PackedValue pv) { return pv.uint16; }

		public static implicit operator System.Int32(PackedValue pv) { return pv.int32; }
		public static implicit operator System.UInt32(PackedValue pv) { return pv.uint32; }

		public static implicit operator System.Int64(PackedValue pv) { return pv.int64; }
		public static implicit operator System.UInt64(PackedValue pv) { return pv.uint64; }

		public static implicit operator System.Single(PackedValue pv) { return pv.float32; }
		public static implicit operator System.Double(PackedValue pv) { return pv.float64; }

		public static implicit operator System.Boolean(PackedValue pv) { return pv.boolean; }
		public static implicit operator System.Char(PackedValue pv) { return pv.character; }

	}


	//public static class BitstreamExtensions
	//{
	//	/// <summary>
	//	/// Write the used bytes (based on the writer position) to the NetworkWriter.
	//	/// </summary>
	//	public static void Write(this UnityEngine.Networking.NetworkWriter writer, ref Bitstream bitstream)
	//	{
	//		// Write the packed bytes from the bitstream into the UNET writer.
	//		int count = bitstream.BytesUsed;
	//		for (int i = 0; i < count; ++i)
	//		{
	//			writer.Write(bitstream.ReadByte());
	//		}
	//	}

	//	public static void Read(this UnityEngine.Networking.NetworkReader reader, ref Bitstream bitstream)
	//	{
	//		// Copy the reader into our buffer so we can extra the packed bits. UNET uses a byte reader so we can't directly read bit fragments out of it.
	//		int count = System.Math.Min(40, reader.Length);
	//		for (int i = (int)reader.Position; i < count; ++i)
	//		{
	//			byte by = reader.ReadByte();
	//			bitstream.WriteByte(by);
	//		}
	//	}
	//}
}

