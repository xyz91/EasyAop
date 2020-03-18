using Mono.Cecil.PE;
using System.IO;

namespace Mono.Cecil.Cil
{
	public sealed class PortablePdbWriterProvider : ISymbolWriterProvider
	{
		public ISymbolWriter GetSymbolWriter(ModuleDefinition module, string fileName)
		{
			Mixin.CheckModule(module);
			Mixin.CheckFileName(fileName);
			FileStream value = File.OpenWrite(Mixin.GetPdbFileName(fileName));
			return GetSymbolWriter(module, Disposable.Owned((Stream)value));
		}

		public ISymbolWriter GetSymbolWriter(ModuleDefinition module, Stream symbolStream)
		{
			Mixin.CheckModule(module);
			Mixin.CheckStream(symbolStream);
			return GetSymbolWriter(module, Disposable.NotOwned(symbolStream));
		}

		private ISymbolWriter GetSymbolWriter(ModuleDefinition module, Disposable<Stream> stream)
		{
			MetadataBuilder metadataBuilder = new MetadataBuilder(module, this);
			ImageWriter writer = ImageWriter.CreateDebugWriter(module, metadataBuilder, stream);
			return new PortablePdbWriter(metadataBuilder, module, writer);
		}
	}
}
