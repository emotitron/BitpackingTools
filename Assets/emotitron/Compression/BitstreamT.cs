using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace emotitron.Compression
{
	public unsafe interface IFixedBuffer
	{

	};

	public unsafe struct Buffer1024 : IFixedBuffer
	{
		public fixed ulong fragments[128];
	}

	public unsafe struct Buffer40 : IFixedBuffer
	{
		public fixed ulong fragments[5];
	}


	/// <summary>
	/// An unsafe-bitpacker (up to 1024 bytes) used for bitpacking. Contains methods for basic serialization.
	/// Supplied T is the basic IFixedBuffer wrapper for a fixed ulong[].
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct Bitstream<T> where T : IFixedBuffer
	{
		/// Roundabout way of getting the size of the fixedbuffer, but works and is fast access later.
		public static int BufferByteCount = Marshal.SizeOf(typeof(T));
		public static int BufferULongCount = BufferByteCount / 8;
		public static int BufferBitCount = BufferByteCount * 8;

		[FieldOffset(0)]
		private int _writePtr;
		[FieldOffset(4)]
		private int _readPtr;

		/// <summary>
		/// The backing unsafe fixed array that dictates the actual size.
		/// </summary>
		[FieldOffset(8)]
		private readonly T _fixedBuffer;

		/// <summary>
		/// The exposed unsafe buffer.
		/// </summary>
		[FieldOffset(8)]
		private fixed ulong fragments[128];

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

		/// <summary>
		/// Reset the bitstream to an empty state.
		/// </summary>
		public void Reset()
		{
			if (_writePtr > 0)
				fixed (ulong* ulongPtr = fragments)
				{
					int cnt = (_writePtr + 7) >> 3;
					for (int i = 0; i < cnt; ++i)
						ulongPtr[i] = 0;
				}
			_writePtr = 0;
			_readPtr = 0;
		}

		#region Constructors

		//Constructor
		public unsafe Bitstream(ulong fragment0, ulong fragment1 = 0, ulong fragment2 = 0, ulong fragment3 = 0, ulong fragment4 = 0) : this()
		{
			fixed (ulong* ulongPtr = fragments)
			{
				ulongPtr[0] = fragment0;
				ulongPtr[1] = fragment1;
				ulongPtr[2] = fragment2;
				ulongPtr[3] = fragment3;
				ulongPtr[4] = fragment4;
			}
			_writePtr = 40 * 8;
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
		public unsafe ulong this[int i]
		{
			get
			{
				if (i < BufferULongCount)
					fixed (ulong* ulongPtr = fragments)
						return ulongPtr[i];
				else
					return 0;
			}
			set
			{
				if (i < BufferULongCount)
					fixed (ulong* ulongPtr = fragments)
						ulongPtr[i] = value;
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
			fixed (ulong* ulongPtr = fragments)
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
			fixed (ulong* ulongPtr = fragments)
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
			fixed (ulong* ulongPtr = fragments)
			{
				return ((uint*)ulongPtr)[arrayIndex];
			}
		}
		/// <summary>
		/// Read out the ulong[] index equivalent. Interally the bitstream is Fixed ulong[].
		/// </summary>
		/// <param name="arrayIndex"></param>
		/// <returns></returns>
		public unsafe ulong GetUint64(int arrayIndex)
		{
			fixed (ulong* ulongPtr = fragments)
				return ulongPtr[arrayIndex];
		}

		#endregion

		/// <summary>
		/// Writes the bits of src bitstream (from 0 to WritePtr) into this bitstream. NOTE this method is the BEST choice. Passing bitstream as a ref when possible
		/// avoids a 48 byte struct allocation.
		/// </summary>
		/// <param name="src"></param>
		public void Write(ref Bitstream<T> src)
		{
			int bits = src._writePtr;
			int pos = 0;

			fixed (ulong* int64Ptr = src.fragments)
			{
				for (int i = 0; i < BufferULongCount; i++)
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
			int index = _writePtr >> 6;
			int offset = _writePtr % 64;
			int endpos = _writePtr + bits;
			int endindex = ((endpos - 1) >> 6);

			_writePtr += bits;

			ulong offsetcomp = value << offset;

			bool overrun = endpos > BufferBitCount;

			//System.Diagnostics.Debug.Assert(!overrun, bufferoverrunerr);

			if (overrun)
			{
				Debug.LogError("Attempted write past end of unsafe buffer. Returning.");
				return;
			}

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

			if (endpos > BufferBitCount)
			{
				Debug.LogError("Attempted read past unsafe buffer end. Returning.");
				return 0;
			}

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
		public static bool Compare(ref Bitstream<T> a, ref Bitstream<T> b)
		{
			if (a._writePtr != b._writePtr)
				return false;

			fixed (ulong* aPtr = a.fragments)
			fixed (ulong* bPtr = b.fragments)
			{
				for (int i = 0; i < BufferULongCount; ++i)
					if (aPtr[0] != bPtr[0])
						return false;

				return true;
			}
		}

		public bool Equals(ref Bitstream<T> other)
		{
			if (_writePtr != other._writePtr)
				return false;

			fixed (ulong* aPtr = fragments)
			fixed (ulong* bPtr = other.fragments)
			{
				for (int i = 0; i < BufferULongCount; ++i)
					if (aPtr[0] != bPtr[0])
						return false;

				return true;
			}
		}

		public override int GetHashCode()
		{
			var hashCode = 298898743;
			hashCode = hashCode * -1521134295 + _writePtr.GetHashCode();
			fixed (ulong* Ptr = fragments)
			{
				for (int i = 0; i < BufferULongCount; ++i)
					hashCode = hashCode * -1521134295 + Ptr[i].GetHashCode();
			}

			return hashCode;
		}

#if UNITY_EDITOR
		private static System.Text.StringBuilder sb = new System.Text.StringBuilder();

		public override string ToString()
		{
			sb.Length = 0;
			sb.Append("Bitstream:");
			sb.Append(BufferULongCount);
			fixed (ulong* ptr = fragments)
			{
				for (int i = 0; i < BufferULongCount; ++i)
				{
					sb.Append(" ");
					sb.Append(i);
					sb.Append(":");
					sb.Append(ptr[i]);
				}
			}
			return sb.ToString();
		}
#endif

	}
}

