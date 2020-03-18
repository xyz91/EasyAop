namespace Mono.Cecil.Cil
{
	public sealed class ImportTarget
	{
		internal ImportTargetKind kind;

		internal string @namespace;

		internal TypeReference type;

		internal AssemblyNameReference reference;

		internal string alias;

		public string Namespace
		{
			get
			{
				return @namespace;
			}
			set
			{
				@namespace = value;
			}
		}

		public TypeReference Type
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
			}
		}

		public AssemblyNameReference AssemblyReference
		{
			get
			{
				return reference;
			}
			set
			{
				reference = value;
			}
		}

		public string Alias
		{
			get
			{
				return alias;
			}
			set
			{
				alias = value;
			}
		}

		public ImportTargetKind Kind
		{
			get
			{
				return kind;
			}
			set
			{
				kind = value;
			}
		}

		public ImportTarget(ImportTargetKind kind)
		{
			this.kind = kind;
		}
	}
}
