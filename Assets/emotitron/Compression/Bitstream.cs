//*
//* The MIT License (MIT)
//* 
//* Copyright (c) 2018-2019 Davin Carten (emotitron) (davincarten@gmail.com)
//* 
//* Permission is hereby granted, free of charge, to any person obtaining a copy
//* of this software and associated documentation files (the "Software"), to deal
//* in the Software without restriction, including without limitation the rights
//* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//* copies of the Software, and to permit persons to whom the Software is
//* furnished to do so, subject to the following conditions:
//* 
//* The above copyright notice and this permission notice shall be included in
//* all copies or substantial portions of the Software.
//* 
//* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//* THE SOFTWARE.
//*/


using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace emotitron.Compression
{

	/// <summary>
	/// An unsafe-bitpacker (up to 1024 bytes) used for bitpacking. Contains methods for basic serialization.
	/// Supplied T is the basic IFixedBuffer wrapper for a fixed ulong[].
	/// </summary>
	public struct Bitstream
	{
		/// Roundabout way of getting the size of the fixedbuffer, but works and is fast access later.
		public int bufferByteCount;
		public int bufferULongCount;
		public int bufferBitCount;

		private int _writePtr;
		private int _readPtr;

		/// <summary>
		/// The exposed unsafe buffer.
		/// </summary>
		private ulong[] _buffer;

		public ulong[] Buffer {
			get { return _buffer; }
			set
			{
				bufferULongCount = value.Length;
				bufferByteCount = bufferULongCount << 3;
				bufferBitCount = bufferByteCount << 3;
				_buffer = value;
			}
		}

		#region Properties

		/// <summary>
		/// The current bit position for writes to this bitstream. The next write will begin at this bit.
		/// </summary>
		public int WritePtr { get { return _writePtr; } }
		/// <summary>
		/// The current bit position for reads from this bitstream. The next read will begin at this bit.
		/// </summary>
		public int ReadPtr { get { return _readPtr; } }
		/// <summary>
		/// Returns the rounded up number of bytes currently used, based on the position of the WritePtr.
		/// </summary>
		public int WritePtrBytes { get { return (_writePtr + 7) >> 3; } }
		/// <summary>
		/// Returns how many bytes have been read, rounded up to the nearest byte. If 0 bits have been read, this will be zero. If 9 bits have been read, this will be 2.
		/// </summary>
		public int ReadPtrBytes { get { return (_readPtr + 7) >> 3; } }
		/// <summary>
		/// Returns the rounded up number of ulongs currently used, based on the position of the WritePtr.
		/// </summary>
		public int WritePtrULongs { get { return (_writePtr + 63) >> 6; } }
		/// <summary>
		/// Returns how many bytes have been read, rounded up to the nearest ulong. If 0 bits have been read, this will be zero. If 9 bits have been read, this will be 2.
		/// </summary>
		public int ReadPtrULongs { get { return (_readPtr + 63) >> 6; } }
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

		#endregion

		public void Reset()
		{
			if (_writePtr > 0)
				{
				int cnt = (_writePtr + 63) >> 6;
				while (cnt-- > 0)

					_buffer[cnt] = 0;
			}
			_writePtr = 0;
			_readPtr = 0;
		}

		#region Constructors

		public unsafe Bitstream(ulong[] buffer) : this()
		{
			this.Buffer = buffer;
		}

		//Constructor
		public Bitstream(ulong[] buffer, ulong fragment0, ulong fragment1 = 0, ulong fragment2 = 0, ulong fragment3 = 0, ulong fragment4 = 0) : this()
		{
			this.Buffer = buffer;

			_buffer[0] = fragment0;
			_buffer[1] = fragment1;
			_buffer[2] = fragment2;
			_buffer[3] = fragment3;
			_buffer[4] = fragment4;

			_writePtr = 40 * 8;
		}


		// Constructor
		public unsafe Bitstream(ulong[] buffer, byte[] bytes) : this()
		{
			this.Buffer = buffer;

			int count = bytes.Length;
			_writePtr = count * 8;

			fixed (ulong* ulongPtr = _buffer)
			{
				byte* arrayPtr = (byte*)ulongPtr;
				for (int i = 0; i < count; ++i)
					arrayPtr[i] = bytes[i];
			}
		}

		// Constructor
		public unsafe Bitstream(ulong[] buffer, byte[] bytes, int count) : this()
		{
			this.Buffer = buffer;

			_writePtr = count * 8;

			fixed (ulong* ulongPtr = _buffer)
			{
				byte* arrayPtr = (byte*)ulongPtr;
				for (int i = 0; i < count; ++i)
					arrayPtr[i] = bytes[i];
			}
		}

		// Constructor
		public unsafe Bitstream(ulong[] buffer, ushort[] array) : this()
		{
			this.Buffer = buffer;

			int count = array.Length;
			_writePtr = count * 16;

			fixed (ulong* ulongPtr = _buffer)
			{
				ushort* arrayPtr = (ushort*)ulongPtr;
				for (int i = 0; i < count; ++i)
					arrayPtr[i] = array[i];
			}
		}

		// Constructor
		public unsafe Bitstream(ulong[] buffer, ushort[] array, int count) : this()
		{
			this.Buffer = buffer;

			_writePtr = count * 16;

			fixed (ulong* ulongPtr = _buffer)
			{
				ushort* arrayPtr = (ushort*)ulongPtr;
				for (int i = 0; i < count; ++i)
					arrayPtr[i] = array[i];
			}
		}

		// Constructor
		public unsafe Bitstream(ulong[] buffer, uint[] array) : this()
		{
			this.Buffer = buffer;

			int count = array.Length;
			_writePtr = count * 32;

			fixed (ulong* ulongPtr = _buffer)
			{
				uint* arrayPtr = (uint*)ulongPtr;
				for (int i = 0; i < count; ++i)
					arrayPtr[i] = array[i];
			}
		}

		// Constructor
		public unsafe Bitstream(ulong[] buffer, uint[] array, int count) : this()
		{
			this.Buffer = buffer;

			_writePtr = count * 32;

			fixed (ulong* ulongPtr = _buffer)
			{
				uint* arrayPtr = (uint*)ulongPtr;
				for (int i = 0; i < count; ++i)
					arrayPtr[i] = array[i];
			}
		}

		#endregion

		/// <summary>
		/// Returns how many bits of the fragment index are used given a total bit count
		/// </summary>
		/// <param name="fragment">Index of the ulong fragment in question</param>
		/// <param name="totalbits"></param>
		/// <returns></returns>
		public static int BitsUsedByFragment(int fragment, int totalbits)
		{
			return System.Math.Max(0, System.Math.Min(64, totalbits - fragment * 64));
		}

		#region Array Indexers

		/// <summary>
		/// Unlong Indexer.
		/// </summary>
		/// <param name="i"></param>
		/// <returns>Returns the UInt64 value of the index fragment.</returns>
		public ulong this[int i]
		{
			get
			{
				if (i < bufferULongCount)
					return _buffer[i];
				else
					return 0;
			}
			set
			{
				if (i < bufferULongCount)
					_buffer[i] = value;
				else
					Debug.LogError("Attempt to access invalid index.");
			}
		}

		/// <summary>
		/// Read out the byte[] index equivalent. Interally the bitstream is Fixed ulong[].
		/// </summary>
		/// <param name="arrayIndex"></param>
		/// <returns></returns>
		public unsafe byte GetByte(int arrayIndex)
		{
			fixed (ulong* ulongPtr = _buffer)
			{
				return ((byte*)ulongPtr)[arrayIndex];
			}
		}
		/// <summary>
		/// Read out the ushort[] index equivalent. Interally the bitstream is Fixed ulong[].
		/// </summary>
		/// <param name="arrayIndex"></param>
		/// <returns></returns>
		public unsafe ushort GetUInt16(int arrayIndex)
		{
			fixed (ulong* ulongPtr = _buffer)
			{
				return ((ushort*)ulongPtr)[arrayIndex];
			}
		}
		/// <summary>
		/// Read out the uint[] index equivalent. Interally the bitstream is Fixed ulong[].
		/// </summary>
		/// <param name="arrayIndex"></param>
		/// <returns></returns>
		public unsafe uint GetUInt32(int arrayIndex)
		{
			fixed (ulong* ulongPtr = _buffer)
			{
				return ((uint*)ulongPtr)[arrayIndex];
			}
		}
		/// <summary>
		/// Read out the ulong[] index equivalent. Interally the bitstream is Fixed ulong[].
		/// </summary>
		/// <param name="arrayIndex"></param>
		/// <returns></returns>
		public ulong GetUint64(int arrayIndex)
		{
			return _buffer[arrayIndex];
		}

		#endregion

		/// <summary>
		/// Writes the bits of src bitstream (from 0 to WritePtr) into this bitstream. NOTE this method is the BEST choice. Passing bitstream as a ref when possible
		/// avoids a 48 byte struct allocation.
		/// </summary>
		/// <param name="src"></param>
		public unsafe void Write(ref Bitstream src)
		{
			int bits = src._writePtr;
			int pos = 0;

			fixed (ulong* int64Ptr = src.Buffer)
			{
				for (int i = 0; i < bufferULongCount; i++)
				{
					if (bits > pos)
						Write(int64Ptr[i], System.Math.Min(64, System.Math.Max(0, bits - pos)));
					else return;
					pos += 64;
				}
			}
		}

		#region Primary Read/Write Methods

		private const string bufferoverrunerr = "Bitstream write would exceed index limitation of 5 ulongs (40 bytes). Aborting write. Data corruption very likely.";
		/// <summary>
		/// The primary write method. All other write methods lead to this one.
		/// </summary>
		/// <param name="value">Value to write to bitstream.</param>
		/// <param name="bits">Number of lower order bits to write.</param>
		public void Write(ulong value, int bits)
		{
			const int MAXBITS = 64;
			const int MODULUS = MAXBITS - 1;
			int offset = -(_writePtr & MODULUS); // this is just a modulus
			int index = _writePtr >> 6;
			int endpos = _writePtr + bits;
			int endindex = ((endpos - 1) >> 6);


			ulong offsetval = value << offset;

			bool overrun = endpos > bufferBitCount;

			if (overrun)
			{
				Debug.LogError("Attempted write past end of unsafe buffer. Returning.");
				return;
			}
			_writePtr += bits;

			_buffer[index] |= offsetval;

			if (index == endindex)
				return;

			offset = MAXBITS - offset;
			offsetval = value >> (offset);

			_buffer[endindex] |= offsetval;
		}

		/// <summary>
		/// Primary Write method, that masks out the write to properly overwrite existing data, 
		/// and masks the incoming value so out of range numbers don't corruped neighboring writes. Slower than Write.
		/// Only advances the writePtr if the result is greater then current pos. Useful for modifying previous writes.
		/// </summary>
		public void Overwrite(ulong value, int bits)
		{
			const int MAXBITS = 64;
			const int MODULUS = MAXBITS - 1;
			int offset = -(_writePtr & MODULUS); // this is just a modulus
			int index = _writePtr >> 6;
			int endpos = _writePtr + bits;
			int endindex = ((endpos - 1)  >> 6);

			ulong mask = ulong.MaxValue >> (64 - bits);

			ulong offsetmask = mask << -offset;
			ulong offsetval = value << -offset;

			//bool overrun = endpos > bufferBitCount;

			if (endpos > _writePtr)
				_writePtr = endpos;

			while (true)
			{
				// set the unwritten bits in byte[] to zero as a safety
				_buffer[index] &= ~offsetmask; // set bits to zero
				_buffer[index] |= (offsetval & offsetmask);

				if (index == endindex)
					break;

				// Push the compressed value and the mask to align with this array element
				offset += MAXBITS;
				offsetmask = mask >> offset;
				offsetval = value >> offset;
				index++;
			}
			_writePtr = endpos;
		}

		public unsafe ulong Read(int bits)
		{
			const int MAXBITS = 64;
			const int MODULUS = MAXBITS - 1;
			int offset = -(_writePtr & MODULUS); // this is just a modulus
			int index = _readPtr / MAXBITS;
			int endpos = _readPtr + bits;
			int endindex = ((endpos - 1) / MAXBITS);

			if (endpos > bufferBitCount)
			{
				Debug.LogError("Attempted read past unsafe buffer end. Returning.");
				return 0;
			}

			_readPtr += bits;

			ulong mask = ulong.MaxValue >> (64 - bits);
			ulong line;

			ulong value = 0;

			line = (_buffer[index] >> offset);

			value |= (line & mask);

			// End here if this value doesn't span two ulongs
			if (index == endindex)
				return value;

			offset = 64 - offset;

			line = (_buffer[endindex] << offset);

			value |= (line & mask);
			return value;
		}


		#endregion

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
		public void WriteUShort(ushort value, int bits = 16) { Write(value, bits); }
		public void WriteUInt(uint value, int bits = 32) { Write(value, bits); }
		public void WriteULong(ulong value, int bits = 64) { Write(value, bits); }
		public void WriteBool(bool value) { Write((value ? (ulong)1 : 0), 1); }

		public byte ReadByte(int bits = 8) { return (byte)Read(bits); }
		public ushort ReadUShort(int bits = 16) { return (ushort)Read(bits); }
		public uint ReadUint32(int bits = 32) { return (uint)Read(bits); }
		public ulong ReadUInt64(int bits = 64) { return Read(bits); }
		public bool ReadBool() { return (Read(1) == 1); }

		public void WriteSigned(int value, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			Write(zigzag, bits);
		}
		public int ReadSigned(int bits)
		{
			uint value = (uint)Read(bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
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

			//fixed (ulong* ulongPtr = _buffer)
			{
				while (remainingbits > 0)
				{
					ulong frag = _buffer[index];
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
		public static bool Compare(ref Bitstream a, ref Bitstream b)
		{
			if (a._writePtr != b._writePtr)
				return false;

			//fixed (ulong* aPtr = a.Buffer)
			//fixed (ulong* bPtr = b.Buffer)
			ulong[] abuff = a.Buffer;
			ulong[] bbuff = b.Buffer;
			{
				for (int i = 0; i < a.WritePtrULongs; ++i)
					if (abuff[0] != bbuff[0])
						return false;

				return true;
			}
		}

		public bool Equals(ref Bitstream other)
		{
			if (_writePtr != other._writePtr)
				return false;

			//fixed (ulong* aPtr = _buffer)
			//fixed (ulong* bPtr = other.Buffer)
			ulong[] otherbuff = other.Buffer;
			{
				for (int i = 0; i < WritePtrULongs; ++i)
					if (_buffer[0] != otherbuff[0])
						return false;

				return true;
			}
		}

		public override int GetHashCode()
		{
			var hashCode = 298898743;
			hashCode = hashCode * -1521134295 + _writePtr.GetHashCode();

			for (int i = 0; i < bufferULongCount; ++i)
				hashCode = hashCode * -1521134295 + _buffer[i].GetHashCode();

			return hashCode;
		}

#if UNITY_EDITOR
		private static System.Text.StringBuilder sb = new System.Text.StringBuilder();

		public override string ToString()
		{
			sb.Length = 0;
			sb.Append("Bitstream:");
			sb.Append(bufferULongCount);
			{
				for (int i = 0; i < bufferULongCount; ++i)
				{
					sb.Append(" ");
					sb.Append(i);
					sb.Append(":");
					sb.Append(_buffer[i]);
				}
			}
			return sb.ToString();
		}
#endif

	}
}

