namespace Mono.Cecil.Metadata
{
	internal sealed class PdbHeap : Heap
	{
		public byte[] Id;

		public uint EntryPoint;

		public long TypeSystemTables;

		public uint[] TypeSystemTableRows;

		public PdbHeap(byte[] data)
			: base(data)
		{
		}

		public bool HasTable(Table table)
		{
			return (TypeSystemTables & 1L << (int)table) != 0;
		}
	}
}
