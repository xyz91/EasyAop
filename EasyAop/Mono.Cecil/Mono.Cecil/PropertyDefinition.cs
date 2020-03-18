using Mono.Collections.Generic;
using System.Text;

namespace Mono.Cecil
{
	public sealed class PropertyDefinition : PropertyReference, IMemberDefinition, ICustomAttributeProvider, IMetadataTokenProvider, IConstantProvider
	{
		private bool? has_this;

		private ushort attributes;

		private Collection<CustomAttribute> custom_attributes;

		internal MethodDefinition get_method;

		internal MethodDefinition set_method;

		internal Collection<MethodDefinition> other_methods;

		private object constant = Mixin.NotResolved;

		public PropertyAttributes Attributes
		{
			get
			{
				return (PropertyAttributes)attributes;
			}
			set
			{
				attributes = (ushort)value;
			}
		}

		public bool HasThis
		{
			get
			{
				if (has_this.HasValue)
				{
					return has_this.Value;
				}
				if (GetMethod != null)
				{
					return get_method.HasThis;
				}
				if (SetMethod != null)
				{
					return set_method.HasThis;
				}
				return false;
			}
			set
			{
				has_this = value;
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
				return this.GetHasCustomAttributes(Module);
			}
		}

		public Collection<CustomAttribute> CustomAttributes => custom_attributes ?? this.GetCustomAttributes(ref custom_attributes, Module);

		public MethodDefinition GetMethod
		{
			get
			{
				if (get_method != null)
				{
					return get_method;
				}
				InitializeMethods();
				return get_method;
			}
			set
			{
				get_method = value;
			}
		}

		public MethodDefinition SetMethod
		{
			get
			{
				if (set_method != null)
				{
					return set_method;
				}
				InitializeMethods();
				return set_method;
			}
			set
			{
				set_method = value;
			}
		}

		public bool HasOtherMethods
		{
			get
			{
				if (other_methods != null)
				{
					return other_methods.Count > 0;
				}
				InitializeMethods();
				return !other_methods.IsNullOrEmpty();
			}
		}

		public Collection<MethodDefinition> OtherMethods
		{
			get
			{
				if (other_methods != null)
				{
					return other_methods;
				}
				InitializeMethods();
				if (other_methods != null)
				{
					return other_methods;
				}
				return other_methods = new Collection<MethodDefinition>();
			}
		}

		public bool HasParameters
		{
			get
			{
				InitializeMethods();
				if (get_method != null)
				{
					return get_method.HasParameters;
				}
				if (set_method != null)
				{
					if (set_method.HasParameters)
					{
						return set_method.Parameters.Count > 1;
					}
					return false;
				}
				return false;
			}
		}

		public override Collection<ParameterDefinition> Parameters
		{
			get
			{
				InitializeMethods();
				if (get_method != null)
				{
					return MirrorParameters(get_method, 0);
				}
				if (set_method != null)
				{
					return MirrorParameters(set_method, 1);
				}
				return new Collection<ParameterDefinition>();
			}
		}

		public bool HasConstant
		{
			get
			{
				this.ResolveConstant(ref constant, Module);
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

		public bool IsSpecialName
		{
			get
			{
				return attributes.GetAttributes(512);
			}
			set
			{
				attributes = attributes.SetAttributes(512, value);
			}
		}

		public bool IsRuntimeSpecialName
		{
			get
			{
				return attributes.GetAttributes(1024);
			}
			set
			{
				attributes = attributes.SetAttributes(1024, value);
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

		public new TypeDefinition DeclaringType
		{
			get
			{
				return (TypeDefinition)base.DeclaringType;
			}
			set
			{
				base.DeclaringType = value;
			}
		}

		public override bool IsDefinition => true;

		public override string FullName
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.PropertyType.ToString());
				stringBuilder.Append(' ');
				stringBuilder.Append(base.MemberFullName());
				stringBuilder.Append('(');
				if (HasParameters)
				{
					Collection<ParameterDefinition> parameters = Parameters;
					for (int i = 0; i < parameters.Count; i++)
					{
						if (i > 0)
						{
							stringBuilder.Append(',');
						}
						stringBuilder.Append(parameters[i].ParameterType.FullName);
					}
				}
				stringBuilder.Append(')');
				return stringBuilder.ToString();
			}
		}

		private static Collection<ParameterDefinition> MirrorParameters(MethodDefinition method, int bound)
		{
			Collection<ParameterDefinition> collection = new Collection<ParameterDefinition>();
			if (!method.HasParameters)
			{
				return collection;
			}
			Collection<ParameterDefinition> parameters = method.Parameters;
			int num = parameters.Count - bound;
			for (int i = 0; i < num; i++)
			{
				collection.Add(parameters[i]);
			}
			return collection;
		}

		public PropertyDefinition(string name, PropertyAttributes attributes, TypeReference propertyType)
			: base(name, propertyType)
		{
			this.attributes = (ushort)attributes;
			base.token = new MetadataToken(TokenType.Property);
		}

		private void InitializeMethods()
		{
			ModuleDefinition module = Module;
			if (module != null)
			{
				lock (module.SyncRoot)
				{
					if (get_method == null && set_method == null && module.HasImage())
					{
						module.Read(this, delegate(PropertyDefinition property, MetadataReader reader)
						{
							reader.ReadMethods(property);
						});
					}
				}
			}
		}

		public override PropertyDefinition Resolve()
		{
			return this;
		}
	}
}
