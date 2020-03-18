using Mono.Cecil.Metadata;
using System;

namespace Mono.Cecil
{
	public sealed class SentinelType : TypeSpecification
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

		public override bool IsSentinel => true;

		public SentinelType(TypeReference type)
			: base(type)
		{
			Mixin.CheckType(type);
			base.etype = Mono.Cecil.Metadata.ElementType.Sentinel;
		}
	}
}
