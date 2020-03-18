using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class FieldRVATable : SortedTable<Row<uint, uint>>
	{
		internal int position;

		public override void Write(TableHeapBuffer buffer)
		{
			position = buffer.position;
			for (int i = 0; i < base.length; i++)
			{
				buffer.WriteUInt32(base.rows[i].Col1);
				buffer.WriteRID(base.rows[i].Col2, Table.Field);
			}
		}

		public override int Compare(Row<uint, uint> x, Row<uint, uint> y)
		{
			return base.Compare(x.Col2, y.Col2);
		}
	}
}
