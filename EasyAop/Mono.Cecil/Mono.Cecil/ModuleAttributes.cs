using System;

namespace Mono.Cecil
{
	[Flags]
	public enum ModuleAttributes
	{
		ILOnly = 0x1,
		Required32Bit = 0x2,
		ILLibrary = 0x4,
		StrongNameSigned = 0x8,
		Preferred32Bit = 0x20000
	}
}
