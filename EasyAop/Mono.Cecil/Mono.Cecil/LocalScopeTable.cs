using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class LocalScopeTable : MetadataTable<Row<uint, uint, uint, uint, uint, uint>>
	{
		public override void Write(TableHeapBuffer buffer)
		{
			for (int i = 0; i < base.length; i++)
			{
				buffer.WriteRID(base.rows[i].Col1, Table.Method);
				buffer.WriteRID(base.rows[i].Col2, Table.ImportScope);
				buffer.WriteRID(base.rows[i].Col3, Table.LocalVariable);
				buffer.WriteRID(base.rows[i].Col4, Table.LocalConstant);
				buffer.WriteUInt32(base.rows[i].Col5);
				buffer.WriteUInt32(base.rows[i].Col6);
			}
		}
	}
}
