using System;

namespace Mono.Cecil
{
	[Flags]
	public enum MethodImplAttributes : ushort
	{
		CodeTypeMask = 0x3,
		IL = 0x0,
		Native = 0x1,
		OPTIL = 0x2,
		Runtime = 0x3,
		ManagedMask = 0x4,
		Unmanaged = 0x4,
		Managed = 0x0,
		ForwardRef = 0x10,
		PreserveSig = 0x80,
		InternalCall = 0x1000,
		Synchronized = 0x20,
		NoOptimization = 0x40,
		NoInlining = 0x8,
		AggressiveInlining = 0x100
	}
}
