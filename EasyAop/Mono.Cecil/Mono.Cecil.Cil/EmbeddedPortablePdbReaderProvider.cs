using Mono.Cecil.PE;
using System;
using System.IO;
using System.IO.Compression;

namespace Mono.Cecil.Cil
{
	public sealed class EmbeddedPortablePdbReaderProvider : ISymbolReaderProvider
	{
		public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
		{
			Mixin.CheckModule(module);
			Mixin.CheckFileName(fileName);
			ImageDebugHeaderEntry embeddedPortablePdbEntry = module.GetDebugHeader().GetEmbeddedPortablePdbEntry();
			if (embeddedPortablePdbEntry == null)
			{
				throw new InvalidOperationException();
			}
			return new EmbeddedPortablePdbReader((PortablePdbReader)new PortablePdbReaderProvider().GetSymbolReader(module, GetPortablePdbStream(embeddedPortablePdbEntry)));
		}

		private static Stream GetPortablePdbStream(ImageDebugHeaderEntry entry)
		{
			MemoryStream stream = new MemoryStream(entry.Data);
			BinaryStreamReader binaryStreamReader = new BinaryStreamReader(stream);
			binaryStreamReader.ReadInt32();
			MemoryStream memoryStream = new MemoryStream(binaryStreamReader.ReadInt32());
			using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress, true))
			{
				deflateStream.CopyTo(memoryStream);
				return memoryStream;
			}
		}

		public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
		{
			throw new NotSupportedException();
		}
	}
}
