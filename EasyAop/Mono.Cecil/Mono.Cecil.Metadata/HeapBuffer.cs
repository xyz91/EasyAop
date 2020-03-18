using Mono.Cecil.PE;

namespace Mono.Cecil.Metadata
{
	internal abstract class HeapBuffer : ByteBuffer
	{
		public bool IsLarge => base.length > 65535;

		public abstract bool IsEmpty
		{
			get;
		}

		protected HeapBuffer(int length)
			: base(length)
		{
		}
	}
}
