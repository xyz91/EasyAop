using System;

namespace Mono.Cecil
{
	[Flags]
	public enum MethodAttributes : ushort
	{
		MemberAccessMask = 0x7,
		CompilerControlled = 0x0,
		Private = 0x1,
		FamANDAssem = 0x2,
		Assembly = 0x3,
		Family = 0x4,
		FamORAssem = 0x5,
		Public = 0x6,
		Static = 0x10,
		Final = 0x20,
		Virtual = 0x40,
		HideBySig = 0x80,
		VtableLayoutMask = 0x100,
		ReuseSlot = 0x0,
		NewSlot = 0x100,
		CheckAccessOnOverride = 0x200,
		Abstract = 0x400,
		SpecialName = 0x800,
		PInvokeImpl = 0x2000,
		UnmanagedExport = 0x8,
		RTSpecialName = 0x1000,
		HasSecurity = 0x4000,
		RequireSecObject = 0x8000
	}
}
