using Mono.Collections.Generic;
using System.Text;

namespace Mono.Cecil
{
	public sealed class GenericInstanceMethod : MethodSpecification, IGenericInstance, IMetadataTokenProvider, IGenericContext
	{
		private Collection<TypeReference> arguments;

		public bool HasGenericArguments => !arguments.IsNullOrEmpty();

		public Collection<TypeReference> GenericArguments => arguments ?? (arguments = new Collection<TypeReference>());

		public override bool IsGenericInstance => true;

		IGenericParameterProvider IGenericContext.Method
		{
			get
			{
				return base.ElementMethod;
			}
		}

		IGenericParameterProvider IGenericContext.Type
		{
			get
			{
				return base.ElementMethod.DeclaringType;
			}
		}

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

		public override string FullName
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				MethodReference elementMethod = base.ElementMethod;
				stringBuilder.Append(elementMethod.ReturnType.FullName).Append(" ").Append(elementMethod.DeclaringType.FullName)
					.Append("::")
					.Append(elementMethod.Name);
				this.GenericInstanceFullName(stringBuilder);
				this.MethodSignatureFullName(stringBuilder);
				return stringBuilder.ToString();
			}
		}

		public GenericInstanceMethod(MethodReference method)
			: base(method)
		{
		}
	}
}
