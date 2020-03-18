using Mono.Collections.Generic;
using System;

namespace Mono.Cecil.Cil
{
	public sealed class ScopeDebugInformation : DebugInformation
	{
		internal InstructionOffset start;

		internal InstructionOffset end;

		internal ImportDebugInformation import;

		internal Collection<ScopeDebugInformation> scopes;

		internal Collection<VariableDebugInformation> variables;

		internal Collection<ConstantDebugInformation> constants;

		public InstructionOffset Start
		{
			get
			{
				return start;
			}
			set
			{
				start = value;
			}
		}

		public InstructionOffset End
		{
			get
			{
				return end;
			}
			set
			{
				end = value;
			}
		}

		public ImportDebugInformation Import
		{
			get
			{
				return import;
			}
			set
			{
				import = value;
			}
		}

		public bool HasScopes => !scopes.IsNullOrEmpty();

		public Collection<ScopeDebugInformation> Scopes => scopes ?? (scopes = new Collection<ScopeDebugInformation>());

		public bool HasVariables => !variables.IsNullOrEmpty();

		public Collection<VariableDebugInformation> Variables => variables ?? (variables = new Collection<VariableDebugInformation>());

		public bool HasConstants => !constants.IsNullOrEmpty();

		public Collection<ConstantDebugInformation> Constants => constants ?? (constants = new Collection<ConstantDebugInformation>());

		internal ScopeDebugInformation()
		{
			base.token = new MetadataToken(TokenType.LocalScope);
		}

		public ScopeDebugInformation(Instruction start, Instruction end)
			: this()
		{
			if (start == null)
			{
				throw new ArgumentNullException("start");
			}
			this.start = new InstructionOffset(start);
			if (end != null)
			{
				this.end = new InstructionOffset(end);
			}
		}

		public bool TryGetName(VariableDefinition variable, out string name)
		{
			name = null;
			if (variables != null && variables.Count != 0)
			{
				for (int i = 0; i < variables.Count; i++)
				{
					if (variables[i].Index == variable.Index)
					{
						name = variables[i].Name;
						return true;
					}
				}
				return false;
			}
			return false;
		}
	}
}
