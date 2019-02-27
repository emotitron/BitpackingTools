# BitpackingTools
Libraries used interally for Unity Store <a href="https://assetstore.unity.com/packages/tools/network/network-sync-transform-nst-98453">NetworkSyncTransform</a> and <a href="https://assetstore.unity.com/packages/tools/network/transform-crusher-free-version-117313">TransformCrusher</a> Assets 

Instructions to follow eventually.

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

## PrimitiveSerializeExt class

The Primitive Serializer extension lets you bitpack directly to and from ulong, uint, ushort and byte primitives. Because there is no wrapper class such as Bitstream, you need to maintain read/write pointers. The methods automatically increment these pointers for you however.

### Basic Usage:
```cs
  ulong myBuffer = 0;
  
  int writepos = 0;
  // Note that primitives are reference types, need to apply the return value of extensions.
  myBuffer = myBuffer.WritetBool(true, ref writepos);
  myBuffer = myBuffer.WriteSigned(-666, ref writepos, 10);
  myBuffer = myBuffer.Write(999, ref writepos, 10);
  
  int readpos = 0;
  bool restoredbool = myBuffer.ReadBool(ref readpos);
  int restoredval1 = myBuffer.ReadSigned(ref readpos, 10);
  uint restoredval2 = (uint)myBuffer.Read(ref readpos, 10);
```
