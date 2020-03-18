using System;

namespace Mono.Cecil
{
	public abstract class TypeSpecification : TypeReference
	{
		private readonly TypeReference element_type;

		public TypeReference ElementType => element_type;

		public override string Name
		{
			get
			{
				return element_type.Name;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override string Namespace
		{
			get
			{
				return element_type.Namespace;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override IMetadataScope Scope
		{
			get
			{
				return element_type.Scope;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override ModuleDefinition Module => element_type.Module;

		public override string FullName => element_type.FullName;

		public override bool ContainsGenericParameter => element_type.ContainsGenericParameter;

		public override MetadataType MetadataType => (MetadataType)base.etype;

		internal TypeSpecification(TypeReference type)
			: base(null, null)
		{
			element_type = type;
			base.token = new MetadataToken(TokenType.TypeSpec);
		}

		public override TypeReference GetElementType()
		{
			return element_type.GetElementType();
		}
	}
}
