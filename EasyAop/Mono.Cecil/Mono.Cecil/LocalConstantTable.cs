using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class LocalConstantTable : MetadataTable<Row<uint, uint>>
	{
		public override void Write(TableHeapBuffer buffer)
		{
			for (int i = 0; i < base.length; i++)
			{
				buffer.WriteString(base.rows[i].Col1);
				buffer.WriteBlob(base.rows[i].Col2);
			}
		}
	}
}
