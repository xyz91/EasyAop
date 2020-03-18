namespace Mono.Cecil
{
	public abstract class Resource
	{
		private string name;

		private uint attributes;

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

		public ManifestResourceAttributes Attributes
		{
			get
			{
				return (ManifestResourceAttributes)attributes;
			}
			set
			{
				attributes = (uint)value;
			}
		}

		public abstract ResourceType ResourceType
		{
			get;
		}

		public bool IsPublic
		{
			get
			{
				return attributes.GetMaskedAttributes(7u, 1u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7u, 1u, value);
			}
		}

		public bool IsPrivate
		{
			get
			{
				return attributes.GetMaskedAttributes(7u, 2u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7u, 2u, value);
			}
		}

		internal Resource(string name, ManifestResourceAttributes attributes)
		{
			this.name = name;
			this.attributes = (uint)attributes;
		}
	}
}
