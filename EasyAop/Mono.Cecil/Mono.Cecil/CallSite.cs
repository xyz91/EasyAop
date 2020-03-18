using Mono.Collections.Generic;
using System;
using System.Text;

namespace Mono.Cecil
{
	public sealed class CallSite : IMethodSignature, IMetadataTokenProvider
	{
		private readonly MethodReference signature;

		public bool HasThis
		{
			get
			{
				return signature.HasThis;
			}
			set
			{
				signature.HasThis = value;
			}
		}

		public bool ExplicitThis
		{
			get
			{
				return signature.ExplicitThis;
			}
			set
			{
				signature.ExplicitThis = value;
			}
		}

		public MethodCallingConvention CallingConvention
		{
			get
			{
				return signature.CallingConvention;
			}
			set
			{
				signature.CallingConvention = value;
			}
		}

		public bool HasParameters => signature.HasParameters;

		public Collection<ParameterDefinition> Parameters => signature.Parameters;

		public TypeReference ReturnType
		{
			get
			{
				return signature.MethodReturnType.ReturnType;
			}
			set
			{
				signature.MethodReturnType.ReturnType = value;
			}
		}

		public MethodReturnType MethodReturnType => signature.MethodReturnType;

		public string Name
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

		public string Namespace
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

		public ModuleDefinition Module => ReturnType.Module;

		public IMetadataScope Scope => signature.ReturnType.Scope;

		public MetadataToken MetadataToken
		{
			get
			{
				return signature.token;
			}
			set
			{
				signature.token = value;
			}
		}

		public string FullName
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(ReturnType.FullName);
				this.MethodSignatureFullName(stringBuilder);
				return stringBuilder.ToString();
			}
		}

		internal CallSite()
		{
			signature = new MethodReference();
			signature.token = new MetadataToken(TokenType.Signature, 0);
		}

		public CallSite(TypeReference returnType)
			: this()
		{
			if (returnType == null)
			{
				throw new ArgumentNullException("returnType");
			}
			signature.ReturnType = returnType;
		}

		public override string ToString()
		{
			return FullName;
		}
	}
}
