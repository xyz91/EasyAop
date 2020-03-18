using Mono.Collections.Generic;
using System;
using System.Diagnostics;

namespace Mono.Cecil
{
	[DebuggerDisplay("{AttributeType}")]
	public sealed class CustomAttribute : ICustomAttribute
	{
		internal CustomAttributeValueProjection projection;

		internal readonly uint signature;

		internal bool resolved;

		private MethodReference constructor;

		private byte[] blob;

		internal Collection<CustomAttributeArgument> arguments;

		internal Collection<CustomAttributeNamedArgument> fields;

		internal Collection<CustomAttributeNamedArgument> properties;

		public MethodReference Constructor
		{
			get
			{
				return constructor;
			}
			set
			{
				constructor = value;
			}
		}

		public TypeReference AttributeType => constructor.DeclaringType;

		public bool IsResolved => resolved;

		public bool HasConstructorArguments
		{
			get
			{
				Resolve();
				return !arguments.IsNullOrEmpty();
			}
		}

		public Collection<CustomAttributeArgument> ConstructorArguments
		{
			get
			{
				Resolve();
				return arguments ?? (arguments = new Collection<CustomAttributeArgument>());
			}
		}

		public bool HasFields
		{
			get
			{
				Resolve();
				return !fields.IsNullOrEmpty();
			}
		}

		public Collection<CustomAttributeNamedArgument> Fields
		{
			get
			{
				Resolve();
				return fields ?? (fields = new Collection<CustomAttributeNamedArgument>());
			}
		}

		public bool HasProperties
		{
			get
			{
				Resolve();
				return !properties.IsNullOrEmpty();
			}
		}

		public Collection<CustomAttributeNamedArgument> Properties
		{
			get
			{
				Resolve();
				return properties ?? (properties = new Collection<CustomAttributeNamedArgument>());
			}
		}

		internal bool HasImage
		{
			get
			{
				if (constructor != null)
				{
					return constructor.HasImage;
				}
				return false;
			}
		}

		internal ModuleDefinition Module => constructor.Module;

		internal CustomAttribute(uint signature, MethodReference constructor)
		{
			this.signature = signature;
			this.constructor = constructor;
			resolved = false;
		}

		public CustomAttribute(MethodReference constructor)
		{
			this.constructor = constructor;
			resolved = true;
		}

		public CustomAttribute(MethodReference constructor, byte[] blob)
		{
			this.constructor = constructor;
			resolved = false;
			this.blob = blob;
		}

		public byte[] GetBlob()
		{
			if (blob != null)
			{
				return blob;
			}
			if (!HasImage)
			{
				throw new NotSupportedException();
			}
			return Module.Read(ref blob, this, (CustomAttribute attribute, MetadataReader reader) => reader.ReadCustomAttributeBlob(attribute.signature));
		}

		private void Resolve()
		{
			if (!resolved && HasImage)
			{
				Module.Read(this, delegate(CustomAttribute attribute, MetadataReader reader)
				{
					try
					{
						reader.ReadCustomAttributeSignature(attribute);
						resolved = true;
					}
					catch (ResolutionException)
					{
						if (arguments != null)
						{
							arguments.Clear();
						}
						if (fields != null)
						{
							fields.Clear();
						}
						if (properties != null)
						{
							properties.Clear();
						}
						resolved = false;
					}
				});
			}
		}
	}
}
