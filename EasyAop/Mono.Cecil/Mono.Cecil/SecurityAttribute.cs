using Mono.Collections.Generic;
using System;
using System.Diagnostics;

namespace Mono.Cecil
{
	[DebuggerDisplay("{AttributeType}")]
	public sealed class SecurityAttribute : ICustomAttribute
	{
		private TypeReference attribute_type;

		internal Collection<CustomAttributeNamedArgument> fields;

		internal Collection<CustomAttributeNamedArgument> properties;

		public TypeReference AttributeType
		{
			get
			{
				return attribute_type;
			}
			set
			{
				attribute_type = value;
			}
		}

		public bool HasFields => !fields.IsNullOrEmpty();

		public Collection<CustomAttributeNamedArgument> Fields => fields ?? (fields = new Collection<CustomAttributeNamedArgument>());

		public bool HasProperties => !properties.IsNullOrEmpty();

		public Collection<CustomAttributeNamedArgument> Properties => properties ?? (properties = new Collection<CustomAttributeNamedArgument>());

		bool ICustomAttribute.HasConstructorArguments
		{
			get
			{
				return false;
			}
		}

		Collection<CustomAttributeArgument> ICustomAttribute.ConstructorArguments
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public SecurityAttribute(TypeReference attributeType)
		{
			attribute_type = attributeType;
		}
	}
}
