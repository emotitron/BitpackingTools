# BitpackingTools
Bitpacking/serialization libraries used interally for Unity Store <a href="https://assetstore.unity.com/packages/tools/network/network-sync-transform-nst-98453">NetworkSyncTransform</a> and <a href="https://assetstore.unity.com/packages/tools/network/transform-crusher-free-version-117313">TransformCrusher</a> Assets.

If you find these tools useful and would like to contribute, my paypal is davincarten@yahoo.com.

To make these extensions accessible add:
```using emotitron.Compression;```

## Summary
Bitpacking allows writing of values using only the number of bits needed, sequentially into a buffer. They are read out in the same order they were read in, and restored to their original type. 

Some examples of packed values:

|Value           |Range    |Bits  |Bit Range |
|----------------|---------|------|----------|
|Health          |0-100    |7     |0-127     |
|Armor           |0-250    |8     |0-255     |
|IsAlive         |0-1      |1     |0-1       |
|IsStunned       |0-1      |1     |0-1       |
|Selected Weapon |1-12     |4     |0-15      |
|Team            |0-4      |3     |0-7       |


The **ArraySerialize** extensions allow you to read/write directly to byte[] uint[] and ulong[] arrays without needing to wrap them in a Bitstream/Bitbuffer/Bitwriter. Unsafe options for arrays are included that allow you to pin an array prior to multiple read/write operations, which treats byte[] and uint[] as a ulong[] - allowing much faster reads/writes, especially for values >16 bits in length.

The **PrimitiveSerialize** extensions allow you to write bits to and from primitives (like ulong and uint). Useful for selectively packing multiple fields into one variable, for RPCs and Commands, and Syncvars.

## Primary Methods:
### Write
```cs
buffer.Write(value, ref int bitposition, int bits)
```
Writes the least significant ``bits`` of the ``value`` into the ``buffer`` starting at the ``bitposition``. The ``bitposition`` is incremented by ``bits``.

### Read
```cs
value = buffer.Read(ref int bitposition, int bits)
```
Returns a restored ``value`` by reading x ``bits`` from the ``buffer``starting at ``bitposition``. The ``bitposition`` is incremented by ``bits``.

## Alternative Methods
### Inject
```cs
value.Inject(buffer, ref int bitposition, int bits)
```
Same as _Write()_, but with a different order of arguments.
Writes the least significant ``bits`` of the ``value`` into the ``buffer`` starting at the ``bitposition``. The ``bitposition`` is incremented by ``bits``. Existing data past the write is preserved, allowing for non-linear injection of values.

### Append
```cs
buffer.Append(value, ref int bitposition, int bits)
```
Similar to _Write()_, this method Appends the least significant ``bits`` of the ``value`` to the ``buffer`` starting at the ``bitposition``. The ``bitposition`` is incremented by ``bits``. However, existing data past the bitposition is not preserved to increase the write speed. Use only for sequential writes, and use Write() and Inject() for non-linear insertions.

### Add
```cs
value.Add(buffer, ref int bitposition, int bits)
```
Same as _Append()_, but with a different order of arguments.
Appends the least significant ''bits'' of the ``value`` to the ``buffer`` starting at the ``bitposition``. The ``bitposition`` is incremented by ``bits``. Existing data past the bitposition is not preserved, so this is only mean for faster sequential writes.

### Peek
```cs
value = buffer.Peek(int bitposition, int bits)
```
Similar to _Read()_, returns a restored ``value`` by reading x ``bits`` from the ``buffer``starting at ``bitposition``. The ``bitposition`` is not incremented with Peek.

### Poke
```cs
value.Poke(buffer, int bitposition, int bits)
```
Similar to _Inject()_, writes x least signifigant ``bits`` of the ``value`` into the ``buffer`` at the ``bitposition``. The ``bitposition`` is not incremented with Poke.


## ArraySerializeExt class

The Array Serializer extension lets you bitpack directly to and from byte[], uint[] and ulong[] buffers. Because there is no wrapper class such as Bitstream, you need to maintain read/write pointers. The methods automatically increment the passer bitposition pointers for you.

### Basic Usage:
```cs
public unsafe void SafeArrayWrites()
{
	byte[] myBuffer = new byte[64];

	int writepos = 0;
	myBuffer.WriteBool(true, ref writepos);
	myBuffer.WriteSigned(-666, ref writepos, 11);
	myBuffer.Write(999, ref writepos, 10);

	int readpos = 0;
	bool restoredbool = myBuffer.ReadBool(ref readpos);
	int restoredval1 = myBuffer.ReadSigned(ref readpos, 11);
	uint restoredval2 = (uint)myBuffer.Read(ref readpos, 10);
}
```
### Advanced Usage (Unsafe)
For sequential writes and reads of a byte[] or uint[] arrays, there are unsafe methods that internally treat these arrays as a ulong[], resulting in up to 4x faster reads and writes. 
```cs
public unsafe void UnsafeArrayWrites()
{
	byte[] myBuffer = new byte[100];
	uint val1 = 666;
	int val2 = -999;

	// Pin the array before long sequences of reads or writes.
	fixed (byte* bPtr = myBuffer)
	{
		// Cast the byte* to ulong*
		ulong* uPtr = (ulong*)bPtr;

		int writepos = 0;
		ArraySerializeUnsafe.Write(uPtr, val1, ref writepos, 10);
		ArraySerializeUnsafe.WriteSigned(uPtr, val2, ref writepos, 11);

		int readpos = 0;
		uint restored1 = (uint)ArraySerializeUnsafe.Read(uPtr, ref readpos, 10);
		int restored2 = ArraySerializeUnsafe.ReadSigned(uPtr, ref readpos, 11);
	}
}
```

