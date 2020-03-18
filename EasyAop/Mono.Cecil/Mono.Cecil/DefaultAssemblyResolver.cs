using System;
using System.Collections.Generic;

namespace Mono.Cecil
{
	public class DefaultAssemblyResolver : BaseAssemblyResolver
	{
		private readonly IDictionary<string, AssemblyDefinition> cache;

		public DefaultAssemblyResolver()
		{
			cache = new Dictionary<string, AssemblyDefinition>(StringComparer.Ordinal);
		}

		public override AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			Mixin.CheckName(name);
			if (cache.TryGetValue(name.FullName, out AssemblyDefinition assemblyDefinition))
			{
				return assemblyDefinition;
			}
			assemblyDefinition = base.Resolve(name);
			cache[name.FullName] = assemblyDefinition;
			return assemblyDefinition;
		}

		protected void RegisterAssembly(AssemblyDefinition assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			string fullName = assembly.Name.FullName;
			if (!cache.ContainsKey(fullName))
			{
				cache[fullName] = assembly;
			}
		}

		protected override void Dispose(bool disposing)
		{
			foreach (AssemblyDefinition value in cache.Values)
			{
				value.Dispose();
			}
			cache.Clear();
			base.Dispose(disposing);
		}
	}
}
