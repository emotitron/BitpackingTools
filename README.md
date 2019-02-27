# BitpackingTools
Bitpacking/serialization libraries used interally for Unity Store <a href="https://assetstore.unity.com/packages/tools/network/network-sync-transform-nst-98453">NetworkSyncTransform</a> and <a href="https://assetstore.unity.com/packages/tools/network/transform-crusher-free-version-117313">TransformCrusher</a> Assets 

To make these extensions accessible add:
```using emotitron.Compression;```

## ArraySerializeExt class

The Array Serializer extension lets you bitpack directly to and from byte[], uint[] and ulong[] buffers. Because there is no wrapper class such as Bitstream, you need to maintain read/write pointers. The methods automatically increment this pointers for you however.

### Basic Usage:
```cs
  byte[] myBuffer = new byte[64];
  
  int writepos = 0;
  myBuffer.WriteBool(true, ref writepos);
  myBuffer.Write(666, ref writepos, 10);
  myBuffer.Write(999, ref writepos, 10);
  
  int readpos = 0;
  bool restoredbool = myBuffer.ReadBool(ref readpos);
  int restoredval1 = myBuffer.Read(ref readpos, 10);
  int restoredval2 = myBuffer.Read(ref readpos, 10);
```
### Advanced Usage (Unsafe)
For sequential writes and reads of a byte[] or uint[] arrays, there are unsafe methods that internally treat these arrays as a ulong[], resulting in up to 4x faster reads and writes. These are all contained in ArraySerializerUnsafe.cs, which can be deleted for projects where you don't want to enable Allow Unsafe Code.
```cs
byte[] myBuffer = byte[100];
uint val1 = 666;
int val2 = -999;

fixed (byte* bPtr = myBuffer)
{
  // Cast the byte* to ulong*
  ulong* uPtr = (ulong*)bPtr;
  
  int writepos = 0;
  // Two different write methods. Inject() and Write(). 
  // Both are about the same speed, so it's up to personal preference.
  val1.InjectUnsafe(uPtr, ref writepos, 10);
  ArraySerializeUnsafe.WriteSigned(uPtr, val2, ref writepos, 11);

  readpos = 0;
  // Unsafe pointers can't be the first argument of extensions, so there is no pretty way to do this.
  uint restored1 = (uint)ArraySerializeUnsafe.ReadUnsafe(uPtr, ref readpos, 10);
  int restored2 = ArraySerializeUnsafe.ReadSigned(uPtr, ref readpos, 11);
}
```

## PrimitiveSerializeExt class

The Primitive Serializer extension lets you bitpack directly to and from ulong, uint, ushort and byte primitives. Because there is no wrapper class such as Bitstream, you need to maintain read/write pointers. The methods automatically increment the pointer for you, as it is passed to the methods as a reference. NOTE: Extension methods cannot pass the first argument reference so the Write() return value must be applied to the buffer being written to.

### Basic Usage:
```cs
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
```

### Alternative Usage
An alternative to Write() is Inject(), which has the value being written as the first argument, allowing us to pass the buffer as a reference. NOTE: There is no return value on this method, as the buffer is passed by reference and is altered by the method.
```cs
  int writepos = 0;
  // Note that the buffer is passed by reference, and is altered by the method.
  true.Inject(ref myBuffer, ref writepos);
  (-666).InjectSigned(ref myBuffer, ref writepos, 11);
  999.InjectUnsigned(ref myBuffer, ref writepos, 10);
 ```

