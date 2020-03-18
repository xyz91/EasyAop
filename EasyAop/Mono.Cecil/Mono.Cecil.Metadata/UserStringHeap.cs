namespace Mono.Cecil.Metadata
{
	internal sealed class UserStringHeap : StringHeap
	{
		public UserStringHeap(byte[] data)
			: base(data)
		{
		}

		protected override string ReadStringAt(uint index)
		{
			int num = (int)index;
			uint num2 = (uint)(base.data.ReadCompressedUInt32(ref num) & -2);
			if (num2 < 1)
			{
				return string.Empty;
			}
			char[] array = new char[num2 / 2u];
			int i = num;
			int num3 = 0;
			for (; i < num + num2; i += 2)
			{
				array[num3++] = (char)(base.data[i] | base.data[i + 1] << 8);
			}
			return new string(array);
		}
	}
}
