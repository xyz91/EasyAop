using System;

namespace Mono.Cecil.Cil
{
	public struct VariableIndex
	{
		private readonly VariableDefinition variable;

		private readonly int? index;

		public int Index
		{
			get
			{
				if (variable != null)
				{
					return variable.Index;
				}
				if (index.HasValue)
				{
					return index.Value;
				}
				throw new NotSupportedException();
			}
		}

		public VariableIndex(VariableDefinition variable)
		{
			if (variable == null)
			{
				throw new ArgumentNullException("variable");
			}
			this.variable = variable;
			index = null;
		}

		public VariableIndex(int index)
		{
			variable = null;
			this.index = index;
		}
	}
}
