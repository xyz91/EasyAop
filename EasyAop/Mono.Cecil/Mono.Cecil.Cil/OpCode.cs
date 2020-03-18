using System;

namespace Mono.Cecil.Cil
{
	public struct OpCode : IEquatable<OpCode>
	{
		private readonly byte op1;

		private readonly byte op2;

		private readonly byte code;

		private readonly byte flow_control;

		private readonly byte opcode_type;

		private readonly byte operand_type;

		private readonly byte stack_behavior_pop;

		private readonly byte stack_behavior_push;

		public string Name => OpCodeNames.names[(int)Code];

		public int Size
		{
			get
			{
				if (op1 != 255)
				{
					return 2;
				}
				return 1;
			}
		}

		public byte Op1 => op1;

		public byte Op2 => op2;

		public short Value
		{
			get
			{
				if (op1 != 255)
				{
					return (short)(op1 << 8 | op2);
				}
				return op2;
			}
		}

		public Code Code => (Code)code;

		public FlowControl FlowControl => (FlowControl)flow_control;

		public OpCodeType OpCodeType => (OpCodeType)opcode_type;

		public OperandType OperandType => (OperandType)operand_type;

		public StackBehaviour StackBehaviourPop => (StackBehaviour)stack_behavior_pop;

		public StackBehaviour StackBehaviourPush => (StackBehaviour)stack_behavior_push;

		internal OpCode(int x, int y)
		{
			op1 = (byte)(x & 0xFF);
			op2 = (byte)(x >> 8 & 0xFF);
			code = (byte)(x >> 16 & 0xFF);
			flow_control = (byte)(x >> 24 & 0xFF);
			opcode_type = (byte)(y & 0xFF);
			operand_type = (byte)(y >> 8 & 0xFF);
			stack_behavior_pop = (byte)(y >> 16 & 0xFF);
			stack_behavior_push = (byte)(y >> 24 & 0xFF);
			if (op1 == 255)
			{
				OpCodes.OneByteOpCode[op2] = this;
			}
			else
			{
				OpCodes.TwoBytesOpCode[op2] = this;
			}
		}

		public override int GetHashCode()
		{
			return Value;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is OpCode))
			{
				return false;
			}
			OpCode opCode = (OpCode)obj;
			if (op1 == opCode.op1)
			{
				return op2 == opCode.op2;
			}
			return false;
		}

		public bool Equals(OpCode opcode)
		{
			if (op1 == opcode.op1)
			{
				return op2 == opcode.op2;
			}
			return false;
		}

		public static bool operator ==(OpCode one, OpCode other)
		{
			if (one.op1 == other.op1)
			{
				return one.op2 == other.op2;
			}
			return false;
		}

		public static bool operator !=(OpCode one, OpCode other)
		{
			if (one.op1 == other.op1)
			{
				return one.op2 != other.op2;
			}
			return true;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
