namespace Mono.Cecil
{
	public sealed class ModuleParameters
	{
		private ModuleKind kind;

		private TargetRuntime runtime;

		private uint? timestamp;

		private TargetArchitecture architecture;

		private IAssemblyResolver assembly_resolver;

		private IMetadataResolver metadata_resolver;

		private IMetadataImporterProvider metadata_importer_provider;

		private IReflectionImporterProvider reflection_importer_provider;

		public ModuleKind Kind
		{
			get
			{
				return kind;
			}
			set
			{
				kind = value;
			}
		}

		public TargetRuntime Runtime
		{
			get
			{
				return runtime;
			}
			set
			{
				runtime = value;
			}
		}

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

		public TargetArchitecture Architecture
		{
			get
			{
				return architecture;
			}
			set
			{
				architecture = value;
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

		public ModuleParameters()
		{
			kind = ModuleKind.Dll;
			Runtime = GetCurrentRuntime();
			architecture = TargetArchitecture.I386;
		}

		private static TargetRuntime GetCurrentRuntime()
		{
			return typeof(object).Assembly.ImageRuntimeVersion.ParseRuntime();
		}
	}
}
