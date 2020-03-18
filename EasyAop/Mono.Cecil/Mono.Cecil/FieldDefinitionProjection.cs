namespace Mono.Cecil
{
	internal sealed class FieldDefinitionProjection
	{
		public readonly FieldAttributes Attributes;

		public readonly FieldDefinitionTreatment Treatment;

		public FieldDefinitionProjection(FieldDefinition field, FieldDefinitionTreatment treatment)
		{
			Attributes = field.Attributes;
			Treatment = treatment;
		}
	}
}
