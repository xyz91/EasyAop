using System;

namespace Mono.Cecil
{
	[Flags]
	public enum ParameterAttributes : ushort
	{
		None = 0x0,
		In = 0x1,
		Out = 0x2,
		Lcid = 0x4,
		Retval = 0x8,
		Optional = 0x10,
		HasDefault = 0x1000,
		HasFieldMarshal = 0x2000,
		Unused = 0xCFE0
	}
}
