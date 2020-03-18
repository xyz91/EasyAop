using Mono.Collections.Generic;

namespace Mono.Cecil.Cil
{
	public interface ICustomDebugInformationProvider : IMetadataTokenProvider
	{
		bool HasCustomDebugInformations
		{
			get;
		}

		Collection<CustomDebugInformation> CustomDebugInformations
		{
			get;
		}
	}
}
