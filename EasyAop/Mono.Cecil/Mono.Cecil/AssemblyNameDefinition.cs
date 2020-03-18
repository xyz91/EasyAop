using System;

namespace Mono.Cecil
{
	public sealed class AssemblyNameDefinition : AssemblyNameReference
	{
		public override byte[] Hash => Empty<byte>.Array;

		internal AssemblyNameDefinition()
		{
			base.token = new MetadataToken(TokenType.Assembly, 1);
		}

		public AssemblyNameDefinition(string name, Version version)
			: base(name, version)
		{
			base.token = new MetadataToken(TokenType.Assembly, 1);
		}
	}
}
