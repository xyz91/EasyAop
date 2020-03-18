using Mono.Cecil.PE;
using System.Collections.Generic;

namespace Mono.Cecil.Metadata
{
	internal sealed class BlobHeapBuffer : HeapBuffer
	{
		private readonly Dictionary<ByteBuffer, uint> blobs = new Dictionary<ByteBuffer, uint>(new ByteBufferEqualityComparer());

		public override bool IsEmpty => base.length <= 1;

		public BlobHeapBuffer()
			: base(1)
		{
			base.WriteByte(0);
		}

		public uint GetBlobIndex(ByteBuffer blob)
		{
			if (blobs.TryGetValue(blob, out uint position))
			{
				return position;
			}
			position = (uint)base.position;
			WriteBlob(blob);
			blobs.Add(blob, position);
			return position;
		}

		private void WriteBlob(ByteBuffer blob)
		{
			base.WriteCompressedUInt32((uint)blob.length);
			base.WriteBytes(blob);
		}
	}
}
