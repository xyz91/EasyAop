namespace Mono.Cecil
{
	public struct ArrayDimension
	{
		private int? lower_bound;

		private int? upper_bound;

		public int? LowerBound
		{
			get
			{
				return lower_bound;
			}
			set
			{
				lower_bound = value;
			}
		}

		public int? UpperBound
		{
			get
			{
				return upper_bound;
			}
			set
			{
				upper_bound = value;
			}
		}

		public bool IsSized
		{
			get
			{
				if (!lower_bound.HasValue)
				{
					return upper_bound.HasValue;
				}
				return true;
			}
		}

		public ArrayDimension(int? lowerBound, int? upperBound)
		{
			lower_bound = lowerBound;
			upper_bound = upperBound;
		}

		public override string ToString()
		{
			if (IsSized)
			{
				return lower_bound + "..." + upper_bound;
			}
			return string.Empty;
		}
	}
}
