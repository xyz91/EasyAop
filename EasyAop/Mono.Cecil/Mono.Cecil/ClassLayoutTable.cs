using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class ClassLayoutTable : SortedTable<Row<ushort, uint, uint>>
	{
		public override void Write(TableHeapBuffer buffer)
		{
			for (int i = 0; i < base.length; i++)
			{
				buffer.WriteUInt16(base.rows[i].Col1);
				buffer.WriteUInt32(base.rows[i].Col2);
				buffer.WriteRID(base.rows[i].Col3, Table.TypeDef);
			}
		}

		public override int Compare(Row<ushort, uint, uint> x, Row<ushort, uint, uint> y)
		{
			return base.Compare(x.Col3, y.Col3);
		}
	}
}
