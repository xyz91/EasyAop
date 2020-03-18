namespace Mono.Cecil
{
	public sealed class LinkedResource : Resource
	{
		internal byte[] hash;

		private string file;

		public byte[] Hash => hash;

		public string File
		{
			get
			{
				return file;
			}
			set
			{
				file = value;
			}
		}

		public override ResourceType ResourceType => ResourceType.Linked;

		public LinkedResource(string name, ManifestResourceAttributes flags)
			: base(name, flags)
		{
		}

		public LinkedResource(string name, ManifestResourceAttributes flags, string file)
			: base(name, flags)
		{
			this.file = file;
		}
	}
}
