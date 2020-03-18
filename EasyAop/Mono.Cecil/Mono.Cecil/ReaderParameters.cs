using Mono.Cecil.Cil;
using System.IO;

namespace Mono.Cecil
{
	public sealed class ReaderParameters
	{
		private ReadingMode reading_mode;

		internal IAssemblyResolver assembly_resolver;

		internal IMetadataResolver metadata_resolver;

		internal IMetadataImporterProvider metadata_importer_provider;

		internal IReflectionImporterProvider reflection_importer_provider;

		private Stream symbol_stream;

		private ISymbolReaderProvider symbol_reader_provider;

		private bool read_symbols;

		private bool throw_symbols_mismatch;

		private bool projections;

		private bool in_memory;

		private bool read_write;

		public ReadingMode ReadingMode
		{
			get
			{
				return reading_mode;
			}
			set
			{
				reading_mode = value;
			}
		}

		public bool InMemory
		{
			get
			{
				return in_memory;
			}
			set
			{
				in_memory = value;
			}
		}

		public IAssemblyResolver AssemblyResolver
		{
			get
			{
				return assembly_resolver;
			}
			set
			{
				assembly_resolver = value;
			}
		}

		public IMetadataResolver MetadataResolver
		{
			get
			{
				return metadata_resolver;
			}
			set
			{
				metadata_resolver = value;
			}
		}

		public IMetadataImporterProvider MetadataImporterProvider
		{
			get
			{
				return metadata_importer_provider;
			}
			set
			{
				metadata_importer_provider = value;
			}
		}

		public IReflectionImporterProvider ReflectionImporterProvider
		{
			get
			{
				return reflection_importer_provider;
			}
			set
			{
				reflection_importer_provider = value;
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

		public ISymbolReaderProvider SymbolReaderProvider
		{
			get
			{
				return symbol_reader_provider;
			}
			set
			{
				symbol_reader_provider = value;
			}
		}

		public bool ReadSymbols
		{
			get
			{
				return read_symbols;
			}
			set
			{
				read_symbols = value;
			}
		}

		public bool ThrowIfSymbolsAreNotMatching
		{
			get
			{
				return throw_symbols_mismatch;
			}
			set
			{
				throw_symbols_mismatch = value;
			}
		}

		public bool ReadWrite
		{
			get
			{
				return read_write;
			}
			set
			{
				read_write = value;
			}
		}

		public bool ApplyWindowsRuntimeProjections
		{
			get
			{
				return projections;
			}
			set
			{
				projections = value;
			}
		}

		public ReaderParameters()
			: this(ReadingMode.Deferred)
		{
		}

		public ReaderParameters(ReadingMode readingMode)
		{
			reading_mode = readingMode;
			throw_symbols_mismatch = true;
		}
	}
}
