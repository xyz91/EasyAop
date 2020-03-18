using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class EventTable : MetadataTable<Row<EventAttributes, uint, uint>>
	{
		public override void Write(TableHeapBuffer buffer)
		{
			for (int i = 0; i < base.length; i++)
			{
				buffer.WriteUInt16((ushort)base.rows[i].Col1);
				buffer.WriteString(base.rows[i].Col2);
				buffer.WriteCodedRID(base.rows[i].Col3, CodedIndex.TypeDefOrRef);
			}
		}
	}
}
