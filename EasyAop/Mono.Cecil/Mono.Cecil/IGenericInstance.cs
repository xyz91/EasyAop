using Mono.Collections.Generic;

namespace Mono.Cecil
{
	public interface IGenericInstance : IMetadataTokenProvider
	{
		bool HasGenericArguments
		{
			get;
		}

		Collection<TypeReference> GenericArguments
		{
			get;
		}
	}
}
