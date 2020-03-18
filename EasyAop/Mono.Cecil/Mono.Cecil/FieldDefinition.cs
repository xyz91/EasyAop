using Mono.Collections.Generic;
using System;

namespace Mono.Cecil
{
	public sealed class FieldDefinition : FieldReference, IMemberDefinition, ICustomAttributeProvider, IMetadataTokenProvider, IConstantProvider, IMarshalInfoProvider
	{
		private ushort attributes;

		private Collection<CustomAttribute> custom_attributes;

		private int offset = -2;

		internal int rva = -2;

		private byte[] initial_value;

		private object constant = Mixin.NotResolved;

		private MarshalInfo marshal_info;

		public bool HasLayoutInfo
		{
			get
			{
				if (offset >= 0)
				{
					return true;
				}
				ResolveLayout();
				return offset >= 0;
			}
		}

		public int Offset
		{
			get
			{
				if (offset >= 0)
				{
					return offset;
				}
				ResolveLayout();
				if (offset < 0)
				{
					return -1;
				}
				return offset;
			}
			set
			{
				offset = value;
			}
		}

		internal new FieldDefinitionProjection WindowsRuntimeProjection
		{
			get
			{
				return (FieldDefinitionProjection)base.projection;
			}
			set
			{
				base.projection = value;
			}
		}

		public int RVA
		{
			get
			{
				if (rva > 0)
				{
					return rva;
				}
				ResolveRVA();
				if (rva <= 0)
				{
					return 0;
				}
				return rva;
			}
		}

		public byte[] InitialValue
		{
			get
			{
				if (initial_value != null)
				{
					return initial_value;
				}
				ResolveRVA();
				if (initial_value == null)
				{
					initial_value = Empty<byte>.Array;
				}
				return initial_value;
			}
			set
			{
				initial_value = value;
				rva = 0;
			}
		}

		public FieldAttributes Attributes
		{
			get
			{
				return (FieldAttributes)attributes;
			}
			set
			{
				if (base.IsWindowsRuntimeProjection && (uint)value != attributes)
				{
					throw new InvalidOperationException();
				}
				attributes = (ushort)value;
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

		public bool HasMarshalInfo
		{
			get
			{
				if (marshal_info != null)
				{
					return true;
				}
				return this.GetHasMarshalInfo(Module);
			}
		}

		public MarshalInfo MarshalInfo
		{
			get
			{
				return marshal_info ?? this.GetMarshalInfo(ref marshal_info, Module);
			}
			set
			{
				marshal_info = value;
			}
		}

		public bool IsCompilerControlled
		{
			get
			{
				return attributes.GetMaskedAttributes(7, 0u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7, 0u, value);
			}
		}

		public bool IsPrivate
		{
			get
			{
				return attributes.GetMaskedAttributes(7, 1u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7, 1u, value);
			}
		}

		public bool IsFamilyAndAssembly
		{
			get
			{
				return attributes.GetMaskedAttributes(7, 2u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7, 2u, value);
			}
		}

		public bool IsAssembly
		{
			get
			{
				return attributes.GetMaskedAttributes(7, 3u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7, 3u, value);
			}
		}

		public bool IsFamily
		{
			get
			{
				return attributes.GetMaskedAttributes(7, 4u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7, 4u, value);
			}
		}

		public bool IsFamilyOrAssembly
		{
			get
			{
				return attributes.GetMaskedAttributes(7, 5u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7, 5u, value);
			}
		}

		public bool IsPublic
		{
			get
			{
				return attributes.GetMaskedAttributes(7, 6u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7, 6u, value);
			}
		}

		public bool IsStatic
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

		public bool IsInitOnly
		{
			get
			{
				return attributes.GetAttributes(32);
			}
			set
			{
				attributes = attributes.SetAttributes(32, value);
			}
		}

		public bool IsLiteral
		{
			get
			{
				return attributes.GetAttributes(64);
			}
			set
			{
				attributes = attributes.SetAttributes(64, value);
			}
		}

		public bool IsNotSerialized
		{
			get
			{
				return attributes.GetAttributes(128);
			}
			set
			{
				attributes = attributes.SetAttributes(128, value);
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

		public bool IsPInvokeImpl
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
				return attributes.GetAttributes(32768);
			}
			set
			{
				attributes = attributes.SetAttributes(32768, value);
			}
		}

		public override bool IsDefinition => true;

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

		private void ResolveLayout()
		{
			if (offset == -2)
			{
				if (!base.HasImage)
				{
					offset = -1;
				}
				else
				{
					offset = Module.Read(this, (FieldDefinition field, MetadataReader reader) => reader.ReadFieldLayout(field));
				}
			}
		}

		private void ResolveRVA()
		{
			if (rva == -2 && base.HasImage)
			{
				rva = Module.Read(this, (FieldDefinition field, MetadataReader reader) => reader.ReadFieldRVA(field));
			}
		}

		public FieldDefinition(string name, FieldAttributes attributes, TypeReference fieldType)
			: base(name, fieldType)
		{
			this.attributes = (ushort)attributes;
		}

		public override FieldDefinition Resolve()
		{
			return this;
		}
	}
}
