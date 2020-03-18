using System;
using System.IO;

namespace Mono.Cecil
{
	public sealed class EmbeddedResource : Resource
	{
		private readonly MetadataReader reader;

		private uint? offset;

		private byte[] data;

		private Stream stream;

		public override ResourceType ResourceType => ResourceType.Embedded;

		public EmbeddedResource(string name, ManifestResourceAttributes attributes, byte[] data)
			: base(name, attributes)
		{
			this.data = data;
		}

		public EmbeddedResource(string name, ManifestResourceAttributes attributes, Stream stream)
			: base(name, attributes)
		{
			this.stream = stream;
		}

		internal EmbeddedResource(string name, ManifestResourceAttributes attributes, uint offset, MetadataReader reader)
			: base(name, attributes)
		{
			this.offset = offset;
			this.reader = reader;
		}

		public Stream GetResourceStream()
		{
			if (stream != null)
			{
				return stream;
			}
			if (data != null)
			{
				return new MemoryStream(data);
			}
			if (offset.HasValue)
			{
				return new MemoryStream(reader.GetManagedResource(offset.Value));
			}
			throw new InvalidOperationException();
		}

		public byte[] GetResourceData()
		{
			if (stream != null)
			{
				return ReadStream(stream);
			}
			if (data != null)
			{
				return data;
			}
			if (offset.HasValue)
			{
				return reader.GetManagedResource(offset.Value);
			}
			throw new InvalidOperationException();
		}

		private static byte[] ReadStream(Stream stream)
		{
			int num3;
			if (stream.CanSeek)
			{
				int num = (int)stream.Length;
				byte[] array = new byte[num];
				int num2 = 0;
				while ((num3 = stream.Read(array, num2, num - num2)) > 0)
				{
					num2 += num3;
				}
				return array;
			}
			byte[] array2 = new byte[8192];
			MemoryStream memoryStream = new MemoryStream();
			while ((num3 = stream.Read(array2, 0, array2.Length)) > 0)
			{
				memoryStream.Write(array2, 0, num3);
			}
			return memoryStream.ToArray();
		}
	}
}
