using Mono.Collections.Generic;

namespace Mono.Cecil.Cil
{
	public sealed class ImportDebugInformation : DebugInformation
	{
		internal ImportDebugInformation parent;

		internal Collection<ImportTarget> targets;

		public bool HasTargets => !targets.IsNullOrEmpty();

		public Collection<ImportTarget> Targets => targets ?? (targets = new Collection<ImportTarget>());

		public ImportDebugInformation Parent
		{
			get
			{
				return parent;
			}
			set
			{
				parent = value;
			}
		}

		public ImportDebugInformation()
		{
			base.token = new MetadataToken(TokenType.ImportScope);
		}
	}
}
