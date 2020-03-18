using Mono.Cecil.Metadata;
using System;

namespace Mono.Cecil
{
	public sealed class PinnedType : TypeSpecification
	{
		public override bool IsValueType
		{
			get
			{
				return false;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override bool IsPinned => true;

		public PinnedType(TypeReference type)
			: base(type)
		{
			Mixin.CheckType(type);
			base.etype = Mono.Cecil.Metadata.ElementType.Pinned;
		}
	}
}
