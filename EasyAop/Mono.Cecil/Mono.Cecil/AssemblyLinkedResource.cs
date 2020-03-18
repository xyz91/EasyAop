namespace Mono.Cecil
{
	public sealed class AssemblyLinkedResource : Resource
	{
		private AssemblyNameReference reference;

		public AssemblyNameReference Assembly
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

		public override ResourceType ResourceType => ResourceType.AssemblyLinked;

		public AssemblyLinkedResource(string name, ManifestResourceAttributes flags)
			: base(name, flags)
		{
		}

		public AssemblyLinkedResource(string name, ManifestResourceAttributes flags, AssemblyNameReference reference)
			: base(name, flags)
		{
			this.reference = reference;
		}
	}
}
