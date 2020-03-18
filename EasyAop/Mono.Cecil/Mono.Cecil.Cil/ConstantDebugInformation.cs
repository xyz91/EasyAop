using System;

namespace Mono.Cecil.Cil
{
	public sealed class ConstantDebugInformation : DebugInformation
	{
		private string name;

		private TypeReference constant_type;

		private object value;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public TypeReference ConstantType
		{
			get
			{
				return constant_type;
			}
			set
			{
				constant_type = value;
			}
		}

		public object Value
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value;
			}
		}

		public ConstantDebugInformation(string name, TypeReference constant_type, object value)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			this.name = name;
			this.constant_type = constant_type;
			this.value = value;
			base.token = new MetadataToken(TokenType.LocalConstant);
		}
	}
}
