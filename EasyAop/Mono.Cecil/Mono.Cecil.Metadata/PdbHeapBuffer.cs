namespace Mono.Cecil.Metadata
{
	internal sealed class PdbHeapBuffer : HeapBuffer
	{
		public override bool IsEmpty => false;

		public PdbHeapBuffer()
			: base(0)
		{
		}
	}
}
