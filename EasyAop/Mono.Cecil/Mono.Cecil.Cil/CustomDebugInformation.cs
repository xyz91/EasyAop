using System;

namespace Mono.Cecil.Cil
{
	public abstract class CustomDebugInformation : DebugInformation
	{
		private Guid identifier;

		public Guid Identifier => identifier;

		public abstract CustomDebugInformationKind Kind
		{
			get;
		}

		internal CustomDebugInformation(Guid identifier)
		{
			this.identifier = identifier;
			base.token = new MetadataToken(TokenType.CustomDebugInformation);
		}
	}
}
