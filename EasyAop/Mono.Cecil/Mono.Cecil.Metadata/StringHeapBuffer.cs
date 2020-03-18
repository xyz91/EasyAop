using System;
using System.Collections.Generic;
using System.Text;

namespace Mono.Cecil.Metadata
{
	internal class StringHeapBuffer : HeapBuffer
	{
		private class SuffixSort : IComparer<KeyValuePair<string, uint>>
		{
			public int Compare(KeyValuePair<string, uint> xPair, KeyValuePair<string, uint> yPair)
			{
				string key = xPair.Key;
				string key2 = yPair.Key;
				int num = key.Length - 1;
				int num2 = key2.Length - 1;
				while (num >= 0 & num2 >= 0)
				{
					if (key[num] < key2[num2])
					{
						return -1;
					}
					if (key[num] > key2[num2])
					{
						return 1;
					}
					num--;
					num2--;
				}
				return key2.Length.CompareTo(key.Length);
			}
		}

		protected Dictionary<string, uint> strings = new Dictionary<string, uint>(StringComparer.Ordinal);

		public sealed override bool IsEmpty => base.length <= 1;

		public StringHeapBuffer()
			: base(1)
		{
			base.WriteByte(0);
		}

		public virtual uint GetStringIndex(string @string)
		{
			if (strings.TryGetValue(@string, out uint num))
			{
				return num;
			}
			num = (uint)(strings.Count + 1);
			strings.Add(@string, num);
			return num;
		}

		public uint[] WriteStrings()
		{
			List<KeyValuePair<string, uint>> list = SortStrings(strings);
			strings = null;
			uint[] array = new uint[list.Count + 1];
			array[0] = 0u;
			string text = string.Empty;
			foreach (KeyValuePair<string, uint> item in list)
			{
				string key = item.Key;
				uint value = item.Value;
				int position = base.position;
				if (text.EndsWith(key, StringComparison.Ordinal) && !IsLowSurrogateChar(item.Key[0]))
				{
					array[value] = (uint)(position - (Encoding.UTF8.GetByteCount(item.Key) + 1));
				}
				else
				{
					array[value] = (uint)position;
					WriteString(key);
				}
				text = item.Key;
			}
			return array;
		}

		private static List<KeyValuePair<string, uint>> SortStrings(Dictionary<string, uint> strings)
		{
			List<KeyValuePair<string, uint>> list = new List<KeyValuePair<string, uint>>(strings);
			list.Sort(new SuffixSort());
			return list;
		}

		private static bool IsLowSurrogateChar(int c)
		{
			return (uint)(c - 56320) <= 1023u;
		}

		protected virtual void WriteString(string @string)
		{
			base.WriteBytes(Encoding.UTF8.GetBytes(@string));
			base.WriteByte(0);
		}
	}
}
