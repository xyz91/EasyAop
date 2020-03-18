using Mono.Cecil.Metadata;
using Mono.Collections.Generic;
using System;
using System.Text;

namespace Mono.Cecil
{
	public sealed class GenericInstanceType : TypeSpecification, IGenericInstance, IMetadataTokenProvider, IGenericContext
	{
		private Collection<TypeReference> arguments;

		public bool HasGenericArguments => !arguments.IsNullOrEmpty();

		public Collection<TypeReference> GenericArguments => arguments ?? (arguments = new Collection<TypeReference>());

		public override TypeReference DeclaringType
		{
			get
			{
				return base.ElementType.DeclaringType;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public override string FullName
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.FullName);
				this.GenericInstanceFullName(stringBuilder);
				return stringBuilder.ToString();
			}
		}

		public override bool IsGenericInstance => true;

		public override bool ContainsGenericParameter
		{
			get
			{
				if (!this.ContainsGenericParameter())
				{
					return base.ContainsGenericParameter;
				}
				return true;
			}
		}

		IGenericParameterProvider IGenericContext.Type
		{
			get
			{
				return base.ElementType;
			}
		}

		public GenericInstanceType(TypeReference type)
			: base(type)
		{
			base.IsValueType = type.IsValueType;
			base.etype = Mono.Cecil.Metadata.ElementType.GenericInst;
		}
	}
}
