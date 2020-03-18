using System;

namespace Mono.Cecil.Cil
{
	public sealed class SourceLinkDebugInformation : CustomDebugInformation
	{
		internal string content;

		public static Guid KindIdentifier = new Guid("{CC110556-A091-4D38-9FEC-25AB9A351A6A}");

		public string Content
		{
			get
			{
				return content;
			}
			set
			{
				content = value;
			}
		}

		public override CustomDebugInformationKind Kind => CustomDebugInformationKind.SourceLink;

		public SourceLinkDebugInformation(string content)
			: base(KindIdentifier)
		{
			this.content = content;
		}
	}
}
