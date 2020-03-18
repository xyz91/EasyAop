using Mono.Cecil.PE;

namespace Mono.Cecil.Metadata
{
	internal sealed class ResourceBuffer : ByteBuffer
	{
		public ResourceBuffer()
			: base(0)
		{
		}

		public uint AddResource(byte[] resource)
		{
			int position = base.position;
			base.WriteInt32(resource.Length);
			base.WriteBytes(resource);
			return (uint)position;
		}
	}
}
