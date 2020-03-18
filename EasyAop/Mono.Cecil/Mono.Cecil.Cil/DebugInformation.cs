using Mono.Collections.Generic;

namespace Mono.Cecil.Cil
{
	public abstract class DebugInformation : ICustomDebugInformationProvider, IMetadataTokenProvider
	{
		internal MetadataToken token;

		internal Collection<CustomDebugInformation> custom_infos;

		public MetadataToken MetadataToken
		{
			get
			{
				return token;
			}
			set
			{
				token = value;
			}
		}

		public bool HasCustomDebugInformations => !custom_infos.IsNullOrEmpty();

		public Collection<CustomDebugInformation> CustomDebugInformations => custom_infos ?? (custom_infos = new Collection<CustomDebugInformation>());

		internal DebugInformation()
		{
		}
	}
}
