using Mono.Collections.Generic;

namespace Mono.Cecil
{
	public sealed class ParameterDefinition : ParameterReference, ICustomAttributeProvider, IMetadataTokenProvider, IConstantProvider, IMarshalInfoProvider
	{
		private ushort attributes;

		internal IMethodSignature method;

		private object constant = Mixin.NotResolved;

		private Collection<CustomAttribute> custom_attributes;

		private MarshalInfo marshal_info;

		public ParameterAttributes Attributes
		{
			get
			{
				return (ParameterAttributes)attributes;
			}
			set
			{
				attributes = (ushort)value;
			}
		}

		public IMethodSignature Method => method;

		public int Sequence
		{
			get
			{
				if (method == null)
				{
					return -1;
				}
				if (!method.HasImplicitThis())
				{
					return base.index;
				}
				return base.index + 1;
			}
		}

		public bool HasConstant
		{
			get
			{
				this.ResolveConstant(ref constant, base.parameter_type.Module);
				return constant != Mixin.NoValue;
			}
			set
			{
				if (!value)
				{
					constant = Mixin.NoValue;
				}
			}
		}

		public object Constant
		{
			get
			{
				if (!HasConstant)
				{
					return null;
				}
				return constant;
			}
			set
			{
				constant = value;
			}
		}

		public bool HasCustomAttributes
		{
			get
			{
				if (custom_attributes != null)
				{
					return custom_attributes.Count > 0;
				}
				return this.GetHasCustomAttributes(base.parameter_type.Module);
			}
		}

		public Collection<CustomAttribute> CustomAttributes => custom_attributes ?? this.GetCustomAttributes(ref custom_attributes, base.parameter_type.Module);

		public bool HasMarshalInfo
		{
			get
			{
				if (marshal_info != null)
				{
					return true;
				}
				return this.GetHasMarshalInfo(base.parameter_type.Module);
			}
		}

		public MarshalInfo MarshalInfo
		{
			get
			{
				return marshal_info ?? this.GetMarshalInfo(ref marshal_info, base.parameter_type.Module);
			}
			set
			{
				marshal_info = value;
			}
		}

		public bool IsIn
		{
			get
			{
				return attributes.GetAttributes(1);
			}
			set
			{
				attributes = attributes.SetAttributes(1, value);
			}
		}

		public bool IsOut
		{
			get
			{
				return attributes.GetAttributes(2);
			}
			set
			{
				attributes = attributes.SetAttributes(2, value);
			}
		}

		public bool IsLcid
		{
			get
			{
				return attributes.GetAttributes(4);
			}
			set
			{
				attributes = attributes.SetAttributes(4, value);
			}
		}

		public bool IsReturnValue
		{
			get
			{
				return attributes.GetAttributes(8);
			}
			set
			{
				attributes = attributes.SetAttributes(8, value);
			}
		}

		public bool IsOptional
		{
			get
			{
				return attributes.GetAttributes(16);
			}
			set
			{
				attributes = attributes.SetAttributes(16, value);
			}
		}

		public bool HasDefault
		{
			get
			{
				return attributes.GetAttributes(4096);
			}
			set
			{
				attributes = attributes.SetAttributes(4096, value);
			}
		}

		public bool HasFieldMarshal
		{
			get
			{
				return attributes.GetAttributes(8192);
			}
			set
			{
				attributes = attributes.SetAttributes(8192, value);
			}
		}

		internal ParameterDefinition(TypeReference parameterType, IMethodSignature method)
			: this(string.Empty, ParameterAttributes.None, parameterType)
		{
			this.method = method;
		}

		public ParameterDefinition(TypeReference parameterType)
			: this(string.Empty, ParameterAttributes.None, parameterType)
		{
		}

		public ParameterDefinition(string name, ParameterAttributes attributes, TypeReference parameterType)
			: base(name, parameterType)
		{
			this.attributes = (ushort)attributes;
			base.token = new MetadataToken(TokenType.Param);
		}

		public override ParameterDefinition Resolve()
		{
			return this;
		}
	}
}
