using Mono.Cecil.Metadata;
using Mono.Collections.Generic;
using System;
using System.Text;

namespace Mono.Cecil
{
	public sealed class FunctionPointerType : TypeSpecification, IMethodSignature, IMetadataTokenProvider
	{
		private readonly MethodReference function;

		public bool HasThis
		{
			get
			{
				return function.HasThis;
			}
			set
			{
				function.HasThis = value;
			}
		}

		public bool ExplicitThis
		{
			get
			{
				return function.ExplicitThis;
			}
			set
			{
				function.ExplicitThis = value;
			}
		}

		public MethodCallingConvention CallingConvention
		{
			get
			{
				return function.CallingConvention;
			}
			set
			{
				function.CallingConvention = value;
			}
		}

		public bool HasParameters => function.HasParameters;

		public Collection<ParameterDefinition> Parameters => function.Parameters;

		public TypeReference ReturnType
		{
			get
			{
				return function.MethodReturnType.ReturnType;
			}
			set
			{
				function.MethodReturnType.ReturnType = value;
			}
		}

		public MethodReturnType MethodReturnType => function.MethodReturnType;

		public override string Name
		{
			get
			{
				return function.Name;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override string Namespace
		{
			get
			{
				return string.Empty;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override ModuleDefinition Module => ReturnType.Module;

		public override IMetadataScope Scope
		{
			get
			{
				return function.ReturnType.Scope;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override bool IsFunctionPointer => true;

		public override bool ContainsGenericParameter => function.ContainsGenericParameter;

		public override string FullName
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(function.Name);
				stringBuilder.Append(" ");
				stringBuilder.Append(function.ReturnType.FullName);
				stringBuilder.Append(" *");
				this.MethodSignatureFullName(stringBuilder);
				return stringBuilder.ToString();
			}
		}

		public FunctionPointerType()
			: base(null)
		{
			function = new MethodReference();
			function.Name = "method";
			base.etype = Mono.Cecil.Metadata.ElementType.FnPtr;
		}

		public override TypeDefinition Resolve()
		{
			return null;
		}

		public override TypeReference GetElementType()
		{
			return this;
		}
	}
}
