namespace Mono.Cecil.PE
{
	internal sealed class TextMap
	{
		private readonly Range[] map = new Range[17];

		public void AddMap(TextSegment segment, int length)
		{
			map[(int)segment] = new Range(GetStart(segment), (uint)length);
		}

		public void AddMap(TextSegment segment, int length, int align)
		{
			align--;
			AddMap(segment, length + align & ~align);
		}

		public void AddMap(TextSegment segment, Range range)
		{
			map[(int)segment] = range;
		}

		public Range GetRange(TextSegment segment)
		{
			return map[(int)segment];
		}

		public DataDirectory GetDataDirectory(TextSegment segment)
		{
			Range range = map[(int)segment];
			return new DataDirectory((range.Length != 0) ? range.Start : 0, range.Length);
		}

		public uint GetRVA(TextSegment segment)
		{
			return map[(int)segment].Start;
		}

		public uint GetNextRVA(TextSegment segment)
		{
			return map[(int)segment].Start + map[(int)segment].Length;
		}

		public int GetLength(TextSegment segment)
		{
			return (int)map[(int)segment].Length;
		}

		private uint GetStart(TextSegment segment)
		{
			if (segment != 0)
			{
				return ComputeStart((int)segment);
			}
			return 8192u;
		}

		private uint ComputeStart(int index)
		{
			index--;
			return map[index].Start + map[index].Length;
		}

		public uint GetLength()
		{
			Range range = map[16];
			return range.Start - 8192 + range.Length;
		}
	}
}
