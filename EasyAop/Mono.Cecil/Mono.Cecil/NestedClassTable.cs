using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class NestedClassTable : SortedTable<Row<uint, uint>>
	{
		public override void Write(TableHeapBuffer buffer)
		{
			for (int i = 0; i < base.length; i++)
			{
				buffer.WriteRID(base.rows[i].Col1, Table.TypeDef);
				buffer.WriteRID(base.rows[i].Col2, Table.TypeDef);
			}
		}

		public override int Compare(Row<uint, uint> x, Row<uint, uint> y)
		{
			return base.Compare(x.Col1, y.Col1);
		}
	}
}
