using Mono.Collections.Generic;
using System;

namespace Mono.Cecil
{
	internal sealed class MemberDefinitionCollection<T> : Collection<T> where T : IMemberDefinition
	{
		private TypeDefinition container;

		internal MemberDefinitionCollection(TypeDefinition container)
		{
			this.container = container;
		}

		internal MemberDefinitionCollection(TypeDefinition container, int capacity)
			: base(capacity)
		{
			this.container = container;
		}

		protected override void OnAdd(T item, int index)
		{
			Attach(item);
		}

		protected sealed override void OnSet(T item, int index)
		{
			Attach(item);
		}

		protected sealed override void OnInsert(T item, int index)
		{
			Attach(item);
		}

		protected sealed override void OnRemove(T item, int index)
		{
			Detach(item);
		}

		protected sealed override void OnClear()
		{
			foreach (T item in this)
			{
				Detach(item);
			}
		}

		private void Attach(T element)
		{
			if (element.DeclaringType != container)
			{
				if (element.DeclaringType != null)
				{
					throw new ArgumentException("Member already attached");
				}
				element.DeclaringType = container;
			}
		}

		private static void Detach(T element)
		{
			element.DeclaringType = null;
		}
	}
}
