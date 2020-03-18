namespace Mono.Cecil.Metadata
{
	internal sealed class UserStringHeapBuffer : StringHeapBuffer
	{
		public override uint GetStringIndex(string @string)
		{
			if (base.strings.TryGetValue(@string, out uint position))
			{
				return position;
			}
			position = (uint)base.position;
			WriteString(@string);
			base.strings.Add(@string, position);
			return position;
		}

		protected override void WriteString(string @string)
		{
			base.WriteCompressedUInt32((uint)(@string.Length * 2 + 1));
			byte b = 0;
			foreach (char c in @string)
			{
				base.WriteUInt16(c);
				if (b != 1 && (c < ' ' || c > '~') && (c > '~' || (c >= '\u0001' && c <= '\b') || (c >= '\u000e' && c <= '\u001f') || c == '\'' || c == '-'))
				{
					b = 1;
				}
			}
			base.WriteByte(b);
		}
	}
}
