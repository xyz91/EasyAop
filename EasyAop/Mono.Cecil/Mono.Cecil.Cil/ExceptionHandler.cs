namespace Mono.Cecil.Cil
{
	public sealed class ExceptionHandler
	{
		private Instruction try_start;

		private Instruction try_end;

		private Instruction filter_start;

		private Instruction handler_start;

		private Instruction handler_end;

		private TypeReference catch_type;

		private ExceptionHandlerType handler_type;

		public Instruction TryStart
		{
			get
			{
				return try_start;
			}
			set
			{
				try_start = value;
			}
		}

		public Instruction TryEnd
		{
			get
			{
				return try_end;
			}
			set
			{
				try_end = value;
			}
		}

		public Instruction FilterStart
		{
			get
			{
				return filter_start;
			}
			set
			{
				filter_start = value;
			}
		}

		public Instruction HandlerStart
		{
			get
			{
				return handler_start;
			}
			set
			{
				handler_start = value;
			}
		}

		public Instruction HandlerEnd
		{
			get
			{
				return handler_end;
			}
			set
			{
				handler_end = value;
			}
		}

		public TypeReference CatchType
		{
			get
			{
				return catch_type;
			}
			set
			{
				catch_type = value;
			}
		}

		public ExceptionHandlerType HandlerType
		{
			get
			{
				return handler_type;
			}
			set
			{
				handler_type = value;
			}
		}

		public ExceptionHandler(ExceptionHandlerType handlerType)
		{
			handler_type = handlerType;
		}
	}
}
