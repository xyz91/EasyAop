using System;

namespace Mono.Cecil.Cil
{
	[Flags]
	public enum VariableAttributes : ushort
	{
		None = 0x0,
		DebuggerHidden = 0x1
	}
}
