using System;

namespace Mono.Cecil.Cil
{
	public sealed class VariableDebugInformation : DebugInformation
	{
		private string name;

		private ushort attributes;

		internal VariableIndex index;

		public int Index => index.Index;

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

		public VariableAttributes Attributes
		{
			get
			{
				return (VariableAttributes)attributes;
			}
			set
			{
				attributes = (ushort)value;
			}
		}

		public bool IsDebuggerHidden
		{
			get
			{
				return attributes.GetAttributes(1);
			}
			set
			{
				attributes = attributes.SetAttributes(1, value);
			}
		}

		internal VariableDebugInformation(int index, string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			this.index = new VariableIndex(index);
			this.name = name;
		}

		public VariableDebugInformation(VariableDefinition variable, string name)
		{
			if (variable == null)
			{
				throw new ArgumentNullException("variable");
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			index = new VariableIndex(variable);
			this.name = name;
			base.token = new MetadataToken(TokenType.LocalVariable);
		}
	}
}
