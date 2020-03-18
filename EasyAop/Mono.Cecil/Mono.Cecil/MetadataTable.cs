using Mono.Cecil.Metadata;
using System;

namespace Mono.Cecil
{
	internal abstract class MetadataTable
	{
		public abstract int Length
		{
			get;
		}

		public bool IsLarge => Length > 65535;

		public abstract void Write(TableHeapBuffer buffer);

		public abstract void Sort();
	}
	internal abstract class MetadataTable<TRow> : MetadataTable where TRow : struct
	{
		internal TRow[] rows = new TRow[2];

		internal int length;

		public sealed override int Length => length;

		public int AddRow(TRow row)
		{
			if (rows.Length == length)
			{
				Grow();
			}
			rows[length++] = row;
			return length;
		}

		private void Grow()
		{
			TRow[] destinationArray = new TRow[rows.Length * 2];
			Array.Copy(rows, destinationArray, rows.Length);
			rows = destinationArray;
		}

		public override void Sort()
		{
		}
	}
}
