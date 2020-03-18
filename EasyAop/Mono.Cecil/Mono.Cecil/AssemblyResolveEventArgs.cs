using System;

namespace Mono.Cecil
{
	public sealed class AssemblyResolveEventArgs : EventArgs
	{
		private readonly AssemblyNameReference reference;

		public AssemblyNameReference AssemblyReference => reference;

		public AssemblyResolveEventArgs(AssemblyNameReference reference)
		{
			this.reference = reference;
		}
	}
}
