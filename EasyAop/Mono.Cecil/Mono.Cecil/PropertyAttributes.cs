using System;

namespace Mono.Cecil
{
	[Flags]
	public enum PropertyAttributes : ushort
	{
		None = 0x0,
		SpecialName = 0x200,
		RTSpecialName = 0x400,
		HasDefault = 0x1000,
		Unused = 0xE9FF
	}
}
