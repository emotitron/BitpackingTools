using System;
using UnityEngine;
using emotitron.Compression.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Compression
{

	public enum LiteIntCompressType
	{
		PackSigned, PackUnsigned, Range
	}

	[Serializable]
	public class LiteIntCrusher : LiteCrusher<int>
	{
		[SerializeField] public LiteIntCompressType compressType;

		[SerializeField] protected int min;
		[SerializeField] protected int max;

		[SerializeField] private int smallest;
		[SerializeField] private int biggest;

		public LiteIntCrusher()
		{
			this.compressType = LiteIntCompressType.PackSigned;
			this.min = sbyte.MinValue;
			this.max = sbyte.MaxValue;

			if (compressType == LiteIntCompressType.Range)
				Recalculate(min, max, ref smallest, ref biggest, ref bits);
		}

		public LiteIntCrusher(LiteIntCompressType comType = LiteIntCompressType.PackSigned, int min = sbyte.MinValue, int max = sbyte.MaxValue)
		{
			this.compressType = comType;
			this.min = min;
			this.max = max;

			if (compressType == LiteIntCompressType.Range)
				Recalculate(min, max, ref smallest, ref biggest, ref bits);
		}

		public override ulong WriteValue(int val, byte[] buffer, ref int bitposition)
		{

			switch (compressType)
			{
				case LiteIntCompressType.PackUnsigned:
					{
						uint cval = (uint)val;
						buffer.WritePackedBytes(cval, ref bitposition, 32);
						return cval;
					}

				case LiteIntCompressType.PackSigned:
					{
						uint zigzag = (uint)((val << 1) ^ (val >> 31));
						buffer.WritePackedBytes(zigzag, ref bitposition, 32);
						return zigzag;
					}

				case LiteIntCompressType.Range:
					{
						ulong cval = Encode(val);
						buffer.Write(cval, ref bitposition, bits);
						return cval;
					}

				default:
					return 0;
			}
		}

		public override void WriteCValue(uint cval, byte[] buffer, ref int bitposition)
		{
			switch (compressType)
			{
				case LiteIntCompressType.PackUnsigned:
					{
						buffer.WritePackedBytes(cval, ref bitposition, 32);
						return;
					}

				case LiteIntCompressType.PackSigned:
					{
						buffer.WritePackedBytes(cval, ref bitposition, 32);
						return;
					}

				case LiteIntCompressType.Range:
					{
						buffer.Write(cval, ref bitposition, bits);
						return;
					}

				default:
					return;
			}
		}

		public override int ReadValue(byte[] buffer, ref int bitposition)
		{
			switch (compressType)
			{
				case LiteIntCompressType.PackUnsigned:
					return (int)buffer.ReadPackedBytes(ref bitposition, 32);

				case LiteIntCompressType.PackSigned:
					return buffer.ReadSignedPackedBytes(ref bitposition, 32);

				case LiteIntCompressType.Range:
					uint cval = (uint)buffer.Read(ref bitposition, bits);
					return Decode(cval);

				default:
					return 0;
			}
		}

		public override ulong Encode(int value)
		{
			value = (value > biggest) ? biggest : (value < smallest) ? smallest : value;
			return (ulong)(value - smallest);
		}

		public override int Decode(uint cvalue)
		{
			return (int)(cvalue + smallest);
		}

		public static void Recalculate(int min, int max, ref int smallest, ref int biggest, ref int bits)
		{
			int range;

			if (min < max)
			{
				smallest = min;
				biggest = max;
			}
			else
			{
				smallest = max;
				biggest = min;
			}

			range = biggest - smallest;
			bits = GetBitsForMaxValue((uint)range);
		}

		public override string ToString()
		{
			return GetType().Name + " " + compressType + " mn: " + min + " mx: " + max + " sm: " + smallest;
		}
	}

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(LiteIntCrusher))]
	[CanEditMultipleObjects]
	public class LiteIntCrusherDrawer : PropertyDrawer
	{
		public static GUIContent accCenterLabel = new GUIContent("center", "Accurate Center reduces precision slightly to allow for an exact mid-value." +
			" Enable this when you need an value exactly between min and max to be lossless after compression. For example if you need 0 to be accurate when your range is -1 to 1.");
		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			SerializedProperty compType = property.FindPropertyRelative("compressType");
			SerializedProperty min = property.FindPropertyRelative("min");
			SerializedProperty max = property.FindPropertyRelative("max");

			float compTypeWidth = 100;
			float stretchleft = r.xMin + compTypeWidth;
			float mLabelWidth = 26;

			Rect rectBits = new Rect(r) { xMax = stretchleft };

			Rect rectStretch = new Rect(r) { xMin = stretchleft, xMax = r.xMax };
			Rect rectMin = new Rect(rectStretch) { width = rectStretch.width * .5f };
			Rect rectMax = new Rect(rectStretch) { xMin = rectStretch.xMin + rectStretch.width * .5f };

			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(rectBits, compType, GUIContent.none);
			if (compType.intValue == (int)LiteIntCompressType.Range)
			{
				EditorGUI.LabelField(new Rect(rectMin) { width = mLabelWidth }, "min", new GUIStyle("MiniLabel") { alignment = TextAnchor.UpperRight });
				EditorGUI.PropertyField(new Rect(rectMin) { xMin = rectMin.xMin + mLabelWidth }, min, GUIContent.none);
				EditorGUI.LabelField(new Rect(rectMax) { width = mLabelWidth }, "max", new GUIStyle("MiniLabel") { alignment = TextAnchor.UpperRight });
				EditorGUI.PropertyField(new Rect(rectMax) { xMin = rectMax.xMin + mLabelWidth }, max, GUIContent.none);
			}

			if (EditorGUI.EndChangeCheck())
			{
				SerializedProperty smallest = property.FindPropertyRelative("smallest");
				SerializedProperty biggest = property.FindPropertyRelative("biggest");
				SerializedProperty bits = property.FindPropertyRelative("bits");

				/// We only modify compressor values if we are using range. Other settings use a hard coded 32 bits.
				if (compType.intValue == (int)LiteIntCompressType.Range)
				{
					int _smallest = 0, _biggest = 0, _bits = 0;
					LiteIntCrusher.Recalculate(min.intValue, max.intValue, ref _smallest, ref _biggest, ref _bits);
					smallest.intValue = _smallest;
					biggest.intValue = _biggest;
					bits.intValue = _bits;
				}

				property.serializedObject.ApplyModifiedProperties();
			}
		}
	}

#endif

}
