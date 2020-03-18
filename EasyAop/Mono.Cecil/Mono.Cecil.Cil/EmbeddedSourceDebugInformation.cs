using System;

namespace Mono.Cecil.Cil
{
	public sealed class EmbeddedSourceDebugInformation : CustomDebugInformation
	{
		internal byte[] content;

		internal bool compress;

		public static Guid KindIdentifier = new Guid("{0E8A571B-6926-466E-B4AD-8AB04611F5FE}");

		public byte[] Content
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

		public bool Compress
		{
			get
			{
				return compress;
			}
			set
			{
				compress = value;
			}
		}

		public override CustomDebugInformationKind Kind => CustomDebugInformationKind.EmbeddedSource;

		public EmbeddedSourceDebugInformation(byte[] content, bool compress)
			: base(KindIdentifier)
		{
			this.content = content;
			this.compress = compress;
		}
	}
}
