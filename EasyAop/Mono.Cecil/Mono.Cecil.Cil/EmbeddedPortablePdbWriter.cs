using Mono.Cecil.PE;
using System;
using System.IO;
using System.IO.Compression;

namespace Mono.Cecil.Cil
{
	public sealed class EmbeddedPortablePdbWriter : ISymbolWriter, IDisposable, IMetadataSymbolWriter
	{
		private readonly Stream stream;

		private readonly PortablePdbWriter writer;

		internal EmbeddedPortablePdbWriter(Stream stream, PortablePdbWriter writer)
		{
			this.stream = stream;
			this.writer = writer;
		}

		public ISymbolReaderProvider GetReaderProvider()
		{
			return new EmbeddedPortablePdbReaderProvider();
		}

		public ImageDebugHeader GetDebugHeader()
		{
			writer.Dispose();
			ImageDebugDirectory imageDebugDirectory = default(ImageDebugDirectory);
			imageDebugDirectory.Type = ImageDebugType.EmbeddedPortablePdb;
			ImageDebugDirectory directory = imageDebugDirectory;
			MemoryStream memoryStream = new MemoryStream();
			BinaryStreamWriter binaryStreamWriter = new BinaryStreamWriter(memoryStream);
			binaryStreamWriter.WriteByte(77);
			binaryStreamWriter.WriteByte(80);
			binaryStreamWriter.WriteByte(68);
			binaryStreamWriter.WriteByte(66);
			binaryStreamWriter.WriteInt32((int)stream.Length);
			stream.Position = 0L;
			using (DeflateStream destination = new DeflateStream(memoryStream, CompressionMode.Compress, true))
			{
				stream.CopyTo(destination);
			}
			directory.SizeOfData = (int)memoryStream.Length;
			return new ImageDebugHeader(new ImageDebugHeaderEntry[2]
			{
				writer.GetDebugHeader().Entries[0],
				new ImageDebugHeaderEntry(directory, memoryStream.ToArray())
			});
		}

		public void Write(MethodDebugInformation info)
		{
			writer.Write(info);
		}

		public void Dispose()
		{
		}

		void IMetadataSymbolWriter.SetMetadata(MetadataBuilder metadata)
		{
			((IMetadataSymbolWriter)writer).SetMetadata(metadata);
		}

		void IMetadataSymbolWriter.WriteModule()
		{
			((IMetadataSymbolWriter)writer).WriteModule();
		}
	}
}
