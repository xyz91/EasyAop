using Mono.Cecil.Cil;
using System.IO;
using System.Reflection;

namespace Mono.Cecil
{
	public sealed class WriterParameters
	{
		private uint? timestamp;

		private Stream symbol_stream;

		private ISymbolWriterProvider symbol_writer_provider;

		private bool write_symbols;

		private StrongNameKeyPair key_pair;

		public uint? Timestamp
		{
			get
			{
				return timestamp;
			}
			set
			{
				timestamp = value;
			}
		}

		public Stream SymbolStream
		{
			get
			{
				return symbol_stream;
			}
			set
			{
				symbol_stream = value;
			}
		}

		public ISymbolWriterProvider SymbolWriterProvider
		{
			get
			{
				return symbol_writer_provider;
			}
			set
			{
				symbol_writer_provider = value;
			}
		}

		public bool WriteSymbols
		{
			get
			{
				return write_symbols;
			}
			set
			{
				write_symbols = value;
			}
		}

		public StrongNameKeyPair StrongNameKeyPair
		{
			get
			{
				return key_pair;
			}
			set
			{
				key_pair = value;
			}
		}
	}
}
