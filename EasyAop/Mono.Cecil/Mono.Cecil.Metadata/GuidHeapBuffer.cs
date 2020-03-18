using System;
using System.Collections.Generic;

namespace Mono.Cecil.Metadata
{
	internal sealed class GuidHeapBuffer : HeapBuffer
	{
		private readonly Dictionary<Guid, uint> guids = new Dictionary<Guid, uint>();

		public override bool IsEmpty => base.length == 0;

		public GuidHeapBuffer()
			: base(16)
		{
		}

		public uint GetGuidIndex(Guid guid)
		{
			if (guids.TryGetValue(guid, out uint num))
			{
				return num;
			}
			num = (uint)(guids.Count + 1);
			WriteGuid(guid);
			guids.Add(guid, num);
			return num;
		}

		private void WriteGuid(Guid guid)
		{
			base.WriteBytes(guid.ToByteArray());
		}
	}
}
