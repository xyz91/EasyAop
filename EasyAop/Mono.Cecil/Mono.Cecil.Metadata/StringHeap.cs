using System.Collections.Generic;
using System.Text;

namespace Mono.Cecil.Metadata
{
	internal class StringHeap : Heap
	{
		private readonly Dictionary<uint, string> strings = new Dictionary<uint, string>();

		public StringHeap(byte[] data)
			: base(data)
		{
		}

		public string Read(uint index)
		{
			if (index == 0)
			{
				return string.Empty;
			}
			if (strings.TryGetValue(index, out string text))
			{
				return text;
			}
			if (index > base.data.Length - 1)
			{
				return string.Empty;
			}
			text = ReadStringAt(index);
			if (text.Length != 0)
			{
				strings.Add(index, text);
			}
			return text;
		}

		protected virtual string ReadStringAt(uint index)
		{
			int num = 0;
			int num2 = (int)index;
			while (true)
			{
				if (base.data[num2] == 0)
				{
					break;
				}
				num++;
				num2++;
			}
			return Encoding.UTF8.GetString(base.data, (int)index, num);
		}
	}
}
