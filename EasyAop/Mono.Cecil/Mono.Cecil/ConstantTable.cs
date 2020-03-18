using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class ConstantTable : SortedTable<Row<ElementType, uint, uint>>
	{
		public override void Write(TableHeapBuffer buffer)
		{
			for (int i = 0; i < base.length; i++)
			{
				buffer.WriteUInt16((ushort)base.rows[i].Col1);
				buffer.WriteCodedRID(base.rows[i].Col2, CodedIndex.HasConstant);
				buffer.WriteBlob(base.rows[i].Col3);
			}
		}

		public override int Compare(Row<ElementType, uint, uint> x, Row<ElementType, uint, uint> y)
		{
			return base.Compare(x.Col2, y.Col2);
		}
	}
}
