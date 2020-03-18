namespace Mono.Cecil
{
	public struct CustomAttributeArgument
	{
		private readonly TypeReference type;

		private readonly object value;

		public TypeReference Type => type;

		public object Value => value;

		public CustomAttributeArgument(TypeReference type, object value)
		{
			Mixin.CheckType(type);
			this.type = type;
			this.value = value;
		}
	}
}
