using Mono.Collections.Generic;
using System.Threading;

namespace Mono.Cecil
{
	public sealed class MethodReturnType : IConstantProvider, IMetadataTokenProvider, ICustomAttributeProvider, IMarshalInfoProvider
	{
		internal IMethodSignature method;

		internal ParameterDefinition parameter;

		private TypeReference return_type;

		public IMethodSignature Method => method;

		public TypeReference ReturnType
		{
			get
			{
				return return_type;
			}
			set
			{
				return_type = value;
			}
		}

		internal ParameterDefinition Parameter
		{
			get
			{
				if (parameter == null)
				{
					Interlocked.CompareExchange(ref parameter, new ParameterDefinition(return_type, method), null);
				}
				return parameter;
			}
		}

		public MetadataToken MetadataToken
		{
			get
			{
				return Parameter.MetadataToken;
			}
			set
			{
				Parameter.MetadataToken = value;
			}
		}

		public ParameterAttributes Attributes
		{
			get
			{
				return Parameter.Attributes;
			}
			set
			{
				Parameter.Attributes = value;
			}
		}

		public string Name
		{
			get
			{
				return Parameter.Name;
			}
			set
			{
				Parameter.Name = value;
			}
		}

		public bool HasCustomAttributes
		{
			get
			{
				if (parameter != null)
				{
					return parameter.HasCustomAttributes;
				}
				return false;
			}
		}

		public Collection<CustomAttribute> CustomAttributes => Parameter.CustomAttributes;

		public bool HasDefault
		{
			get
			{
				if (parameter != null)
				{
					return parameter.HasDefault;
				}
				return false;
			}
			set
			{
				Parameter.HasDefault = value;
			}
		}

		public bool HasConstant
		{
			get
			{
				if (parameter != null)
				{
					return parameter.HasConstant;
				}
				return false;
			}
			set
			{
				Parameter.HasConstant = value;
			}
		}

		public object Constant
		{
			get
			{
				return Parameter.Constant;
			}
			set
			{
				Parameter.Constant = value;
			}
		}

		public bool HasFieldMarshal
		{
			get
			{
				if (parameter != null)
				{
					return parameter.HasFieldMarshal;
				}
				return false;
			}
			set
			{
				Parameter.HasFieldMarshal = value;
			}
		}

		public bool HasMarshalInfo
		{
			get
			{
				if (parameter != null)
				{
					return parameter.HasMarshalInfo;
				}
				return false;
			}
		}

		public MarshalInfo MarshalInfo
		{
			get
			{
				return Parameter.MarshalInfo;
			}
			set
			{
				Parameter.MarshalInfo = value;
			}
		}

		public MethodReturnType(IMethodSignature method)
		{
			this.method = method;
		}
	}
}
