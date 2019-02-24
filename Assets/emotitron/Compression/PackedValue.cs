///*
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

//using System.Runtime.InteropServices;
//using System;

///// <summary>
///// A simple wrapper for unsigned ints, which also contains how many bits that value is packed down to.
///// </summary>
//[StructLayout(LayoutKind.Explicit)]
//public struct PackedValue
//{
//	[FieldOffset(0)]
//	public System.Byte int8;
//	[FieldOffset(0)]
//	public System.SByte uint8;
//	[FieldOffset(0)]
//	public System.Int16 int16;
//	[FieldOffset(0)]
//	public System.UInt16 uint16;
//	[FieldOffset(0)]
//	public System.Int32 int32;
//	[FieldOffset(0)]
//	public System.UInt32 uint32;
//	[FieldOffset(0)]
//	public System.Int64 int64;
//	[FieldOffset(0)]
//	public System.UInt64 uint64;
//	[FieldOffset(0)]
//	public System.Single float32;
//	[FieldOffset(0)]
//	public System.Double float64;
//	[FieldOffset(0)]
//	public System.Boolean boolean;
//	[FieldOffset(0)]
//	public System.Char character;

//	[FieldOffset(8)]
//	public int bits;

//	public PackedValue(System.Byte int8, int bits = 8) : this()
//	{
//		this.int8 = int8;
//		this.bits = bits;
//	}
//	public PackedValue(System.SByte uint8, int bits = 8) : this()
//	{
//		this.uint8 = uint8;
//		this.bits = bits;
//	}
//	public PackedValue(System.Int16 int16, int bits = 16) : this()
//	{
//		this.int16 = int16;
//		this.bits = bits;
//	}
//	public PackedValue(System.UInt16 uint16, int bits = 16) : this()
//	{
//		this.uint16 = uint16;
//		this.bits = bits;
//	}
//	public PackedValue(System.Int32 int32, int bits = 32) : this()
//	{
//		this.int32 = int32;
//		this.bits = bits;
//	}
//	public PackedValue(System.UInt32 uint32, int bits = 32) : this()
//	{
//		this.uint32 = uint32;
//		this.bits = bits;
//	}
//	public PackedValue(System.Int64 int64, int bits = 64) : this()
//	{
//		this.int64 = int64;
//		this.bits = bits;
//	}
//	public PackedValue(System.UInt64 uint64, int bits = 64) : this()
//	{
//		this.uint64 = uint64;
//		this.bits = bits;
//	}
//	public PackedValue(System.Single float32) : this()
//	{
//		this.float32 = float32;
//		this.bits = 32;
//	}
//	public PackedValue(System.Double float64) : this()
//	{
//		this.float64 = float64;
//		this.bits = 64;
//	}
//	public PackedValue(System.Boolean boolean) : this()
//	{
//		this.boolean = boolean;
//		this.bits = 1;
//	}
//	public PackedValue(System.Char character, int bits = 16) : this()
//	{
//		this.character = character;
//		this.bits = bits;
//	}


//	public static implicit operator System.Byte(PackedValue pv) { return pv.int8; }
//	public static implicit operator System.SByte(PackedValue pv) { return pv.uint8; }

//	public static implicit operator System.Int16(PackedValue pv) { return pv.int16; }
//	public static implicit operator System.UInt16(PackedValue pv) { return pv.uint16; }

//	public static implicit operator System.Int32(PackedValue pv) { return pv.int32; }
//	public static implicit operator System.UInt32(PackedValue pv) { return pv.uint32; }

//	public static implicit operator System.Int64(PackedValue pv) { return pv.int64; }
//	public static implicit operator System.UInt64(PackedValue pv) { return pv.uint64; }

//	public static implicit operator System.Single(PackedValue pv) { return pv.float32; }
//	public static implicit operator System.Double(PackedValue pv) { return pv.float64; }

//	public static implicit operator System.Boolean(PackedValue pv) { return pv.boolean; }
//	public static implicit operator System.Char(PackedValue pv) { return pv.character; }

//}