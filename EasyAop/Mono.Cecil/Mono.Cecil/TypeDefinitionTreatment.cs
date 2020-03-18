using System;

namespace Mono.Cecil
{
	[Flags]
	internal enum TypeDefinitionTreatment
	{
		None = 0x0,
		KindMask = 0xF,
		NormalType = 0x1,
		NormalAttribute = 0x2,
		UnmangleWindowsRuntimeName = 0x3,
		PrefixWindowsRuntimeName = 0x4,
		RedirectToClrType = 0x5,
		RedirectToClrAttribute = 0x6,
		Abstract = 0x10,
		Internal = 0x20
	}
}
