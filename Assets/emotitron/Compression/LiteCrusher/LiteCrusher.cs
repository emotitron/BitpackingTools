using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using emotitron.Compression.Utilities;
#endif

namespace emotitron.Compression
{

	public enum LiteCompressLevel
	{
		Bits2 = 2, Bits3, Bits4, Bits5, Bits6, Bits7, Bits8, Bits9, Bits10, Bits12 = 12, Bits14 = 14,
		Half16 = 16, Full32 = 32
	}

	[Serializable]
	public class LiteCrusher
	{
		[SerializeField] private float min;
		[SerializeField] private float max;
		[SerializeField] private LiteCompressLevel bits;
		[SerializeField] private bool accurateCenter = true;

		[SerializeField] private float encoder;
		[SerializeField] private float decoder;
		[SerializeField] private ulong maxCVal;

		public ulong Encode(float val)
		{
			if (bits == LiteCompressLevel.Half16)
				return HalfFloat.HalfUtilities.Pack(val);

			else if (bits == LiteCompressLevel.Full32)
				return ((ByteConverter)val).uint32;

			/// Before casting check that the encoded float isn't negative.
			/// If negative, clamp to zero.
			float encval = ((val - min) * encoder + .5f);
			if (encval < 0)
				return 0;

			/// If positive, cast to int, and clamp to our max compresssed value.
			ulong cval = (ulong)encval;
			return (cval > maxCVal) ?  maxCVal : cval;
		}

		public float Decode(ulong cval)
		{
			if (bits == LiteCompressLevel.Half16)
				return HalfFloat.HalfUtilities.Unpack((ushort)cval);

			else if (bits == LiteCompressLevel.Full32)
				return ((ByteConverter)cval).float32;
			
			/// For the min and max cvals, just return the min max.
			/// Less work, and no sensless float errors.
			if (cval == 0)
				return min;

			if (cval == maxCVal)
				return max;

			return (cval * decoder) + min;
		}
	}

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(LiteCrusher))]
	[CanEditMultipleObjects]
	public class LiteCrusherDrawer : PropertyDrawer
	{
		public static GUIContent accCenterLabel = new GUIContent("Acc.Center", "Accurate Center reduces precision slightly to allow for an exact mid-value." +
			" Enable this when you need an value exactly between min and max to be lossless after compression. For example if you need 0 to be accurate when your range is -1 to 1.");
		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			SerializedProperty bits = property.FindPropertyRelative("bits");
			SerializedProperty min = property.FindPropertyRelative("min");
			SerializedProperty max = property.FindPropertyRelative("max");
			SerializedProperty ac = property.FindPropertyRelative("accurateCenter");

			float bitsWidth = 64;
			float accCenterLabelLeft = 80;
			float accCenterCheckWidth = 16;
			float stretchwidth = r.width - (bitsWidth + accCenterLabelLeft);
			float stretchleft = r.xMin + bitsWidth;

			float col1 = stretchleft + stretchwidth * .25f;
			float col2 = stretchleft + stretchwidth * .50f;
			float col3 = stretchleft + stretchwidth * .75f;
			float col4 = stretchleft + stretchwidth * .6f;
			float col5 = stretchleft + stretchwidth;
			//float col7 = r.xMin + r.width * .75f;
			Rect rbits = new Rect(r) { xMax = stretchleft };

			Rect r2 = new Rect(r) { xMin = stretchleft, xMax = col1 };
			Rect r3min = new Rect(r) { xMin = col1, xMax = col2 };
			Rect r4 = new Rect(r) { xMin = col2, xMax = col3 };
			Rect r5max = new Rect(r) { xMin = col3, xMax = col5 };

			Rect racl = new Rect(r) { xMin = r.xMax - accCenterLabelLeft, width = accCenterLabelLeft - accCenterCheckWidth };
			Rect racc = new Rect(r) { xMin = r.xMax - accCenterCheckWidth };

			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(rbits,  bits, GUIContent.none);
			EditorGUI.LabelField(r2, "min", (GUIStyle)"MiniLabelRight");
			EditorGUI.PropertyField(r3min, min, GUIContent.none);
			EditorGUI.LabelField(r4, "max", (GUIStyle)"MiniLabelRight");
			EditorGUI.PropertyField(r5max, max, GUIContent.none);

			EditorGUI.LabelField(racl, accCenterLabel, (GUIStyle)"MiniLabelRight");
			EditorGUI.PropertyField(racc, ac, GUIContent.none);

			if (EditorGUI.EndChangeCheck())
			{
				SerializedProperty encoder = property.FindPropertyRelative("encoder");
				SerializedProperty decoder = property.FindPropertyRelative("decoder");
				SerializedProperty maxCVal = property.FindPropertyRelative("maxCVal");
				Recalculate(bits.intValue, min.floatValue, max.floatValue, ac.boolValue, encoder, decoder, maxCVal);

				property.serializedObject.ApplyModifiedProperties();
			}

		}

		private void Recalculate(int bits, float min, float max, bool accurateCenter, 
			SerializedProperty encoder, SerializedProperty decoder, SerializedProperty maxCVal)
		{
			float range = max - min;
			ulong maxcval = (bits == 64) ? ulong.MaxValue : (((ulong)1 << (int)bits) - 1);

			if (accurateCenter && maxcval != 0)
				maxcval --;

			encoder.floatValue = range == 0 ? 0 : maxcval / range;
			decoder.floatValue = maxcval == 0 ? 0 : range / maxcval;

			maxCVal.longValue = (long)maxcval;
		}
	}

#endif

}
