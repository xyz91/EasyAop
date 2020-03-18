using Mono.Collections.Generic;
using System;

namespace Mono.Cecil
{
	public abstract class MethodSpecification : MethodReference
	{
		private readonly MethodReference method;

		public MethodReference ElementMethod => method;

		public override string Name
		{
			get
			{
				return method.Name;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override MethodCallingConvention CallingConvention
		{
			get
			{
				return method.CallingConvention;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override bool HasThis
		{
			get
			{
				return method.HasThis;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override bool ExplicitThis
		{
			get
			{
				return method.ExplicitThis;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override MethodReturnType MethodReturnType
		{
			get
			{
				return method.MethodReturnType;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override TypeReference DeclaringType
		{
			get
			{
				return method.DeclaringType;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override ModuleDefinition Module => method.Module;

		public override bool HasParameters => method.HasParameters;

		public override Collection<ParameterDefinition> Parameters => method.Parameters;

		public override bool ContainsGenericParameter => method.ContainsGenericParameter;

		internal MethodSpecification(MethodReference method)
		{
			Mixin.CheckMethod(method);
			this.method = method;
			base.token = new MetadataToken(TokenType.MethodSpec);
		}

		public sealed override MethodReference GetElementMethod()
		{
			return method.GetElementMethod();
		}
	}
}
