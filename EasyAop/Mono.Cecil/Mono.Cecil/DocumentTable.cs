using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class DocumentTable : MetadataTable<Row<uint, uint, uint, uint>>
	{
		public override void Write(TableHeapBuffer buffer)
		{
			for (int i = 0; i < base.length; i++)
			{
				buffer.WriteBlob(base.rows[i].Col1);
				buffer.WriteGuid(base.rows[i].Col2);
				buffer.WriteBlob(base.rows[i].Col3);
				buffer.WriteGuid(base.rows[i].Col4);
			}
		}
	}
}
