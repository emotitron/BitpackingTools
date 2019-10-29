using System;
using UnityEngine;
using emotitron.Compression.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace emotitron.Compression
{

	public enum LiteFloatCompressType
	{
		Bits2 = 2, Bits3, Bits4, Bits5, Bits6, Bits7, Bits8, Bits9, Bits10, Bits12 = 12, Bits14 = 14,
		Half16 = 16, Full32 = 32
	}

	[Serializable]
	public class LiteFloatCrusher : LiteCrusher<float>
	{
		[SerializeField] protected float min;
		[SerializeField] protected float max;

		[SerializeField] public LiteFloatCompressType compressType = LiteFloatCompressType.Half16;
		[SerializeField] private bool accurateCenter = true;

		[SerializeField] private float encoder;
		[SerializeField] private float decoder;
		[SerializeField] private ulong maxCVal;

		public LiteFloatCrusher()
		{
			this.compressType = LiteFloatCompressType.Half16;
			this.min = 0;
			this.max = 1;
			this.accurateCenter = true;

			Recalculate(compressType, min, max, accurateCenter, ref bits, ref encoder, ref decoder, ref maxCVal);
		}

		public LiteFloatCrusher(LiteFloatCompressType compressType, float min, float max, bool accurateCenter)
		{
			this.compressType = compressType;
			this.min = min;
			this.max = max;
			this.accurateCenter = accurateCenter;

			Recalculate(compressType, min, max, accurateCenter, ref bits, ref encoder, ref decoder, ref maxCVal);
		}

		public static void Recalculate(LiteFloatCompressType compressType, float min, float max, bool accurateCenter,
			ref int bits, ref float encoder, ref float decoder, ref ulong maxCVal)
		{
			bits = (int)compressType;

			float range = max - min;
			ulong maxcval = (bits == 64) ? ulong.MaxValue : (((ulong)1 << (int)bits) - 1);

			if (accurateCenter && maxcval != 0)
				maxcval--;

			encoder = range == 0 ? 0 : maxcval / range;
			decoder = maxcval == 0 ? 0 : range / maxcval;

			maxCVal = maxcval;
		}

		public override ulong Encode(float val)
		{
			if (compressType == LiteFloatCompressType.Half16)
				return HalfFloat.HalfUtilities.Pack(val);

			else if (compressType == LiteFloatCompressType.Full32)
				return ((ByteConverter)val).uint32;

			/// Before casting check that the encoded float isn't negative.
			/// If negative, clamp to zero. .5f is used to round up 50% of of a quantized value.
			float encval = ((val - min) * encoder + .5f);
			if (encval < 0)
				return 0;

			/// If positive, cast to int, and clamp to our max compresssed value.
			ulong cval = (ulong)encval;
			return (cval > maxCVal) ? maxCVal : cval;
		}

		public override float Decode(uint cval)
		{
			if (compressType == LiteFloatCompressType.Half16)
				return HalfFloat.HalfUtilities.Unpack((ushort)cval);

			else if (compressType == LiteFloatCompressType.Full32)
				return ((ByteConverter)cval).float32;

			/// For the min and max cvals, just return the min max.
			/// Less work, and no sensless float errors.
			if (cval == 0)
				return min;

			if (cval == maxCVal)
				return max;

			return (cval * decoder) + min;
		}

		public override ulong WriteValue(float val, byte[] buffer, ref int bitposition)
		{
			if (compressType == LiteFloatCompressType.Half16)
			{
				ulong cval = HalfFloat.HalfUtilities.Pack(val);
				buffer.Write(cval, ref bitposition, 16);
				return cval;
			}

			else if (compressType == LiteFloatCompressType.Full32)
			{
				ulong cval = ((ByteConverter)val).uint32;
				buffer.Write(cval, ref bitposition, 32);
				return cval;
			}

			else
			{
				ulong cval = Encode(val);
				buffer.Write(cval, ref bitposition, (int)compressType);
				return cval;
			}
		}

		public override void WriteCValue(uint cval, byte[] buffer, ref int bitposition)
		{
			if (compressType == LiteFloatCompressType.Half16)
			{
				buffer.Write(cval, ref bitposition, 16);
			}

			else if (compressType == LiteFloatCompressType.Full32)
			{
				buffer.Write(cval, ref bitposition, 32);
			}
			else
			{
				buffer.Write(cval, ref bitposition, (int)compressType);
			}
		}

		public override float ReadValue(byte[] buffer, ref int bitposition)
		{
			if (compressType == LiteFloatCompressType.Half16)
			{
				ushort cval = (ushort)buffer.Read(ref bitposition, 16);
				return HalfFloat.HalfUtilities.Unpack(cval);
			}

			else if (compressType == LiteFloatCompressType.Full32)
			{
				ByteConverter cval = (uint)buffer.Read(ref bitposition, 32);
				return cval.float32;
			}

			else
			{
				uint cval = (uint)buffer.Read(ref bitposition, (int)compressType);
				return Decode(cval);
			}

		}

		public override string ToString()
		{
			return GetType().Name + " " + compressType + " mn: " + min + " mx: " + max + " e: " + encoder + " d: " + decoder;
		}
	}

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(LiteFloatCrusher))]
	[CanEditMultipleObjects]
	public class LiteFloatCrusherDrawer : PropertyDrawer
	{
		public static GUIContent accCenterLabel = new GUIContent("center", "Accurate Center reduces precision slightly to allow for an exact mid-value." +
			" Enable this when you need an value exactly between min and max to be lossless after compression. For example if you need 0 to be accurate when your range is -1 to 1.");
		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			SerializedProperty compressType = property.FindPropertyRelative("compressType");
			SerializedProperty min = property.FindPropertyRelative("min");
			SerializedProperty max = property.FindPropertyRelative("max");
			SerializedProperty ac = property.FindPropertyRelative("accurateCenter");

			float bitsWidth = 60;
			float accCenterLeft = 52;
			float accCenterCheckWidth = 14;
			float stretchleft = r.xMin + bitsWidth;
			float mLabelWidth = 26;

			Rect rectBits = new Rect(r) { xMax = stretchleft };

			Rect rectStretch = new Rect(r) { xMin = stretchleft, xMax = r.xMax - accCenterLeft };
			Rect rectMin = new Rect(rectStretch) { width = rectStretch.width * .5f };
			Rect rectMax = new Rect(rectStretch) { xMin = rectStretch.xMin + rectStretch.width * .5f };

			Rect racl = new Rect(r) { xMin = r.xMax - accCenterLeft, width = accCenterLeft - accCenterCheckWidth };
			Rect racc = new Rect(r) { xMin = r.xMax - accCenterCheckWidth };

			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(rectBits, compressType, GUIContent.none);
			if (compressType.intValue != (int)LiteFloatCompressType.Half16 && compressType.intValue != (int)LiteFloatCompressType.Full32)
			{
				EditorGUI.LabelField(new Rect(rectMin) { width = mLabelWidth }, "min", new GUIStyle("MiniLabel") { alignment = TextAnchor.UpperRight });
				EditorGUI.PropertyField(new Rect(rectMin) { xMin = rectMin.xMin + mLabelWidth }, min, GUIContent.none);
				EditorGUI.LabelField(new Rect(rectMax) { width = mLabelWidth }, "max", new GUIStyle("MiniLabel") { alignment = TextAnchor.UpperRight });
				EditorGUI.PropertyField(new Rect(rectMax) { xMin = rectMax.xMin + mLabelWidth }, max, GUIContent.none);

				EditorGUI.LabelField(racl, accCenterLabel, new GUIStyle("MiniLabel") { alignment = TextAnchor.UpperRight });
				ac.boolValue = EditorGUI.Toggle(racc, GUIContent.none, ac.boolValue, (GUIStyle)"OL Toggle");
			}

			if (EditorGUI.EndChangeCheck())
			{
				SerializedProperty encoder = property.FindPropertyRelative("encoder");
				SerializedProperty decoder = property.FindPropertyRelative("decoder");
				SerializedProperty maxCVal = property.FindPropertyRelative("maxCVal");
				SerializedProperty bits = property.FindPropertyRelative("bits");

				float _encoder = 0, _decoder = 0;
				int _bits = 0;
				ulong _maxCVal = 0;
				LiteFloatCrusher.Recalculate((LiteFloatCompressType)compressType.intValue, min.floatValue, max.floatValue, ac.boolValue, ref _bits, ref _encoder, ref _decoder, ref _maxCVal);

				encoder.floatValue = _encoder;
				decoder.floatValue = _decoder;
				bits.intValue = _bits;
				maxCVal.longValue = (long)_maxCVal;

				property.serializedObject.ApplyModifiedProperties();
			}
		}
	}

#endif

}
