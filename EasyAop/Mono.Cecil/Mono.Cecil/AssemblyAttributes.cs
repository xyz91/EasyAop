using System;

namespace Mono.Cecil
{
	[Flags]
	public enum AssemblyAttributes : uint
	{
		PublicKey = 0x1,
		SideBySideCompatible = 0x0,
		Retargetable = 0x100,
		WindowsRuntime = 0x200,
		DisableJITCompileOptimizer = 0x4000,
		EnableJITCompileTracking = 0x8000
	}
}
