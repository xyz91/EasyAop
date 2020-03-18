namespace Mono.Cecil
{
	public sealed class ExportedType : IMetadataTokenProvider
	{
		private string @namespace;

		private string name;

		private uint attributes;

		private IMetadataScope scope;

		private ModuleDefinition module;

		private int identifier;

		private ExportedType declaring_type;

		internal MetadataToken token;

		public string Namespace
		{
			get
			{
				return @namespace;
			}
			set
			{
				@namespace = value;
			}
		}

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public TypeAttributes Attributes
		{
			get
			{
				return (TypeAttributes)attributes;
			}
			set
			{
				attributes = (uint)value;
			}
		}

		public IMetadataScope Scope
		{
			get
			{
				if (declaring_type != null)
				{
					return declaring_type.Scope;
				}
				return scope;
			}
			set
			{
				if (declaring_type != null)
				{
					declaring_type.Scope = value;
				}
				else
				{
					scope = value;
				}
			}
		}

		public ExportedType DeclaringType
		{
			get
			{
				return declaring_type;
			}
			set
			{
				declaring_type = value;
			}
		}

		public MetadataToken MetadataToken
		{
			get
			{
				return token;
			}
			set
			{
				token = value;
			}
		}

		public int Identifier
		{
			get
			{
				return identifier;
			}
			set
			{
				identifier = value;
			}
		}

		public bool IsNotPublic
		{
			get
			{
				return attributes.GetMaskedAttributes(7u, 0u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7u, 0u, value);
			}
		}

		public bool IsPublic
		{
			get
			{
				return attributes.GetMaskedAttributes(7u, 1u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7u, 1u, value);
			}
		}

		public bool IsNestedPublic
		{
			get
			{
				return attributes.GetMaskedAttributes(7u, 2u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7u, 2u, value);
			}
		}

		public bool IsNestedPrivate
		{
			get
			{
				return attributes.GetMaskedAttributes(7u, 3u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7u, 3u, value);
			}
		}

		public bool IsNestedFamily
		{
			get
			{
				return attributes.GetMaskedAttributes(7u, 4u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7u, 4u, value);
			}
		}

		public bool IsNestedAssembly
		{
			get
			{
				return attributes.GetMaskedAttributes(7u, 5u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7u, 5u, value);
			}
		}

		public bool IsNestedFamilyAndAssembly
		{
			get
			{
				return attributes.GetMaskedAttributes(7u, 6u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7u, 6u, value);
			}
		}

		public bool IsNestedFamilyOrAssembly
		{
			get
			{
				return attributes.GetMaskedAttributes(7u, 7u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(7u, 7u, value);
			}
		}

		public bool IsAutoLayout
		{
			get
			{
				return attributes.GetMaskedAttributes(24u, 0u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(24u, 0u, value);
			}
		}

		public bool IsSequentialLayout
		{
			get
			{
				return attributes.GetMaskedAttributes(24u, 8u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(24u, 8u, value);
			}
		}

		public bool IsExplicitLayout
		{
			get
			{
				return attributes.GetMaskedAttributes(24u, 16u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(24u, 16u, value);
			}
		}

		public bool IsClass
		{
			get
			{
				return attributes.GetMaskedAttributes(32u, 0u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(32u, 0u, value);
			}
		}

		public bool IsInterface
		{
			get
			{
				return attributes.GetMaskedAttributes(32u, 32u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(32u, 32u, value);
			}
		}

		public bool IsAbstract
		{
			get
			{
				return attributes.GetAttributes(128u);
			}
			set
			{
				attributes = attributes.SetAttributes(128u, value);
			}
		}

		public bool IsSealed
		{
			get
			{
				return attributes.GetAttributes(256u);
			}
			set
			{
				attributes = attributes.SetAttributes(256u, value);
			}
		}

		public bool IsSpecialName
		{
			get
			{
				return attributes.GetAttributes(1024u);
			}
			set
			{
				attributes = attributes.SetAttributes(1024u, value);
			}
		}

		public bool IsImport
		{
			get
			{
				return attributes.GetAttributes(4096u);
			}
			set
			{
				attributes = attributes.SetAttributes(4096u, value);
			}
		}

		public bool IsSerializable
		{
			get
			{
				return attributes.GetAttributes(8192u);
			}
			set
			{
				attributes = attributes.SetAttributes(8192u, value);
			}
		}

		public bool IsAnsiClass
		{
			get
			{
				return attributes.GetMaskedAttributes(196608u, 0u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(196608u, 0u, value);
			}
		}

		public bool IsUnicodeClass
		{
			get
			{
				return attributes.GetMaskedAttributes(196608u, 65536u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(196608u, 65536u, value);
			}
		}

		public bool IsAutoClass
		{
			get
			{
				return attributes.GetMaskedAttributes(196608u, 131072u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(196608u, 131072u, value);
			}
		}

		public bool IsBeforeFieldInit
		{
			get
			{
				return attributes.GetAttributes(1048576u);
			}
			set
			{
				attributes = attributes.SetAttributes(1048576u, value);
			}
		}

		public bool IsRuntimeSpecialName
		{
			get
			{
				return attributes.GetAttributes(2048u);
			}
			set
			{
				attributes = attributes.SetAttributes(2048u, value);
			}
		}

		public bool HasSecurity
		{
			get
			{
				return attributes.GetAttributes(262144u);
			}
			set
			{
				attributes = attributes.SetAttributes(262144u, value);
			}
		}

		public bool IsForwarder
		{
			get
			{
				return attributes.GetAttributes(2097152u);
			}
			set
			{
				attributes = attributes.SetAttributes(2097152u, value);
			}
		}

		public string FullName
		{
			get
			{
				string text = string.IsNullOrEmpty(@namespace) ? name : (@namespace + "." + name);
				if (declaring_type != null)
				{
					return declaring_type.FullName + "/" + text;
				}
				return text;
			}
		}

		public ExportedType(string @namespace, string name, ModuleDefinition module, IMetadataScope scope)
		{
			this.@namespace = @namespace;
			this.name = name;
			this.scope = scope;
			this.module = module;
		}

		public override string ToString()
		{
			return FullName;
		}

		public TypeDefinition Resolve()
		{
			return module.Resolve(CreateReference());
		}

		internal TypeReference CreateReference()
		{
			return new TypeReference(@namespace, name, module, scope)
			{
				DeclaringType = ((declaring_type != null) ? declaring_type.CreateReference() : null)
			};
		}
	}
}
