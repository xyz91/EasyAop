namespace Mono.Cecil
{
	public sealed class FixedArrayMarshalInfo : MarshalInfo
	{
		internal NativeType element_type;

		internal int size;

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

		public FixedArrayMarshalInfo()
			: base(NativeType.FixedArray)
		{
			element_type = NativeType.None;
		}
	}
}
