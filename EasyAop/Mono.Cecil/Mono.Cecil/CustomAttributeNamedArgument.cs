namespace Mono.Cecil
{
	public struct CustomAttributeNamedArgument
	{
		private readonly string name;

		private readonly CustomAttributeArgument argument;

		public string Name => name;

		public CustomAttributeArgument Argument => argument;

		public CustomAttributeNamedArgument(string name, CustomAttributeArgument argument)
		{
			Mixin.CheckName(name);
			this.name = name;
			this.argument = argument;
		}
	}
}
