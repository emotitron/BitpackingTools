using System;
using UnityEngine;

namespace emotitron.Compression
{
	[Serializable]
	public abstract class LiteCrusher
	{
		[SerializeField] protected int bits;

		/// <summary>
		/// Returns the min number of bits required to describe any value between 0 and uint maxvalue
		/// </summary>
		public static int GetBitsForMaxValue(uint maxvalue)
		{
			for (int i = 0; i < 32; ++i)
				if (maxvalue >> i == 0)
					return i;
			return 32;
		}
	}

	[Serializable]
	public abstract class LiteCrusher<T> : LiteCrusher where T : struct
	{
		public abstract ulong Encode(T val);
		public abstract T Decode(uint val);

		public abstract ulong WriteValue(T val, byte[] buffer, ref int bitposition);
		public abstract void WriteCValue(uint val, byte[] buffer, ref int bitposition);
		public abstract T ReadValue(byte[] buffer, ref int bitposition);
	}
}
