using Mono.Cecil.PE;
using System.IO;

namespace Mono.Cecil.Cil
{
	public sealed class PortablePdbReaderProvider : ISymbolReaderProvider
	{
		public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
		{
			Mixin.CheckModule(module);
			Mixin.CheckFileName(fileName);
			FileStream fileStream = File.OpenRead(Mixin.GetPdbFileName(fileName));
			return GetSymbolReader(module, Disposable.Owned((Stream)fileStream), fileStream.Name);
		}

		public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
		{
			Mixin.CheckModule(module);
			Mixin.CheckStream(symbolStream);
			return GetSymbolReader(module, Disposable.NotOwned(symbolStream), symbolStream.GetFileName());
		}

		private ISymbolReader GetSymbolReader(ModuleDefinition module, Disposable<Stream> symbolStream, string fileName)
		{
			return new PortablePdbReader(ImageReader.ReadPortablePdb(symbolStream, fileName), module);
		}
	}
}
