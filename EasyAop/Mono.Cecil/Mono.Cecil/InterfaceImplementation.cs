using Mono.Collections.Generic;

namespace Mono.Cecil
{
	public sealed class InterfaceImplementation : ICustomAttributeProvider, IMetadataTokenProvider
	{
		internal TypeDefinition type;

		internal MetadataToken token;

		private TypeReference interface_type;

		private Collection<CustomAttribute> custom_attributes;

		public TypeReference InterfaceType
		{
			get
			{
				return interface_type;
			}
			set
			{
				interface_type = value;
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
				if (type == null)
				{
					return false;
				}
				return this.GetHasCustomAttributes(type.Module);
			}
		}

		public Collection<CustomAttribute> CustomAttributes
		{
			get
			{
				if (type == null)
				{
					return custom_attributes = new Collection<CustomAttribute>();
				}
				return custom_attributes ?? this.GetCustomAttributes(ref custom_attributes, type.Module);
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

		internal InterfaceImplementation(TypeReference interfaceType, MetadataToken token)
		{
			interface_type = interfaceType;
			this.token = token;
		}

		public InterfaceImplementation(TypeReference interfaceType)
		{
			Mixin.CheckType(interfaceType, Mixin.Argument.interfaceType);
			interface_type = interfaceType;
			token = new MetadataToken(TokenType.InterfaceImpl);
		}
	}
}
