using Mono.Collections.Generic;
using System;

namespace Mono.Cecil
{
	public interface IAssemblyResolver : IDisposable
	{
		AssemblyDefinition Resolve(AssemblyNameReference name);

		AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters);

        void AddSearchDirectory(string directory);

        void RemoveSearchDirectory(string directory);
    }
}
