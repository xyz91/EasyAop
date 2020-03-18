namespace Mono.Cecil
{
	public enum TokenType : uint
	{
		Module,
		TypeRef = 0x1000000,
		TypeDef = 0x2000000,
		Field = 0x4000000,
		Method = 100663296u,
		Param = 0x8000000,
		InterfaceImpl = 150994944u,
		MemberRef = 167772160u,
		CustomAttribute = 201326592u,
		Permission = 234881024u,
		Signature = 285212672u,
		Event = 335544320u,
		Property = 385875968u,
		ModuleRef = 436207616u,
		TypeSpec = 452984832u,
		Assembly = 0x20000000,
		AssemblyRef = 587202560u,
		File = 637534208u,
		ExportedType = 654311424u,
		ManifestResource = 671088640u,
		GenericParam = 704643072u,
		MethodSpec = 721420288u,
		GenericParamConstraint = 738197504u,
		Document = 805306368u,
		MethodDebugInformation = 822083584u,
		LocalScope = 838860800u,
		LocalVariable = 855638016u,
		LocalConstant = 872415232u,
		ImportScope = 889192448u,
		StateMachineMethod = 905969664u,
		CustomDebugInformation = 922746880u,
		String = 1879048192u
	}
}