## PrimitiveSerializeExt class

The Primitive Serializer extension lets you bitpack directly to and from ulong, uint, ushort and byte primitives. Because there is no wrapper class such as Bitstream, you need to maintain read/write pointers. The methods automatically increment the pointer for you, as it is passed to the methods as a reference. NOTE: Extension methods cannot pass the first argument reference so the Write() return value must be applied to the buffer being written to.

### Basic Usage:
```cs
public unsafe void SafePrimitiveWrites()
{
	ulong myBuffer = 0;

	int writepos = 0;
	// Note that primitives are reference types, so the return value needs to be applied.
	myBuffer = myBuffer.WritetBool(true, ref writepos);
	myBuffer = myBuffer.WriteSigned(-666, ref writepos, 11);
	myBuffer = myBuffer.Write(999, ref writepos, 10);

	int readpos = 0;
	bool restoredbool = myBuffer.ReadBool(ref readpos);
	int restoredval1 = myBuffer.ReadSigned(ref readpos, 11);
	uint restoredval2 = (uint)myBuffer.Read(ref readpos, 10);
}
```

### Alternative Usage
An alternative to Write() is Inject(), which has the value being written as the first argument, allowing us to pass the buffer as a reference. NOTE: There is no return value on this method, as the buffer is passed by reference and is altered by the method.
```cs
 public unsafe void PrimitiveInjects()
{
	ulong myBuffer = 0;

	int writepos = 0;
	// Note that the buffer is passed by reference, and is altered by the method.
	true.Inject(ref myBuffer, ref writepos);
	(-666).InjectSigned(ref myBuffer, ref writepos, 11);
	999.InjectUnsigned(ref myBuffer, ref writepos, 10);
}
 ```
## Bitpacking Signed Values
Signed integer values (long, int, short and sbyte) all use the upper bit for the sign value. This is problematic for bitpacking since bitpacking trims off all of the unused bits on the left of values. To solve this a method called ZigZag can be employed, which simply moves the sign to the rightmost bit, and nudges all of the value bits to the left one position.

You can manually zigzag values prior to bitpacking and use the normal Write(), Append(), and Read() methods:
```cs
int original = -747;
uint shifted = original.ZigZag();
int restored = shifted.UnZigZag();
```
Or you can make use of the WriteSigned(), ReadSigned() methods that automatically handle the zigzagging.

## PackedBits and PackedBytes
For fields that have large potential ranges, but have values that hover at or near zero there are PackedBits and PackedBytes serialization options.

```cs
public unsafe void WritePackedBits()
{
	int holdpos, writepos = 0, readpos = 0;

	buffer.WriteSignedPackedBits(0, ref writepos, 32);
	buffer.WriteSignedPackedBits(-100, ref writepos, 32);
	buffer.WriteSignedPackedBits(int.MinValue, ref writepos, 32);
	buffer.WritePackedBits(ulong.MaxValue, ref writepos, 64);

	holdpos = readpos;
	int restored1 = buffer.ReadSignedPackedBits(ref readpos, 32);
	Debug.Log("ZERO = " + restored1 + " with " + (readpos - holdpos) + " written bits");

	holdpos = readpos;
	int restored2 = buffer.ReadSignedPackedBits(ref readpos, 32);
	Debug.Log("-100 = " + restored2 + " with " + (readpos - holdpos) + " written bits");

	holdpos = readpos;
	int restored3 = buffer.ReadSignedPackedBits(ref readpos, 32);
	Debug.Log("MIN = " + restored3 + " with " + (readpos - holdpos) + " written bits");

	holdpos = readpos;
	ulong restored4 = buffer.ReadPackedBits(ref readpos, 64);
	Debug.Log("MAX = " + restored4 + " with " + (readpos - holdpos) + " written bits");
}
```

This returns the results of 
```
ZERO = 0 with 6 written bits
-100 = -100 with 14 written bits
MIN = -2147483648 with 38 written bits
MAX = 18446744073709551615 with 71 written bits
```
For 32 bits of variable size, there is a 6 bit sizer added, making this less than ideal if the values will be large. However if the values often stay closer to zero, this can save quite a bit of space. Note that a value of zero only took 6 bits, and a value of -100 only took 14 bits.

### PackedBits
Values serialized using ``WritePackedBits()`` are checked for the position of the highest used significant bit. All zero bits on the left of the value are not serialized, and the value is preceded by a write of several bits for size info.

| sizer | bits | break even  |
|-------|------|-------------|
| 4     | 8    | 15          |
| 5     | 16   | 2047        |
| 6     | 32   | 67108863    |
| 7     | 64   | 1.44115E+17 |

### PackedBytes
PackedBytes work in a similar way to PackedBits, except rather than counting bits, it counts used bytes. Values serialized using ``WritePackedBytes()`` are checked for the position of the highest used significant bit, and that is rounded up the the nearest byte. All empty bytes on the left of the value are not serialized, and the value is preceded by a write of several bits for size info. The resulting compression is similar in size to Varints. The disadvantage of PackedBytes vs PackedBits is the rounding up to the nearest whole number of bytes, which may or may not be worth the reduced sizer size. It also has a lower threshold of where there is a savings.

| sizer | bits | break even  |
|-------|------|-------------|
| 1     | 8    | 0           |
| 2     | 16   | 255         |
| 3     | 32   | 16777215    |
| 4     | 64   | 7.20576E+16 |

### SignedPackedBits / SignedPackedBytes
Signed types (int, short and sbyte) are automatically zigzagged to move the sign bit from the msb position to the lsb position, keeping the pattern of "closer to zero, the smaller the write" true for negative numbers.
