using Mono.Cecil.Cil;
using Mono.Cecil.Metadata;
using Mono.Cecil.PE;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Mono.Cecil
{
	public sealed class ModuleDefinition : ModuleReference, ICustomAttributeProvider, IMetadataTokenProvider, ICustomDebugInformationProvider, IDisposable
	{
		internal Image Image;

		internal MetadataSystem MetadataSystem;

		internal ReadingMode ReadingMode;

		internal ISymbolReaderProvider SymbolReaderProvider;

		internal ISymbolReader symbol_reader;

		internal Disposable<IAssemblyResolver> assembly_resolver;

		internal IMetadataResolver metadata_resolver;

		internal TypeSystem type_system;

		internal readonly MetadataReader reader;

		private readonly string file_name;

		internal string runtime_version;

		internal ModuleKind kind;

		private WindowsRuntimeProjections projections;

		private MetadataKind metadata_kind;

		private TargetRuntime runtime;

		private TargetArchitecture architecture;

		private ModuleAttributes attributes;

		private ModuleCharacteristics characteristics;

		internal ushort linker_version = 8;

		private Guid mvid;

		internal uint timestamp;

		internal AssemblyDefinition assembly;

		private MethodDefinition entry_point;

		internal IReflectionImporter reflection_importer;

		internal IMetadataImporter metadata_importer;

		private Collection<CustomAttribute> custom_attributes;

		private Collection<AssemblyNameReference> references;

		private Collection<ModuleReference> modules;

		private Collection<Resource> resources;

		private Collection<ExportedType> exported_types;

		private TypeDefinitionCollection types;

		internal Collection<CustomDebugInformation> custom_infos;

		private readonly object module_lock = new object();

		public bool IsMain => kind != ModuleKind.NetModule;

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

		public MetadataKind MetadataKind
		{
			get
			{
				return metadata_kind;
			}
			set
			{
				metadata_kind = value;
			}
		}

		internal WindowsRuntimeProjections Projections
		{
			get
			{
				if (projections == null)
				{
					Interlocked.CompareExchange(ref projections, new WindowsRuntimeProjections(this), null);
				}
				return projections;
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
				runtime_version = runtime.RuntimeVersionString();
			}
		}

		public string RuntimeVersion
		{
			get
			{
				return runtime_version;
			}
			set
			{
				runtime_version = value;
				runtime = runtime_version.ParseRuntime();
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

		public ModuleAttributes Attributes
		{
			get
			{
				return attributes;
			}
			set
			{
				attributes = value;
			}
		}

		public ModuleCharacteristics Characteristics
		{
			get
			{
				return characteristics;
			}
			set
			{
				characteristics = value;
			}
		}

		[Obsolete("Use FileName")]
		public string FullyQualifiedName
		{
			get
			{
				return file_name;
			}
		}

		public string FileName => file_name;

		public Guid Mvid
		{
			get
			{
				return mvid;
			}
			set
			{
				mvid = value;
			}
		}

		internal bool HasImage => Image != null;

		public bool HasSymbols => symbol_reader != null;

		public ISymbolReader SymbolReader => symbol_reader;

		public override MetadataScopeType MetadataScopeType => MetadataScopeType.ModuleDefinition;

		public AssemblyDefinition Assembly => assembly;

		internal IReflectionImporter ReflectionImporter
		{
			get
			{
				if (reflection_importer == null)
				{
					Interlocked.CompareExchange(ref reflection_importer, new DefaultReflectionImporter(this), null);
				}
				return reflection_importer;
			}
		}

		internal IMetadataImporter MetadataImporter
		{
			get
			{
				if (metadata_importer == null)
				{
					Interlocked.CompareExchange(ref metadata_importer, new DefaultMetadataImporter(this), null);
				}
				return metadata_importer;
			}
		}

		public IAssemblyResolver AssemblyResolver
		{
			get
			{
				if (assembly_resolver.value == null)
				{
					lock (module_lock)
					{
						assembly_resolver = Disposable.Owned((IAssemblyResolver)new DefaultAssemblyResolver());
					}
				}
				return assembly_resolver.value;
			}
		}

		public IMetadataResolver MetadataResolver
		{
			get
			{
				if (metadata_resolver == null)
				{
					Interlocked.CompareExchange(ref metadata_resolver, new MetadataResolver(AssemblyResolver), null);
				}
				return metadata_resolver;
			}
		}

		public TypeSystem TypeSystem
		{
			get
			{
				if (type_system == null)
				{
					Interlocked.CompareExchange(ref type_system, TypeSystem.CreateTypeSystem(this), null);
				}
				return type_system;
			}
		}

		public bool HasAssemblyReferences
		{
			get
			{
				if (references != null)
				{
					return references.Count > 0;
				}
				if (HasImage)
				{
					return Image.HasTable(Table.AssemblyRef);
				}
				return false;
			}
		}

		public Collection<AssemblyNameReference> AssemblyReferences
		{
			get
			{
				if (references != null)
				{
					return references;
				}
				if (HasImage)
				{
					return Read(ref references, this, (ModuleDefinition _, MetadataReader reader) => reader.ReadAssemblyReferences());
				}
				return references = new Collection<AssemblyNameReference>();
			}
		}

		public bool HasModuleReferences
		{
			get
			{
				if (modules != null)
				{
					return modules.Count > 0;
				}
				if (HasImage)
				{
					return Image.HasTable(Table.ModuleRef);
				}
				return false;
			}
		}

		public Collection<ModuleReference> ModuleReferences
		{
			get
			{
				if (modules != null)
				{
					return modules;
				}
				if (HasImage)
				{
					return Read(ref modules, this, (ModuleDefinition _, MetadataReader reader) => reader.ReadModuleReferences());
				}
				return modules = new Collection<ModuleReference>();
			}
		}

		public bool HasResources
		{
			get
			{
				if (resources != null)
				{
					return resources.Count > 0;
				}
				if (HasImage)
				{
					if (!Image.HasTable(Table.ManifestResource))
					{
						return Read(this, (ModuleDefinition _, MetadataReader reader) => reader.HasFileResource());
					}
					return true;
				}
				return false;
			}
		}

		public Collection<Resource> Resources
		{
			get
			{
				if (resources != null)
				{
					return resources;
				}
				if (HasImage)
				{
					return Read(ref resources, this, (ModuleDefinition _, MetadataReader reader) => reader.ReadResources());
				}
				return resources = new Collection<Resource>();
			}
		}

		public bool HasCustomAttributes
		{
			get
			{
				if (custom_attributes != null)
				{
					return custom_attributes.Count > 0;
				}
				return this.GetHasCustomAttributes(this);
			}
		}

		public Collection<CustomAttribute> CustomAttributes => custom_attributes ?? this.GetCustomAttributes(ref custom_attributes, this);

		public bool HasTypes
		{
			get
			{
				if (types != null)
				{
					return types.Count > 0;
				}
				if (HasImage)
				{
					return Image.HasTable(Table.TypeDef);
				}
				return false;
			}
		}

		public Collection<TypeDefinition> Types
		{
			get
			{
				if (types != null)
				{
					return types;
				}
				if (HasImage)
				{
					return Read(ref types, this, (ModuleDefinition _, MetadataReader reader) => reader.ReadTypes());
				}
				return types = new TypeDefinitionCollection(this);
			}
		}

		public bool HasExportedTypes
		{
			get
			{
				if (exported_types != null)
				{
					return exported_types.Count > 0;
				}
				if (HasImage)
				{
					return Image.HasTable(Table.ExportedType);
				}
				return false;
			}
		}

		public Collection<ExportedType> ExportedTypes
		{
			get
			{
				if (exported_types != null)
				{
					return exported_types;
				}
				if (HasImage)
				{
					return Read(ref exported_types, this, (ModuleDefinition _, MetadataReader reader) => reader.ReadExportedTypes());
				}
				return exported_types = new Collection<ExportedType>();
			}
		}

		public MethodDefinition EntryPoint
		{
			get
			{
				if (entry_point != null)
				{
					return entry_point;
				}
				if (HasImage)
				{
					return Read(ref entry_point, this, (ModuleDefinition _, MetadataReader reader) => reader.ReadEntryPoint());
				}
				return entry_point = null;
			}
			set
			{
				entry_point = value;
			}
		}

		public bool HasCustomDebugInformations
		{
			get
			{
				if (custom_infos != null)
				{
					return custom_infos.Count > 0;
				}
				return false;
			}
		}

		public Collection<CustomDebugInformation> CustomDebugInformations => custom_infos ?? (custom_infos = new Collection<CustomDebugInformation>());

		internal object SyncRoot => module_lock;

		public bool HasDebugHeader
		{
			get
			{
				if (Image != null)
				{
					return Image.DebugHeader != null;
				}
				return false;
			}
		}

		internal ModuleDefinition()
		{
			MetadataSystem = new MetadataSystem();
			base.token = new MetadataToken(TokenType.Module, 1);
		}

		internal ModuleDefinition(Image image)
			: this()
		{
			Image = image;
			kind = image.Kind;
			RuntimeVersion = image.RuntimeVersion;
			architecture = image.Architecture;
			attributes = image.Attributes;
			characteristics = image.Characteristics;
			linker_version = image.LinkerVersion;
			file_name = image.FileName;
			timestamp = image.Timestamp;
			reader = new MetadataReader(this);
		}

		public void Dispose()
		{
			if (Image != null)
			{
				Image.Dispose();
			}
			if (symbol_reader != null)
			{
				symbol_reader.Dispose();
			}
			if (assembly_resolver.value != null)
			{
				assembly_resolver.Dispose();
			}
		}

		public bool HasTypeReference(string fullName)
		{
			return HasTypeReference(string.Empty, fullName);
		}

		public bool HasTypeReference(string scope, string fullName)
		{
			Mixin.CheckFullName(fullName);
			if (!HasImage)
			{
				return false;
			}
			return GetTypeReference(scope, fullName) != null;
		}

		public bool TryGetTypeReference(string fullName, out TypeReference type)
		{
			return TryGetTypeReference(string.Empty, fullName, out type);
		}

		public bool TryGetTypeReference(string scope, string fullName, out TypeReference type)
		{
			Mixin.CheckFullName(fullName);
			if (!HasImage)
			{
				type = null;
				return false;
			}
			return (type = GetTypeReference(scope, fullName)) != null;
		}

		private TypeReference GetTypeReference(string scope, string fullname)
		{
			return Read(new Row<string, string>(scope, fullname), (Row<string, string> row, MetadataReader reader) => reader.GetTypeReference(row.Col1, row.Col2));
		}

		public IEnumerable<TypeReference> GetTypeReferences()
		{
			if (!HasImage)
			{
				return Empty<TypeReference>.Array;
			}
			return Read(this, (ModuleDefinition _, MetadataReader reader) => reader.GetTypeReferences());
		}

		public IEnumerable<MemberReference> GetMemberReferences()
		{
			if (!HasImage)
			{
				return Empty<MemberReference>.Array;
			}
			return Read(this, (ModuleDefinition _, MetadataReader reader) => reader.GetMemberReferences());
		}

		public IEnumerable<CustomAttribute> GetCustomAttributes()
		{
			if (!HasImage)
			{
				return Empty<CustomAttribute>.Array;
			}
			return Read(this, (ModuleDefinition _, MetadataReader reader) => reader.GetCustomAttributes());
		}

		public TypeReference GetType(string fullName, bool runtimeName)
		{
			if (!runtimeName)
			{
				return GetType(fullName);
			}
			return TypeParser.ParseType(this, fullName, true);
		}

		public TypeDefinition GetType(string fullName)
		{
			Mixin.CheckFullName(fullName);
			if (fullName.IndexOf('/') > 0)
			{
				return GetNestedType(fullName);
			}
			return ((TypeDefinitionCollection)Types).GetType(fullName);
		}

		public TypeDefinition GetType(string @namespace, string name)
		{
			Mixin.CheckName(name);
			return ((TypeDefinitionCollection)Types).GetType(@namespace ?? string.Empty, name);
		}

		public IEnumerable<TypeDefinition> GetTypes()
		{
			return GetTypes(Types);
		}

		private static IEnumerable<TypeDefinition> GetTypes(Collection<TypeDefinition> types)
		{
			for (int i = 0; i < types.Count; i++)
			{
				TypeDefinition type = types[i];
				yield return type;
				if (type.HasNestedTypes)
				{
					foreach (TypeDefinition type2 in GetTypes(type.NestedTypes))
					{
						yield return type2;
					}
				}
			}
		}

		private TypeDefinition GetNestedType(string fullname)
		{
			string[] array = fullname.Split('/');
			TypeDefinition typeDefinition = GetType(array[0]);
			if (typeDefinition == null)
			{
				return null;
			}
			for (int i = 1; i < array.Length; i++)
			{
				TypeDefinition nestedType = typeDefinition.GetNestedType(array[i]);
				if (nestedType == null)
				{
					return null;
				}
				typeDefinition = nestedType;
			}
			return typeDefinition;
		}

		internal FieldDefinition Resolve(FieldReference field)
		{
			return MetadataResolver.Resolve(field);
		}

		internal MethodDefinition Resolve(MethodReference method)
		{
			return MetadataResolver.Resolve(method);
		}

		internal TypeDefinition Resolve(TypeReference type)
		{
			return MetadataResolver.Resolve(type);
		}

		private static void CheckContext(IGenericParameterProvider context, ModuleDefinition module)
		{
			if (context != null && context.Module != module)
			{
				throw new ArgumentException();
			}
		}

		[Obsolete("Use ImportReference", false)]
		public TypeReference Import(Type type)
		{
			return ImportReference(type, null);
		}

		public TypeReference ImportReference(Type type)
		{
			return ImportReference(type, null);
		}

		[Obsolete("Use ImportReference", false)]
		public TypeReference Import(Type type, IGenericParameterProvider context)
		{
			return ImportReference(type, context);
		}

		public TypeReference ImportReference(Type type, IGenericParameterProvider context)
		{
			Mixin.CheckType(type);
			CheckContext(context, this);
			return ReflectionImporter.ImportReference(type, context);
		}

		[Obsolete("Use ImportReference", false)]
		public FieldReference Import(FieldInfo field)
		{
			return ImportReference(field, null);
		}

		[Obsolete("Use ImportReference", false)]
		public FieldReference Import(FieldInfo field, IGenericParameterProvider context)
		{
			return ImportReference(field, context);
		}

		public FieldReference ImportReference(FieldInfo field)
		{
			return ImportReference(field, null);
		}

		public FieldReference ImportReference(FieldInfo field, IGenericParameterProvider context)
		{
			Mixin.CheckField(field);
			CheckContext(context, this);
			return ReflectionImporter.ImportReference(field, context);
		}

		[Obsolete("Use ImportReference", false)]
		public MethodReference Import(MethodBase method)
		{
			return ImportReference(method, null);
		}

		[Obsolete("Use ImportReference", false)]
		public MethodReference Import(MethodBase method, IGenericParameterProvider context)
		{
			return ImportReference(method, context);
		}

		public MethodReference ImportReference(MethodBase method)
		{
			return ImportReference(method, null);
		}

		public MethodReference ImportReference(MethodBase method, IGenericParameterProvider context)
		{
			Mixin.CheckMethod(method);
			CheckContext(context, this);
			return ReflectionImporter.ImportReference(method, context);
		}

		[Obsolete("Use ImportReference", false)]
		public TypeReference Import(TypeReference type)
		{
			return ImportReference(type, null);
		}

		[Obsolete("Use ImportReference", false)]
		public TypeReference Import(TypeReference type, IGenericParameterProvider context)
		{
			return ImportReference(type, context);
		}

		public TypeReference ImportReference(TypeReference type)
		{
			return ImportReference(type, null);
		}

		public TypeReference ImportReference(TypeReference type, IGenericParameterProvider context)
		{
			Mixin.CheckType(type);
			if (type.Module == this)
			{
				return type;
			}
			CheckContext(context, this);
			return MetadataImporter.ImportReference(type, context);
		}

		[Obsolete("Use ImportReference", false)]
		public FieldReference Import(FieldReference field)
		{
			return ImportReference(field, null);
		}

		[Obsolete("Use ImportReference", false)]
		public FieldReference Import(FieldReference field, IGenericParameterProvider context)
		{
			return ImportReference(field, context);
		}

		public FieldReference ImportReference(FieldReference field)
		{
			return ImportReference(field, null);
		}

		public FieldReference ImportReference(FieldReference field, IGenericParameterProvider context)
		{
			Mixin.CheckField(field);
			if (field.Module == this)
			{
				return field;
			}
			CheckContext(context, this);
			return MetadataImporter.ImportReference(field, context);
		}

		[Obsolete("Use ImportReference", false)]
		public MethodReference Import(MethodReference method)
		{
			return ImportReference(method, null);
		}

		[Obsolete("Use ImportReference", false)]
		public MethodReference Import(MethodReference method, IGenericParameterProvider context)
		{
			return ImportReference(method, context);
		}

		public MethodReference ImportReference(MethodReference method)
		{
			return ImportReference(method, null);
		}

		public MethodReference ImportReference(MethodReference method, IGenericParameterProvider context)
		{
			Mixin.CheckMethod(method);
			if (method.Module == this)
			{
				return method;
			}
			CheckContext(context, this);
			return MetadataImporter.ImportReference(method, context);
		}

		public IMetadataTokenProvider LookupToken(int token)
		{
			return LookupToken(new MetadataToken((uint)token));
		}

		public IMetadataTokenProvider LookupToken(MetadataToken token)
		{
			return Read(token, (MetadataToken t, MetadataReader reader) => reader.LookupToken(t));
		}

		internal void Read<TItem>(TItem item, Action<TItem, MetadataReader> read)
		{
			lock (module_lock)
			{
				int position = reader.position;
				IGenericContext context = reader.context;
				read(item, reader);
				reader.position = position;
				reader.context = context;
			}
		}

		internal TRet Read<TItem, TRet>(TItem item, Func<TItem, MetadataReader, TRet> read)
		{
			lock (module_lock)
			{
				int position = reader.position;
				IGenericContext context = reader.context;
				TRet result = read(item, reader);
				reader.position = position;
				reader.context = context;
				return result;
			}
		}

		internal TRet Read<TItem, TRet>(ref TRet variable, TItem item, Func<TItem, MetadataReader, TRet> read) where TRet : class
		{
			lock (module_lock)
			{
				if (variable != null)
				{
					return variable;
				}
				int position = reader.position;
				IGenericContext context = reader.context;
				TRet val = read(item, reader);
				reader.position = position;
				reader.context = context;
				return variable = val;
			}
		}

		public ImageDebugHeader GetDebugHeader()
		{
			return Image.DebugHeader ?? new ImageDebugHeader();
		}

		public static ModuleDefinition CreateModule(string name, ModuleKind kind)
		{
			return CreateModule(name, new ModuleParameters
			{
				Kind = kind
			});
		}

		public static ModuleDefinition CreateModule(string name, ModuleParameters parameters)
		{
			Mixin.CheckName(name);
			Mixin.CheckParameters(parameters);
			ModuleDefinition moduleDefinition = new ModuleDefinition
			{
				Name = name,
				kind = parameters.Kind,
				timestamp = (parameters.Timestamp ?? Mixin.GetTimestamp()),
				Runtime = parameters.Runtime,
				architecture = parameters.Architecture,
				mvid = Guid.NewGuid(),
				Attributes = ModuleAttributes.ILOnly,
				Characteristics = (ModuleCharacteristics.DynamicBase | ModuleCharacteristics.NoSEH | ModuleCharacteristics.NXCompat | ModuleCharacteristics.TerminalServerAware)
			};
			if (parameters.AssemblyResolver != null)
			{
				moduleDefinition.assembly_resolver = Disposable.NotOwned(parameters.AssemblyResolver);
			}
			if (parameters.MetadataResolver != null)
			{
				moduleDefinition.metadata_resolver = parameters.MetadataResolver;
			}
			if (parameters.MetadataImporterProvider != null)
			{
				moduleDefinition.metadata_importer = parameters.MetadataImporterProvider.GetMetadataImporter(moduleDefinition);
			}
			if (parameters.ReflectionImporterProvider != null)
			{
				moduleDefinition.reflection_importer = parameters.ReflectionImporterProvider.GetReflectionImporter(moduleDefinition);
			}
			if (parameters.Kind != ModuleKind.NetModule)
			{
				AssemblyDefinition assemblyDefinition = moduleDefinition.assembly = new AssemblyDefinition();
				moduleDefinition.assembly.Name = CreateAssemblyName(name);
				assemblyDefinition.main_module = moduleDefinition;
			}
			moduleDefinition.Types.Add(new TypeDefinition(string.Empty, "<Module>", TypeAttributes.NotPublic));
			return moduleDefinition;
		}

		private static AssemblyNameDefinition CreateAssemblyName(string name)
		{
			if (name.EndsWith(".dll") || name.EndsWith(".exe"))
			{
				name = name.Substring(0, name.Length - 4);
			}
			return new AssemblyNameDefinition(name, Mixin.ZeroVersion);
		}

		public void ReadSymbols()
		{
			if (string.IsNullOrEmpty(file_name))
			{
				throw new InvalidOperationException();
			}
			DefaultSymbolReaderProvider defaultSymbolReaderProvider = new DefaultSymbolReaderProvider(true);
			ReadSymbols(defaultSymbolReaderProvider.GetSymbolReader(this, file_name), true);
		}

		public void ReadSymbols(ISymbolReader reader)
		{
			ReadSymbols(reader, true);
		}

		public void ReadSymbols(ISymbolReader reader, bool throwIfSymbolsAreNotMaching)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			symbol_reader = reader;
			if (!symbol_reader.ProcessDebugHeader(GetDebugHeader()))
			{
				symbol_reader = null;
				if (!throwIfSymbolsAreNotMaching)
				{
					return;
				}
				throw new SymbolsNotMatchingException("Symbols were found but are not matching the assembly");
			}
			if (HasImage && ReadingMode == ReadingMode.Immediate)
			{
				new ImmediateModuleReader(Image).ReadSymbols(this);
			}
		}

		public static ModuleDefinition ReadModule(string fileName)
		{
			return ReadModule(fileName, new ReaderParameters(ReadingMode.Deferred));
		}

		public static ModuleDefinition ReadModule(string fileName, ReaderParameters parameters)
		{
			Stream stream = GetFileStream(fileName, FileMode.Open, (!parameters.ReadWrite) ? FileAccess.Read : FileAccess.ReadWrite, FileShare.Read);
			if (parameters.InMemory)
			{
				MemoryStream memoryStream = new MemoryStream((int)(stream.CanSeek ? stream.Length : 0));
				using (stream)
				{
					stream.CopyTo(memoryStream);
				}
				memoryStream.Position = 0L;
				stream = memoryStream;
			}
			try
			{
				return ReadModule(Disposable.Owned(stream), fileName, parameters);
			}
			catch (Exception)
			{
				stream.Dispose();
				throw;
			}
		}

		private static Stream GetFileStream(string fileName, FileMode mode, FileAccess access, FileShare share)
		{
			Mixin.CheckFileName(fileName);
			return new FileStream(fileName, mode, access, share);
		}

		public static ModuleDefinition ReadModule(Stream stream)
		{
			return ReadModule(stream, new ReaderParameters(ReadingMode.Deferred));
		}

		public static ModuleDefinition ReadModule(Stream stream, ReaderParameters parameters)
		{
			Mixin.CheckStream(stream);
			Mixin.CheckReadSeek(stream);
			return ReadModule(Disposable.NotOwned(stream), stream.GetFileName(), parameters);
		}

		private static ModuleDefinition ReadModule(Disposable<Stream> stream, string fileName, ReaderParameters parameters)
		{
			Mixin.CheckParameters(parameters);
			return ModuleReader.CreateModule(ImageReader.ReadImage(stream, fileName), parameters);
		}

		public void Write(string fileName)
		{
			Write(fileName, new WriterParameters());
		}

		public void Write(string fileName, WriterParameters parameters)
		{
			Mixin.CheckParameters(parameters);
			Stream fileStream = GetFileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
			ModuleWriter.WriteModule(this, Disposable.Owned(fileStream), parameters);
		}

		public void Write()
		{
			Write(new WriterParameters());
		}

		public void Write(WriterParameters parameters)
		{
			if (!HasImage)
			{
				throw new InvalidOperationException();
			}
			Write(Image.Stream.value, parameters);
		}

		public void Write(Stream stream)
		{
			Write(stream, new WriterParameters());
		}

		public void Write(Stream stream, WriterParameters parameters)
		{
			Mixin.CheckStream(stream);
			Mixin.CheckWriteSeek(stream);
			Mixin.CheckParameters(parameters);
			ModuleWriter.WriteModule(this, Disposable.NotOwned(stream), parameters);
		}
	}
}
