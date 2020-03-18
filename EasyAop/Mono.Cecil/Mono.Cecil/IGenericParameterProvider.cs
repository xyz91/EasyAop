using Mono.Collections.Generic;

namespace Mono.Cecil
{
	public interface IGenericParameterProvider : IMetadataTokenProvider
	{
		bool HasGenericParameters
		{
			get;
		}

		bool IsDefinition
		{
			get;
		}

		ModuleDefinition Module
		{
			get;
		}

		Collection<GenericParameter> GenericParameters
		{
			get;
		}

		GenericParameterType GenericParameterType
		{
			get;
		}
	}
}
