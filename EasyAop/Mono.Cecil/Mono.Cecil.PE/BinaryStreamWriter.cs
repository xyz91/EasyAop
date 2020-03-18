using System.IO;

namespace Mono.Cecil.PE
{
	internal class BinaryStreamWriter : BinaryWriter
	{
		public int Position
		{
			get
			{
				return (int)BaseStream.Position;
			}
			set
			{
				BaseStream.Position = value;
			}
		}

		public BinaryStreamWriter(Stream stream)
			: base(stream)
		{
		}

		public void WriteByte(byte value)
		{
			Write(value);
		}

		public void WriteUInt16(ushort value)
		{
			Write(value);
		}

		public void WriteInt16(short value)
		{
			Write(value);
		}

		public void WriteUInt32(uint value)
		{
			Write(value);
		}

		public void WriteInt32(int value)
		{
			Write(value);
		}

		public void WriteUInt64(ulong value)
		{
			Write(value);
		}

		public void WriteBytes(byte[] bytes)
		{
			Write(bytes);
		}

		public void WriteDataDirectory(DataDirectory directory)
		{
			Write(directory.VirtualAddress);
			Write(directory.Size);
		}

		public void WriteBuffer(ByteBuffer buffer)
		{
			Write(buffer.buffer, 0, buffer.length);
		}

		protected void Advance(int bytes)
		{
			BaseStream.Seek(bytes, SeekOrigin.Current);
		}

		public void Align(int align)
		{
			align--;
			int position = Position;
			int num = (position + align & ~align) - position;
			for (int i = 0; i < num; i++)
			{
				WriteByte(0);
			}
		}
	}
}
