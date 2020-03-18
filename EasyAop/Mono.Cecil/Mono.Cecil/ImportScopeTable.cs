using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class ImportScopeTable : MetadataTable<Row<uint, uint>>
	{
		public override void Write(TableHeapBuffer buffer)
		{
			for (int i = 0; i < base.length; i++)
			{
				buffer.WriteRID(base.rows[i].Col1, Table.ImportScope);
				buffer.WriteBlob(base.rows[i].Col2);
			}
		}
	}
}
