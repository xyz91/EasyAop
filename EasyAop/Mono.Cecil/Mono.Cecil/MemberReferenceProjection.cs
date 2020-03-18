namespace Mono.Cecil
{
	internal sealed class MemberReferenceProjection
	{
		public readonly string Name;

		public readonly MemberReferenceTreatment Treatment;

		public MemberReferenceProjection(MemberReference member, MemberReferenceTreatment treatment)
		{
			Name = member.Name;
			Treatment = treatment;
		}
	}
}
