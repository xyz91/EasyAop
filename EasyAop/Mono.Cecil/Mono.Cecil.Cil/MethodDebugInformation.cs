using Mono.Collections.Generic;
using System;
using System.Collections.Generic;

namespace Mono.Cecil.Cil
{
	public sealed class MethodDebugInformation : DebugInformation
	{
		internal MethodDefinition method;

		internal Collection<SequencePoint> sequence_points;

		internal ScopeDebugInformation scope;

		internal MethodDefinition kickoff_method;

		internal int code_size;

		internal MetadataToken local_var_token;

		public MethodDefinition Method => method;

		public bool HasSequencePoints => !sequence_points.IsNullOrEmpty();

		public Collection<SequencePoint> SequencePoints => sequence_points ?? (sequence_points = new Collection<SequencePoint>());

		public ScopeDebugInformation Scope
		{
			get
			{
				return scope;
			}
			set
			{
				scope = value;
			}
		}

		public MethodDefinition StateMachineKickOffMethod
		{
			get
			{
				return kickoff_method;
			}
			set
			{
				kickoff_method = value;
			}
		}

		internal MethodDebugInformation(MethodDefinition method)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			this.method = method;
			base.token = new MetadataToken(TokenType.MethodDebugInformation, method.MetadataToken.RID);
		}

		public SequencePoint GetSequencePoint(Instruction instruction)
		{
			if (!HasSequencePoints)
			{
				return null;
			}
			for (int i = 0; i < sequence_points.Count; i++)
			{
				if (sequence_points[i].Offset == instruction.Offset)
				{
					return sequence_points[i];
				}
			}
			return null;
		}

		public IDictionary<Instruction, SequencePoint> GetSequencePointMapping()
		{
			Dictionary<Instruction, SequencePoint> dictionary = new Dictionary<Instruction, SequencePoint>();
			if (HasSequencePoints && method.HasBody)
			{
				Dictionary<int, SequencePoint> dictionary2 = new Dictionary<int, SequencePoint>(sequence_points.Count);
				for (int i = 0; i < sequence_points.Count; i++)
				{
					if (!dictionary2.ContainsKey(sequence_points[i].Offset))
					{
						dictionary2.Add(sequence_points[i].Offset, sequence_points[i]);
					}
				}
				Collection<Instruction> instructions = method.Body.Instructions;
				for (int j = 0; j < instructions.Count; j++)
				{
					if (dictionary2.TryGetValue(instructions[j].Offset, out SequencePoint value))
					{
						dictionary.Add(instructions[j], value);
					}
				}
				return dictionary;
			}
			return dictionary;
		}

		public IEnumerable<ScopeDebugInformation> GetScopes()
		{
			if (scope == null)
			{
				return Empty<ScopeDebugInformation>.Array;
			}
			return GetScopes(new ScopeDebugInformation[1]
			{
				scope
			});
		}

		private static IEnumerable<ScopeDebugInformation> GetScopes(IList<ScopeDebugInformation> scopes)
		{
			for (int i = 0; i < scopes.Count; i++)
			{
				ScopeDebugInformation scope = scopes[i];
				yield return scope;
				if (scope.HasScopes)
				{
					foreach (ScopeDebugInformation scope2 in GetScopes(scope.Scopes))
					{
						yield return scope2;
					}
				}
			}
		}

		public bool TryGetName(VariableDefinition variable, out string name)
		{
			name = null;
			bool flag = false;
			string text = "";
			foreach (ScopeDebugInformation scope2 in GetScopes())
			{
				if (scope2.TryGetName(variable, out string text2))
				{
					if (!flag)
					{
						flag = true;
						text = text2;
					}
					else if (text != text2)
					{
						return false;
					}
				}
			}
			name = text;
			return flag;
		}
	}
}
