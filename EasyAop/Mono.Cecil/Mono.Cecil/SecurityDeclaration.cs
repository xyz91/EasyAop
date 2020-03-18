using Mono.Collections.Generic;
using System;

namespace Mono.Cecil
{
	public sealed class SecurityDeclaration
	{
		internal readonly uint signature;

		private byte[] blob;

		private readonly ModuleDefinition module;

		internal bool resolved;

		private SecurityAction action;

		internal Collection<SecurityAttribute> security_attributes;

		public SecurityAction Action
		{
			get
			{
				return action;
			}
			set
			{
				action = value;
			}
		}

		public bool HasSecurityAttributes
		{
			get
			{
				Resolve();
				return !security_attributes.IsNullOrEmpty();
			}
		}

		public Collection<SecurityAttribute> SecurityAttributes
		{
			get
			{
				Resolve();
				return security_attributes ?? (security_attributes = new Collection<SecurityAttribute>());
			}
		}

		internal bool HasImage
		{
			get
			{
				if (module != null)
				{
					return module.HasImage;
				}
				return false;
			}
		}

		internal SecurityDeclaration(SecurityAction action, uint signature, ModuleDefinition module)
		{
			this.action = action;
			this.signature = signature;
			this.module = module;
		}

		public SecurityDeclaration(SecurityAction action)
		{
			this.action = action;
			resolved = true;
		}

		public SecurityDeclaration(SecurityAction action, byte[] blob)
		{
			this.action = action;
			resolved = false;
			this.blob = blob;
		}

		public byte[] GetBlob()
		{
			if (blob != null)
			{
				return blob;
			}
			if (HasImage && signature != 0)
			{
				return blob = module.Read(this, (SecurityDeclaration declaration, MetadataReader reader) => reader.ReadSecurityDeclarationBlob(declaration.signature));
			}
			throw new NotSupportedException();
		}

		private void Resolve()
		{
			if (!resolved && HasImage)
			{
				module.Read(this, delegate(SecurityDeclaration declaration, MetadataReader reader)
				{
					reader.ReadSecurityDeclarationSignature(declaration);
				});
				resolved = true;
			}
		}
	}
}
