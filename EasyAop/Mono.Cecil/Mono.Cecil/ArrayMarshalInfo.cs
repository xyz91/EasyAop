namespace Mono.Cecil
{
	public sealed class ArrayMarshalInfo : MarshalInfo
	{
		internal NativeType element_type;

		internal int size_parameter_index;

		internal int size;

		internal int size_parameter_multiplier;

		public NativeType ElementType
		{
			get
			{
				return element_type;
			}
			set
			{
				element_type = value;
			}
		}

		public int SizeParameterIndex
		{
			get
			{
				return size_parameter_index;
			}
			set
			{
				size_parameter_index = value;
			}
		}

		public int Size
		{
			get
			{
				return size;
			}
			set
			{
				size = value;
			}
		}

		public int SizeParameterMultiplier
		{
			get
			{
				return size_parameter_multiplier;
			}
			set
			{
				size_parameter_multiplier = value;
			}
		}

		public ArrayMarshalInfo()
			: base(NativeType.Array)
		{
			element_type = NativeType.None;
			size_parameter_index = -1;
			size = -1;
			size_parameter_multiplier = -1;
		}
	}
}
