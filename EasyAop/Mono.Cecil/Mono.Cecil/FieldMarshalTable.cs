using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class FieldMarshalTable : SortedTable<Row<uint, uint>>
	{
		public override void Write(TableHeapBuffer buffer)
		{
			for (int i = 0; i < base.length; i++)
			{
				buffer.WriteCodedRID(base.rows[i].Col1, CodedIndex.HasFieldMarshal);
				buffer.WriteBlob(base.rows[i].Col2);
			}
		}

		public override int Compare(Row<uint, uint> x, Row<uint, uint> y)
		{
			return base.Compare(x.Col1, y.Col1);
		}
	}
}
