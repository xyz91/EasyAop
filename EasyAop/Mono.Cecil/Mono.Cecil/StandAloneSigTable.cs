using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class StandAloneSigTable : MetadataTable<uint>
	{
		public override void Write(TableHeapBuffer buffer)
		{
			for (int i = 0; i < base.length; i++)
			{
				buffer.WriteBlob(base.rows[i]);
			}
		}
	}
}
