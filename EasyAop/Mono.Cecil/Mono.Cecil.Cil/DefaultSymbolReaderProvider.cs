using System;
using System.IO;

namespace Mono.Cecil.Cil
{
	public class DefaultSymbolReaderProvider : ISymbolReaderProvider
	{
		private readonly bool throw_if_no_symbol;

		public DefaultSymbolReaderProvider()
			: this(true)
		{
		}

		public DefaultSymbolReaderProvider(bool throwIfNoSymbol)
		{
			throw_if_no_symbol = throwIfNoSymbol;
		}

		public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
		{
			if (module.Image.HasDebugTables())
			{
				return null;
			}
			if (module.HasDebugHeader && module.GetDebugHeader().GetEmbeddedPortablePdbEntry() != null)
			{
				return new EmbeddedPortablePdbReaderProvider().GetSymbolReader(module, fileName);
			}
			if (File.Exists(Mixin.GetPdbFileName(fileName)))
			{
				if (Mixin.IsPortablePdb(Mixin.GetPdbFileName(fileName)))
				{
					return new PortablePdbReaderProvider().GetSymbolReader(module, fileName);
				}
				try
				{
					return SymbolProvider.GetReaderProvider(SymbolKind.NativePdb).GetSymbolReader(module, fileName);
				}
				catch (Exception)
				{
				}
			}
			if (File.Exists(Mixin.GetMdbFileName(fileName)))
			{
				try
				{
					return SymbolProvider.GetReaderProvider(SymbolKind.Mdb).GetSymbolReader(module, fileName);
				}
				catch (Exception)
				{
				}
			}
			if (throw_if_no_symbol)
			{
				throw new SymbolsNotFoundException($"No symbol found for file: {fileName}");
			}
			return null;
		}

		public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
		{
			throw new NotSupportedException();
		}
	}
}
