using Mono.Cecil.Metadata;
using Mono.Collections.Generic;
using System;

namespace Mono.Cecil
{
	public sealed class GenericParameter : TypeReference, ICustomAttributeProvider, IMetadataTokenProvider
	{
		internal int position;

		internal GenericParameterType type;

		internal IGenericParameterProvider owner;

		private ushort attributes;

		private Collection<TypeReference> constraints;

		private Collection<CustomAttribute> custom_attributes;

		public GenericParameterAttributes Attributes
		{
			get
			{
				return (GenericParameterAttributes)attributes;
			}
			set
			{
				attributes = (ushort)value;
			}
		}

		public int Position => position;

		public GenericParameterType Type => type;

		public IGenericParameterProvider Owner => owner;

		public bool HasConstraints
		{
			get
			{
				if (constraints != null)
				{
					return constraints.Count > 0;
				}
				if (base.HasImage)
				{
					return Module.Read(this, (GenericParameter generic_parameter, MetadataReader reader) => reader.HasGenericConstraints(generic_parameter));
				}
				return false;
			}
		}

		public Collection<TypeReference> Constraints
		{
			get
			{
				if (constraints != null)
				{
					return constraints;
				}
				if (base.HasImage)
				{
					return Module.Read(ref constraints, this, (GenericParameter generic_parameter, MetadataReader reader) => reader.ReadGenericConstraints(generic_parameter));
				}
				return constraints = new Collection<TypeReference>();
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

		public override IMetadataScope Scope
		{
			get
			{
				if (owner == null)
				{
					return null;
				}
				if (owner.GenericParameterType != GenericParameterType.Method)
				{
					return ((TypeReference)owner).Scope;
				}
				return ((MethodReference)owner).DeclaringType.Scope;
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
				return owner as TypeReference;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public MethodReference DeclaringMethod => owner as MethodReference;

		public override ModuleDefinition Module => base.module ?? owner.Module;

		public override string Name
		{
			get
			{
				if (!string.IsNullOrEmpty(base.Name))
				{
					return base.Name;
				}
				string arg = (type == GenericParameterType.Method) ? "!!" : "!";
				return base.Name = arg + position;
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

		public override string FullName => Name;

		public override bool IsGenericParameter => true;

		public override bool ContainsGenericParameter => true;

		public override MetadataType MetadataType => (MetadataType)base.etype;

		public bool IsNonVariant
		{
			get
			{
				return attributes.GetMaskedAttributes(3, 0u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(3, 0u, value);
			}
		}

		public bool IsCovariant
		{
			get
			{
				return attributes.GetMaskedAttributes(3, 1u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(3, 1u, value);
			}
		}

		public bool IsContravariant
		{
			get
			{
				return attributes.GetMaskedAttributes(3, 2u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(3, 2u, value);
			}
		}

		public bool HasReferenceTypeConstraint
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

		public bool HasNotNullableValueTypeConstraint
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

		public bool HasDefaultConstructorConstraint
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

		public GenericParameter(IGenericParameterProvider owner)
			: this(string.Empty, owner)
		{
		}

		public GenericParameter(string name, IGenericParameterProvider owner)
			: base(string.Empty, name)
		{
			if (owner == null)
			{
				throw new ArgumentNullException();
			}
			position = -1;
			this.owner = owner;
			type = owner.GenericParameterType;
			base.etype = ConvertGenericParameterType(type);
			base.token = new MetadataToken(TokenType.GenericParam);
		}

		internal GenericParameter(int position, GenericParameterType type, ModuleDefinition module)
			: base(string.Empty, string.Empty)
		{
			Mixin.CheckModule(module);
			this.position = position;
			this.type = type;
			base.etype = ConvertGenericParameterType(type);
			base.module = module;
			base.token = new MetadataToken(TokenType.GenericParam);
		}

		private static ElementType ConvertGenericParameterType(GenericParameterType type)
		{
			switch (type)
			{
			case GenericParameterType.Type:
				return ElementType.Var;
			case GenericParameterType.Method:
				return ElementType.MVar;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public override TypeDefinition Resolve()
		{
			return null;
		}
	}
}
