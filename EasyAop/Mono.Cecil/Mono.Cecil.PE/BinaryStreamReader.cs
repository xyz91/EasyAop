using System.IO;

namespace Mono.Cecil.PE
{
	internal class BinaryStreamReader : BinaryReader
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

		public int Length => (int)BaseStream.Length;

		public BinaryStreamReader(Stream stream)
			: base(stream)
		{
		}

		public void Advance(int bytes)
		{
			BaseStream.Seek(bytes, SeekOrigin.Current);
		}

		public void MoveTo(uint position)
		{
			BaseStream.Seek(position, SeekOrigin.Begin);
		}

		public void Align(int align)
		{
			align--;
			int position = Position;
			Advance((position + align & ~align) - position);
		}

		public DataDirectory ReadDataDirectory()
		{
			return new DataDirectory(ReadUInt32(), ReadUInt32());
		}
	}
}
