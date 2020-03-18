using System;
using System.Collections;
using System.Collections.Generic;

namespace Mono.Collections.Generic
{
	public sealed class ReadOnlyCollection<T> : Collection<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection
	{
		private static ReadOnlyCollection<T> empty;

		public static ReadOnlyCollection<T> Empty => empty ?? (empty = new ReadOnlyCollection<T>());

		bool ICollection<T>.IsReadOnly
		{
			get
			{
				return true;
			}
		}

		bool IList.IsFixedSize
		{
			get
			{
				return true;
			}
		}

		bool IList.IsReadOnly
		{
			get
			{
				return true;
			}
		}

		private ReadOnlyCollection()
		{
		}

		public ReadOnlyCollection(T[] array)
		{
			if (array == null)
			{
				throw new ArgumentNullException();
			}
			Initialize(array, array.Length);
		}

		public ReadOnlyCollection(Collection<T> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException();
			}
			Initialize(collection.items, collection.size);
		}

		private void Initialize(T[] items, int size)
		{
			base.items = new T[size];
			Array.Copy(items, 0, base.items, 0, size);
			base.size = size;
		}

		internal override void Grow(int desired)
		{
			throw new InvalidOperationException();
		}

		protected override void OnAdd(T item, int index)
		{
			throw new InvalidOperationException();
		}

		protected override void OnClear()
		{
			throw new InvalidOperationException();
		}

		protected override void OnInsert(T item, int index)
		{
			throw new InvalidOperationException();
		}

		protected override void OnRemove(T item, int index)
		{
			throw new InvalidOperationException();
		}

		protected override void OnSet(T item, int index)
		{
			throw new InvalidOperationException();
		}
	}
}
