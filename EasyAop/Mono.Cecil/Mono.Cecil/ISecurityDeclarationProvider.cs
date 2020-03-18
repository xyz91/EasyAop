using Mono.Collections.Generic;

namespace Mono.Cecil
{
	public interface ISecurityDeclarationProvider : IMetadataTokenProvider
	{
		bool HasSecurityDeclarations
		{
			get;
		}

		Collection<SecurityDeclaration> SecurityDeclarations
		{
			get;
		}
	}
}
