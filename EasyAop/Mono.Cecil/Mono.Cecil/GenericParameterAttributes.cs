using System;

namespace Mono.Cecil
{
	[Flags]
	public enum GenericParameterAttributes : ushort
	{
		VarianceMask = 0x3,
		NonVariant = 0x0,
		Covariant = 0x1,
		Contravariant = 0x2,
		SpecialConstraintMask = 0x1C,
		ReferenceTypeConstraint = 0x4,
		NotNullableValueTypeConstraint = 0x8,
		DefaultConstructorConstraint = 0x10
	}
}
