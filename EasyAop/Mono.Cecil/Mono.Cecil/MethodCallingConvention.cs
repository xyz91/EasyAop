namespace Mono.Cecil
{
	public enum MethodCallingConvention : byte
	{
		Default,
		C,
		StdCall,
		ThisCall,
		FastCall,
		VarArg,
		Generic = 0x10
	}
}
