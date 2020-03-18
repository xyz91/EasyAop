using Mono.Cecil.Cil;
using Mono.Cecil.Metadata;
using Mono.Cecil.PE;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Mono.Cecil
{
	internal sealed class MetadataBuilder
	{
		private sealed class GenericParameterComparer : IComparer<GenericParameter>
		{
			public int Compare(GenericParameter a, GenericParameter b)
			{
				uint num = MakeCodedRID(a.Owner, CodedIndex.TypeOrMethodDef);
				uint num2 = MakeCodedRID(b.Owner, CodedIndex.TypeOrMethodDef);
				if (num == num2)
				{
					int position = a.Position;
					int position2 = b.Position;
					if (position != position2)
					{
						if (position <= position2)
						{
							return -1;
						}
						return 1;
					}
					return 0;
				}
				if (num <= num2)
				{
					return -1;
				}
				return 1;
			}
		}

		internal readonly ModuleDefinition module;

		internal readonly ISymbolWriterProvider symbol_writer_provider;

		internal readonly ISymbolWriter symbol_writer;

		internal readonly TextMap text_map;

		internal readonly string fq_name;

		internal readonly uint timestamp;

		private readonly Dictionary<Row<uint, uint, uint>, MetadataToken> type_ref_map;

		private readonly Dictionary<uint, MetadataToken> type_spec_map;

		private readonly Dictionary<Row<uint, uint, uint>, MetadataToken> member_ref_map;

		private readonly Dictionary<Row<uint, uint>, MetadataToken> method_spec_map;

		private readonly Collection<GenericParameter> generic_parameters;

		internal readonly CodeWriter code;

		internal readonly DataBuffer data;

		internal readonly ResourceBuffer resources;

		internal readonly StringHeapBuffer string_heap;

		internal readonly GuidHeapBuffer guid_heap;

		internal readonly UserStringHeapBuffer user_string_heap;

		internal readonly BlobHeapBuffer blob_heap;

		internal readonly TableHeapBuffer table_heap;

		internal readonly PdbHeapBuffer pdb_heap;

		internal MetadataToken entry_point;

		internal uint type_rid = 1u;

		internal uint field_rid = 1u;

		internal uint method_rid = 1u;

		internal uint param_rid = 1u;

		internal uint property_rid = 1u;

		internal uint event_rid = 1u;

		internal uint local_variable_rid = 1u;

		internal uint local_constant_rid = 1u;

		private readonly TypeRefTable type_ref_table;

		private readonly TypeDefTable type_def_table;

		private readonly FieldTable field_table;

		private readonly MethodTable method_table;

		private readonly ParamTable param_table;

		private readonly InterfaceImplTable iface_impl_table;

		private readonly MemberRefTable member_ref_table;

		private readonly ConstantTable constant_table;

		private readonly CustomAttributeTable custom_attribute_table;

		private readonly DeclSecurityTable declsec_table;

		private readonly StandAloneSigTable standalone_sig_table;

		private readonly EventMapTable event_map_table;

		private readonly EventTable event_table;

		private readonly PropertyMapTable property_map_table;

		private readonly PropertyTable property_table;

		private readonly TypeSpecTable typespec_table;

		private readonly MethodSpecTable method_spec_table;

		private readonly bool portable_pdb;

		internal MetadataBuilder metadata_builder;

		private readonly DocumentTable document_table;

		private readonly MethodDebugInformationTable method_debug_information_table;

		private readonly LocalScopeTable local_scope_table;

		private readonly LocalVariableTable local_variable_table;

		private readonly LocalConstantTable local_constant_table;

		private readonly ImportScopeTable import_scope_table;

		private readonly StateMachineMethodTable state_machine_method_table;

		private readonly CustomDebugInformationTable custom_debug_information_table;

		private readonly Dictionary<Row<uint, uint>, MetadataToken> import_scope_map;

		private readonly Dictionary<string, MetadataToken> document_map;

		public MetadataBuilder(ModuleDefinition module, string fq_name, uint timestamp, ISymbolWriterProvider symbol_writer_provider, ISymbolWriter symbol_writer)
		{
			this.module = module;
			text_map = CreateTextMap();
			this.fq_name = fq_name;
			this.timestamp = timestamp;
			this.symbol_writer_provider = symbol_writer_provider;
			if (symbol_writer == null && module.HasImage && module.Image.HasDebugTables())
			{
				symbol_writer = new PortablePdbWriter(this, module);
			}
			this.symbol_writer = symbol_writer;
			IMetadataSymbolWriter metadataSymbolWriter = symbol_writer as IMetadataSymbolWriter;
			if (metadataSymbolWriter != null)
			{
				portable_pdb = true;
				metadataSymbolWriter.SetMetadata(this);
			}
			code = new CodeWriter(this);
			data = new DataBuffer();
			resources = new ResourceBuffer();
			string_heap = new StringHeapBuffer();
			guid_heap = new GuidHeapBuffer();
			user_string_heap = new UserStringHeapBuffer();
			blob_heap = new BlobHeapBuffer();
			table_heap = new TableHeapBuffer(module, this);
			type_ref_table = GetTable<TypeRefTable>(Table.TypeRef);
			type_def_table = GetTable<TypeDefTable>(Table.TypeDef);
			field_table = GetTable<FieldTable>(Table.Field);
			method_table = GetTable<MethodTable>(Table.Method);
			param_table = GetTable<ParamTable>(Table.Param);
			iface_impl_table = GetTable<InterfaceImplTable>(Table.InterfaceImpl);
			member_ref_table = GetTable<MemberRefTable>(Table.MemberRef);
			constant_table = GetTable<ConstantTable>(Table.Constant);
			custom_attribute_table = GetTable<CustomAttributeTable>(Table.CustomAttribute);
			declsec_table = GetTable<DeclSecurityTable>(Table.DeclSecurity);
			standalone_sig_table = GetTable<StandAloneSigTable>(Table.StandAloneSig);
			event_map_table = GetTable<EventMapTable>(Table.EventMap);
			event_table = GetTable<EventTable>(Table.Event);
			property_map_table = GetTable<PropertyMapTable>(Table.PropertyMap);
			property_table = GetTable<PropertyTable>(Table.Property);
			typespec_table = GetTable<TypeSpecTable>(Table.TypeSpec);
			method_spec_table = GetTable<MethodSpecTable>(Table.MethodSpec);
			RowEqualityComparer comparer = new RowEqualityComparer();
			type_ref_map = new Dictionary<Row<uint, uint, uint>, MetadataToken>(comparer);
			type_spec_map = new Dictionary<uint, MetadataToken>();
			member_ref_map = new Dictionary<Row<uint, uint, uint>, MetadataToken>(comparer);
			method_spec_map = new Dictionary<Row<uint, uint>, MetadataToken>(comparer);
			generic_parameters = new Collection<GenericParameter>();
			if (portable_pdb)
			{
				document_table = GetTable<DocumentTable>(Table.Document);
				method_debug_information_table = GetTable<MethodDebugInformationTable>(Table.MethodDebugInformation);
				local_scope_table = GetTable<LocalScopeTable>(Table.LocalScope);
				local_variable_table = GetTable<LocalVariableTable>(Table.LocalVariable);
				local_constant_table = GetTable<LocalConstantTable>(Table.LocalConstant);
				import_scope_table = GetTable<ImportScopeTable>(Table.ImportScope);
				state_machine_method_table = GetTable<StateMachineMethodTable>(Table.StateMachineMethod);
				custom_debug_information_table = GetTable<CustomDebugInformationTable>(Table.CustomDebugInformation);
				document_map = new Dictionary<string, MetadataToken>(StringComparer.Ordinal);
				import_scope_map = new Dictionary<Row<uint, uint>, MetadataToken>(comparer);
			}
		}

		public MetadataBuilder(ModuleDefinition module, PortablePdbWriterProvider writer_provider)
		{
			this.module = module;
			text_map = new TextMap();
			symbol_writer_provider = writer_provider;
			portable_pdb = true;
			string_heap = new StringHeapBuffer();
			guid_heap = new GuidHeapBuffer();
			user_string_heap = new UserStringHeapBuffer();
			blob_heap = new BlobHeapBuffer();
			table_heap = new TableHeapBuffer(module, this);
			pdb_heap = new PdbHeapBuffer();
			document_table = GetTable<DocumentTable>(Table.Document);
			method_debug_information_table = GetTable<MethodDebugInformationTable>(Table.MethodDebugInformation);
			local_scope_table = GetTable<LocalScopeTable>(Table.LocalScope);
			local_variable_table = GetTable<LocalVariableTable>(Table.LocalVariable);
			local_constant_table = GetTable<LocalConstantTable>(Table.LocalConstant);
			import_scope_table = GetTable<ImportScopeTable>(Table.ImportScope);
			state_machine_method_table = GetTable<StateMachineMethodTable>(Table.StateMachineMethod);
			custom_debug_information_table = GetTable<CustomDebugInformationTable>(Table.CustomDebugInformation);
			RowEqualityComparer comparer = new RowEqualityComparer();
			document_map = new Dictionary<string, MetadataToken>();
			import_scope_map = new Dictionary<Row<uint, uint>, MetadataToken>(comparer);
		}

		private TextMap CreateTextMap()
		{
			TextMap textMap = new TextMap();
			textMap.AddMap(TextSegment.ImportAddressTable, (module.Architecture == TargetArchitecture.I386) ? 8 : 0);
			textMap.AddMap(TextSegment.CLIHeader, 72, 8);
			return textMap;
		}

		private TTable GetTable<TTable>(Table table) where TTable : MetadataTable, new()
		{
			return table_heap.GetTable<TTable>(table);
		}

		private uint GetStringIndex(string @string)
		{
			if (string.IsNullOrEmpty(@string))
			{
				return 0u;
			}
			return string_heap.GetStringIndex(@string);
		}

		private uint GetGuidIndex(Guid guid)
		{
			return guid_heap.GetGuidIndex(guid);
		}

		private uint GetBlobIndex(ByteBuffer blob)
		{
			if (blob.length == 0)
			{
				return 0u;
			}
			return blob_heap.GetBlobIndex(blob);
		}

		private uint GetBlobIndex(byte[] blob)
		{
			if (blob.IsNullOrEmpty())
			{
				return 0u;
			}
			return GetBlobIndex(new ByteBuffer(blob));
		}

		public void BuildMetadata()
		{
			BuildModule();
			table_heap.string_offsets = string_heap.WriteStrings();
			table_heap.ComputeTableInformations();
			table_heap.WriteTableHeap();
		}

		private void BuildModule()
		{
			ModuleTable table = GetTable<ModuleTable>(Table.Module);
			table.row.Col1 = GetStringIndex(module.Name);
			table.row.Col2 = GetGuidIndex(module.Mvid);
			AssemblyDefinition assembly = module.Assembly;
			if (assembly != null)
			{
				BuildAssembly();
			}
			if (module.HasAssemblyReferences)
			{
				AddAssemblyReferences();
			}
			if (module.HasModuleReferences)
			{
				AddModuleReferences();
			}
			if (module.HasResources)
			{
				AddResources();
			}
			if (module.HasExportedTypes)
			{
				AddExportedTypes();
			}
			BuildTypes();
			if (assembly != null)
			{
				if (assembly.HasCustomAttributes)
				{
					AddCustomAttributes(assembly);
				}
				if (assembly.HasSecurityDeclarations)
				{
					AddSecurityDeclarations(assembly);
				}
			}
			if (module.HasCustomAttributes)
			{
				AddCustomAttributes(module);
			}
			if (module.EntryPoint != null)
			{
				entry_point = LookupToken(module.EntryPoint);
			}
			IMetadataSymbolWriter metadataSymbolWriter = symbol_writer as IMetadataSymbolWriter;
			metadataSymbolWriter?.WriteModule();
		}

		private void BuildAssembly()
		{
			AssemblyDefinition assembly = module.Assembly;
			AssemblyNameDefinition name = assembly.Name;
			GetTable<AssemblyTable>(Table.Assembly).row = new Row<AssemblyHashAlgorithm, ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint>(name.HashAlgorithm, (ushort)name.Version.Major, (ushort)name.Version.Minor, (ushort)name.Version.Build, (ushort)name.Version.Revision, name.Attributes, GetBlobIndex(name.PublicKey), GetStringIndex(name.Name), GetStringIndex(name.Culture));
			if (assembly.Modules.Count > 1)
			{
				BuildModules();
			}
		}

		private void BuildModules()
		{
			Collection<ModuleDefinition> modules = module.Assembly.Modules;
			FileTable table = GetTable<FileTable>(Table.File);
			for (int i = 0; i < modules.Count; i++)
			{
				ModuleDefinition moduleDefinition = modules[i];
				if (!moduleDefinition.IsMain)
				{
					WriterParameters parameters = new WriterParameters
					{
						SymbolWriterProvider = symbol_writer_provider
					};
					string moduleFileName = GetModuleFileName(moduleDefinition.Name);
					moduleDefinition.Write(moduleFileName, parameters);
					byte[] blob = CryptoService.ComputeHash(moduleFileName);
					table.AddRow(new Row<FileAttributes, uint, uint>(FileAttributes.ContainsMetaData, GetStringIndex(moduleDefinition.Name), GetBlobIndex(blob)));
				}
			}
		}

		private string GetModuleFileName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new NotSupportedException();
			}
			return Path.Combine(Path.GetDirectoryName(fq_name), name);
		}

		private void AddAssemblyReferences()
		{
			Collection<AssemblyNameReference> assemblyReferences = module.AssemblyReferences;
			AssemblyRefTable table = GetTable<AssemblyRefTable>(Table.AssemblyRef);
			if (module.IsWindowsMetadata())
			{
				module.Projections.RemoveVirtualReferences(assemblyReferences);
			}
			for (int i = 0; i < assemblyReferences.Count; i++)
			{
				AssemblyNameReference assemblyNameReference = assemblyReferences[i];
				byte[] blob = assemblyNameReference.PublicKey.IsNullOrEmpty() ? assemblyNameReference.PublicKeyToken : assemblyNameReference.PublicKey;
				Version version = assemblyNameReference.Version;
				int rid = table.AddRow(new Row<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint>((ushort)version.Major, (ushort)version.Minor, (ushort)version.Build, (ushort)version.Revision, assemblyNameReference.Attributes, GetBlobIndex(blob), GetStringIndex(assemblyNameReference.Name), GetStringIndex(assemblyNameReference.Culture), GetBlobIndex(assemblyNameReference.Hash)));
				assemblyNameReference.token = new MetadataToken(TokenType.AssemblyRef, rid);
			}
			if (module.IsWindowsMetadata())
			{
				module.Projections.AddVirtualReferences(assemblyReferences);
			}
		}

		private void AddModuleReferences()
		{
			Collection<ModuleReference> moduleReferences = module.ModuleReferences;
			ModuleRefTable table = GetTable<ModuleRefTable>(Table.ModuleRef);
			for (int i = 0; i < moduleReferences.Count; i++)
			{
				ModuleReference moduleReference = moduleReferences[i];
				moduleReference.token = new MetadataToken(TokenType.ModuleRef, table.AddRow(GetStringIndex(moduleReference.Name)));
			}
		}

		private void AddResources()
		{
			Collection<Resource> collection = module.Resources;
			ManifestResourceTable table = GetTable<ManifestResourceTable>(Table.ManifestResource);
			for (int i = 0; i < collection.Count; i++)
			{
				Resource resource = collection[i];
				Row<uint, ManifestResourceAttributes, uint, uint> row = new Row<uint, ManifestResourceAttributes, uint, uint>(0u, resource.Attributes, GetStringIndex(resource.Name), 0u);
				switch (resource.ResourceType)
				{
				case ResourceType.Embedded:
					row.Col1 = AddEmbeddedResource((EmbeddedResource)resource);
					break;
				case ResourceType.Linked:
					row.Col4 = CodedIndex.Implementation.CompressMetadataToken(new MetadataToken(TokenType.File, AddLinkedResource((LinkedResource)resource)));
					break;
				case ResourceType.AssemblyLinked:
					row.Col4 = CodedIndex.Implementation.CompressMetadataToken(((AssemblyLinkedResource)resource).Assembly.MetadataToken);
					break;
				default:
					throw new NotSupportedException();
				}
				table.AddRow(row);
			}
		}

		private uint AddLinkedResource(LinkedResource resource)
		{
			FileTable table = GetTable<FileTable>(Table.File);
			byte[] array = resource.Hash;
			if (array.IsNullOrEmpty())
			{
				array = CryptoService.ComputeHash(resource.File);
			}
			return (uint)table.AddRow(new Row<FileAttributes, uint, uint>(FileAttributes.ContainsNoMetaData, GetStringIndex(resource.File), GetBlobIndex(array)));
		}

		private uint AddEmbeddedResource(EmbeddedResource resource)
		{
			return resources.AddResource(resource.GetResourceData());
		}

		private void AddExportedTypes()
		{
			Collection<ExportedType> exportedTypes = module.ExportedTypes;
			ExportedTypeTable table = GetTable<ExportedTypeTable>(Table.ExportedType);
			for (int i = 0; i < exportedTypes.Count; i++)
			{
				ExportedType exportedType = exportedTypes[i];
				int rid = table.AddRow(new Row<TypeAttributes, uint, uint, uint, uint>(exportedType.Attributes, (uint)exportedType.Identifier, GetStringIndex(exportedType.Name), GetStringIndex(exportedType.Namespace), MakeCodedRID(GetExportedTypeScope(exportedType), CodedIndex.Implementation)));
				exportedType.token = new MetadataToken(TokenType.ExportedType, rid);
			}
		}

		private MetadataToken GetExportedTypeScope(ExportedType exported_type)
		{
			if (exported_type.DeclaringType != null)
			{
				return exported_type.DeclaringType.MetadataToken;
			}
			IMetadataScope scope = exported_type.Scope;
			switch (scope.MetadataToken.TokenType)
			{
			case TokenType.AssemblyRef:
				return scope.MetadataToken;
			case TokenType.ModuleRef:
			{
				FileTable table = GetTable<FileTable>(Table.File);
				for (int i = 0; i < table.length; i++)
				{
					if (table.rows[i].Col2 == GetStringIndex(scope.Name))
					{
						return new MetadataToken(TokenType.File, i + 1);
					}
				}
				break;
			}
			}
			throw new NotSupportedException();
		}

		private void BuildTypes()
		{
			if (module.HasTypes)
			{
				AttachTokens();
				AddTypes();
				AddGenericParameters();
			}
		}

		private void AttachTokens()
		{
			Collection<TypeDefinition> types = module.Types;
			for (int i = 0; i < types.Count; i++)
			{
				AttachTypeToken(types[i]);
			}
		}

		private void AttachTypeToken(TypeDefinition type)
		{
			type.token = new MetadataToken(TokenType.TypeDef, type_rid++);
			type.fields_range.Start = field_rid;
			type.methods_range.Start = method_rid;
			if (type.HasFields)
			{
				AttachFieldsToken(type);
			}
			if (type.HasMethods)
			{
				AttachMethodsToken(type);
			}
			if (type.HasNestedTypes)
			{
				AttachNestedTypesToken(type);
			}
		}

		private void AttachNestedTypesToken(TypeDefinition type)
		{
			Collection<TypeDefinition> nestedTypes = type.NestedTypes;
			for (int i = 0; i < nestedTypes.Count; i++)
			{
				AttachTypeToken(nestedTypes[i]);
			}
		}

		private void AttachFieldsToken(TypeDefinition type)
		{
			Collection<FieldDefinition> fields = type.Fields;
			type.fields_range.Length = (uint)fields.Count;
			for (int i = 0; i < fields.Count; i++)
			{
				fields[i].token = new MetadataToken(TokenType.Field, field_rid++);
			}
		}

		private void AttachMethodsToken(TypeDefinition type)
		{
			Collection<MethodDefinition> methods = type.Methods;
			type.methods_range.Length = (uint)methods.Count;
			for (int i = 0; i < methods.Count; i++)
			{
				methods[i].token = new MetadataToken(TokenType.Method, method_rid++);
			}
		}

		private MetadataToken GetTypeToken(TypeReference type)
		{
			if (type == null)
			{
				return MetadataToken.Zero;
			}
			if (type.IsDefinition)
			{
				return type.token;
			}
			if (type.IsTypeSpecification())
			{
				return GetTypeSpecToken(type);
			}
			return GetTypeRefToken(type);
		}

		private MetadataToken GetTypeSpecToken(TypeReference type)
		{
			uint blobIndex = GetBlobIndex(GetTypeSpecSignature(type));
			if (type_spec_map.TryGetValue(blobIndex, out MetadataToken result))
			{
				return result;
			}
			return AddTypeSpecification(type, blobIndex);
		}

		private MetadataToken AddTypeSpecification(TypeReference type, uint row)
		{
			type.token = new MetadataToken(TokenType.TypeSpec, typespec_table.AddRow(row));
			MetadataToken token = type.token;
			type_spec_map.Add(row, token);
			return token;
		}

		private MetadataToken GetTypeRefToken(TypeReference type)
		{
			TypeReferenceProjection projection = WindowsRuntimeProjections.RemoveProjection(type);
			Row<uint, uint, uint> row = CreateTypeRefRow(type);
			if (!type_ref_map.TryGetValue(row, out MetadataToken result))
			{
				result = AddTypeReference(type, row);
			}
			WindowsRuntimeProjections.ApplyProjection(type, projection);
			return result;
		}

		private Row<uint, uint, uint> CreateTypeRefRow(TypeReference type)
		{
			return new Row<uint, uint, uint>(MakeCodedRID(GetScopeToken(type), CodedIndex.ResolutionScope), GetStringIndex(type.Name), GetStringIndex(type.Namespace));
		}

		private MetadataToken GetScopeToken(TypeReference type)
		{
			if (type.IsNested)
			{
				return GetTypeRefToken(type.DeclaringType);
			}
			IMetadataScope scope = type.Scope;
			return scope?.MetadataToken ?? MetadataToken.Zero;
		}

		private static uint MakeCodedRID(IMetadataTokenProvider provider, CodedIndex index)
		{
			return MakeCodedRID(provider.MetadataToken, index);
		}

		private static uint MakeCodedRID(MetadataToken token, CodedIndex index)
		{
			return index.CompressMetadataToken(token);
		}

		private MetadataToken AddTypeReference(TypeReference type, Row<uint, uint, uint> row)
		{
			type.token = new MetadataToken(TokenType.TypeRef, type_ref_table.AddRow(row));
			MetadataToken token = type.token;
			type_ref_map.Add(row, token);
			return token;
		}

		private void AddTypes()
		{
			Collection<TypeDefinition> types = module.Types;
			for (int i = 0; i < types.Count; i++)
			{
				AddType(types[i]);
			}
		}

		private void AddType(TypeDefinition type)
		{
			TypeDefinitionProjection projection = WindowsRuntimeProjections.RemoveProjection(type);
			type_def_table.AddRow(new Row<TypeAttributes, uint, uint, uint, uint, uint>(type.Attributes, GetStringIndex(type.Name), GetStringIndex(type.Namespace), MakeCodedRID(GetTypeToken(type.BaseType), CodedIndex.TypeDefOrRef), type.fields_range.Start, type.methods_range.Start));
			if (type.HasGenericParameters)
			{
				AddGenericParameters(type);
			}
			if (type.HasInterfaces)
			{
				AddInterfaces(type);
			}
			if (type.HasLayoutInfo)
			{
				AddLayoutInfo(type);
			}
			if (type.HasFields)
			{
				AddFields(type);
			}
			if (type.HasMethods)
			{
				AddMethods(type);
			}
			if (type.HasProperties)
			{
				AddProperties(type);
			}
			if (type.HasEvents)
			{
				AddEvents(type);
			}
			if (type.HasCustomAttributes)
			{
				AddCustomAttributes(type);
			}
			if (type.HasSecurityDeclarations)
			{
				AddSecurityDeclarations(type);
			}
			if (type.HasNestedTypes)
			{
				AddNestedTypes(type);
			}
			WindowsRuntimeProjections.ApplyProjection(type, projection);
		}

		private void AddGenericParameters(IGenericParameterProvider owner)
		{
			Collection<GenericParameter> genericParameters = owner.GenericParameters;
			for (int i = 0; i < genericParameters.Count; i++)
			{
				generic_parameters.Add(genericParameters[i]);
			}
		}

		private void AddGenericParameters()
		{
			GenericParameter[] items = generic_parameters.items;
			int size = generic_parameters.size;
			Array.Sort(items, 0, size, new GenericParameterComparer());
			GenericParamTable table = GetTable<GenericParamTable>(Table.GenericParam);
			GenericParamConstraintTable table2 = GetTable<GenericParamConstraintTable>(Table.GenericParamConstraint);
			for (int i = 0; i < size; i++)
			{
				GenericParameter genericParameter = items[i];
				int rid = table.AddRow(new Row<ushort, GenericParameterAttributes, uint, uint>((ushort)genericParameter.Position, genericParameter.Attributes, MakeCodedRID(genericParameter.Owner, CodedIndex.TypeOrMethodDef), GetStringIndex(genericParameter.Name)));
				genericParameter.token = new MetadataToken(TokenType.GenericParam, rid);
				if (genericParameter.HasConstraints)
				{
					AddConstraints(genericParameter, table2);
				}
				if (genericParameter.HasCustomAttributes)
				{
					AddCustomAttributes(genericParameter);
				}
			}
		}

		private void AddConstraints(GenericParameter generic_parameter, GenericParamConstraintTable table)
		{
			Collection<TypeReference> constraints = generic_parameter.Constraints;
			uint rID = generic_parameter.token.RID;
			for (int i = 0; i < constraints.Count; i++)
			{
				table.AddRow(new Row<uint, uint>(rID, MakeCodedRID(GetTypeToken(constraints[i]), CodedIndex.TypeDefOrRef)));
			}
		}

		private void AddInterfaces(TypeDefinition type)
		{
			Collection<InterfaceImplementation> interfaces = type.Interfaces;
			uint rID = type.token.RID;
			for (int i = 0; i < interfaces.Count; i++)
			{
				InterfaceImplementation interfaceImplementation = interfaces[i];
				int rid = iface_impl_table.AddRow(new Row<uint, uint>(rID, MakeCodedRID(GetTypeToken(interfaceImplementation.InterfaceType), CodedIndex.TypeDefOrRef)));
				interfaceImplementation.token = new MetadataToken(TokenType.InterfaceImpl, rid);
				if (interfaceImplementation.HasCustomAttributes)
				{
					AddCustomAttributes(interfaceImplementation);
				}
			}
		}

		private void AddLayoutInfo(TypeDefinition type)
		{
			GetTable<ClassLayoutTable>(Table.ClassLayout).AddRow(new Row<ushort, uint, uint>((ushort)type.PackingSize, (uint)type.ClassSize, type.token.RID));
		}

		private void AddNestedTypes(TypeDefinition type)
		{
			Collection<TypeDefinition> nestedTypes = type.NestedTypes;
			NestedClassTable table = GetTable<NestedClassTable>(Table.NestedClass);
			for (int i = 0; i < nestedTypes.Count; i++)
			{
				TypeDefinition typeDefinition = nestedTypes[i];
				AddType(typeDefinition);
				table.AddRow(new Row<uint, uint>(typeDefinition.token.RID, type.token.RID));
			}
		}

		private void AddFields(TypeDefinition type)
		{
			Collection<FieldDefinition> fields = type.Fields;
			for (int i = 0; i < fields.Count; i++)
			{
				AddField(fields[i]);
			}
		}

		private void AddField(FieldDefinition field)
		{
			FieldDefinitionProjection projection = WindowsRuntimeProjections.RemoveProjection(field);
			field_table.AddRow(new Row<FieldAttributes, uint, uint>(field.Attributes, GetStringIndex(field.Name), GetBlobIndex(GetFieldSignature(field))));
			if (!field.InitialValue.IsNullOrEmpty())
			{
				AddFieldRVA(field);
			}
			if (field.HasLayoutInfo)
			{
				AddFieldLayout(field);
			}
			if (field.HasCustomAttributes)
			{
				AddCustomAttributes(field);
			}
			if (field.HasConstant)
			{
				AddConstant(field, field.FieldType);
			}
			if (field.HasMarshalInfo)
			{
				AddMarshalInfo(field);
			}
			WindowsRuntimeProjections.ApplyProjection(field, projection);
		}

		private void AddFieldRVA(FieldDefinition field)
		{
			GetTable<FieldRVATable>(Table.FieldRVA).AddRow(new Row<uint, uint>(data.AddData(field.InitialValue), field.token.RID));
		}

		private void AddFieldLayout(FieldDefinition field)
		{
			GetTable<FieldLayoutTable>(Table.FieldLayout).AddRow(new Row<uint, uint>((uint)field.Offset, field.token.RID));
		}

		private void AddMethods(TypeDefinition type)
		{
			Collection<MethodDefinition> methods = type.Methods;
			for (int i = 0; i < methods.Count; i++)
			{
				AddMethod(methods[i]);
			}
		}

		private void AddMethod(MethodDefinition method)
		{
			MethodDefinitionProjection projection = WindowsRuntimeProjections.RemoveProjection(method);
			method_table.AddRow(new Row<uint, MethodImplAttributes, MethodAttributes, uint, uint, uint>(method.HasBody ? code.WriteMethodBody(method) : 0, method.ImplAttributes, method.Attributes, GetStringIndex(method.Name), GetBlobIndex(GetMethodSignature(method)), param_rid));
			AddParameters(method);
			if (method.HasGenericParameters)
			{
				AddGenericParameters(method);
			}
			if (method.IsPInvokeImpl)
			{
				AddPInvokeInfo(method);
			}
			if (method.HasCustomAttributes)
			{
				AddCustomAttributes(method);
			}
			if (method.HasSecurityDeclarations)
			{
				AddSecurityDeclarations(method);
			}
			if (method.HasOverrides)
			{
				AddOverrides(method);
			}
			WindowsRuntimeProjections.ApplyProjection(method, projection);
		}

		private void AddParameters(MethodDefinition method)
		{
			ParameterDefinition parameter = method.MethodReturnType.parameter;
			if (parameter != null && RequiresParameterRow(parameter))
			{
				AddParameter(0, parameter, param_table);
			}
			if (method.HasParameters)
			{
				Collection<ParameterDefinition> parameters = method.Parameters;
				for (int i = 0; i < parameters.Count; i++)
				{
					ParameterDefinition parameter2 = parameters[i];
					if (RequiresParameterRow(parameter2))
					{
						AddParameter((ushort)(i + 1), parameter2, param_table);
					}
				}
			}
		}

		private void AddPInvokeInfo(MethodDefinition method)
		{
			PInvokeInfo pInvokeInfo = method.PInvokeInfo;
			if (pInvokeInfo != null)
			{
				GetTable<ImplMapTable>(Table.ImplMap).AddRow(new Row<PInvokeAttributes, uint, uint, uint>(pInvokeInfo.Attributes, MakeCodedRID(method, CodedIndex.MemberForwarded), GetStringIndex(pInvokeInfo.EntryPoint), pInvokeInfo.Module.MetadataToken.RID));
			}
		}

		private void AddOverrides(MethodDefinition method)
		{
			Collection<MethodReference> overrides = method.Overrides;
			MethodImplTable table = GetTable<MethodImplTable>(Table.MethodImpl);
			for (int i = 0; i < overrides.Count; i++)
			{
				table.AddRow(new Row<uint, uint, uint>(method.DeclaringType.token.RID, MakeCodedRID(method, CodedIndex.MethodDefOrRef), MakeCodedRID(LookupToken(overrides[i]), CodedIndex.MethodDefOrRef)));
			}
		}

		private static bool RequiresParameterRow(ParameterDefinition parameter)
		{
			if (string.IsNullOrEmpty(parameter.Name) && parameter.Attributes == ParameterAttributes.None && !parameter.HasMarshalInfo && !parameter.HasConstant)
			{
				return parameter.HasCustomAttributes;
			}
			return true;
		}

		private void AddParameter(ushort sequence, ParameterDefinition parameter, ParamTable table)
		{
			table.AddRow(new Row<ParameterAttributes, ushort, uint>(parameter.Attributes, sequence, GetStringIndex(parameter.Name)));
			parameter.token = new MetadataToken(TokenType.Param, param_rid++);
			if (parameter.HasCustomAttributes)
			{
				AddCustomAttributes(parameter);
			}
			if (parameter.HasConstant)
			{
				AddConstant(parameter, parameter.ParameterType);
			}
			if (parameter.HasMarshalInfo)
			{
				AddMarshalInfo(parameter);
			}
		}

		private void AddMarshalInfo(IMarshalInfoProvider owner)
		{
			GetTable<FieldMarshalTable>(Table.FieldMarshal).AddRow(new Row<uint, uint>(MakeCodedRID(owner, CodedIndex.HasFieldMarshal), GetBlobIndex(GetMarshalInfoSignature(owner))));
		}

		private void AddProperties(TypeDefinition type)
		{
			Collection<PropertyDefinition> properties = type.Properties;
			property_map_table.AddRow(new Row<uint, uint>(type.token.RID, property_rid));
			for (int i = 0; i < properties.Count; i++)
			{
				AddProperty(properties[i]);
			}
		}

		private void AddProperty(PropertyDefinition property)
		{
			property_table.AddRow(new Row<PropertyAttributes, uint, uint>(property.Attributes, GetStringIndex(property.Name), GetBlobIndex(GetPropertySignature(property))));
			property.token = new MetadataToken(TokenType.Property, property_rid++);
			MethodDefinition getMethod = property.GetMethod;
			if (getMethod != null)
			{
				AddSemantic(MethodSemanticsAttributes.Getter, property, getMethod);
			}
			getMethod = property.SetMethod;
			if (getMethod != null)
			{
				AddSemantic(MethodSemanticsAttributes.Setter, property, getMethod);
			}
			if (property.HasOtherMethods)
			{
				AddOtherSemantic(property, property.OtherMethods);
			}
			if (property.HasCustomAttributes)
			{
				AddCustomAttributes(property);
			}
			if (property.HasConstant)
			{
				AddConstant(property, property.PropertyType);
			}
		}

		private void AddOtherSemantic(IMetadataTokenProvider owner, Collection<MethodDefinition> others)
		{
			for (int i = 0; i < others.Count; i++)
			{
				AddSemantic(MethodSemanticsAttributes.Other, owner, others[i]);
			}
		}

		private void AddEvents(TypeDefinition type)
		{
			Collection<EventDefinition> events = type.Events;
			event_map_table.AddRow(new Row<uint, uint>(type.token.RID, event_rid));
			for (int i = 0; i < events.Count; i++)
			{
				AddEvent(events[i]);
			}
		}

		private void AddEvent(EventDefinition @event)
		{
			event_table.AddRow(new Row<EventAttributes, uint, uint>(@event.Attributes, GetStringIndex(@event.Name), MakeCodedRID(GetTypeToken(@event.EventType), CodedIndex.TypeDefOrRef)));
			@event.token = new MetadataToken(TokenType.Event, event_rid++);
			MethodDefinition addMethod = @event.AddMethod;
			if (addMethod != null)
			{
				AddSemantic(MethodSemanticsAttributes.AddOn, @event, addMethod);
			}
			addMethod = @event.InvokeMethod;
			if (addMethod != null)
			{
				AddSemantic(MethodSemanticsAttributes.Fire, @event, addMethod);
			}
			addMethod = @event.RemoveMethod;
			if (addMethod != null)
			{
				AddSemantic(MethodSemanticsAttributes.RemoveOn, @event, addMethod);
			}
			if (@event.HasOtherMethods)
			{
				AddOtherSemantic(@event, @event.OtherMethods);
			}
			if (@event.HasCustomAttributes)
			{
				AddCustomAttributes(@event);
			}
		}

		private void AddSemantic(MethodSemanticsAttributes semantics, IMetadataTokenProvider provider, MethodDefinition method)
		{
			method.SemanticsAttributes = semantics;
			GetTable<MethodSemanticsTable>(Table.MethodSemantics).AddRow(new Row<MethodSemanticsAttributes, uint, uint>(semantics, method.token.RID, MakeCodedRID(provider, CodedIndex.HasSemantics)));
		}

		private void AddConstant(IConstantProvider owner, TypeReference type)
		{
			object constant = owner.Constant;
			ElementType constantType = GetConstantType(type, constant);
			constant_table.AddRow(new Row<ElementType, uint, uint>(constantType, MakeCodedRID(owner.MetadataToken, CodedIndex.HasConstant), GetBlobIndex(GetConstantSignature(constantType, constant))));
		}

		private static ElementType GetConstantType(TypeReference constant_type, object constant)
		{
			if (constant == null)
			{
				return ElementType.Class;
			}
			ElementType etype = constant_type.etype;
			switch (etype)
			{
			case ElementType.None:
			{
				TypeDefinition typeDefinition = constant_type.CheckedResolve();
				if (typeDefinition.IsEnum)
				{
					return GetConstantType(typeDefinition.GetEnumUnderlyingType(), constant);
				}
				return ElementType.Class;
			}
			case ElementType.String:
				return ElementType.String;
			case ElementType.Object:
				return GetConstantType(constant.GetType());
			case ElementType.Var:
			case ElementType.Array:
			case ElementType.SzArray:
			case ElementType.MVar:
				return ElementType.Class;
			case ElementType.GenericInst:
			{
				GenericInstanceType genericInstanceType = (GenericInstanceType)constant_type;
				if (genericInstanceType.ElementType.IsTypeOf("System", "Nullable`1"))
				{
					return GetConstantType(genericInstanceType.GenericArguments[0], constant);
				}
				return GetConstantType(((TypeSpecification)constant_type).ElementType, constant);
			}
			case ElementType.ByRef:
			case ElementType.CModReqD:
			case ElementType.CModOpt:
			case ElementType.Sentinel:
				return GetConstantType(((TypeSpecification)constant_type).ElementType, constant);
			case ElementType.Boolean:
			case ElementType.Char:
			case ElementType.I1:
			case ElementType.U1:
			case ElementType.I2:
			case ElementType.U2:
			case ElementType.I4:
			case ElementType.U4:
			case ElementType.I8:
			case ElementType.U8:
			case ElementType.R4:
			case ElementType.R8:
			case ElementType.I:
			case ElementType.U:
				return GetConstantType(constant.GetType());
			default:
				return etype;
			}
		}

		private static ElementType GetConstantType(Type type)
		{
			switch (type.GetTypeCode())
			{
			case TypeCode.Boolean:
				return ElementType.Boolean;
			case TypeCode.Byte:
				return ElementType.U1;
			case TypeCode.SByte:
				return ElementType.I1;
			case TypeCode.Char:
				return ElementType.Char;
			case TypeCode.Int16:
				return ElementType.I2;
			case TypeCode.UInt16:
				return ElementType.U2;
			case TypeCode.Int32:
				return ElementType.I4;
			case TypeCode.UInt32:
				return ElementType.U4;
			case TypeCode.Int64:
				return ElementType.I8;
			case TypeCode.UInt64:
				return ElementType.U8;
			case TypeCode.Single:
				return ElementType.R4;
			case TypeCode.Double:
				return ElementType.R8;
			case TypeCode.String:
				return ElementType.String;
			default:
				throw new NotSupportedException(type.FullName);
			}
		}

		private void AddCustomAttributes(ICustomAttributeProvider owner)
		{
			Collection<CustomAttribute> customAttributes = owner.CustomAttributes;
			for (int i = 0; i < customAttributes.Count; i++)
			{
				CustomAttribute customAttribute = customAttributes[i];
				CustomAttributeValueProjection projection = WindowsRuntimeProjections.RemoveProjection(customAttribute);
				custom_attribute_table.AddRow(new Row<uint, uint, uint>(MakeCodedRID(owner, CodedIndex.HasCustomAttribute), MakeCodedRID(LookupToken(customAttribute.Constructor), CodedIndex.CustomAttributeType), GetBlobIndex(GetCustomAttributeSignature(customAttribute))));
				WindowsRuntimeProjections.ApplyProjection(customAttribute, projection);
			}
		}

		private void AddSecurityDeclarations(ISecurityDeclarationProvider owner)
		{
			Collection<SecurityDeclaration> securityDeclarations = owner.SecurityDeclarations;
			for (int i = 0; i < securityDeclarations.Count; i++)
			{
				SecurityDeclaration securityDeclaration = securityDeclarations[i];
				declsec_table.AddRow(new Row<SecurityAction, uint, uint>(securityDeclaration.Action, MakeCodedRID(owner, CodedIndex.HasDeclSecurity), GetBlobIndex(GetSecurityDeclarationSignature(securityDeclaration))));
			}
		}

		private MetadataToken GetMemberRefToken(MemberReference member)
		{
			MemberReferenceProjection projection = WindowsRuntimeProjections.RemoveProjection(member);
			Row<uint, uint, uint> row = CreateMemberRefRow(member);
			if (!member_ref_map.TryGetValue(row, out MetadataToken result))
			{
				result = AddMemberReference(member, row);
			}
			WindowsRuntimeProjections.ApplyProjection(member, projection);
			return result;
		}

		private Row<uint, uint, uint> CreateMemberRefRow(MemberReference member)
		{
			return new Row<uint, uint, uint>(MakeCodedRID(GetTypeToken(member.DeclaringType), CodedIndex.MemberRefParent), GetStringIndex(member.Name), GetBlobIndex(GetMemberRefSignature(member)));
		}

		private MetadataToken AddMemberReference(MemberReference member, Row<uint, uint, uint> row)
		{
			member.token = new MetadataToken(TokenType.MemberRef, member_ref_table.AddRow(row));
			MetadataToken token = member.token;
			member_ref_map.Add(row, token);
			return token;
		}

		private MetadataToken GetMethodSpecToken(MethodSpecification method_spec)
		{
			Row<uint, uint> row = CreateMethodSpecRow(method_spec);
			if (method_spec_map.TryGetValue(row, out MetadataToken result))
			{
				return result;
			}
			AddMethodSpecification(method_spec, row);
			return method_spec.token;
		}

		private void AddMethodSpecification(MethodSpecification method_spec, Row<uint, uint> row)
		{
			method_spec.token = new MetadataToken(TokenType.MethodSpec, method_spec_table.AddRow(row));
			method_spec_map.Add(row, method_spec.token);
		}

		private Row<uint, uint> CreateMethodSpecRow(MethodSpecification method_spec)
		{
			return new Row<uint, uint>(MakeCodedRID(LookupToken(method_spec.ElementMethod), CodedIndex.MethodDefOrRef), GetBlobIndex(GetMethodSpecSignature(method_spec)));
		}

		private SignatureWriter CreateSignatureWriter()
		{
			return new SignatureWriter(this);
		}

		private SignatureWriter GetMethodSpecSignature(MethodSpecification method_spec)
		{
			if (!method_spec.IsGenericInstance)
			{
				throw new NotSupportedException();
			}
			GenericInstanceMethod instance = (GenericInstanceMethod)method_spec;
			SignatureWriter signatureWriter = CreateSignatureWriter();
			signatureWriter.WriteByte(10);
			signatureWriter.WriteGenericInstanceSignature(instance);
			return signatureWriter;
		}

		public uint AddStandAloneSignature(uint signature)
		{
			return (uint)standalone_sig_table.AddRow(signature);
		}

		public uint GetLocalVariableBlobIndex(Collection<VariableDefinition> variables)
		{
			return GetBlobIndex(GetVariablesSignature(variables));
		}

		public uint GetCallSiteBlobIndex(CallSite call_site)
		{
			return GetBlobIndex(GetMethodSignature(call_site));
		}

		public uint GetConstantTypeBlobIndex(TypeReference constant_type)
		{
			return GetBlobIndex(GetConstantTypeSignature(constant_type));
		}

		private SignatureWriter GetVariablesSignature(Collection<VariableDefinition> variables)
		{
			SignatureWriter signatureWriter = CreateSignatureWriter();
			signatureWriter.WriteByte(7);
			signatureWriter.WriteCompressedUInt32((uint)variables.Count);
			for (int i = 0; i < variables.Count; i++)
			{
				signatureWriter.WriteTypeSignature(variables[i].VariableType);
			}
			return signatureWriter;
		}

		private SignatureWriter GetConstantTypeSignature(TypeReference constant_type)
		{
			SignatureWriter signatureWriter = CreateSignatureWriter();
			signatureWriter.WriteByte(6);
			signatureWriter.WriteTypeSignature(constant_type);
			return signatureWriter;
		}

		private SignatureWriter GetFieldSignature(FieldReference field)
		{
			SignatureWriter signatureWriter = CreateSignatureWriter();
			signatureWriter.WriteByte(6);
			signatureWriter.WriteTypeSignature(field.FieldType);
			return signatureWriter;
		}

		private SignatureWriter GetMethodSignature(IMethodSignature method)
		{
			SignatureWriter signatureWriter = CreateSignatureWriter();
			signatureWriter.WriteMethodSignature(method);
			return signatureWriter;
		}

		private SignatureWriter GetMemberRefSignature(MemberReference member)
		{
			FieldReference fieldReference = member as FieldReference;
			if (fieldReference != null)
			{
				return GetFieldSignature(fieldReference);
			}
			MethodReference methodReference = member as MethodReference;
			if (methodReference != null)
			{
				return GetMethodSignature(methodReference);
			}
			throw new NotSupportedException();
		}

		private SignatureWriter GetPropertySignature(PropertyDefinition property)
		{
			SignatureWriter signatureWriter = CreateSignatureWriter();
			byte b = 8;
			if (property.HasThis)
			{
				b = (byte)(b | 0x20);
			}
			uint num = 0u;
			Collection<ParameterDefinition> collection = null;
			if (property.HasParameters)
			{
				collection = property.Parameters;
				num = (uint)collection.Count;
			}
			signatureWriter.WriteByte(b);
			signatureWriter.WriteCompressedUInt32(num);
			signatureWriter.WriteTypeSignature(property.PropertyType);
			if (num == 0)
			{
				return signatureWriter;
			}
			for (int i = 0; i < num; i++)
			{
				signatureWriter.WriteTypeSignature(collection[i].ParameterType);
			}
			return signatureWriter;
		}

		private SignatureWriter GetTypeSpecSignature(TypeReference type)
		{
			SignatureWriter signatureWriter = CreateSignatureWriter();
			signatureWriter.WriteTypeSignature(type);
			return signatureWriter;
		}

		private SignatureWriter GetConstantSignature(ElementType type, object value)
		{
			SignatureWriter signatureWriter = CreateSignatureWriter();
			switch (type)
			{
			case ElementType.None:
			case ElementType.Class:
			case ElementType.Var:
			case ElementType.Array:
			case ElementType.Object:
			case ElementType.SzArray:
			case ElementType.MVar:
				signatureWriter.WriteInt32(0);
				break;
			case ElementType.String:
				signatureWriter.WriteConstantString((string)value);
				break;
			default:
				signatureWriter.WriteConstantPrimitive(value);
				break;
			}
			return signatureWriter;
		}

		private SignatureWriter GetCustomAttributeSignature(CustomAttribute attribute)
		{
			SignatureWriter signatureWriter = CreateSignatureWriter();
			if (!attribute.resolved)
			{
				signatureWriter.WriteBytes(attribute.GetBlob());
				return signatureWriter;
			}
			signatureWriter.WriteUInt16(1);
			signatureWriter.WriteCustomAttributeConstructorArguments(attribute);
			signatureWriter.WriteCustomAttributeNamedArguments(attribute);
			return signatureWriter;
		}

		private SignatureWriter GetSecurityDeclarationSignature(SecurityDeclaration declaration)
		{
			SignatureWriter signatureWriter = CreateSignatureWriter();
			if (!declaration.resolved)
			{
				signatureWriter.WriteBytes(declaration.GetBlob());
			}
			else if (module.Runtime < TargetRuntime.Net_2_0)
			{
				signatureWriter.WriteXmlSecurityDeclaration(declaration);
			}
			else
			{
				signatureWriter.WriteSecurityDeclaration(declaration);
			}
			return signatureWriter;
		}

		private SignatureWriter GetMarshalInfoSignature(IMarshalInfoProvider owner)
		{
			SignatureWriter signatureWriter = CreateSignatureWriter();
			signatureWriter.WriteMarshalInfo(owner.MarshalInfo);
			return signatureWriter;
		}

		private static Exception CreateForeignMemberException(MemberReference member)
		{
			return new ArgumentException($"Member '{member}' is declared in another module and needs to be imported");
		}

		public MetadataToken LookupToken(IMetadataTokenProvider provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException();
			}
			if (metadata_builder != null)
			{
				return metadata_builder.LookupToken(provider);
			}
			MemberReference memberReference = provider as MemberReference;
			if (memberReference != null && memberReference.Module == module)
			{
				MetadataToken metadataToken = provider.MetadataToken;
				switch (metadataToken.TokenType)
				{
				case TokenType.TypeDef:
				case TokenType.Field:
				case TokenType.Method:
				case TokenType.Event:
				case TokenType.Property:
					return metadataToken;
				case TokenType.TypeRef:
				case TokenType.TypeSpec:
				case TokenType.GenericParam:
					return GetTypeToken((TypeReference)provider);
				case TokenType.MethodSpec:
					return GetMethodSpecToken((MethodSpecification)provider);
				case TokenType.MemberRef:
					return GetMemberRefToken(memberReference);
				default:
					throw new NotSupportedException();
				}
			}
			throw CreateForeignMemberException(memberReference);
		}

		public void AddMethodDebugInformation(MethodDebugInformation method_info)
		{
			if (method_info.HasSequencePoints)
			{
				AddSequencePoints(method_info);
			}
			if (method_info.Scope != null)
			{
				AddLocalScope(method_info, method_info.Scope);
			}
			if (method_info.StateMachineKickOffMethod != null)
			{
				AddStateMachineMethod(method_info);
			}
			AddCustomDebugInformations(method_info.Method);
		}

		private void AddStateMachineMethod(MethodDebugInformation method_info)
		{
			StateMachineMethodTable stateMachineMethodTable = state_machine_method_table;
			MetadataToken metadataToken = method_info.Method.MetadataToken;
			uint rID = metadataToken.RID;
			metadataToken = method_info.StateMachineKickOffMethod.MetadataToken;
			stateMachineMethodTable.AddRow(new Row<uint, uint>(rID, metadataToken.RID));
		}

		private void AddLocalScope(MethodDebugInformation method_info, ScopeDebugInformation scope)
		{
			LocalScopeTable localScopeTable = local_scope_table;
			uint rID = method_info.Method.MetadataToken.RID;
			uint col = (scope.import != null) ? AddImportScope(scope.import) : 0;
			uint col2 = local_variable_rid;
			uint col3 = local_constant_rid;
			InstructionOffset instructionOffset = scope.Start;
			int offset = instructionOffset.Offset;
			instructionOffset = scope.End;
			int num;
			if (!instructionOffset.IsEndOfMethod)
			{
				instructionOffset = scope.End;
				num = instructionOffset.Offset;
			}
			else
			{
				num = method_info.code_size;
			}
			instructionOffset = scope.Start;
			int rid = localScopeTable.AddRow(new Row<uint, uint, uint, uint, uint, uint>(rID, col, col2, col3, (uint)offset, (uint)(num - instructionOffset.Offset)));
			scope.token = new MetadataToken(TokenType.LocalScope, rid);
			AddCustomDebugInformations(scope);
			if (scope.HasVariables)
			{
				AddLocalVariables(scope);
			}
			if (scope.HasConstants)
			{
				AddLocalConstants(scope);
			}
			for (int i = 0; i < scope.Scopes.Count; i++)
			{
				AddLocalScope(method_info, scope.Scopes[i]);
			}
		}

		private void AddLocalVariables(ScopeDebugInformation scope)
		{
			for (int i = 0; i < scope.Variables.Count; i++)
			{
				VariableDebugInformation variableDebugInformation = scope.Variables[i];
				local_variable_table.AddRow(new Row<VariableAttributes, ushort, uint>(variableDebugInformation.Attributes, (ushort)variableDebugInformation.Index, GetStringIndex(variableDebugInformation.Name)));
				variableDebugInformation.token = new MetadataToken(TokenType.LocalVariable, local_variable_rid);
				local_variable_rid += 1u;
				AddCustomDebugInformations(variableDebugInformation);
			}
		}

		private void AddLocalConstants(ScopeDebugInformation scope)
		{
			for (int i = 0; i < scope.Constants.Count; i++)
			{
				ConstantDebugInformation constantDebugInformation = scope.Constants[i];
				local_constant_table.AddRow(new Row<uint, uint>(GetStringIndex(constantDebugInformation.Name), GetBlobIndex(GetConstantSignature(constantDebugInformation))));
				constantDebugInformation.token = new MetadataToken(TokenType.LocalConstant, local_constant_rid);
				local_constant_rid += 1u;
			}
		}

		private SignatureWriter GetConstantSignature(ConstantDebugInformation constant)
		{
			TypeReference constantType = constant.ConstantType;
			SignatureWriter signatureWriter = CreateSignatureWriter();
			signatureWriter.WriteTypeSignature(constantType);
			if (constantType.IsTypeOf("System", "Decimal"))
			{
				int[] bits = decimal.GetBits((decimal)constant.Value);
				uint value = (uint)bits[0];
				uint value2 = (uint)bits[1];
				uint value3 = (uint)bits[2];
				byte b = (byte)(bits[3] >> 16);
				bool flag = (bits[3] & 2147483648u) != 0;
				signatureWriter.WriteByte((byte)(b | (flag ? 128 : 0)));
				signatureWriter.WriteUInt32(value);
				signatureWriter.WriteUInt32(value2);
				signatureWriter.WriteUInt32(value3);
				return signatureWriter;
			}
			if (constantType.IsTypeOf("System", "DateTime"))
			{
				signatureWriter.WriteInt64(((DateTime)constant.Value).Ticks);
				return signatureWriter;
			}
			signatureWriter.WriteBytes(GetConstantSignature(constantType.etype, constant.Value));
			return signatureWriter;
		}

		public void AddCustomDebugInformations(ICustomDebugInformationProvider provider)
		{
			if (provider.HasCustomDebugInformations)
			{
				Collection<CustomDebugInformation> customDebugInformations = provider.CustomDebugInformations;
				for (int i = 0; i < customDebugInformations.Count; i++)
				{
					CustomDebugInformation customDebugInformation = customDebugInformations[i];
					switch (customDebugInformation.Kind)
					{
					case CustomDebugInformationKind.Binary:
					{
						BinaryCustomDebugInformation binaryCustomDebugInformation = (BinaryCustomDebugInformation)customDebugInformation;
						AddCustomDebugInformation(provider, binaryCustomDebugInformation, GetBlobIndex(binaryCustomDebugInformation.Data));
						break;
					}
					case CustomDebugInformationKind.AsyncMethodBody:
						AddAsyncMethodBodyDebugInformation(provider, (AsyncMethodBodyDebugInformation)customDebugInformation);
						break;
					case CustomDebugInformationKind.StateMachineScope:
						AddStateMachineScopeDebugInformation(provider, (StateMachineScopeDebugInformation)customDebugInformation);
						break;
					case CustomDebugInformationKind.EmbeddedSource:
						AddEmbeddedSourceDebugInformation(provider, (EmbeddedSourceDebugInformation)customDebugInformation);
						break;
					case CustomDebugInformationKind.SourceLink:
						AddSourceLinkDebugInformation(provider, (SourceLinkDebugInformation)customDebugInformation);
						break;
					default:
						throw new NotImplementedException();
					}
				}
			}
		}

		private void AddStateMachineScopeDebugInformation(ICustomDebugInformationProvider provider, StateMachineScopeDebugInformation state_machine_scope)
		{
			MethodDebugInformation debugInformation = ((MethodDefinition)provider).DebugInformation;
			SignatureWriter signatureWriter = CreateSignatureWriter();
			Collection<StateMachineScope> scopes = state_machine_scope.Scopes;
			for (int i = 0; i < scopes.Count; i++)
			{
				StateMachineScope stateMachineScope = scopes[i];
				SignatureWriter signatureWriter2 = signatureWriter;
				InstructionOffset instructionOffset = stateMachineScope.Start;
				signatureWriter2.WriteUInt32((uint)instructionOffset.Offset);
				instructionOffset = stateMachineScope.End;
				int num;
				if (!instructionOffset.IsEndOfMethod)
				{
					instructionOffset = stateMachineScope.End;
					num = instructionOffset.Offset;
				}
				else
				{
					num = debugInformation.code_size;
				}
				int num2 = num;
				SignatureWriter signatureWriter3 = signatureWriter;
				int num3 = num2;
				instructionOffset = stateMachineScope.Start;
				signatureWriter3.WriteUInt32((uint)(num3 - instructionOffset.Offset));
			}
			AddCustomDebugInformation(provider, state_machine_scope, signatureWriter);
		}

		private void AddAsyncMethodBodyDebugInformation(ICustomDebugInformationProvider provider, AsyncMethodBodyDebugInformation async_method)
		{
			SignatureWriter signatureWriter = CreateSignatureWriter();
			signatureWriter.WriteUInt32((uint)(async_method.catch_handler.Offset + 1));
			if (!async_method.yields.IsNullOrEmpty())
			{
				for (int i = 0; i < async_method.yields.Count; i++)
				{
					SignatureWriter signatureWriter2 = signatureWriter;
					InstructionOffset instructionOffset = async_method.yields[i];
					signatureWriter2.WriteUInt32((uint)instructionOffset.Offset);
					SignatureWriter signatureWriter3 = signatureWriter;
					instructionOffset = async_method.resumes[i];
					signatureWriter3.WriteUInt32((uint)instructionOffset.Offset);
					signatureWriter.WriteCompressedUInt32(async_method.resume_methods[i].MetadataToken.RID);
				}
			}
			AddCustomDebugInformation(provider, async_method, signatureWriter);
		}

		private void AddEmbeddedSourceDebugInformation(ICustomDebugInformationProvider provider, EmbeddedSourceDebugInformation embedded_source)
		{
			SignatureWriter signatureWriter = CreateSignatureWriter();
			byte[] array = embedded_source.content ?? Empty<byte>.Array;
			if (embedded_source.compress)
			{
				signatureWriter.WriteInt32(array.Length);
				MemoryStream memoryStream = new MemoryStream(array);
				MemoryStream memoryStream2 = new MemoryStream();
				using (DeflateStream destination = new DeflateStream(memoryStream2, CompressionMode.Compress, true))
				{
					memoryStream.CopyTo(destination);
				}
				signatureWriter.WriteBytes(memoryStream2.ToArray());
			}
			else
			{
				signatureWriter.WriteInt32(0);
				signatureWriter.WriteBytes(array);
			}
			AddCustomDebugInformation(provider, embedded_source, signatureWriter);
		}

		private void AddSourceLinkDebugInformation(ICustomDebugInformationProvider provider, SourceLinkDebugInformation source_link)
		{
			SignatureWriter signatureWriter = CreateSignatureWriter();
			signatureWriter.WriteBytes(Encoding.UTF8.GetBytes(source_link.content));
			AddCustomDebugInformation(provider, source_link, signatureWriter);
		}

		private void AddCustomDebugInformation(ICustomDebugInformationProvider provider, CustomDebugInformation custom_info, SignatureWriter signature)
		{
			AddCustomDebugInformation(provider, custom_info, GetBlobIndex(signature));
		}

		private void AddCustomDebugInformation(ICustomDebugInformationProvider provider, CustomDebugInformation custom_info, uint blob_index)
		{
			int rid = custom_debug_information_table.AddRow(new Row<uint, uint, uint>(MakeCodedRID(provider.MetadataToken, CodedIndex.HasCustomDebugInformation), GetGuidIndex(custom_info.Identifier), blob_index));
			custom_info.token = new MetadataToken(TokenType.CustomDebugInformation, rid);
		}

		private uint AddImportScope(ImportDebugInformation import)
		{
			uint col = 0u;
			if (import.Parent != null)
			{
				col = AddImportScope(import.Parent);
			}
			uint col2 = 0u;
			if (import.HasTargets)
			{
				SignatureWriter signatureWriter = CreateSignatureWriter();
				for (int i = 0; i < import.Targets.Count; i++)
				{
					AddImportTarget(import.Targets[i], signatureWriter);
				}
				col2 = GetBlobIndex(signatureWriter);
			}
			Row<uint, uint> row = new Row<uint, uint>(col, col2);
			if (import_scope_map.TryGetValue(row, out MetadataToken value))
			{
				return value.RID;
			}
			value = new MetadataToken(TokenType.ImportScope, import_scope_table.AddRow(row));
			import_scope_map.Add(row, value);
			return value.RID;
		}

		private void AddImportTarget(ImportTarget target, SignatureWriter signature)
		{
			signature.WriteCompressedUInt32((uint)target.kind);
			MetadataToken metadataToken;
			switch (target.kind)
			{
			case ImportTargetKind.ImportNamespace:
				signature.WriteCompressedUInt32(GetUTF8StringBlobIndex(target.@namespace));
				break;
			case ImportTargetKind.ImportNamespaceInAssembly:
				metadataToken = target.reference.MetadataToken;
				signature.WriteCompressedUInt32(metadataToken.RID);
				signature.WriteCompressedUInt32(GetUTF8StringBlobIndex(target.@namespace));
				break;
			case ImportTargetKind.ImportType:
				signature.WriteTypeToken(target.type);
				break;
			case ImportTargetKind.ImportXmlNamespaceWithAlias:
				signature.WriteCompressedUInt32(GetUTF8StringBlobIndex(target.alias));
				signature.WriteCompressedUInt32(GetUTF8StringBlobIndex(target.@namespace));
				break;
			case ImportTargetKind.ImportAlias:
				signature.WriteCompressedUInt32(GetUTF8StringBlobIndex(target.alias));
				break;
			case ImportTargetKind.DefineAssemblyAlias:
				signature.WriteCompressedUInt32(GetUTF8StringBlobIndex(target.alias));
				metadataToken = target.reference.MetadataToken;
				signature.WriteCompressedUInt32(metadataToken.RID);
				break;
			case ImportTargetKind.DefineNamespaceAlias:
				signature.WriteCompressedUInt32(GetUTF8StringBlobIndex(target.alias));
				signature.WriteCompressedUInt32(GetUTF8StringBlobIndex(target.@namespace));
				break;
			case ImportTargetKind.DefineNamespaceInAssemblyAlias:
				signature.WriteCompressedUInt32(GetUTF8StringBlobIndex(target.alias));
				metadataToken = target.reference.MetadataToken;
				signature.WriteCompressedUInt32(metadataToken.RID);
				signature.WriteCompressedUInt32(GetUTF8StringBlobIndex(target.@namespace));
				break;
			case ImportTargetKind.DefineTypeAlias:
				signature.WriteCompressedUInt32(GetUTF8StringBlobIndex(target.alias));
				signature.WriteTypeToken(target.type);
				break;
			}
		}

		private uint GetUTF8StringBlobIndex(string s)
		{
			return GetBlobIndex(Encoding.UTF8.GetBytes(s));
		}

		public MetadataToken GetDocumentToken(Document document)
		{
			if (document_map.TryGetValue(document.Url, out MetadataToken metadataToken))
			{
				return metadataToken;
			}
			metadataToken = (document.token = new MetadataToken(TokenType.Document, document_table.AddRow(new Row<uint, uint, uint, uint>(GetBlobIndex(GetDocumentNameSignature(document)), GetGuidIndex(document.HashAlgorithm.ToGuid()), GetBlobIndex(document.Hash), GetGuidIndex(document.Language.ToGuid())))));
			AddCustomDebugInformations(document);
			document_map.Add(document.Url, metadataToken);
			return metadataToken;
		}

		private SignatureWriter GetDocumentNameSignature(Document document)
		{
			string url = document.Url;
			SignatureWriter signatureWriter = CreateSignatureWriter();
			if (!TryGetDocumentNameSeparator(url, out char c))
			{
				signatureWriter.WriteByte(0);
				signatureWriter.WriteCompressedUInt32(GetUTF8StringBlobIndex(url));
				return signatureWriter;
			}
			signatureWriter.WriteByte((byte)c);
			string[] array = url.Split(c);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == string.Empty)
				{
					signatureWriter.WriteCompressedUInt32(0u);
				}
				else
				{
					signatureWriter.WriteCompressedUInt32(GetUTF8StringBlobIndex(array[i]));
				}
			}
			return signatureWriter;
		}

		private static bool TryGetDocumentNameSeparator(string path, out char separator)
		{
			separator = '\0';
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < path.Length; i++)
			{
				if (path[i] == '/')
				{
					num++;
				}
				else if (path[i] == '\\')
				{
					num2++;
				}
			}
			if (num == 0 && num2 == 0)
			{
				return false;
			}
			if (num >= num2)
			{
				separator = '/';
				return true;
			}
			separator = '\\';
			return true;
		}

		private void AddSequencePoints(MethodDebugInformation info)
		{
			MetadataToken metadataToken = info.Method.MetadataToken;
			uint rID = metadataToken.RID;
			if (info.TryGetUniqueDocument(out Document document))
			{
				ref Row<uint, uint> val = ref method_debug_information_table.rows[rID - 1];
				metadataToken = GetDocumentToken(document);
				val.Col1 = metadataToken.RID;
			}
			SignatureWriter signatureWriter = CreateSignatureWriter();
			signatureWriter.WriteSequencePoints(info);
			method_debug_information_table.rows[rID - 1].Col2 = GetBlobIndex(signatureWriter);
		}
	}
}
