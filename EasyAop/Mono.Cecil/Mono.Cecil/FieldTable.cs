using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class FieldTable : MetadataTable<Row<FieldAttributes, uint, uint>>
	{
		public override void Write(TableHeapBuffer buffer)
		{
			for (int i = 0; i < base.length; i++)
			{
				buffer.WriteUInt16((ushort)base.rows[i].Col1);
				buffer.WriteString(base.rows[i].Col2);
				buffer.WriteBlob(base.rows[i].Col3);
			}
		}
	}
}
