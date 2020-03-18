using Mono.Cecil.Cil;
using Mono.Cecil.PE;
using System;
using System.IO;

namespace Mono.Cecil
{
	internal static class ModuleWriter
	{
		public static void WriteModule(ModuleDefinition module, Disposable<Stream> stream, WriterParameters parameters)
		{
			using (stream)
			{
				Write(module, stream, parameters);
			}
		}

		private static void Write(ModuleDefinition module, Disposable<Stream> stream, WriterParameters parameters)
		{
			if ((module.Attributes & ModuleAttributes.ILOnly) == (ModuleAttributes)0)
			{
				throw new NotSupportedException("Writing mixed-mode assemblies is not supported");
			}
			if (module.HasImage && module.ReadingMode == ReadingMode.Deferred)
			{
				ImmediateModuleReader immediateModuleReader = new ImmediateModuleReader(module.Image);
				immediateModuleReader.ReadModule(module, false);
				immediateModuleReader.ReadSymbols(module);
			}
			module.MetadataSystem.Clear();
			if (module.symbol_reader != null)
			{
				module.symbol_reader.Dispose();
			}
			AssemblyNameDefinition assemblyNameDefinition = (module.assembly != null) ? module.assembly.Name : null;
			string fileName = stream.value.GetFileName();
			uint timestamp = parameters.Timestamp ?? module.timestamp;
			ISymbolWriterProvider symbolWriterProvider = parameters.SymbolWriterProvider;
			if (symbolWriterProvider == null && parameters.WriteSymbols)
			{
				symbolWriterProvider = new DefaultSymbolWriterProvider();
			}
			if (parameters.StrongNameKeyPair != null && assemblyNameDefinition != null)
			{
				assemblyNameDefinition.PublicKey = parameters.StrongNameKeyPair.PublicKey;
				module.Attributes |= ModuleAttributes.StrongNameSigned;
			}
			using (ISymbolWriter symbol_writer = GetSymbolWriter(module, fileName, symbolWriterProvider, parameters))
			{
				MetadataBuilder metadata = new MetadataBuilder(module, fileName, timestamp, symbolWriterProvider, symbol_writer);
				BuildMetadata(module, metadata);
				ImageWriter imageWriter = ImageWriter.CreateWriter(module, metadata, stream);
				stream.value.SetLength(0L);
				imageWriter.WriteImage();
				if (parameters.StrongNameKeyPair != null)
				{
					CryptoService.StrongName(stream.value, imageWriter, parameters.StrongNameKeyPair);
				}
			}
		}

		private static void BuildMetadata(ModuleDefinition module, MetadataBuilder metadata)
		{
			if (!module.HasImage)
			{
				metadata.BuildMetadata();
			}
			else
			{
				module.Read(metadata, delegate(MetadataBuilder builder, MetadataReader _)
				{
					builder.BuildMetadata();
					return builder;
				});
			}
		}

		private static ISymbolWriter GetSymbolWriter(ModuleDefinition module, string fq_name, ISymbolWriterProvider symbol_writer_provider, WriterParameters parameters)
		{
			if (symbol_writer_provider == null)
			{
				return null;
			}
			if (parameters.SymbolStream != null)
			{
				return symbol_writer_provider.GetSymbolWriter(module, parameters.SymbolStream);
			}
			return symbol_writer_provider.GetSymbolWriter(module, fq_name);
		}
	}
}
