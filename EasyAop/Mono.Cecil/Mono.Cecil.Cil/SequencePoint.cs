using System;

namespace Mono.Cecil.Cil
{
	public sealed class SequencePoint
	{
		internal InstructionOffset offset;

		private Document document;

		private int start_line;

		private int start_column;

		private int end_line;

		private int end_column;

		public int Offset => offset.Offset;

		public int StartLine
		{
			get
			{
				return start_line;
			}
			set
			{
				start_line = value;
			}
		}

		public int StartColumn
		{
			get
			{
				return start_column;
			}
			set
			{
				start_column = value;
			}
		}

		public int EndLine
		{
			get
			{
				return end_line;
			}
			set
			{
				end_line = value;
			}
		}

		public int EndColumn
		{
			get
			{
				return end_column;
			}
			set
			{
				end_column = value;
			}
		}

		public bool IsHidden
		{
			get
			{
				if (start_line == 16707566)
				{
					return start_line == end_line;
				}
				return false;
			}
		}

		public Document Document
		{
			get
			{
				return document;
			}
			set
			{
				document = value;
			}
		}

		internal SequencePoint(int offset, Document document)
		{
			if (document == null)
			{
				throw new ArgumentNullException("document");
			}
			this.offset = new InstructionOffset(offset);
			this.document = document;
		}

		public SequencePoint(Instruction instruction, Document document)
		{
			if (document == null)
			{
				throw new ArgumentNullException("document");
			}
			offset = new InstructionOffset(instruction);
			this.document = document;
		}
	}
}
