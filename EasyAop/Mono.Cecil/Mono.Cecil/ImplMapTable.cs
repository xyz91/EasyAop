using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class ImplMapTable : SortedTable<Row<PInvokeAttributes, uint, uint, uint>>
	{
		public override void Write(TableHeapBuffer buffer)
		{
			for (int i = 0; i < base.length; i++)
			{
				buffer.WriteUInt16((ushort)base.rows[i].Col1);
				buffer.WriteCodedRID(base.rows[i].Col2, CodedIndex.MemberForwarded);
				buffer.WriteString(base.rows[i].Col3);
				buffer.WriteRID(base.rows[i].Col4, Table.ModuleRef);
			}
		}

		public override int Compare(Row<PInvokeAttributes, uint, uint, uint> x, Row<PInvokeAttributes, uint, uint, uint> y)
		{
			return base.Compare(x.Col2, y.Col2);
		}
	}
}
