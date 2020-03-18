using Mono.Collections.Generic;

namespace Mono.Cecil
{
	public interface ICustomAttributeProvider : IMetadataTokenProvider
	{
		Collection<CustomAttribute> CustomAttributes
		{
			get;
		}

		bool HasCustomAttributes
		{
			get;
		}
	}
}
