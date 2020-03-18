using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System;

namespace Mono.Cecil
{
	public sealed class MethodDefinition : MethodReference, IMemberDefinition, ICustomAttributeProvider, IMetadataTokenProvider, ISecurityDeclarationProvider, ICustomDebugInformationProvider
	{
		private ushort attributes;

		private ushort impl_attributes;

		internal volatile bool sem_attrs_ready;

		internal MethodSemanticsAttributes sem_attrs;

		private Collection<CustomAttribute> custom_attributes;

		private Collection<SecurityDeclaration> security_declarations;

		internal uint rva;

		internal PInvokeInfo pinvoke;

		private Collection<MethodReference> overrides;

		internal MethodBody body;

		internal MethodDebugInformation debug_info;

		internal Collection<CustomDebugInformation> custom_infos;

		public override string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				if (base.IsWindowsRuntimeProjection && value != base.Name)
				{
					throw new InvalidOperationException();
				}
				base.Name = value;
			}
		}

		public MethodAttributes Attributes
		{
			get
			{
				return (MethodAttributes)attributes;
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

		public MethodImplAttributes ImplAttributes
		{
			get
			{
				return (MethodImplAttributes)impl_attributes;
			}
			set
			{
				if (base.IsWindowsRuntimeProjection && (uint)value != impl_attributes)
				{
					throw new InvalidOperationException();
				}
				impl_attributes = (ushort)value;
			}
		}

		public MethodSemanticsAttributes SemanticsAttributes
		{
			get
			{
				if (sem_attrs_ready)
				{
					return sem_attrs;
				}
				if (base.HasImage)
				{
					ReadSemantics();
					return sem_attrs;
				}
				sem_attrs = MethodSemanticsAttributes.None;
				sem_attrs_ready = true;
				return sem_attrs;
			}
			set
			{
				sem_attrs = value;
			}
		}

		internal new MethodDefinitionProjection WindowsRuntimeProjection
		{
			get
			{
				return (MethodDefinitionProjection)base.projection;
			}
			set
			{
				base.projection = value;
			}
		}

		public bool HasSecurityDeclarations
		{
			get
			{
				if (security_declarations != null)
				{
					return security_declarations.Count > 0;
				}
				return this.GetHasSecurityDeclarations(Module);
			}
		}

		public Collection<SecurityDeclaration> SecurityDeclarations => security_declarations ?? this.GetSecurityDeclarations(ref security_declarations, Module);

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

		public int RVA => (int)rva;

		public bool HasBody
		{
			get
			{
				if ((attributes & 0x400) == 0 && (attributes & 0x2000) == 0 && (impl_attributes & 0x1000) == 0 && (impl_attributes & 1) == 0 && (impl_attributes & 4) == 0)
				{
					return (impl_attributes & 3) == 0;
				}
				return false;
			}
		}

		public MethodBody Body
		{
			get
			{
				MethodBody methodBody = body;
				if (methodBody != null)
				{
					return methodBody;
				}
				if (!HasBody)
				{
					return null;
				}
				if (base.HasImage && rva != 0)
				{
					return Module.Read(ref body, this, (MethodDefinition method, MetadataReader reader) => reader.ReadMethodBody(method));
				}
				return body = new MethodBody(this);
			}
			set
			{
				ModuleDefinition module = Module;
				if (module == null)
				{
					body = value;
				}
				else
				{
					lock (module.SyncRoot)
					{
						body = value;
						if (value == null)
						{
							debug_info = null;
						}
					}
				}
			}
		}

		public MethodDebugInformation DebugInformation
		{
			get
			{
				Mixin.Read(Body);
				if (debug_info != null)
				{
					return debug_info;
				}
				return debug_info ?? (debug_info = new MethodDebugInformation(this));
			}
		}

		public bool HasPInvokeInfo
		{
			get
			{
				if (pinvoke != null)
				{
					return true;
				}
				return IsPInvokeImpl;
			}
		}

		public PInvokeInfo PInvokeInfo
		{
			get
			{
				if (pinvoke != null)
				{
					return pinvoke;
				}
				if (base.HasImage && IsPInvokeImpl)
				{
					return Module.Read(ref pinvoke, this, (MethodDefinition method, MetadataReader reader) => reader.ReadPInvokeInfo(method));
				}
				return null;
			}
			set
			{
				IsPInvokeImpl = true;
				pinvoke = value;
			}
		}

		public bool HasOverrides
		{
			get
			{
				if (overrides != null)
				{
					return overrides.Count > 0;
				}
				if (base.HasImage)
				{
					return Module.Read(this, (MethodDefinition method, MetadataReader reader) => reader.HasOverrides(method));
				}
				return false;
			}
		}

		public Collection<MethodReference> Overrides
		{
			get
			{
				if (overrides != null)
				{
					return overrides;
				}
				if (base.HasImage)
				{
					return Module.Read(ref overrides, this, (MethodDefinition method, MetadataReader reader) => reader.ReadOverrides(method));
				}
				return overrides = new Collection<MethodReference>();
			}
		}

		public override bool HasGenericParameters
		{
			get
			{
				if (base.generic_parameters != null)
				{
					return base.generic_parameters.Count > 0;
				}
				return this.GetHasGenericParameters(Module);
			}
		}

		public override Collection<GenericParameter> GenericParameters => base.generic_parameters ?? this.GetGenericParameters(ref base.generic_parameters, Module);

		public bool HasCustomDebugInformations
		{
			get
			{
				Mixin.Read(Body);
				return !custom_infos.IsNullOrEmpty();
			}
		}

		public Collection<CustomDebugInformation> CustomDebugInformations
		{
			get
			{
				Mixin.Read(Body);
				return custom_infos ?? (custom_infos = new Collection<CustomDebugInformation>());
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

		public bool IsFinal
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

		public bool IsVirtual
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

		public bool IsHideBySig
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

		public bool IsReuseSlot
		{
			get
			{
				return attributes.GetMaskedAttributes(256, 0u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(256, 0u, value);
			}
		}

		public bool IsNewSlot
		{
			get
			{
				return attributes.GetMaskedAttributes(256, 256u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(256, 256u, value);
			}
		}

		public bool IsCheckAccessOnOverride
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

		public bool IsAbstract
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

		public bool IsSpecialName
		{
			get
			{
				return attributes.GetAttributes(2048);
			}
			set
			{
				attributes = attributes.SetAttributes(2048, value);
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

		public bool IsUnmanagedExport
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

		public bool IsRuntimeSpecialName
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

		public bool HasSecurity
		{
			get
			{
				return attributes.GetAttributes(16384);
			}
			set
			{
				attributes = attributes.SetAttributes(16384, value);
			}
		}

		public bool IsIL
		{
			get
			{
				return impl_attributes.GetMaskedAttributes(3, 0u);
			}
			set
			{
				impl_attributes = impl_attributes.SetMaskedAttributes(3, 0u, value);
			}
		}

		public bool IsNative
		{
			get
			{
				return impl_attributes.GetMaskedAttributes(3, 1u);
			}
			set
			{
				impl_attributes = impl_attributes.SetMaskedAttributes(3, 1u, value);
			}
		}

		public bool IsRuntime
		{
			get
			{
				return impl_attributes.GetMaskedAttributes(3, 3u);
			}
			set
			{
				impl_attributes = impl_attributes.SetMaskedAttributes(3, 3u, value);
			}
		}

		public bool IsUnmanaged
		{
			get
			{
				return impl_attributes.GetMaskedAttributes(4, 4u);
			}
			set
			{
				impl_attributes = impl_attributes.SetMaskedAttributes(4, 4u, value);
			}
		}

		public bool IsManaged
		{
			get
			{
				return impl_attributes.GetMaskedAttributes(4, 0u);
			}
			set
			{
				impl_attributes = impl_attributes.SetMaskedAttributes(4, 0u, value);
			}
		}

		public bool IsForwardRef
		{
			get
			{
				return impl_attributes.GetAttributes(16);
			}
			set
			{
				impl_attributes = impl_attributes.SetAttributes(16, value);
			}
		}

		public bool IsPreserveSig
		{
			get
			{
				return impl_attributes.GetAttributes(128);
			}
			set
			{
				impl_attributes = impl_attributes.SetAttributes(128, value);
			}
		}

		public bool IsInternalCall
		{
			get
			{
				return impl_attributes.GetAttributes(4096);
			}
			set
			{
				impl_attributes = impl_attributes.SetAttributes(4096, value);
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return impl_attributes.GetAttributes(32);
			}
			set
			{
				impl_attributes = impl_attributes.SetAttributes(32, value);
			}
		}

		public bool NoInlining
		{
			get
			{
				return impl_attributes.GetAttributes(8);
			}
			set
			{
				impl_attributes = impl_attributes.SetAttributes(8, value);
			}
		}

		public bool NoOptimization
		{
			get
			{
				return impl_attributes.GetAttributes(64);
			}
			set
			{
				impl_attributes = impl_attributes.SetAttributes(64, value);
			}
		}

		public bool AggressiveInlining
		{
			get
			{
				return impl_attributes.GetAttributes(256);
			}
			set
			{
				impl_attributes = impl_attributes.SetAttributes(256, value);
			}
		}

		public bool IsSetter
		{
			get
			{
				return this.GetSemantics(MethodSemanticsAttributes.Setter);
			}
			set
			{
				this.SetSemantics(MethodSemanticsAttributes.Setter, value);
			}
		}

		public bool IsGetter
		{
			get
			{
				return this.GetSemantics(MethodSemanticsAttributes.Getter);
			}
			set
			{
				this.SetSemantics(MethodSemanticsAttributes.Getter, value);
			}
		}

		public bool IsOther
		{
			get
			{
				return this.GetSemantics(MethodSemanticsAttributes.Other);
			}
			set
			{
				this.SetSemantics(MethodSemanticsAttributes.Other, value);
			}
		}

		public bool IsAddOn
		{
			get
			{
				return this.GetSemantics(MethodSemanticsAttributes.AddOn);
			}
			set
			{
				this.SetSemantics(MethodSemanticsAttributes.AddOn, value);
			}
		}

		public bool IsRemoveOn
		{
			get
			{
				return this.GetSemantics(MethodSemanticsAttributes.RemoveOn);
			}
			set
			{
				this.SetSemantics(MethodSemanticsAttributes.RemoveOn, value);
			}
		}

		public bool IsFire
		{
			get
			{
				return this.GetSemantics(MethodSemanticsAttributes.Fire);
			}
			set
			{
				this.SetSemantics(MethodSemanticsAttributes.Fire, value);
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

		public bool IsConstructor
		{
			get
			{
				if (IsRuntimeSpecialName && IsSpecialName)
				{
					if (!(Name == ".cctor"))
					{
						return Name == ".ctor";
					}
					return true;
				}
				return false;
			}
		}

		public override bool IsDefinition => true;

		internal void ReadSemantics()
		{
			if (!sem_attrs_ready)
			{
				ModuleDefinition module = Module;
				if (module != null && module.HasImage)
				{
					module.Read(this, delegate(MethodDefinition method, MetadataReader reader)
					{
						reader.ReadAllSemantics(method);
					});
				}
			}
		}

		internal MethodDefinition()
		{
			base.token = new MetadataToken(TokenType.Method);
		}

		public MethodDefinition(string name, MethodAttributes attributes, TypeReference returnType)
			: base(name, returnType)
		{
			this.attributes = (ushort)attributes;
			HasThis = !IsStatic;
			base.token = new MetadataToken(TokenType.Method);
		}

		public override MethodDefinition Resolve()
		{
			return this;
		}
	}
}
