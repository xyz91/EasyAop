using System;

namespace Mono.Cecil
{
	[Flags]
	public enum FieldAttributes : ushort
	{
		FieldAccessMask = 0x7,
		CompilerControlled = 0x0,
		Private = 0x1,
		FamANDAssem = 0x2,
		Assembly = 0x3,
		Family = 0x4,
		FamORAssem = 0x5,
		Public = 0x6,
		Static = 0x10,
		InitOnly = 0x20,
		Literal = 0x40,
		NotSerialized = 0x80,
		SpecialName = 0x200,
		PInvokeImpl = 0x2000,
		RTSpecialName = 0x400,
		HasFieldMarshal = 0x1000,
		HasDefault = 0x8000,
		HasFieldRVA = 0x100
	}
}
