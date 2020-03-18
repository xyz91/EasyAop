using System;

namespace Mono.Cecil
{
	[Flags]
	internal enum MethodDefinitionTreatment
	{
		None = 0x0,
		Dispose = 0x1,
		Abstract = 0x2,
		Private = 0x4,
		Public = 0x8,
		Runtime = 0x10,
		InternalCall = 0x20
	}
}
