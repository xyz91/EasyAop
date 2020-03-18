namespace Mono.Cecil
{
	public class ModuleReference : IMetadataScope, IMetadataTokenProvider
	{
		private string name;

		internal MetadataToken token;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public virtual MetadataScopeType MetadataScopeType => MetadataScopeType.ModuleReference;

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

		internal ModuleReference()
		{
			token = new MetadataToken(TokenType.ModuleRef);
		}

		public ModuleReference(string name)
			: this()
		{
			this.name = name;
		}

		public override string ToString()
		{
			return name;
		}
	}
}
