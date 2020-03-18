using System;

namespace Mono.Cecil
{
	public class FieldReference : MemberReference
	{
		private TypeReference field_type;

		public TypeReference FieldType
		{
			get
			{
				return field_type;
			}
			set
			{
				field_type = value;
			}
		}

		public override string FullName => field_type.FullName + " " + base.MemberFullName();

		public override bool ContainsGenericParameter
		{
			get
			{
				if (!field_type.ContainsGenericParameter)
				{
					return base.ContainsGenericParameter;
				}
				return true;
			}
		}

		internal FieldReference()
		{
			base.token = new MetadataToken(TokenType.MemberRef);
		}

		public FieldReference(string name, TypeReference fieldType)
			: base(name)
		{
			Mixin.CheckType(fieldType, Mixin.Argument.fieldType);
			field_type = fieldType;
			base.token = new MetadataToken(TokenType.MemberRef);
		}

		public FieldReference(string name, TypeReference fieldType, TypeReference declaringType)
			: this(name, fieldType)
		{
			Mixin.CheckType(declaringType, Mixin.Argument.declaringType);
			DeclaringType = declaringType;
		}

		protected override IMemberDefinition ResolveDefinition()
		{
			return Resolve();
		}

		public new virtual FieldDefinition Resolve()
		{
			ModuleDefinition module = Module;
			if (module == null)
			{
				throw new NotSupportedException();
			}
			return module.Resolve(this);
		}
	}
}
