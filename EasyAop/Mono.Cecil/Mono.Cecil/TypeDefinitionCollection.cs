using Mono.Cecil.Metadata;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;

namespace Mono.Cecil
{
	internal sealed class TypeDefinitionCollection : Collection<TypeDefinition>
	{
		private readonly ModuleDefinition container;

		private readonly Dictionary<Row<string, string>, TypeDefinition> name_cache;

		internal TypeDefinitionCollection(ModuleDefinition container)
		{
			this.container = container;
			name_cache = new Dictionary<Row<string, string>, TypeDefinition>(new RowEqualityComparer());
		}

		internal TypeDefinitionCollection(ModuleDefinition container, int capacity)
			: base(capacity)
		{
			this.container = container;
			name_cache = new Dictionary<Row<string, string>, TypeDefinition>(capacity, new RowEqualityComparer());
		}

		protected override void OnAdd(TypeDefinition item, int index)
		{
			Attach(item);
		}

		protected override void OnSet(TypeDefinition item, int index)
		{
			Attach(item);
		}

		protected override void OnInsert(TypeDefinition item, int index)
		{
			Attach(item);
		}

		protected override void OnRemove(TypeDefinition item, int index)
		{
			Detach(item);
		}

		protected override void OnClear()
		{
			foreach (TypeDefinition item in this)
			{
				Detach(item);
			}
		}

		private void Attach(TypeDefinition type)
		{
			if (type.Module != null && type.Module != container)
			{
				throw new ArgumentException("Type already attached");
			}
			type.module = container;
			type.scope = container;
			name_cache[new Row<string, string>(type.Namespace, type.Name)] = type;
		}

		private void Detach(TypeDefinition type)
		{
			type.module = null;
			type.scope = null;
			name_cache.Remove(new Row<string, string>(type.Namespace, type.Name));
		}

		public TypeDefinition GetType(string fullname)
		{
			TypeParser.SplitFullName(fullname, out string @namespace, out string name);
			return GetType(@namespace, name);
		}

		public TypeDefinition GetType(string @namespace, string name)
		{
			if (name_cache.TryGetValue(new Row<string, string>(@namespace, name), out TypeDefinition result))
			{
				return result;
			}
			return null;
		}
	}
}
