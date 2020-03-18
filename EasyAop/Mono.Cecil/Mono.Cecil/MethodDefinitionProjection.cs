namespace Mono.Cecil
{
	internal sealed class MethodDefinitionProjection
	{
		public readonly MethodAttributes Attributes;

		public readonly MethodImplAttributes ImplAttributes;

		public readonly string Name;

		public readonly MethodDefinitionTreatment Treatment;

		public MethodDefinitionProjection(MethodDefinition method, MethodDefinitionTreatment treatment)
		{
			Attributes = method.Attributes;
			ImplAttributes = method.ImplAttributes;
			Name = method.Name;
			Treatment = treatment;
		}
	}
}
