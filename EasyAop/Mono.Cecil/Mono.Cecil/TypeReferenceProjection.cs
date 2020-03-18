namespace Mono.Cecil
{
	internal sealed class TypeReferenceProjection
	{
		public readonly string Name;

		public readonly string Namespace;

		public readonly IMetadataScope Scope;

		public readonly TypeReferenceTreatment Treatment;

		public TypeReferenceProjection(TypeReference type, TypeReferenceTreatment treatment)
		{
			Name = type.Name;
			Namespace = type.Namespace;
			Scope = type.Scope;
			Treatment = treatment;
		}
	}
}
