namespace Mono.Cecil
{
	internal sealed class TypeDefinitionProjection
	{
		public readonly TypeAttributes Attributes;

		public readonly string Name;

		public readonly TypeDefinitionTreatment Treatment;

		public TypeDefinitionProjection(TypeDefinition type, TypeDefinitionTreatment treatment)
		{
			Attributes = type.Attributes;
			Name = type.Name;
			Treatment = treatment;
		}
	}
}
