using System;

namespace Mono.Cecil.Metadata
{
	internal sealed class BlobHeap : Heap
	{
		public BlobHeap(byte[] data)
			: base(data)
		{
		}

		public byte[] Read(uint index)
		{
			if (index != 0 && index <= base.data.Length - 1)
			{
				int num = (int)index;
				int num2 = (int)base.data.ReadCompressedUInt32(ref num);
				if (num2 > base.data.Length - num)
				{
					return Empty<byte>.Array;
				}
				byte[] array = new byte[num2];
				Buffer.BlockCopy(base.data, num, array, 0, num2);
				return array;
			}
			return Empty<byte>.Array;
		}

		public void GetView(uint signature, out byte[] buffer, out int index, out int length)
		{
			if (signature == 0 || signature > base.data.Length - 1)
			{
				buffer = null;
				index = (length = 0);
			}
			else
			{
				buffer = base.data;
				index = (int)signature;
				length = (int)buffer.ReadCompressedUInt32(ref index);
			}
		}
	}
}
