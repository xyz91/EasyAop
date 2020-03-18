using System;

namespace Mono.Cecil.PE
{
	internal class ByteBuffer
	{
		internal byte[] buffer;

		internal int length;

		internal int position;

		public ByteBuffer()
		{
			buffer = Empty<byte>.Array;
		}

		public ByteBuffer(int length)
		{
			buffer = new byte[length];
		}

		public ByteBuffer(byte[] buffer)
		{
			this.buffer = (buffer ?? Empty<byte>.Array);
			length = this.buffer.Length;
		}

		public void Advance(int length)
		{
			position += length;
		}

		public byte ReadByte()
		{
			return buffer[position++];
		}

		public sbyte ReadSByte()
		{
			return (sbyte)ReadByte();
		}

		public byte[] ReadBytes(int length)
		{
			byte[] array = new byte[length];
			Buffer.BlockCopy(buffer, position, array, 0, length);
			position += length;
			return array;
		}

		public ushort ReadUInt16()
		{
			ushort result = (ushort)(buffer[position] | buffer[position + 1] << 8);
			position += 2;
			return result;
		}

		public short ReadInt16()
		{
			return (short)ReadUInt16();
		}

		public uint ReadUInt32()
		{
			int result = buffer[position] | buffer[position + 1] << 8 | buffer[position + 2] << 16 | buffer[position + 3] << 24;
			position += 4;
			return (uint)result;
		}

		public int ReadInt32()
		{
			return (int)ReadUInt32();
		}

		public ulong ReadUInt64()
		{
			uint num = ReadUInt32();
			return (ulong)ReadUInt32() << 32 | num;
		}

		public long ReadInt64()
		{
			return (long)ReadUInt64();
		}

		public uint ReadCompressedUInt32()
		{
			byte b = ReadByte();
			if ((b & 0x80) == 0)
			{
				return b;
			}
			if ((b & 0x40) == 0)
			{
				return (uint)((b & -129) << 8 | ReadByte());
			}
			return (uint)((b & -193) << 24 | ReadByte() << 16 | ReadByte() << 8 | ReadByte());
		}

		public int ReadCompressedInt32()
		{
			byte b = buffer[position];
			uint num = ReadCompressedUInt32();
			int num2 = (int)num >> 1;
			if ((num & 1) == 0)
			{
				return num2;
			}
			switch (b & 0xC0)
			{
			case 0:
			case 64:
				return num2 - 64;
			case 128:
				return num2 - 8192;
			default:
				return num2 - 268435456;
			}
		}

		public float ReadSingle()
		{
			if (!BitConverter.IsLittleEndian)
			{
				byte[] array = ReadBytes(4);
				Array.Reverse(array);
				return BitConverter.ToSingle(array, 0);
			}
			float result = BitConverter.ToSingle(buffer, position);
			position += 4;
			return result;
		}

		public double ReadDouble()
		{
			if (!BitConverter.IsLittleEndian)
			{
				byte[] array = ReadBytes(8);
				Array.Reverse(array);
				return BitConverter.ToDouble(array, 0);
			}
			double result = BitConverter.ToDouble(buffer, position);
			position += 8;
			return result;
		}

		public void WriteByte(byte value)
		{
			if (position == buffer.Length)
			{
				Grow(1);
			}
			buffer[position++] = value;
			if (position > length)
			{
				length = position;
			}
		}

		public void WriteSByte(sbyte value)
		{
			WriteByte((byte)value);
		}

		public void WriteUInt16(ushort value)
		{
			if (position + 2 > buffer.Length)
			{
				Grow(2);
			}
			buffer[position++] = (byte)value;
			buffer[position++] = (byte)(value >> 8);
			if (position > length)
			{
				length = position;
			}
		}

		public void WriteInt16(short value)
		{
			WriteUInt16((ushort)value);
		}

		public void WriteUInt32(uint value)
		{
			if (position + 4 > buffer.Length)
			{
				Grow(4);
			}
			buffer[position++] = (byte)value;
			buffer[position++] = (byte)(value >> 8);
			buffer[position++] = (byte)(value >> 16);
			buffer[position++] = (byte)(value >> 24);
			if (position > length)
			{
				length = position;
			}
		}

		public void WriteInt32(int value)
		{
			WriteUInt32((uint)value);
		}

		public void WriteUInt64(ulong value)
		{
			if (position + 8 > buffer.Length)
			{
				Grow(8);
			}
			buffer[position++] = (byte)value;
			buffer[position++] = (byte)(value >> 8);
			buffer[position++] = (byte)(value >> 16);
			buffer[position++] = (byte)(value >> 24);
			buffer[position++] = (byte)(value >> 32);
			buffer[position++] = (byte)(value >> 40);
			buffer[position++] = (byte)(value >> 48);
			buffer[position++] = (byte)(value >> 56);
			if (position > length)
			{
				length = position;
			}
		}

		public void WriteInt64(long value)
		{
			WriteUInt64((ulong)value);
		}

		public void WriteCompressedUInt32(uint value)
		{
			if (value < 128)
			{
				WriteByte((byte)value);
			}
			else if (value < 16384)
			{
				WriteByte((byte)(0x80 | value >> 8));
				WriteByte((byte)(value & 0xFF));
			}
			else
			{
				WriteByte((byte)(value >> 24 | 0xC0));
				WriteByte((byte)(value >> 16 & 0xFF));
				WriteByte((byte)(value >> 8 & 0xFF));
				WriteByte((byte)(value & 0xFF));
			}
		}

		public void WriteCompressedInt32(int value)
		{
			if (value >= 0)
			{
				WriteCompressedUInt32((uint)(value << 1));
			}
			else
			{
				if (value > -64)
				{
					value = 64 + value;
				}
				else if (value >= -8192)
				{
					value = 8192 + value;
				}
				else if (value >= -536870912)
				{
					value = 536870912 + value;
				}
				WriteCompressedUInt32((uint)(value << 1 | 1));
			}
		}

		public void WriteBytes(byte[] bytes)
		{
			int num = bytes.Length;
			if (position + num > buffer.Length)
			{
				Grow(num);
			}
			Buffer.BlockCopy(bytes, 0, buffer, position, num);
			position += num;
			if (position > length)
			{
				length = position;
			}
		}

		public void WriteBytes(int length)
		{
			if (position + length > buffer.Length)
			{
				Grow(length);
			}
			position += length;
			if (position > this.length)
			{
				this.length = position;
			}
		}

		public void WriteBytes(ByteBuffer buffer)
		{
			if (position + buffer.length > this.buffer.Length)
			{
				Grow(buffer.length);
			}
			Buffer.BlockCopy(buffer.buffer, 0, this.buffer, position, buffer.length);
			position += buffer.length;
			if (position > length)
			{
				length = position;
			}
		}

		public void WriteSingle(float value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public void WriteDouble(double value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		private void Grow(int desired)
		{
			byte[] array = buffer;
			int num = array.Length;
			byte[] dst = new byte[Math.Max(num + desired, num * 2)];
			Buffer.BlockCopy(array, 0, dst, 0, num);
			buffer = dst;
		}
	}
}
