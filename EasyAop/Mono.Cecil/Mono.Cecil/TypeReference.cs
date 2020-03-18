using Mono.Cecil.Metadata;
using Mono.Collections.Generic;
using System;

namespace Mono.Cecil
{
	public class TypeReference : MemberReference, IGenericParameterProvider, IMetadataTokenProvider, IGenericContext
	{
		private string @namespace;

		private bool value_type;

		internal IMetadataScope scope;

		internal ModuleDefinition module;

		internal ElementType etype;

		private string fullname;

		protected Collection<GenericParameter> generic_parameters;

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
					throw new InvalidOperationException("Projected type reference name can't be changed.");
				}
				base.Name = value;
				ClearFullName();
			}
		}

		public virtual string Namespace
		{
			get
			{
				return @namespace;
			}
			set
			{
				if (base.IsWindowsRuntimeProjection && value != @namespace)
				{
					throw new InvalidOperationException("Projected type reference namespace can't be changed.");
				}
				@namespace = value;
				ClearFullName();
			}
		}

		public virtual bool IsValueType
		{
			get
			{
				return value_type;
			}
			set
			{
				value_type = value;
			}
		}

		public override ModuleDefinition Module
		{
			get
			{
				if (module != null)
				{
					return module;
				}
				TypeReference declaringType = DeclaringType;
				return declaringType?.Module;
			}
		}

		internal new TypeReferenceProjection WindowsRuntimeProjection
		{
			get
			{
				return (TypeReferenceProjection)base.projection;
			}
			set
			{
				base.projection = value;
			}
		}

		IGenericParameterProvider IGenericContext.Type
		{
			get
			{
				return this;
			}
		}

		IGenericParameterProvider IGenericContext.Method
		{
			get
			{
				return null;
			}
		}

		GenericParameterType IGenericParameterProvider.GenericParameterType
		{
			get
			{
				return GenericParameterType.Type;
			}
		}

		public virtual bool HasGenericParameters => !generic_parameters.IsNullOrEmpty();

		public virtual Collection<GenericParameter> GenericParameters
		{
			get
			{
				if (generic_parameters != null)
				{
					return generic_parameters;
				}
				return generic_parameters = new GenericParameterCollection(this);
			}
		}

		public virtual IMetadataScope Scope
		{
			get
			{
				TypeReference declaringType = DeclaringType;
				if (declaringType != null)
				{
					return declaringType.Scope;
				}
				return scope;
			}
			set
			{
				TypeReference declaringType = DeclaringType;
				if (declaringType != null)
				{
					if (base.IsWindowsRuntimeProjection && value != declaringType.Scope)
					{
						throw new InvalidOperationException("Projected type scope can't be changed.");
					}
					declaringType.Scope = value;
				}
				else
				{
					if (base.IsWindowsRuntimeProjection && value != scope)
					{
						throw new InvalidOperationException("Projected type scope can't be changed.");
					}
					scope = value;
				}
			}
		}

		public bool IsNested => DeclaringType != null;

		public override TypeReference DeclaringType
		{
			get
			{
				return base.DeclaringType;
			}
			set
			{
				if (base.IsWindowsRuntimeProjection && value != base.DeclaringType)
				{
					throw new InvalidOperationException("Projected type declaring type can't be changed.");
				}
				base.DeclaringType = value;
				ClearFullName();
			}
		}

		public override string FullName
		{
			get
			{
				if (fullname != null)
				{
					return fullname;
				}
				fullname = this.TypeFullName();
				if (IsNested)
				{
					fullname = DeclaringType.FullName + "/" + fullname;
				}
				return fullname;
			}
		}

		public virtual bool IsByReference => false;

		public virtual bool IsPointer => false;

		public virtual bool IsSentinel => false;

		public virtual bool IsArray => false;

		public virtual bool IsGenericParameter => false;

		public virtual bool IsGenericInstance => false;

		public virtual bool IsRequiredModifier => false;

		public virtual bool IsOptionalModifier => false;

		public virtual bool IsPinned => false;

		public virtual bool IsFunctionPointer => false;

		public virtual bool IsPrimitive => etype.IsPrimitive();

		public virtual MetadataType MetadataType
		{
			get
			{
				if (etype == ElementType.None)
				{
					if (!IsValueType)
					{
						return MetadataType.Class;
					}
					return MetadataType.ValueType;
				}
				return (MetadataType)etype;
			}
		}

		protected TypeReference(string @namespace, string name)
			: base(name)
		{
			this.@namespace = (@namespace ?? string.Empty);
			base.token = new MetadataToken(TokenType.TypeRef, 0);
		}

		public TypeReference(string @namespace, string name, ModuleDefinition module, IMetadataScope scope)
			: this(@namespace, name)
		{
			this.module = module;
			this.scope = scope;
		}

		public TypeReference(string @namespace, string name, ModuleDefinition module, IMetadataScope scope, bool valueType)
			: this(@namespace, name, module, scope)
		{
			value_type = valueType;
		}

		protected virtual void ClearFullName()
		{
			fullname = null;
		}

		public virtual TypeReference GetElementType()
		{
			return this;
		}

		protected override IMemberDefinition ResolveDefinition()
		{
			return Resolve();
		}

		public new virtual TypeDefinition Resolve()
		{
			ModuleDefinition moduleDefinition = Module;
			if (moduleDefinition == null)
			{
				throw new NotSupportedException();
			}
			return moduleDefinition.Resolve(this);
		}
	}
}
