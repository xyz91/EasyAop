namespace Mono.Cecil.Metadata
{
	internal sealed class TableHeap : Heap
	{
		public long Valid;

		public long Sorted;

		public readonly TableInformation[] Tables = new TableInformation[58];

		public TableInformation this[Table table]
		{
			get
			{
				return Tables[(uint)table];
			}
		}

		public TableHeap(byte[] data)
			: base(data)
		{
		}

		public bool HasTable(Table table)
		{
			return (Valid & 1L << (int)table) != 0;
		}
	}
}
