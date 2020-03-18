using System;

namespace Mono.Cecil
{
	public abstract class MemberReference : IMetadataTokenProvider
	{
		private string name;

		private TypeReference declaring_type;

		internal MetadataToken token;

		internal object projection;

		public virtual string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (IsWindowsRuntimeProjection && value != name)
				{
					throw new InvalidOperationException();
				}
				name = value;
			}
		}

		public abstract string FullName
		{
			get;
		}

		public virtual TypeReference DeclaringType
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

		public bool IsWindowsRuntimeProjection => projection != null;

		internal MemberReferenceProjection WindowsRuntimeProjection
		{
			get
			{
				return (MemberReferenceProjection)projection;
			}
			set
			{
				projection = value;
			}
		}

		internal bool HasImage
		{
			get
			{
				ModuleDefinition module = Module;
				return module?.HasImage ?? false;
			}
		}

		public virtual ModuleDefinition Module
		{
			get
			{
				if (declaring_type == null)
				{
					return null;
				}
				return declaring_type.Module;
			}
		}

		public virtual bool IsDefinition => false;

		public virtual bool ContainsGenericParameter
		{
			get
			{
				if (declaring_type != null)
				{
					return declaring_type.ContainsGenericParameter;
				}
				return false;
			}
		}

		internal MemberReference()
		{
		}

		internal MemberReference(string name)
		{
			this.name = (name ?? string.Empty);
		}

		internal string MemberFullName()
		{
			if (declaring_type == null)
			{
				return name;
			}
			return declaring_type.FullName + "::" + name;
		}

		public IMemberDefinition Resolve()
		{
			return ResolveDefinition();
		}

		protected abstract IMemberDefinition ResolveDefinition();

		public override string ToString()
		{
			return FullName;
		}
	}
}
