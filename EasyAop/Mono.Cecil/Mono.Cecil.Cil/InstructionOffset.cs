using System;

namespace Mono.Cecil.Cil
{
	public struct InstructionOffset
	{
		private readonly Instruction instruction;

		private readonly int? offset;

		public int Offset
		{
			get
			{
				if (instruction != null)
				{
					return instruction.Offset;
				}
				if (offset.HasValue)
				{
					return offset.Value;
				}
				throw new NotSupportedException();
			}
		}

		public bool IsEndOfMethod
		{
			get
			{
				if (instruction == null)
				{
					return !offset.HasValue;
				}
				return false;
			}
		}

		public InstructionOffset(Instruction instruction)
		{
			if (instruction == null)
			{
				throw new ArgumentNullException("instruction");
			}
			this.instruction = instruction;
			offset = null;
		}

		public InstructionOffset(int offset)
		{
			instruction = null;
			this.offset = offset;
		}
	}
}
