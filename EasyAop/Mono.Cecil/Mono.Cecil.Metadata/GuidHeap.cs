using System;

namespace Mono.Cecil.Metadata
{
	internal sealed class GuidHeap : Heap
	{
		public GuidHeap(byte[] data)
			: base(data)
		{
		}

		public Guid Read(uint index)
		{
			if (index != 0 && index - 1 + 16 <= base.data.Length)
			{
				byte[] array = new byte[16];
				Buffer.BlockCopy(base.data, (int)((index - 1) * 16), array, 0, 16);
				return new Guid(array);
			}
			return default(Guid);
		}
	}
}
