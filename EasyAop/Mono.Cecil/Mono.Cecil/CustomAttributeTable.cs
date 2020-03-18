using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class CustomAttributeTable : SortedTable<Row<uint, uint, uint>>
	{
		public override void Write(TableHeapBuffer buffer)
		{
			for (int i = 0; i < base.length; i++)
			{
				buffer.WriteCodedRID(base.rows[i].Col1, CodedIndex.HasCustomAttribute);
				buffer.WriteCodedRID(base.rows[i].Col2, CodedIndex.CustomAttributeType);
				buffer.WriteBlob(base.rows[i].Col3);
			}
		}

		public override int Compare(Row<uint, uint, uint> x, Row<uint, uint, uint> y)
		{
			return base.Compare(x.Col1, y.Col1);
		}
	}
}
