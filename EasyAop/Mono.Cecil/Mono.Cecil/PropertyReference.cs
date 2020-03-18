using Mono.Collections.Generic;

namespace Mono.Cecil
{
	public abstract class PropertyReference : MemberReference
	{
		private TypeReference property_type;

		public TypeReference PropertyType
		{
			get
			{
				return property_type;
			}
			set
			{
				property_type = value;
			}
		}

		public abstract Collection<ParameterDefinition> Parameters
		{
			get;
		}

		internal PropertyReference(string name, TypeReference propertyType)
			: base(name)
		{
			Mixin.CheckType(propertyType, Mixin.Argument.propertyType);
			property_type = propertyType;
		}

		protected override IMemberDefinition ResolveDefinition()
		{
			return Resolve();
		}

		public new abstract PropertyDefinition Resolve();
	}
}
