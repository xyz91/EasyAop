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
	internal sealed class MetadataReader : ByteBuffer
	{
		internal readonly Image image;

		internal readonly ModuleDefinition module;

		internal readonly MetadataSystem metadata;

		internal CodeReader code;

		internal IGenericContext context;

		private readonly MetadataReader metadata_reader;

		public MetadataReader(ModuleDefinition module)
			: base(module.Image.TableHeap.data)
		{
			image = module.Image;
			this.module = module;
			metadata = module.MetadataSystem;
			code = new CodeReader(this);
		}

		public MetadataReader(Image image, ModuleDefinition module, MetadataReader metadata_reader)
			: base(image.TableHeap.data)
		{
			this.image = image;
			this.module = module;
			metadata = module.MetadataSystem;
			this.metadata_reader = metadata_reader;
		}

		private int GetCodedIndexSize(CodedIndex index)
		{
			return image.GetCodedIndexSize(index);
		}

		private uint ReadByIndexSize(int size)
		{
			if (size == 4)
			{
				return base.ReadUInt32();
			}
			return base.ReadUInt16();
		}

		private byte[] ReadBlob()
		{
			BlobHeap blobHeap = image.BlobHeap;
			if (blobHeap == null)
			{
				base.position += 2;
				return Empty<byte>.Array;
			}
			return blobHeap.Read(ReadBlobIndex());
		}

		private byte[] ReadBlob(uint signature)
		{
			BlobHeap blobHeap = image.BlobHeap;
			if (blobHeap == null)
			{
				return Empty<byte>.Array;
			}
			return blobHeap.Read(signature);
		}

		private uint ReadBlobIndex()
		{
			BlobHeap blobHeap = image.BlobHeap;
			return ReadByIndexSize(blobHeap?.IndexSize ?? 2);
		}

		private void GetBlobView(uint signature, out byte[] blob, out int index, out int count)
		{
			BlobHeap blobHeap = image.BlobHeap;
			if (blobHeap == null)
			{
				blob = null;
				index = (count = 0);
			}
			else
			{
				blobHeap.GetView(signature, out blob, out index, out count);
			}
		}

		private string ReadString()
		{
			return image.StringHeap.Read(ReadByIndexSize(image.StringHeap.IndexSize));
		}

		private uint ReadStringIndex()
		{
			return ReadByIndexSize(image.StringHeap.IndexSize);
		}

		private Guid ReadGuid()
		{
			return image.GuidHeap.Read(ReadByIndexSize(image.GuidHeap.IndexSize));
		}

		private uint ReadTableIndex(Table table)
		{
			return ReadByIndexSize(image.GetTableIndexSize(table));
		}

		private MetadataToken ReadMetadataToken(CodedIndex index)
		{
			return index.GetMetadataToken(ReadByIndexSize(GetCodedIndexSize(index)));
		}

		private int MoveTo(Table table)
		{
			TableInformation tableInformation = image.TableHeap[table];
			if (tableInformation.Length != 0)
			{
				base.position = (int)tableInformation.Offset;
			}
			return (int)tableInformation.Length;
		}

		private bool MoveTo(Table table, uint row)
		{
			TableInformation tableInformation = image.TableHeap[table];
			uint length = tableInformation.Length;
			if (length != 0 && row <= length)
			{
				base.position = (int)(tableInformation.Offset + tableInformation.RowSize * (row - 1));
				return true;
			}
			return false;
		}

		public AssemblyNameDefinition ReadAssemblyNameDefinition()
		{
			if (MoveTo(Table.Assembly) == 0)
			{
				return null;
			}
			AssemblyNameDefinition assemblyNameDefinition = new AssemblyNameDefinition();
			assemblyNameDefinition.HashAlgorithm = (AssemblyHashAlgorithm)base.ReadUInt32();
			PopulateVersionAndFlags(assemblyNameDefinition);
			assemblyNameDefinition.PublicKey = ReadBlob();
			PopulateNameAndCulture(assemblyNameDefinition);
			return assemblyNameDefinition;
		}

		public ModuleDefinition Populate(ModuleDefinition module)
		{
			if (MoveTo(Table.Module) == 0)
			{
				return module;
			}
			base.Advance(2);
			module.Name = ReadString();
			module.Mvid = ReadGuid();
			return module;
		}

		private void InitializeAssemblyReferences()
		{
			if (metadata.AssemblyReferences == null)
			{
				int num = MoveTo(Table.AssemblyRef);
				AssemblyNameReference[] array = metadata.AssemblyReferences = new AssemblyNameReference[num];
				for (uint num2 = 0u; num2 < num; num2++)
				{
					AssemblyNameReference assemblyNameReference = new AssemblyNameReference();
					assemblyNameReference.token = new MetadataToken(TokenType.AssemblyRef, num2 + 1);
					PopulateVersionAndFlags(assemblyNameReference);
					byte[] array2 = ReadBlob();
					if (assemblyNameReference.HasPublicKey)
					{
						assemblyNameReference.PublicKey = array2;
					}
					else
					{
						assemblyNameReference.PublicKeyToken = array2;
					}
					PopulateNameAndCulture(assemblyNameReference);
					assemblyNameReference.Hash = ReadBlob();
					array[num2] = assemblyNameReference;
				}
			}
		}

		public Collection<AssemblyNameReference> ReadAssemblyReferences()
		{
			InitializeAssemblyReferences();
			Collection<AssemblyNameReference> collection = new Collection<AssemblyNameReference>(metadata.AssemblyReferences);
			if (module.IsWindowsMetadata())
			{
				module.Projections.AddVirtualReferences(collection);
			}
			return collection;
		}

		public MethodDefinition ReadEntryPoint()
		{
			if (module.Image.EntryPointToken == 0)
			{
				return null;
			}
			return GetMethodDefinition(new MetadataToken(module.Image.EntryPointToken).RID);
		}

		public Collection<ModuleDefinition> ReadModules()
		{
			Collection<ModuleDefinition> collection = new Collection<ModuleDefinition>(1);
			collection.Add(module);
			int num = MoveTo(Table.File);
			for (uint num2 = 1u; num2 <= num; num2++)
			{
				uint num3 = base.ReadUInt32();
				string name = ReadString();
				ReadBlobIndex();
				if (num3 == 0)
				{
					ReaderParameters parameters = new ReaderParameters
					{
						ReadingMode = module.ReadingMode,
						SymbolReaderProvider = module.SymbolReaderProvider,
						AssemblyResolver = module.AssemblyResolver
					};
					collection.Add(ModuleDefinition.ReadModule(GetModuleFileName(name), parameters));
				}
			}
			return collection;
		}

		private string GetModuleFileName(string name)
		{
			if (module.FileName == null)
			{
				throw new NotSupportedException();
			}
			return Path.Combine(Path.GetDirectoryName(module.FileName), name);
		}

		private void InitializeModuleReferences()
		{
			if (metadata.ModuleReferences == null)
			{
				int num = MoveTo(Table.ModuleRef);
				ModuleReference[] array = metadata.ModuleReferences = new ModuleReference[num];
				for (uint num2 = 0u; num2 < num; num2++)
				{
					ModuleReference moduleReference = new ModuleReference(ReadString());
					moduleReference.token = new MetadataToken(TokenType.ModuleRef, num2 + 1);
					array[num2] = moduleReference;
				}
			}
		}

		public Collection<ModuleReference> ReadModuleReferences()
		{
			InitializeModuleReferences();
			return new Collection<ModuleReference>(metadata.ModuleReferences);
		}

		public bool HasFileResource()
		{
			int num = MoveTo(Table.File);
			if (num == 0)
			{
				return false;
			}
			for (uint num2 = 1u; num2 <= num; num2++)
			{
				if (ReadFileRecord(num2).Col1 == FileAttributes.ContainsNoMetaData)
				{
					return true;
				}
			}
			return false;
		}

		public Collection<Resource> ReadResources()
		{
			int num = MoveTo(Table.ManifestResource);
			Collection<Resource> collection = new Collection<Resource>(num);
			for (int i = 1; i <= num; i++)
			{
				uint offset = base.ReadUInt32();
				ManifestResourceAttributes manifestResourceAttributes = (ManifestResourceAttributes)base.ReadUInt32();
				string name = ReadString();
				MetadataToken scope = ReadMetadataToken(CodedIndex.Implementation);
				Resource item;
				if (scope.RID == 0)
				{
					item = new EmbeddedResource(name, manifestResourceAttributes, offset, this);
					goto IL_00c6;
				}
				if (scope.TokenType == TokenType.AssemblyRef)
				{
					item = new AssemblyLinkedResource(name, manifestResourceAttributes)
					{
						Assembly = (AssemblyNameReference)GetTypeReferenceScope(scope)
					};
					goto IL_00c6;
				}
				if (scope.TokenType == TokenType.File)
				{
					Row<FileAttributes, string, uint> row = ReadFileRecord(scope.RID);
					item = new LinkedResource(name, manifestResourceAttributes)
					{
						File = row.Col2,
						hash = ReadBlob(row.Col3)
					};
					goto IL_00c6;
				}
				continue;
				IL_00c6:
				collection.Add(item);
			}
			return collection;
		}

		private Row<FileAttributes, string, uint> ReadFileRecord(uint rid)
		{
			int position = base.position;
			if (!MoveTo(Table.File, rid))
			{
				throw new ArgumentException();
			}
			Row<FileAttributes, string, uint> result = new Row<FileAttributes, string, uint>((FileAttributes)base.ReadUInt32(), ReadString(), ReadBlobIndex());
			base.position = position;
			return result;
		}

		public byte[] GetManagedResource(uint offset)
		{
			return image.GetReaderAt(image.Resources.VirtualAddress, offset, delegate(uint o, BinaryStreamReader reader)
			{
				reader.Advance((int)o);
				return reader.ReadBytes(reader.ReadInt32());
			}) ?? Empty<byte>.Array;
		}

		private void PopulateVersionAndFlags(AssemblyNameReference name)
		{
			name.Version = new Version(base.ReadUInt16(), base.ReadUInt16(), base.ReadUInt16(), base.ReadUInt16());
			name.Attributes = (AssemblyAttributes)base.ReadUInt32();
		}

		private void PopulateNameAndCulture(AssemblyNameReference name)
		{
			name.Name = ReadString();
			name.Culture = ReadString();
		}

		public TypeDefinitionCollection ReadTypes()
		{
			InitializeTypeDefinitions();
			TypeDefinition[] types = metadata.Types;
			int capacity = types.Length - metadata.NestedTypes.Count;
			TypeDefinitionCollection typeDefinitionCollection = new TypeDefinitionCollection(module, capacity);
			foreach (TypeDefinition typeDefinition in types)
			{
				if (!IsNested(typeDefinition.Attributes))
				{
					typeDefinitionCollection.Add(typeDefinition);
				}
			}
			if (image.HasTable(Table.MethodPtr) || image.HasTable(Table.FieldPtr))
			{
				CompleteTypes();
			}
			return typeDefinitionCollection;
		}

		private void CompleteTypes()
		{
			TypeDefinition[] types = metadata.Types;
			foreach (TypeDefinition obj in types)
			{
				Mixin.Read(obj.Fields);
				Mixin.Read(obj.Methods);
			}
		}

		private void InitializeTypeDefinitions()
		{
			if (metadata.Types == null)
			{
				InitializeNestedTypes();
				InitializeFields();
				InitializeMethods();
				int num = MoveTo(Table.TypeDef);
				TypeDefinition[] array = metadata.Types = new TypeDefinition[num];
				for (uint num2 = 0u; num2 < num; num2++)
				{
					if (array[num2] == null)
					{
						array[num2] = ReadType(num2 + 1);
					}
				}
				if (module.IsWindowsMetadata())
				{
					for (uint num3 = 0u; num3 < num; num3++)
					{
						WindowsRuntimeProjections.Project(array[num3]);
					}
				}
			}
		}

		private static bool IsNested(TypeAttributes attributes)
		{
			switch (attributes & TypeAttributes.VisibilityMask)
			{
			case TypeAttributes.NestedPublic:
			case TypeAttributes.NestedPrivate:
			case TypeAttributes.NestedFamily:
			case TypeAttributes.NestedAssembly:
			case TypeAttributes.NestedFamANDAssem:
			case TypeAttributes.VisibilityMask:
				return true;
			default:
				return false;
			}
		}

		public bool HasNestedTypes(TypeDefinition type)
		{
			InitializeNestedTypes();
			if (!metadata.TryGetNestedTypeMapping(type, out Collection<uint> collection))
			{
				return false;
			}
			return collection.Count > 0;
		}

		public Collection<TypeDefinition> ReadNestedTypes(TypeDefinition type)
		{
			InitializeNestedTypes();
			if (!metadata.TryGetNestedTypeMapping(type, out Collection<uint> collection))
			{
				return new MemberDefinitionCollection<TypeDefinition>(type);
			}
			MemberDefinitionCollection<TypeDefinition> memberDefinitionCollection = new MemberDefinitionCollection<TypeDefinition>(type, collection.Count);
			for (int i = 0; i < collection.Count; i++)
			{
				TypeDefinition typeDefinition = GetTypeDefinition(collection[i]);
				if (typeDefinition != null)
				{
					memberDefinitionCollection.Add(typeDefinition);
				}
			}
			metadata.RemoveNestedTypeMapping(type);
			return memberDefinitionCollection;
		}

		private void InitializeNestedTypes()
		{
			if (metadata.NestedTypes == null)
			{
				int num = MoveTo(Table.NestedClass);
				metadata.NestedTypes = new Dictionary<uint, Collection<uint>>(num);
				metadata.ReverseNestedTypes = new Dictionary<uint, uint>(num);
				if (num != 0)
				{
					for (int i = 1; i <= num; i++)
					{
						uint nested = ReadTableIndex(Table.TypeDef);
						uint declaring = ReadTableIndex(Table.TypeDef);
						AddNestedMapping(declaring, nested);
					}
				}
			}
		}

		private void AddNestedMapping(uint declaring, uint nested)
		{
			metadata.SetNestedTypeMapping(declaring, AddMapping(metadata.NestedTypes, declaring, nested));
			metadata.SetReverseNestedTypeMapping(nested, declaring);
		}

		private static Collection<TValue> AddMapping<TKey, TValue>(Dictionary<TKey, Collection<TValue>> cache, TKey key, TValue value)
		{
			if (!cache.TryGetValue(key, out Collection<TValue> collection))
			{
				collection = new Collection<TValue>();
			}
			collection.Add(value);
			return collection;
		}

		private TypeDefinition ReadType(uint rid)
		{
			if (!MoveTo(Table.TypeDef, rid))
			{
				return null;
			}
			TypeAttributes attributes = (TypeAttributes)base.ReadUInt32();
			string name = ReadString();
			TypeDefinition typeDefinition = new TypeDefinition(ReadString(), name, attributes);
			typeDefinition.token = new MetadataToken(TokenType.TypeDef, rid);
			typeDefinition.scope = module;
			typeDefinition.module = module;
			metadata.AddTypeDefinition(typeDefinition);
			context = typeDefinition;
			typeDefinition.BaseType = GetTypeDefOrRef(ReadMetadataToken(CodedIndex.TypeDefOrRef));
			typeDefinition.fields_range = ReadListRange(rid, Table.TypeDef, Table.Field);
			typeDefinition.methods_range = ReadListRange(rid, Table.TypeDef, Table.Method);
			if (IsNested(attributes))
			{
				typeDefinition.DeclaringType = GetNestedTypeDeclaringType(typeDefinition);
			}
			return typeDefinition;
		}

		private TypeDefinition GetNestedTypeDeclaringType(TypeDefinition type)
		{
			if (!metadata.TryGetReverseNestedTypeMapping(type, out uint rid))
			{
				return null;
			}
			metadata.RemoveReverseNestedTypeMapping(type);
			return GetTypeDefinition(rid);
		}

		private Range ReadListRange(uint current_index, Table current, Table target)
		{
			Range result = default(Range);
			uint num = ReadTableIndex(target);
			if (num == 0)
			{
				return result;
			}
			TableInformation tableInformation = image.TableHeap[current];
			uint num2;
			if (current_index == tableInformation.Length)
			{
				num2 = image.TableHeap[target].Length + 1;
			}
			else
			{
				int position = base.position;
				base.position += (int)(tableInformation.RowSize - image.GetTableIndexSize(target));
				num2 = ReadTableIndex(target);
				base.position = position;
			}
			result.Start = num;
			result.Length = num2 - num;
			return result;
		}

		public Row<short, int> ReadTypeLayout(TypeDefinition type)
		{
			InitializeTypeLayouts();
			uint rID = type.token.RID;
			if (!metadata.ClassLayouts.TryGetValue(rID, out Row<ushort, uint> row))
			{
				return new Row<short, int>(-1, -1);
			}
			type.PackingSize = (short)row.Col1;
			type.ClassSize = (int)row.Col2;
			metadata.ClassLayouts.Remove(rID);
			return new Row<short, int>((short)row.Col1, (int)row.Col2);
		}

		private void InitializeTypeLayouts()
		{
			if (metadata.ClassLayouts == null)
			{
				int num = MoveTo(Table.ClassLayout);
				Dictionary<uint, Row<ushort, uint>> dictionary = metadata.ClassLayouts = new Dictionary<uint, Row<ushort, uint>>(num);
				for (uint num2 = 0u; num2 < num; num2++)
				{
					ushort col = base.ReadUInt16();
					uint col2 = base.ReadUInt32();
					uint key = ReadTableIndex(Table.TypeDef);
					dictionary.Add(key, new Row<ushort, uint>(col, col2));
				}
			}
		}

		public TypeReference GetTypeDefOrRef(MetadataToken token)
		{
			return (TypeReference)LookupToken(token);
		}

		public TypeDefinition GetTypeDefinition(uint rid)
		{
			InitializeTypeDefinitions();
			TypeDefinition typeDefinition = metadata.GetTypeDefinition(rid);
			if (typeDefinition != null)
			{
				return typeDefinition;
			}
			typeDefinition = ReadTypeDefinition(rid);
			if (module.IsWindowsMetadata())
			{
				WindowsRuntimeProjections.Project(typeDefinition);
			}
			return typeDefinition;
		}

		private TypeDefinition ReadTypeDefinition(uint rid)
		{
			if (!MoveTo(Table.TypeDef, rid))
			{
				return null;
			}
			return ReadType(rid);
		}

		private void InitializeTypeReferences()
		{
			if (metadata.TypeReferences == null)
			{
				metadata.TypeReferences = new TypeReference[image.GetTableLength(Table.TypeRef)];
			}
		}

		public TypeReference GetTypeReference(string scope, string full_name)
		{
			InitializeTypeReferences();
			int num = metadata.TypeReferences.Length;
			for (uint num2 = 1u; num2 <= num; num2++)
			{
				TypeReference typeReference = GetTypeReference(num2);
				if (!(typeReference.FullName != full_name))
				{
					if (string.IsNullOrEmpty(scope))
					{
						return typeReference;
					}
					if (typeReference.Scope.Name == scope)
					{
						return typeReference;
					}
				}
			}
			return null;
		}

		private TypeReference GetTypeReference(uint rid)
		{
			InitializeTypeReferences();
			TypeReference typeReference = metadata.GetTypeReference(rid);
			if (typeReference != null)
			{
				return typeReference;
			}
			return ReadTypeReference(rid);
		}

		private TypeReference ReadTypeReference(uint rid)
		{
			if (!MoveTo(Table.TypeRef, rid))
			{
				return null;
			}
			TypeReference typeReference = null;
			MetadataToken metadataToken = ReadMetadataToken(CodedIndex.ResolutionScope);
			string name = ReadString();
			TypeReference typeReference2 = new TypeReference(ReadString(), name, module, null);
			typeReference2.token = new MetadataToken(TokenType.TypeRef, rid);
			metadata.AddTypeReference(typeReference2);
			IMetadataScope scope;
			if (metadataToken.TokenType == TokenType.TypeRef)
			{
				typeReference = GetTypeDefOrRef(metadataToken);
				object metadataScope2;
				if (typeReference == null)
				{
					IMetadataScope metadataScope = module;
					metadataScope2 = metadataScope;
				}
				else
				{
					metadataScope2 = typeReference.Scope;
				}
				scope = (IMetadataScope)metadataScope2;
			}
			else
			{
				scope = GetTypeReferenceScope(metadataToken);
			}
			typeReference2.scope = scope;
			typeReference2.DeclaringType = typeReference;
			MetadataSystem.TryProcessPrimitiveTypeReference(typeReference2);
			if (typeReference2.Module.IsWindowsMetadata())
			{
				WindowsRuntimeProjections.Project(typeReference2);
			}
			return typeReference2;
		}

		private IMetadataScope GetTypeReferenceScope(MetadataToken scope)
		{
			if (scope.TokenType == TokenType.Module)
			{
				return module;
			}
			IMetadataScope[] array;
			switch (scope.TokenType)
			{
			case TokenType.AssemblyRef:
				InitializeAssemblyReferences();
				array = metadata.AssemblyReferences;
				break;
			case TokenType.ModuleRef:
				InitializeModuleReferences();
				array = metadata.ModuleReferences;
				break;
			default:
				throw new NotSupportedException();
			}
			uint num = scope.RID - 1;
			if (num >= 0 && num < array.Length)
			{
				return array[num];
			}
			return null;
		}

		public IEnumerable<TypeReference> GetTypeReferences()
		{
			InitializeTypeReferences();
			int tableLength = image.GetTableLength(Table.TypeRef);
			TypeReference[] array = new TypeReference[tableLength];
			for (uint num = 1u; num <= tableLength; num++)
			{
				array[num - 1] = GetTypeReference(num);
			}
			return array;
		}

		private TypeReference GetTypeSpecification(uint rid)
		{
			if (!MoveTo(Table.TypeSpec, rid))
			{
				return null;
			}
			TypeReference typeReference = ReadSignature(ReadBlobIndex()).ReadTypeSignature();
			if (typeReference.token.RID == 0)
			{
				typeReference.token = new MetadataToken(TokenType.TypeSpec, rid);
			}
			return typeReference;
		}

		private SignatureReader ReadSignature(uint signature)
		{
			return new SignatureReader(signature, this);
		}

		public bool HasInterfaces(TypeDefinition type)
		{
			InitializeInterfaces();
			Collection<Row<uint, MetadataToken>> collection;
			return metadata.TryGetInterfaceMapping(type, out collection);
		}

		public InterfaceImplementationCollection ReadInterfaces(TypeDefinition type)
		{
			InitializeInterfaces();
			if (!metadata.TryGetInterfaceMapping(type, out Collection<Row<uint, MetadataToken>> collection))
			{
				return new InterfaceImplementationCollection(type);
			}
			InterfaceImplementationCollection interfaceImplementationCollection = new InterfaceImplementationCollection(type, collection.Count);
			context = type;
			for (int i = 0; i < collection.Count; i++)
			{
				interfaceImplementationCollection.Add(new InterfaceImplementation(GetTypeDefOrRef(collection[i].Col2), new MetadataToken(TokenType.InterfaceImpl, collection[i].Col1)));
			}
			metadata.RemoveInterfaceMapping(type);
			return interfaceImplementationCollection;
		}

		private void InitializeInterfaces()
		{
			if (metadata.Interfaces == null)
			{
				int num = MoveTo(Table.InterfaceImpl);
				metadata.Interfaces = new Dictionary<uint, Collection<Row<uint, MetadataToken>>>(num);
				for (uint num2 = 1u; num2 <= num; num2++)
				{
					uint type = ReadTableIndex(Table.TypeDef);
					MetadataToken col = ReadMetadataToken(CodedIndex.TypeDefOrRef);
					AddInterfaceMapping(type, new Row<uint, MetadataToken>(num2, col));
				}
			}
		}

		private void AddInterfaceMapping(uint type, Row<uint, MetadataToken> @interface)
		{
			metadata.SetInterfaceMapping(type, AddMapping(metadata.Interfaces, type, @interface));
		}

		public Collection<FieldDefinition> ReadFields(TypeDefinition type)
		{
			Range fields_range = type.fields_range;
			if (fields_range.Length == 0)
			{
				return new MemberDefinitionCollection<FieldDefinition>(type);
			}
			MemberDefinitionCollection<FieldDefinition> memberDefinitionCollection = new MemberDefinitionCollection<FieldDefinition>(type, (int)fields_range.Length);
			context = type;
			if (!MoveTo(Table.FieldPtr, fields_range.Start))
			{
				if (!MoveTo(Table.Field, fields_range.Start))
				{
					return memberDefinitionCollection;
				}
				for (uint num = 0u; num < fields_range.Length; num++)
				{
					ReadField(fields_range.Start + num, memberDefinitionCollection);
				}
			}
			else
			{
				ReadPointers(Table.FieldPtr, Table.Field, fields_range, memberDefinitionCollection, ReadField);
			}
			return memberDefinitionCollection;
		}

		private void ReadField(uint field_rid, Collection<FieldDefinition> fields)
		{
			FieldAttributes attributes = (FieldAttributes)base.ReadUInt16();
			string name = ReadString();
			uint signature = ReadBlobIndex();
			FieldDefinition fieldDefinition = new FieldDefinition(name, attributes, ReadFieldType(signature));
			fieldDefinition.token = new MetadataToken(TokenType.Field, field_rid);
			metadata.AddFieldDefinition(fieldDefinition);
			if (!IsDeleted(fieldDefinition))
			{
				fields.Add(fieldDefinition);
				if (module.IsWindowsMetadata())
				{
					WindowsRuntimeProjections.Project(fieldDefinition);
				}
			}
		}

		private void InitializeFields()
		{
			if (metadata.Fields == null)
			{
				metadata.Fields = new FieldDefinition[image.GetTableLength(Table.Field)];
			}
		}

		private TypeReference ReadFieldType(uint signature)
		{
			SignatureReader signatureReader = ReadSignature(signature);
			if (signatureReader.ReadByte() != 6)
			{
				throw new NotSupportedException();
			}
			return signatureReader.ReadTypeSignature();
		}

		public int ReadFieldRVA(FieldDefinition field)
		{
			InitializeFieldRVAs();
			uint rID = field.token.RID;
			if (!metadata.FieldRVAs.TryGetValue(rID, out uint num))
			{
				return 0;
			}
			int fieldTypeSize = GetFieldTypeSize(field.FieldType);
			if (fieldTypeSize != 0 && num != 0)
			{
				metadata.FieldRVAs.Remove(rID);
				field.InitialValue = GetFieldInitializeValue(fieldTypeSize, num);
				return (int)num;
			}
			return 0;
		}

		private byte[] GetFieldInitializeValue(int size, uint rva)
		{
			return image.GetReaderAt(rva, size, (int s, BinaryStreamReader reader) => reader.ReadBytes(s)) ?? Empty<byte>.Array;
		}

		private static int GetFieldTypeSize(TypeReference type)
		{
			int result = 0;
			switch (type.etype)
			{
			case ElementType.Boolean:
			case ElementType.I1:
			case ElementType.U1:
				result = 1;
				break;
			case ElementType.Char:
			case ElementType.I2:
			case ElementType.U2:
				result = 2;
				break;
			case ElementType.I4:
			case ElementType.U4:
			case ElementType.R4:
				result = 4;
				break;
			case ElementType.I8:
			case ElementType.U8:
			case ElementType.R8:
				result = 8;
				break;
			case ElementType.Ptr:
			case ElementType.FnPtr:
				result = IntPtr.Size;
				break;
			case ElementType.CModReqD:
			case ElementType.CModOpt:
				return GetFieldTypeSize(((IModifierType)type).ElementType);
			default:
			{
				TypeDefinition typeDefinition = type.Resolve();
				if (typeDefinition != null && typeDefinition.HasLayoutInfo)
				{
					result = typeDefinition.ClassSize;
				}
				break;
			}
			}
			return result;
		}

		private void InitializeFieldRVAs()
		{
			if (metadata.FieldRVAs == null)
			{
				int num = MoveTo(Table.FieldRVA);
				Dictionary<uint, uint> dictionary = metadata.FieldRVAs = new Dictionary<uint, uint>(num);
				for (int i = 0; i < num; i++)
				{
					uint value = base.ReadUInt32();
					uint key = ReadTableIndex(Table.Field);
					dictionary.Add(key, value);
				}
			}
		}

		public int ReadFieldLayout(FieldDefinition field)
		{
			InitializeFieldLayouts();
			uint rID = field.token.RID;
			if (!metadata.FieldLayouts.TryGetValue(rID, out uint result))
			{
				return -1;
			}
			metadata.FieldLayouts.Remove(rID);
			return (int)result;
		}

		private void InitializeFieldLayouts()
		{
			if (metadata.FieldLayouts == null)
			{
				int num = MoveTo(Table.FieldLayout);
				Dictionary<uint, uint> dictionary = metadata.FieldLayouts = new Dictionary<uint, uint>(num);
				for (int i = 0; i < num; i++)
				{
					uint value = base.ReadUInt32();
					uint key = ReadTableIndex(Table.Field);
					dictionary.Add(key, value);
				}
			}
		}

		public bool HasEvents(TypeDefinition type)
		{
			InitializeEvents();
			if (!metadata.TryGetEventsRange(type, out Range range))
			{
				return false;
			}
			return range.Length != 0;
		}

		public Collection<EventDefinition> ReadEvents(TypeDefinition type)
		{
			InitializeEvents();
			if (!metadata.TryGetEventsRange(type, out Range range))
			{
				return new MemberDefinitionCollection<EventDefinition>(type);
			}
			MemberDefinitionCollection<EventDefinition> memberDefinitionCollection = new MemberDefinitionCollection<EventDefinition>(type, (int)range.Length);
			metadata.RemoveEventsRange(type);
			if (range.Length == 0)
			{
				return memberDefinitionCollection;
			}
			context = type;
			if (!MoveTo(Table.EventPtr, range.Start))
			{
				if (!MoveTo(Table.Event, range.Start))
				{
					return memberDefinitionCollection;
				}
				for (uint num = 0u; num < range.Length; num++)
				{
					ReadEvent(range.Start + num, memberDefinitionCollection);
				}
			}
			else
			{
				ReadPointers(Table.EventPtr, Table.Event, range, memberDefinitionCollection, ReadEvent);
			}
			return memberDefinitionCollection;
		}

		private void ReadEvent(uint event_rid, Collection<EventDefinition> events)
		{
			EventAttributes attributes = (EventAttributes)base.ReadUInt16();
			string name = ReadString();
			TypeReference typeDefOrRef = GetTypeDefOrRef(ReadMetadataToken(CodedIndex.TypeDefOrRef));
			EventDefinition eventDefinition = new EventDefinition(name, attributes, typeDefOrRef);
			eventDefinition.token = new MetadataToken(TokenType.Event, event_rid);
			if (!IsDeleted(eventDefinition))
			{
				events.Add(eventDefinition);
			}
		}

		private void InitializeEvents()
		{
			if (metadata.Events == null)
			{
				int num = MoveTo(Table.EventMap);
				metadata.Events = new Dictionary<uint, Range>(num);
				for (uint num2 = 1u; num2 <= num; num2++)
				{
					uint type_rid = ReadTableIndex(Table.TypeDef);
					Range range = ReadListRange(num2, Table.EventMap, Table.Event);
					metadata.AddEventsRange(type_rid, range);
				}
			}
		}

		public bool HasProperties(TypeDefinition type)
		{
			InitializeProperties();
			if (!metadata.TryGetPropertiesRange(type, out Range range))
			{
				return false;
			}
			return range.Length != 0;
		}

		public Collection<PropertyDefinition> ReadProperties(TypeDefinition type)
		{
			InitializeProperties();
			if (!metadata.TryGetPropertiesRange(type, out Range range))
			{
				return new MemberDefinitionCollection<PropertyDefinition>(type);
			}
			metadata.RemovePropertiesRange(type);
			MemberDefinitionCollection<PropertyDefinition> memberDefinitionCollection = new MemberDefinitionCollection<PropertyDefinition>(type, (int)range.Length);
			if (range.Length == 0)
			{
				return memberDefinitionCollection;
			}
			context = type;
			if (!MoveTo(Table.PropertyPtr, range.Start))
			{
				if (!MoveTo(Table.Property, range.Start))
				{
					return memberDefinitionCollection;
				}
				for (uint num = 0u; num < range.Length; num++)
				{
					ReadProperty(range.Start + num, memberDefinitionCollection);
				}
			}
			else
			{
				ReadPointers(Table.PropertyPtr, Table.Property, range, memberDefinitionCollection, ReadProperty);
			}
			return memberDefinitionCollection;
		}

		private void ReadProperty(uint property_rid, Collection<PropertyDefinition> properties)
		{
			PropertyAttributes attributes = (PropertyAttributes)base.ReadUInt16();
			string name = ReadString();
			uint signature = ReadBlobIndex();
			SignatureReader signatureReader = ReadSignature(signature);
			byte num = signatureReader.ReadByte();
			if ((num & 8) == 0)
			{
				throw new NotSupportedException();
			}
			bool hasThis = (num & 0x20) != 0;
			signatureReader.ReadCompressedUInt32();
			PropertyDefinition propertyDefinition = new PropertyDefinition(name, attributes, signatureReader.ReadTypeSignature());
			propertyDefinition.HasThis = hasThis;
			propertyDefinition.token = new MetadataToken(TokenType.Property, property_rid);
			if (!IsDeleted(propertyDefinition))
			{
				properties.Add(propertyDefinition);
			}
		}

		private void InitializeProperties()
		{
			if (metadata.Properties == null)
			{
				int num = MoveTo(Table.PropertyMap);
				metadata.Properties = new Dictionary<uint, Range>(num);
				for (uint num2 = 1u; num2 <= num; num2++)
				{
					uint type_rid = ReadTableIndex(Table.TypeDef);
					Range range = ReadListRange(num2, Table.PropertyMap, Table.Property);
					metadata.AddPropertiesRange(type_rid, range);
				}
			}
		}

		private MethodSemanticsAttributes ReadMethodSemantics(MethodDefinition method)
		{
			InitializeMethodSemantics();
			if (!metadata.Semantics.TryGetValue(method.token.RID, out Row<MethodSemanticsAttributes, MetadataToken> row))
			{
				return MethodSemanticsAttributes.None;
			}
			TypeDefinition declaringType = method.DeclaringType;
			switch (row.Col1)
			{
			case MethodSemanticsAttributes.AddOn:
				GetEvent(declaringType, row.Col2).add_method = method;
				break;
			case MethodSemanticsAttributes.Fire:
				GetEvent(declaringType, row.Col2).invoke_method = method;
				break;
			case MethodSemanticsAttributes.RemoveOn:
				GetEvent(declaringType, row.Col2).remove_method = method;
				break;
			case MethodSemanticsAttributes.Getter:
				GetProperty(declaringType, row.Col2).get_method = method;
				break;
			case MethodSemanticsAttributes.Setter:
				GetProperty(declaringType, row.Col2).set_method = method;
				break;
			case MethodSemanticsAttributes.Other:
				switch (row.Col2.TokenType)
				{
				case TokenType.Event:
				{
					EventDefinition @event = GetEvent(declaringType, row.Col2);
					if (@event.other_methods == null)
					{
						@event.other_methods = new Collection<MethodDefinition>();
					}
					@event.other_methods.Add(method);
					break;
				}
				case TokenType.Property:
				{
					PropertyDefinition property = GetProperty(declaringType, row.Col2);
					if (property.other_methods == null)
					{
						property.other_methods = new Collection<MethodDefinition>();
					}
					property.other_methods.Add(method);
					break;
				}
				default:
					throw new NotSupportedException();
				}
				break;
			default:
				throw new NotSupportedException();
			}
			metadata.Semantics.Remove(method.token.RID);
			return row.Col1;
		}

		private static EventDefinition GetEvent(TypeDefinition type, MetadataToken token)
		{
			if (token.TokenType != TokenType.Event)
			{
				throw new ArgumentException();
			}
			return GetMember(type.Events, token);
		}

		private static PropertyDefinition GetProperty(TypeDefinition type, MetadataToken token)
		{
			if (token.TokenType != TokenType.Property)
			{
				throw new ArgumentException();
			}
			return GetMember(type.Properties, token);
		}

		private static TMember GetMember<TMember>(Collection<TMember> members, MetadataToken token) where TMember : IMemberDefinition
		{
			for (int i = 0; i < members.Count; i++)
			{
				TMember result = members[i];
				if (result.MetadataToken == token)
				{
					return result;
				}
			}
			throw new ArgumentException();
		}

		private void InitializeMethodSemantics()
		{
			if (metadata.Semantics == null)
			{
				int num = MoveTo(Table.MethodSemantics);
				Dictionary<uint, Row<MethodSemanticsAttributes, MetadataToken>> dictionary = metadata.Semantics = new Dictionary<uint, Row<MethodSemanticsAttributes, MetadataToken>>(0);
				for (uint num2 = 0u; num2 < num; num2++)
				{
					MethodSemanticsAttributes col = (MethodSemanticsAttributes)base.ReadUInt16();
					uint key = ReadTableIndex(Table.Method);
					MetadataToken col2 = ReadMetadataToken(CodedIndex.HasSemantics);
					dictionary[key] = new Row<MethodSemanticsAttributes, MetadataToken>(col, col2);
				}
			}
		}

		public void ReadMethods(PropertyDefinition property)
		{
			ReadAllSemantics(property.DeclaringType);
		}

		public void ReadMethods(EventDefinition @event)
		{
			ReadAllSemantics(@event.DeclaringType);
		}

		public void ReadAllSemantics(MethodDefinition method)
		{
			ReadAllSemantics(method.DeclaringType);
		}

		private void ReadAllSemantics(TypeDefinition type)
		{
			Collection<MethodDefinition> methods = type.Methods;
			for (int i = 0; i < methods.Count; i++)
			{
				MethodDefinition methodDefinition = methods[i];
				if (!methodDefinition.sem_attrs_ready)
				{
					methodDefinition.sem_attrs = ReadMethodSemantics(methodDefinition);
					methodDefinition.sem_attrs_ready = true;
				}
			}
		}

		public Collection<MethodDefinition> ReadMethods(TypeDefinition type)
		{
			Range methods_range = type.methods_range;
			if (methods_range.Length == 0)
			{
				return new MemberDefinitionCollection<MethodDefinition>(type);
			}
			MemberDefinitionCollection<MethodDefinition> memberDefinitionCollection = new MemberDefinitionCollection<MethodDefinition>(type, (int)methods_range.Length);
			if (!MoveTo(Table.MethodPtr, methods_range.Start))
			{
				if (!MoveTo(Table.Method, methods_range.Start))
				{
					return memberDefinitionCollection;
				}
				for (uint num = 0u; num < methods_range.Length; num++)
				{
					ReadMethod(methods_range.Start + num, memberDefinitionCollection);
				}
			}
			else
			{
				ReadPointers(Table.MethodPtr, Table.Method, methods_range, memberDefinitionCollection, ReadMethod);
			}
			return memberDefinitionCollection;
		}

		private void ReadPointers<TMember>(Table ptr, Table table, Range range, Collection<TMember> members, Action<uint, Collection<TMember>> reader) where TMember : IMemberDefinition
		{
			for (uint num = 0u; num < range.Length; num++)
			{
				MoveTo(ptr, range.Start + num);
				uint num2 = ReadTableIndex(table);
				MoveTo(table, num2);
				reader(num2, members);
			}
		}

		private static bool IsDeleted(IMemberDefinition member)
		{
			if (member.IsSpecialName)
			{
				return member.Name == "_Deleted";
			}
			return false;
		}

		private void InitializeMethods()
		{
			if (metadata.Methods == null)
			{
				metadata.Methods = new MethodDefinition[image.GetTableLength(Table.Method)];
			}
		}

		private void ReadMethod(uint method_rid, Collection<MethodDefinition> methods)
		{
			MethodDefinition methodDefinition = new MethodDefinition();
			methodDefinition.rva = base.ReadUInt32();
			methodDefinition.ImplAttributes = (MethodImplAttributes)base.ReadUInt16();
			methodDefinition.Attributes = (MethodAttributes)base.ReadUInt16();
			methodDefinition.Name = ReadString();
			methodDefinition.token = new MetadataToken(TokenType.Method, method_rid);
			if (!IsDeleted(methodDefinition))
			{
				methods.Add(methodDefinition);
				uint signature = ReadBlobIndex();
				Range range = ReadListRange(method_rid, Table.Method, Table.Param);
				context = methodDefinition;
				ReadMethodSignature(signature, methodDefinition);
				metadata.AddMethodDefinition(methodDefinition);
				if (range.Length != 0)
				{
					int position = base.position;
					ReadParameters(methodDefinition, range);
					base.position = position;
				}
				if (module.IsWindowsMetadata())
				{
					WindowsRuntimeProjections.Project(methodDefinition);
				}
			}
		}

		private void ReadParameters(MethodDefinition method, Range param_range)
		{
			if (!MoveTo(Table.ParamPtr, param_range.Start))
			{
				if (MoveTo(Table.Param, param_range.Start))
				{
					for (uint num = 0u; num < param_range.Length; num++)
					{
						ReadParameter(param_range.Start + num, method);
					}
				}
			}
			else
			{
				ReadParameterPointers(method, param_range);
			}
		}

		private void ReadParameterPointers(MethodDefinition method, Range range)
		{
			for (uint num = 0u; num < range.Length; num++)
			{
				MoveTo(Table.ParamPtr, range.Start + num);
				uint num2 = ReadTableIndex(Table.Param);
				MoveTo(Table.Param, num2);
				ReadParameter(num2, method);
			}
		}

		private void ReadParameter(uint param_rid, MethodDefinition method)
		{
			ParameterAttributes attributes = (ParameterAttributes)base.ReadUInt16();
			ushort num = base.ReadUInt16();
			string name = ReadString();
			ParameterDefinition obj = (num == 0) ? method.MethodReturnType.Parameter : method.Parameters[num - 1];
			obj.token = new MetadataToken(TokenType.Param, param_rid);
			obj.Name = name;
			obj.Attributes = attributes;
		}

		private void ReadMethodSignature(uint signature, IMethodSignature method)
		{
			ReadSignature(signature).ReadMethodSignature(method);
		}

		public PInvokeInfo ReadPInvokeInfo(MethodDefinition method)
		{
			InitializePInvokes();
			uint rID = method.token.RID;
			if (!metadata.PInvokes.TryGetValue(rID, out Row<PInvokeAttributes, uint, uint> row))
			{
				return null;
			}
			metadata.PInvokes.Remove(rID);
			return new PInvokeInfo(row.Col1, image.StringHeap.Read(row.Col2), module.ModuleReferences[(int)(row.Col3 - 1)]);
		}

		private void InitializePInvokes()
		{
			if (metadata.PInvokes == null)
			{
				int num = MoveTo(Table.ImplMap);
				Dictionary<uint, Row<PInvokeAttributes, uint, uint>> dictionary = metadata.PInvokes = new Dictionary<uint, Row<PInvokeAttributes, uint, uint>>(num);
				for (int i = 1; i <= num; i++)
				{
					PInvokeAttributes col = (PInvokeAttributes)base.ReadUInt16();
					MetadataToken metadataToken = ReadMetadataToken(CodedIndex.MemberForwarded);
					uint col2 = ReadStringIndex();
					uint col3 = ReadTableIndex(Table.File);
					if (metadataToken.TokenType == TokenType.Method)
					{
						dictionary.Add(metadataToken.RID, new Row<PInvokeAttributes, uint, uint>(col, col2, col3));
					}
				}
			}
		}

		public bool HasGenericParameters(IGenericParameterProvider provider)
		{
			InitializeGenericParameters();
			if (!metadata.TryGetGenericParameterRanges(provider, out Range[] ranges))
			{
				return false;
			}
			return RangesSize(ranges) > 0;
		}

		public Collection<GenericParameter> ReadGenericParameters(IGenericParameterProvider provider)
		{
			InitializeGenericParameters();
			if (!metadata.TryGetGenericParameterRanges(provider, out Range[] array))
			{
				return new GenericParameterCollection(provider);
			}
			metadata.RemoveGenericParameterRange(provider);
			GenericParameterCollection genericParameterCollection = new GenericParameterCollection(provider, RangesSize(array));
			for (int i = 0; i < array.Length; i++)
			{
				ReadGenericParametersRange(array[i], provider, genericParameterCollection);
			}
			return genericParameterCollection;
		}

		private void ReadGenericParametersRange(Range range, IGenericParameterProvider provider, GenericParameterCollection generic_parameters)
		{
			if (MoveTo(Table.GenericParam, range.Start))
			{
				for (uint num = 0u; num < range.Length; num++)
				{
					base.ReadUInt16();
					GenericParameterAttributes attributes = (GenericParameterAttributes)base.ReadUInt16();
					ReadMetadataToken(CodedIndex.TypeOrMethodDef);
					GenericParameter genericParameter = new GenericParameter(ReadString(), provider);
					genericParameter.token = new MetadataToken(TokenType.GenericParam, range.Start + num);
					genericParameter.Attributes = attributes;
					generic_parameters.Add(genericParameter);
				}
			}
		}

		private void InitializeGenericParameters()
		{
			if (metadata.GenericParameters == null)
			{
				metadata.GenericParameters = InitializeRanges(Table.GenericParam, delegate
				{
					base.Advance(4);
					MetadataToken result = ReadMetadataToken(CodedIndex.TypeOrMethodDef);
					ReadStringIndex();
					return result;
				});
			}
		}

		private Dictionary<MetadataToken, Range[]> InitializeRanges(Table table, Func<MetadataToken> get_next)
		{
			int num = MoveTo(table);
			Dictionary<MetadataToken, Range[]> dictionary = new Dictionary<MetadataToken, Range[]>(num);
			if (num == 0)
			{
				return dictionary;
			}
			MetadataToken metadataToken = MetadataToken.Zero;
			Range range = new Range(1u, 0u);
			for (uint num2 = 1u; num2 <= num; num2++)
			{
				MetadataToken metadataToken2 = get_next();
				if (num2 == 1)
				{
					metadataToken = metadataToken2;
					range.Length += 1u;
				}
				else if (metadataToken2 != metadataToken)
				{
					AddRange(dictionary, metadataToken, range);
					range = new Range(num2, 1u);
					metadataToken = metadataToken2;
				}
				else
				{
					range.Length += 1u;
				}
			}
			AddRange(dictionary, metadataToken, range);
			return dictionary;
		}

		private static void AddRange(Dictionary<MetadataToken, Range[]> ranges, MetadataToken owner, Range range)
		{
			if (owner.RID != 0)
			{
				if (!ranges.TryGetValue(owner, out Range[] self))
				{
					ranges.Add(owner, new Range[1]
					{
						range
					});
				}
				else
				{
					ranges[owner] = self.Add(range);
				}
			}
		}

		public bool HasGenericConstraints(GenericParameter generic_parameter)
		{
			InitializeGenericConstraints();
			if (!metadata.TryGetGenericConstraintMapping(generic_parameter, out Collection<MetadataToken> collection))
			{
				return false;
			}
			return collection.Count > 0;
		}

		public Collection<TypeReference> ReadGenericConstraints(GenericParameter generic_parameter)
		{
			InitializeGenericConstraints();
			if (!metadata.TryGetGenericConstraintMapping(generic_parameter, out Collection<MetadataToken> collection))
			{
				return new Collection<TypeReference>();
			}
			Collection<TypeReference> collection2 = new Collection<TypeReference>(collection.Count);
			context = (IGenericContext)generic_parameter.Owner;
			for (int i = 0; i < collection.Count; i++)
			{
				collection2.Add(GetTypeDefOrRef(collection[i]));
			}
			metadata.RemoveGenericConstraintMapping(generic_parameter);
			return collection2;
		}

		private void InitializeGenericConstraints()
		{
			if (metadata.GenericConstraints == null)
			{
				int num = MoveTo(Table.GenericParamConstraint);
				metadata.GenericConstraints = new Dictionary<uint, Collection<MetadataToken>>(num);
				for (int i = 1; i <= num; i++)
				{
					AddGenericConstraintMapping(ReadTableIndex(Table.GenericParam), ReadMetadataToken(CodedIndex.TypeDefOrRef));
				}
			}
		}

		private void AddGenericConstraintMapping(uint generic_parameter, MetadataToken constraint)
		{
			metadata.SetGenericConstraintMapping(generic_parameter, AddMapping(metadata.GenericConstraints, generic_parameter, constraint));
		}

		public bool HasOverrides(MethodDefinition method)
		{
			InitializeOverrides();
			if (!metadata.TryGetOverrideMapping(method, out Collection<MetadataToken> collection))
			{
				return false;
			}
			return collection.Count > 0;
		}

		public Collection<MethodReference> ReadOverrides(MethodDefinition method)
		{
			InitializeOverrides();
			if (!metadata.TryGetOverrideMapping(method, out Collection<MetadataToken> collection))
			{
				return new Collection<MethodReference>();
			}
			Collection<MethodReference> collection2 = new Collection<MethodReference>(collection.Count);
			context = method;
			for (int i = 0; i < collection.Count; i++)
			{
				collection2.Add((MethodReference)LookupToken(collection[i]));
			}
			metadata.RemoveOverrideMapping(method);
			return collection2;
		}

		private void InitializeOverrides()
		{
			if (metadata.Overrides == null)
			{
				int num = MoveTo(Table.MethodImpl);
				metadata.Overrides = new Dictionary<uint, Collection<MetadataToken>>(num);
				int num2 = 1;
				while (true)
				{
					if (num2 <= num)
					{
						ReadTableIndex(Table.TypeDef);
						MetadataToken metadataToken = ReadMetadataToken(CodedIndex.MethodDefOrRef);
						if (metadataToken.TokenType == TokenType.Method)
						{
							MetadataToken @override = ReadMetadataToken(CodedIndex.MethodDefOrRef);
							AddOverrideMapping(metadataToken.RID, @override);
							num2++;
							continue;
						}
						break;
					}
					return;
				}
				throw new NotSupportedException();
			}
		}

		private void AddOverrideMapping(uint method_rid, MetadataToken @override)
		{
			metadata.SetOverrideMapping(method_rid, AddMapping(metadata.Overrides, method_rid, @override));
		}

		public MethodBody ReadMethodBody(MethodDefinition method)
		{
			return code.ReadMethodBody(method);
		}

		public int ReadCodeSize(MethodDefinition method)
		{
			return code.ReadCodeSize(method);
		}

		public CallSite ReadCallSite(MetadataToken token)
		{
			if (!MoveTo(Table.StandAloneSig, token.RID))
			{
				return null;
			}
			uint signature = ReadBlobIndex();
			CallSite callSite = new CallSite();
			ReadMethodSignature(signature, callSite);
			callSite.MetadataToken = token;
			return callSite;
		}

		public VariableDefinitionCollection ReadVariables(MetadataToken local_var_token)
		{
			if (!MoveTo(Table.StandAloneSig, local_var_token.RID))
			{
				return null;
			}
			SignatureReader signatureReader = ReadSignature(ReadBlobIndex());
			if (signatureReader.ReadByte() != 7)
			{
				throw new NotSupportedException();
			}
			uint num = signatureReader.ReadCompressedUInt32();
			if (num == 0)
			{
				return null;
			}
			VariableDefinitionCollection variableDefinitionCollection = new VariableDefinitionCollection((int)num);
			for (int i = 0; i < num; i++)
			{
				variableDefinitionCollection.Add(new VariableDefinition(signatureReader.ReadTypeSignature()));
			}
			return variableDefinitionCollection;
		}

		public IMetadataTokenProvider LookupToken(MetadataToken token)
		{
			uint rID = token.RID;
			if (rID == 0)
			{
				return null;
			}
			if (metadata_reader != null)
			{
				return metadata_reader.LookupToken(token);
			}
			int position = base.position;
			IGenericContext genericContext = context;
			IMetadataTokenProvider result;
			switch (token.TokenType)
			{
			case TokenType.TypeDef:
				result = GetTypeDefinition(rID);
				break;
			case TokenType.TypeRef:
				result = GetTypeReference(rID);
				break;
			case TokenType.TypeSpec:
				result = GetTypeSpecification(rID);
				break;
			case TokenType.Field:
				result = GetFieldDefinition(rID);
				break;
			case TokenType.Method:
				result = GetMethodDefinition(rID);
				break;
			case TokenType.MemberRef:
				result = GetMemberReference(rID);
				break;
			case TokenType.MethodSpec:
				result = GetMethodSpecification(rID);
				break;
			default:
				return null;
			}
			base.position = position;
			context = genericContext;
			return result;
		}

		public FieldDefinition GetFieldDefinition(uint rid)
		{
			InitializeTypeDefinitions();
			FieldDefinition fieldDefinition = metadata.GetFieldDefinition(rid);
			if (fieldDefinition != null)
			{
				return fieldDefinition;
			}
			return LookupField(rid);
		}

		private FieldDefinition LookupField(uint rid)
		{
			TypeDefinition fieldDeclaringType = metadata.GetFieldDeclaringType(rid);
			if (fieldDeclaringType == null)
			{
				return null;
			}
			Mixin.Read(fieldDeclaringType.Fields);
			return metadata.GetFieldDefinition(rid);
		}

		public MethodDefinition GetMethodDefinition(uint rid)
		{
			InitializeTypeDefinitions();
			MethodDefinition methodDefinition = metadata.GetMethodDefinition(rid);
			if (methodDefinition != null)
			{
				return methodDefinition;
			}
			return LookupMethod(rid);
		}

		private MethodDefinition LookupMethod(uint rid)
		{
			TypeDefinition methodDeclaringType = metadata.GetMethodDeclaringType(rid);
			if (methodDeclaringType == null)
			{
				return null;
			}
			Mixin.Read(methodDeclaringType.Methods);
			return metadata.GetMethodDefinition(rid);
		}

		private MethodSpecification GetMethodSpecification(uint rid)
		{
			if (!MoveTo(Table.MethodSpec, rid))
			{
				return null;
			}
			MethodReference method = (MethodReference)LookupToken(ReadMetadataToken(CodedIndex.MethodDefOrRef));
			uint signature = ReadBlobIndex();
			MethodSpecification methodSpecification = ReadMethodSpecSignature(signature, method);
			methodSpecification.token = new MetadataToken(TokenType.MethodSpec, rid);
			return methodSpecification;
		}

		private MethodSpecification ReadMethodSpecSignature(uint signature, MethodReference method)
		{
			SignatureReader signatureReader = ReadSignature(signature);
			if (signatureReader.ReadByte() != 10)
			{
				throw new NotSupportedException();
			}
			GenericInstanceMethod genericInstanceMethod = new GenericInstanceMethod(method);
			signatureReader.ReadGenericInstanceSignature(method, genericInstanceMethod);
			return genericInstanceMethod;
		}

		private MemberReference GetMemberReference(uint rid)
		{
			InitializeMemberReferences();
			MemberReference memberReference = metadata.GetMemberReference(rid);
			if (memberReference != null)
			{
				return memberReference;
			}
			memberReference = ReadMemberReference(rid);
			if (memberReference != null && !memberReference.ContainsGenericParameter)
			{
				metadata.AddMemberReference(memberReference);
			}
			return memberReference;
		}

		private MemberReference ReadMemberReference(uint rid)
		{
			if (!MoveTo(Table.MemberRef, rid))
			{
				return null;
			}
			MetadataToken metadataToken = ReadMetadataToken(CodedIndex.MemberRefParent);
			string name = ReadString();
			uint signature = ReadBlobIndex();
			MemberReference memberReference;
			switch (metadataToken.TokenType)
			{
			case TokenType.TypeRef:
			case TokenType.TypeDef:
			case TokenType.TypeSpec:
				memberReference = ReadTypeMemberReference(metadataToken, name, signature);
				break;
			case TokenType.Method:
				memberReference = ReadMethodMemberReference(metadataToken, name, signature);
				break;
			default:
				throw new NotSupportedException();
			}
			memberReference.token = new MetadataToken(TokenType.MemberRef, rid);
			if (module.IsWindowsMetadata())
			{
				WindowsRuntimeProjections.Project(memberReference);
			}
			return memberReference;
		}

		private MemberReference ReadTypeMemberReference(MetadataToken type, string name, uint signature)
		{
			TypeReference typeDefOrRef = GetTypeDefOrRef(type);
			if (!typeDefOrRef.IsArray)
			{
				context = typeDefOrRef;
			}
			MemberReference memberReference = ReadMemberReferenceSignature(signature, typeDefOrRef);
			memberReference.Name = name;
			return memberReference;
		}

		private MemberReference ReadMemberReferenceSignature(uint signature, TypeReference declaring_type)
		{
			SignatureReader signatureReader = ReadSignature(signature);
			if (signatureReader.buffer[signatureReader.position] == 6)
			{
				signatureReader.position++;
				return new FieldReference
				{
					DeclaringType = declaring_type,
					FieldType = signatureReader.ReadTypeSignature()
				};
			}
			MethodReference methodReference = new MethodReference();
			methodReference.DeclaringType = declaring_type;
			signatureReader.ReadMethodSignature(methodReference);
			return methodReference;
		}

		private MemberReference ReadMethodMemberReference(MetadataToken token, string name, uint signature)
		{
			MemberReference memberReference = ReadMemberReferenceSignature(signature, ((MethodDefinition)(context = GetMethodDefinition(token.RID))).DeclaringType);
			memberReference.Name = name;
			return memberReference;
		}

		private void InitializeMemberReferences()
		{
			if (metadata.MemberReferences == null)
			{
				metadata.MemberReferences = new MemberReference[image.GetTableLength(Table.MemberRef)];
			}
		}

		public IEnumerable<MemberReference> GetMemberReferences()
		{
			InitializeMemberReferences();
			int tableLength = image.GetTableLength(Table.MemberRef);
			TypeSystem typeSystem = module.TypeSystem;
			MethodDefinition methodDefinition = new MethodDefinition(string.Empty, MethodAttributes.Static, typeSystem.Void);
			methodDefinition.DeclaringType = new TypeDefinition(string.Empty, string.Empty, TypeAttributes.Public);
			MemberReference[] array = new MemberReference[tableLength];
			for (uint num = 1u; num <= tableLength; num++)
			{
				context = methodDefinition;
				array[num - 1] = GetMemberReference(num);
			}
			return array;
		}

		private void InitializeConstants()
		{
			if (metadata.Constants == null)
			{
				int num = MoveTo(Table.Constant);
				Dictionary<MetadataToken, Row<ElementType, uint>> dictionary = metadata.Constants = new Dictionary<MetadataToken, Row<ElementType, uint>>(num);
				for (uint num2 = 1u; num2 <= num; num2++)
				{
					ElementType col = (ElementType)base.ReadUInt16();
					MetadataToken key = ReadMetadataToken(CodedIndex.HasConstant);
					uint col2 = ReadBlobIndex();
					dictionary.Add(key, new Row<ElementType, uint>(col, col2));
				}
			}
		}

		public TypeReference ReadConstantSignature(MetadataToken token)
		{
			if (token.TokenType != TokenType.Signature)
			{
				throw new NotSupportedException();
			}
			if (!MoveTo(Table.StandAloneSig, token.RID))
			{
				return null;
			}
			return ReadFieldType(ReadBlobIndex());
		}

		public object ReadConstant(IConstantProvider owner)
		{
			InitializeConstants();
			if (!metadata.Constants.TryGetValue(owner.MetadataToken, out Row<ElementType, uint> row))
			{
				return Mixin.NoValue;
			}
			metadata.Constants.Remove(owner.MetadataToken);
			return ReadConstantValue(row.Col1, row.Col2);
		}

		private object ReadConstantValue(ElementType etype, uint signature)
		{
			switch (etype)
			{
			case ElementType.Class:
			case ElementType.Object:
				return null;
			case ElementType.String:
				return ReadConstantString(signature);
			default:
				return ReadConstantPrimitive(etype, signature);
			}
		}

		private string ReadConstantString(uint signature)
		{
			GetBlobView(signature, out byte[] bytes, out int index, out int num);
			if (num == 0)
			{
				return string.Empty;
			}
			if ((num & 1) == 1)
			{
				num--;
			}
			return Encoding.Unicode.GetString(bytes, index, num);
		}

		private object ReadConstantPrimitive(ElementType type, uint signature)
		{
			return ReadSignature(signature).ReadConstantSignature(type);
		}

		internal void InitializeCustomAttributes()
		{
			if (metadata.CustomAttributes == null)
			{
				metadata.CustomAttributes = InitializeRanges(Table.CustomAttribute, delegate
				{
					MetadataToken result = ReadMetadataToken(CodedIndex.HasCustomAttribute);
					ReadMetadataToken(CodedIndex.CustomAttributeType);
					ReadBlobIndex();
					return result;
				});
			}
		}

		public bool HasCustomAttributes(ICustomAttributeProvider owner)
		{
			InitializeCustomAttributes();
			if (!metadata.TryGetCustomAttributeRanges(owner, out Range[] ranges))
			{
				return false;
			}
			return RangesSize(ranges) > 0;
		}

		public Collection<CustomAttribute> ReadCustomAttributes(ICustomAttributeProvider owner)
		{
			InitializeCustomAttributes();
			if (!metadata.TryGetCustomAttributeRanges(owner, out Range[] array))
			{
				return new Collection<CustomAttribute>();
			}
			Collection<CustomAttribute> collection = new Collection<CustomAttribute>(RangesSize(array));
			for (int i = 0; i < array.Length; i++)
			{
				ReadCustomAttributeRange(array[i], collection);
			}
			metadata.RemoveCustomAttributeRange(owner);
			if (module.IsWindowsMetadata())
			{
				{
					foreach (CustomAttribute item in collection)
					{
						WindowsRuntimeProjections.Project(owner, item);
					}
					return collection;
				}
			}
			return collection;
		}

		private void ReadCustomAttributeRange(Range range, Collection<CustomAttribute> custom_attributes)
		{
			if (MoveTo(Table.CustomAttribute, range.Start))
			{
				for (int i = 0; i < range.Length; i++)
				{
					ReadMetadataToken(CodedIndex.HasCustomAttribute);
					MethodReference constructor = (MethodReference)LookupToken(ReadMetadataToken(CodedIndex.CustomAttributeType));
					uint signature = ReadBlobIndex();
					custom_attributes.Add(new CustomAttribute(signature, constructor));
				}
			}
		}

		private static int RangesSize(Range[] ranges)
		{
			uint num = 0u;
			for (int i = 0; i < ranges.Length; i++)
			{
				num += ranges[i].Length;
			}
			return (int)num;
		}

		public IEnumerable<CustomAttribute> GetCustomAttributes()
		{
			InitializeTypeDefinitions();
			uint length = image.TableHeap[Table.CustomAttribute].Length;
			Collection<CustomAttribute> collection = new Collection<CustomAttribute>((int)length);
			ReadCustomAttributeRange(new Range(1u, length), collection);
			return collection;
		}

		public byte[] ReadCustomAttributeBlob(uint signature)
		{
			return ReadBlob(signature);
		}

		public void ReadCustomAttributeSignature(CustomAttribute attribute)
		{
			SignatureReader signatureReader = ReadSignature(attribute.signature);
			if (signatureReader.CanReadMore())
			{
				if (signatureReader.ReadUInt16() != 1)
				{
					throw new InvalidOperationException();
				}
				MethodReference constructor = attribute.Constructor;
				if (constructor.HasParameters)
				{
					signatureReader.ReadCustomAttributeConstructorArguments(attribute, constructor.Parameters);
				}
				if (signatureReader.CanReadMore())
				{
					ushort num = signatureReader.ReadUInt16();
					if (num != 0)
					{
						signatureReader.ReadCustomAttributeNamedArguments(num, ref attribute.fields, ref attribute.properties);
					}
				}
			}
		}

		private void InitializeMarshalInfos()
		{
			if (metadata.FieldMarshals == null)
			{
				int num = MoveTo(Table.FieldMarshal);
				Dictionary<MetadataToken, uint> dictionary = metadata.FieldMarshals = new Dictionary<MetadataToken, uint>(num);
				for (int i = 0; i < num; i++)
				{
					MetadataToken key = ReadMetadataToken(CodedIndex.HasFieldMarshal);
					uint value = ReadBlobIndex();
					if (key.RID != 0)
					{
						dictionary.Add(key, value);
					}
				}
			}
		}

		public bool HasMarshalInfo(IMarshalInfoProvider owner)
		{
			InitializeMarshalInfos();
			return metadata.FieldMarshals.ContainsKey(owner.MetadataToken);
		}

		public MarshalInfo ReadMarshalInfo(IMarshalInfoProvider owner)
		{
			InitializeMarshalInfos();
			if (!metadata.FieldMarshals.TryGetValue(owner.MetadataToken, out uint signature))
			{
				return null;
			}
			SignatureReader signatureReader = ReadSignature(signature);
			metadata.FieldMarshals.Remove(owner.MetadataToken);
			return signatureReader.ReadMarshalInfo();
		}

		private void InitializeSecurityDeclarations()
		{
			if (metadata.SecurityDeclarations == null)
			{
				metadata.SecurityDeclarations = InitializeRanges(Table.DeclSecurity, delegate
				{
					base.ReadUInt16();
					MetadataToken result = ReadMetadataToken(CodedIndex.HasDeclSecurity);
					ReadBlobIndex();
					return result;
				});
			}
		}

		public bool HasSecurityDeclarations(ISecurityDeclarationProvider owner)
		{
			InitializeSecurityDeclarations();
			if (!metadata.TryGetSecurityDeclarationRanges(owner, out Range[] ranges))
			{
				return false;
			}
			return RangesSize(ranges) > 0;
		}

		public Collection<SecurityDeclaration> ReadSecurityDeclarations(ISecurityDeclarationProvider owner)
		{
			InitializeSecurityDeclarations();
			if (!metadata.TryGetSecurityDeclarationRanges(owner, out Range[] array))
			{
				return new Collection<SecurityDeclaration>();
			}
			Collection<SecurityDeclaration> collection = new Collection<SecurityDeclaration>(RangesSize(array));
			for (int i = 0; i < array.Length; i++)
			{
				ReadSecurityDeclarationRange(array[i], collection);
			}
			metadata.RemoveSecurityDeclarationRange(owner);
			return collection;
		}

		private void ReadSecurityDeclarationRange(Range range, Collection<SecurityDeclaration> security_declarations)
		{
			if (MoveTo(Table.DeclSecurity, range.Start))
			{
				for (int i = 0; i < range.Length; i++)
				{
					SecurityAction action = (SecurityAction)base.ReadUInt16();
					ReadMetadataToken(CodedIndex.HasDeclSecurity);
					uint signature = ReadBlobIndex();
					security_declarations.Add(new SecurityDeclaration(action, signature, module));
				}
			}
		}

		public byte[] ReadSecurityDeclarationBlob(uint signature)
		{
			return ReadBlob(signature);
		}

		public void ReadSecurityDeclarationSignature(SecurityDeclaration declaration)
		{
			uint signature = declaration.signature;
			SignatureReader signatureReader = ReadSignature(signature);
			if (signatureReader.buffer[signatureReader.position] != 46)
			{
				ReadXmlSecurityDeclaration(signature, declaration);
			}
			else
			{
				signatureReader.position++;
				uint num = signatureReader.ReadCompressedUInt32();
				Collection<SecurityAttribute> collection = new Collection<SecurityAttribute>((int)num);
				for (int i = 0; i < num; i++)
				{
					collection.Add(signatureReader.ReadSecurityAttribute());
				}
				declaration.security_attributes = collection;
			}
		}

		private void ReadXmlSecurityDeclaration(uint signature, SecurityDeclaration declaration)
		{
			Collection<SecurityAttribute> collection = new Collection<SecurityAttribute>(1);
			SecurityAttribute securityAttribute = new SecurityAttribute(module.TypeSystem.LookupType("System.Security.Permissions", "PermissionSetAttribute"));
			securityAttribute.properties = new Collection<CustomAttributeNamedArgument>(1);
			securityAttribute.properties.Add(new CustomAttributeNamedArgument("XML", new CustomAttributeArgument(module.TypeSystem.String, ReadUnicodeStringBlob(signature))));
			collection.Add(securityAttribute);
			declaration.security_attributes = collection;
		}

		public Collection<ExportedType> ReadExportedTypes()
		{
			int num = MoveTo(Table.ExportedType);
			if (num == 0)
			{
				return new Collection<ExportedType>();
			}
			Collection<ExportedType> collection = new Collection<ExportedType>(num);
			for (int i = 1; i <= num; i++)
			{
				TypeAttributes attributes = (TypeAttributes)base.ReadUInt32();
				uint identifier = base.ReadUInt32();
				string name = ReadString();
				string @namespace = ReadString();
				MetadataToken token = ReadMetadataToken(CodedIndex.Implementation);
				ExportedType declaringType = null;
				IMetadataScope scope = null;
				switch (token.TokenType)
				{
				case TokenType.AssemblyRef:
				case TokenType.File:
					scope = GetExportedTypeScope(token);
					break;
				case TokenType.ExportedType:
					declaringType = collection[(int)(token.RID - 1)];
					break;
				}
				ExportedType exportedType = new ExportedType(@namespace, name, module, scope)
				{
					Attributes = attributes,
					Identifier = (int)identifier,
					DeclaringType = declaringType
				};
				exportedType.token = new MetadataToken(TokenType.ExportedType, i);
				collection.Add(exportedType);
			}
			return collection;
		}

		private IMetadataScope GetExportedTypeScope(MetadataToken token)
		{
			int position = base.position;
			IMetadataScope result;
			switch (token.TokenType)
			{
			case TokenType.AssemblyRef:
				InitializeAssemblyReferences();
				result = metadata.GetAssemblyNameReference(token.RID);
				break;
			case TokenType.File:
				InitializeModuleReferences();
				result = GetModuleReferenceFromFile(token);
				break;
			default:
				throw new NotSupportedException();
			}
			base.position = position;
			return result;
		}

		private ModuleReference GetModuleReferenceFromFile(MetadataToken token)
		{
			if (!MoveTo(Table.File, token.RID))
			{
				return null;
			}
			base.ReadUInt32();
			string text = ReadString();
			Collection<ModuleReference> moduleReferences = module.ModuleReferences;
			ModuleReference moduleReference;
			for (int i = 0; i < moduleReferences.Count; i++)
			{
				moduleReference = moduleReferences[i];
				if (moduleReference.Name == text)
				{
					return moduleReference;
				}
			}
			moduleReference = new ModuleReference(text);
			moduleReferences.Add(moduleReference);
			return moduleReference;
		}

		private void InitializeDocuments()
		{
			if (metadata.Documents == null)
			{
				int num = MoveTo(Table.Document);
				Document[] array = metadata.Documents = new Document[num];
				for (uint num2 = 1u; num2 <= num; num2++)
				{
					uint signature = ReadBlobIndex();
					Guid hashAlgorithmGuid = ReadGuid();
					byte[] hash = ReadBlob();
					Guid languageGuid = ReadGuid();
					string url = ReadSignature(signature).ReadDocumentName();
					array[num2 - 1] = new Document(url)
					{
						HashAlgorithmGuid = hashAlgorithmGuid,
						Hash = hash,
						LanguageGuid = languageGuid,
						token = new MetadataToken(TokenType.Document, num2)
					};
				}
			}
		}

		public Collection<SequencePoint> ReadSequencePoints(MethodDefinition method)
		{
			InitializeDocuments();
			if (!MoveTo(Table.MethodDebugInformation, method.MetadataToken.RID))
			{
				return new Collection<SequencePoint>(0);
			}
			uint rid = ReadTableIndex(Table.Document);
			uint num = ReadBlobIndex();
			if (num == 0)
			{
				return new Collection<SequencePoint>(0);
			}
			Document document = GetDocument(rid);
			return ReadSignature(num).ReadSequencePoints(document);
		}

		public Document GetDocument(uint rid)
		{
			Document document = metadata.GetDocument(rid);
			if (document == null)
			{
				return null;
			}
			document.custom_infos = GetCustomDebugInformation(document);
			return document;
		}

		private void InitializeLocalScopes()
		{
			if (metadata.LocalScopes == null)
			{
				InitializeMethods();
				int num = MoveTo(Table.LocalScope);
				metadata.LocalScopes = new Dictionary<uint, Collection<Row<uint, Range, Range, uint, uint, uint>>>();
				for (uint num2 = 1u; num2 <= num; num2++)
				{
					uint num3 = ReadTableIndex(Table.Method);
					uint col = ReadTableIndex(Table.ImportScope);
					Range col2 = ReadListRange(num2, Table.LocalScope, Table.LocalVariable);
					Range col3 = ReadListRange(num2, Table.LocalScope, Table.LocalConstant);
					uint col4 = base.ReadUInt32();
					uint col5 = base.ReadUInt32();
					metadata.SetLocalScopes(num3, AddMapping(metadata.LocalScopes, num3, new Row<uint, Range, Range, uint, uint, uint>(col, col2, col3, col4, col5, num2)));
				}
			}
		}

		public ScopeDebugInformation ReadScope(MethodDefinition method)
		{
			InitializeLocalScopes();
			InitializeImportScopes();
			if (!metadata.TryGetLocalScopes(method, out Collection<Row<uint, Range, Range, uint, uint, uint>> collection))
			{
				return null;
			}
			ScopeDebugInformation scopeDebugInformation = null;
			for (int i = 0; i < collection.Count; i++)
			{
				ScopeDebugInformation scopeDebugInformation2 = ReadLocalScope(collection[i]);
				if (i == 0)
				{
					scopeDebugInformation = scopeDebugInformation2;
				}
				else if (!AddScope(scopeDebugInformation.scopes, scopeDebugInformation2))
				{
					scopeDebugInformation.Scopes.Add(scopeDebugInformation2);
				}
			}
			return scopeDebugInformation;
		}

		private static bool AddScope(Collection<ScopeDebugInformation> scopes, ScopeDebugInformation scope)
		{
			if (scopes.IsNullOrEmpty())
			{
				return false;
			}
			foreach (ScopeDebugInformation scope2 in scopes)
			{
				if (scope2.HasScopes && AddScope(scope2.Scopes, scope))
				{
					return true;
				}
				InstructionOffset instructionOffset = scope.Start;
				int offset = instructionOffset.Offset;
				instructionOffset = scope2.Start;
				if (offset >= instructionOffset.Offset)
				{
					instructionOffset = scope.End;
					int offset2 = instructionOffset.Offset;
					instructionOffset = scope2.End;
					if (offset2 <= instructionOffset.Offset)
					{
						scope2.Scopes.Add(scope);
						return true;
					}
				}
			}
			return false;
		}

		private ScopeDebugInformation ReadLocalScope(Row<uint, Range, Range, uint, uint, uint> record)
		{
			ScopeDebugInformation scopeDebugInformation = new ScopeDebugInformation
			{
				start = new InstructionOffset((int)record.Col4),
				end = new InstructionOffset((int)(record.Col4 + record.Col5)),
				token = new MetadataToken(TokenType.LocalScope, record.Col6)
			};
			if (record.Col1 != 0)
			{
				scopeDebugInformation.import = metadata.GetImportScope(record.Col1);
			}
			if (record.Col2.Length != 0)
			{
				scopeDebugInformation.variables = new Collection<VariableDebugInformation>((int)record.Col2.Length);
				for (uint num = 0u; num < record.Col2.Length; num++)
				{
					VariableDebugInformation variableDebugInformation = ReadLocalVariable(record.Col2.Start + num);
					if (variableDebugInformation != null)
					{
						scopeDebugInformation.variables.Add(variableDebugInformation);
					}
				}
			}
			if (record.Col3.Length != 0)
			{
				scopeDebugInformation.constants = new Collection<ConstantDebugInformation>((int)record.Col3.Length);
				for (uint num2 = 0u; num2 < record.Col3.Length; num2++)
				{
					ConstantDebugInformation constantDebugInformation = ReadLocalConstant(record.Col3.Start + num2);
					if (constantDebugInformation != null)
					{
						scopeDebugInformation.constants.Add(constantDebugInformation);
					}
				}
			}
			return scopeDebugInformation;
		}

		private VariableDebugInformation ReadLocalVariable(uint rid)
		{
			if (!MoveTo(Table.LocalVariable, rid))
			{
				return null;
			}
			VariableAttributes attributes = (VariableAttributes)base.ReadUInt16();
			ushort index = base.ReadUInt16();
			string name = ReadString();
			VariableDebugInformation variableDebugInformation = new VariableDebugInformation(index, name)
			{
				Attributes = attributes,
				token = new MetadataToken(TokenType.LocalVariable, rid)
			};
			variableDebugInformation.custom_infos = GetCustomDebugInformation(variableDebugInformation);
			return variableDebugInformation;
		}

		private ConstantDebugInformation ReadLocalConstant(uint rid)
		{
			if (!MoveTo(Table.LocalConstant, rid))
			{
				return null;
			}
			string name = ReadString();
			SignatureReader signatureReader = ReadSignature(ReadBlobIndex());
			TypeReference typeReference = signatureReader.ReadTypeSignature();
			object value;
			if (typeReference.etype == ElementType.String)
			{
				if (signatureReader.buffer[signatureReader.position] != 255)
				{
					byte[] array = signatureReader.ReadBytes((int)(signatureReader.sig_length - (signatureReader.position - signatureReader.start)));
					value = Encoding.Unicode.GetString(array, 0, array.Length);
				}
				else
				{
					value = null;
				}
			}
			else if (typeReference.IsTypeOf("System", "Decimal"))
			{
				byte b = signatureReader.ReadByte();
				value = new decimal(signatureReader.ReadInt32(), signatureReader.ReadInt32(), signatureReader.ReadInt32(), (b & 0x80) != 0, (byte)(b & 0x7F));
			}
			else
			{
				value = ((!typeReference.IsTypeOf("System", "DateTime")) ? ((typeReference.etype != ElementType.Object && typeReference.etype != 0 && typeReference.etype != ElementType.Class) ? signatureReader.ReadConstantSignature(typeReference.etype) : null) : ((object)new DateTime(signatureReader.ReadInt64())));
			}
			ConstantDebugInformation constantDebugInformation = new ConstantDebugInformation(name, typeReference, value)
			{
				token = new MetadataToken(TokenType.LocalConstant, rid)
			};
			constantDebugInformation.custom_infos = GetCustomDebugInformation(constantDebugInformation);
			return constantDebugInformation;
		}

		private void InitializeImportScopes()
		{
			if (metadata.ImportScopes == null)
			{
				int num = MoveTo(Table.ImportScope);
				metadata.ImportScopes = new ImportDebugInformation[num];
				for (int i = 1; i <= num; i++)
				{
					ReadTableIndex(Table.ImportScope);
					ImportDebugInformation importDebugInformation = new ImportDebugInformation();
					importDebugInformation.token = new MetadataToken(TokenType.ImportScope, i);
					SignatureReader signatureReader = ReadSignature(ReadBlobIndex());
					while (signatureReader.CanReadMore())
					{
						importDebugInformation.Targets.Add(ReadImportTarget(signatureReader));
					}
					metadata.ImportScopes[i - 1] = importDebugInformation;
				}
				MoveTo(Table.ImportScope);
				for (int j = 0; j < num; j++)
				{
					uint num2 = ReadTableIndex(Table.ImportScope);
					ReadBlobIndex();
					if (num2 != 0)
					{
						metadata.ImportScopes[j].Parent = metadata.GetImportScope(num2);
					}
				}
			}
		}

		public string ReadUTF8StringBlob(uint signature)
		{
			return ReadStringBlob(signature, Encoding.UTF8);
		}

		private string ReadUnicodeStringBlob(uint signature)
		{
			return ReadStringBlob(signature, Encoding.Unicode);
		}

		private string ReadStringBlob(uint signature, Encoding encoding)
		{
			GetBlobView(signature, out byte[] bytes, out int index, out int num);
			if (num == 0)
			{
				return string.Empty;
			}
			return encoding.GetString(bytes, index, num);
		}

		private ImportTarget ReadImportTarget(SignatureReader signature)
		{
			AssemblyNameReference reference = null;
			string @namespace = null;
			string alias = null;
			TypeReference type = null;
			ImportTargetKind importTargetKind = (ImportTargetKind)signature.ReadCompressedUInt32();
			switch (importTargetKind)
			{
			case ImportTargetKind.ImportNamespace:
				@namespace = ReadUTF8StringBlob(signature.ReadCompressedUInt32());
				break;
			case ImportTargetKind.ImportNamespaceInAssembly:
				reference = metadata.GetAssemblyNameReference(signature.ReadCompressedUInt32());
				@namespace = ReadUTF8StringBlob(signature.ReadCompressedUInt32());
				break;
			case ImportTargetKind.ImportType:
				type = signature.ReadTypeToken();
				break;
			case ImportTargetKind.ImportXmlNamespaceWithAlias:
				alias = ReadUTF8StringBlob(signature.ReadCompressedUInt32());
				@namespace = ReadUTF8StringBlob(signature.ReadCompressedUInt32());
				break;
			case ImportTargetKind.ImportAlias:
				alias = ReadUTF8StringBlob(signature.ReadCompressedUInt32());
				break;
			case ImportTargetKind.DefineAssemblyAlias:
				alias = ReadUTF8StringBlob(signature.ReadCompressedUInt32());
				reference = metadata.GetAssemblyNameReference(signature.ReadCompressedUInt32());
				break;
			case ImportTargetKind.DefineNamespaceAlias:
				alias = ReadUTF8StringBlob(signature.ReadCompressedUInt32());
				@namespace = ReadUTF8StringBlob(signature.ReadCompressedUInt32());
				break;
			case ImportTargetKind.DefineNamespaceInAssemblyAlias:
				alias = ReadUTF8StringBlob(signature.ReadCompressedUInt32());
				reference = metadata.GetAssemblyNameReference(signature.ReadCompressedUInt32());
				@namespace = ReadUTF8StringBlob(signature.ReadCompressedUInt32());
				break;
			case ImportTargetKind.DefineTypeAlias:
				alias = ReadUTF8StringBlob(signature.ReadCompressedUInt32());
				type = signature.ReadTypeToken();
				break;
			}
			return new ImportTarget(importTargetKind)
			{
				alias = alias,
				type = type,
				@namespace = @namespace,
				reference = reference
			};
		}

		private void InitializeStateMachineMethods()
		{
			if (metadata.StateMachineMethods == null)
			{
				int num = MoveTo(Table.StateMachineMethod);
				metadata.StateMachineMethods = new Dictionary<uint, uint>(num);
				for (int i = 0; i < num; i++)
				{
					metadata.StateMachineMethods.Add(ReadTableIndex(Table.Method), ReadTableIndex(Table.Method));
				}
			}
		}

		public MethodDefinition ReadStateMachineKickoffMethod(MethodDefinition method)
		{
			InitializeStateMachineMethods();
			if (!metadata.TryGetStateMachineKickOffMethod(method, out uint rid))
			{
				return null;
			}
			return GetMethodDefinition(rid);
		}

		private void InitializeCustomDebugInformations()
		{
			if (metadata.CustomDebugInformations == null)
			{
				int num = MoveTo(Table.CustomDebugInformation);
				metadata.CustomDebugInformations = new Dictionary<MetadataToken, Row<Guid, uint, uint>[]>();
				for (uint num2 = 1u; num2 <= num; num2++)
				{
					MetadataToken key = ReadMetadataToken(CodedIndex.HasCustomDebugInformation);
					Row<Guid, uint, uint> item = new Row<Guid, uint, uint>(ReadGuid(), ReadBlobIndex(), num2);
					metadata.CustomDebugInformations.TryGetValue(key, out Row<Guid, uint, uint>[] self);
					metadata.CustomDebugInformations[key] = self.Add(item);
				}
			}
		}

		public Collection<CustomDebugInformation> GetCustomDebugInformation(ICustomDebugInformationProvider provider)
		{
			InitializeCustomDebugInformations();
			if (!metadata.CustomDebugInformations.TryGetValue(provider.MetadataToken, out Row<Guid, uint, uint>[] array))
			{
				return null;
			}
			Collection<CustomDebugInformation> collection = new Collection<CustomDebugInformation>(array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Col1 == StateMachineScopeDebugInformation.KindIdentifier)
				{
					SignatureReader signatureReader = ReadSignature(array[i].Col2);
					Collection<StateMachineScope> collection2 = new Collection<StateMachineScope>();
					while (signatureReader.CanReadMore())
					{
						int num = signatureReader.ReadInt32();
						int end = num + signatureReader.ReadInt32();
						collection2.Add(new StateMachineScope(num, end));
					}
					StateMachineScopeDebugInformation stateMachineScopeDebugInformation = new StateMachineScopeDebugInformation();
					stateMachineScopeDebugInformation.scopes = collection2;
					collection.Add(stateMachineScopeDebugInformation);
				}
				else if (array[i].Col1 == AsyncMethodBodyDebugInformation.KindIdentifier)
				{
					SignatureReader signatureReader2 = ReadSignature(array[i].Col2);
					int catchHandler = signatureReader2.ReadInt32() - 1;
					Collection<InstructionOffset> collection3 = new Collection<InstructionOffset>();
					Collection<InstructionOffset> collection4 = new Collection<InstructionOffset>();
					Collection<MethodDefinition> collection5 = new Collection<MethodDefinition>();
					while (signatureReader2.CanReadMore())
					{
						collection3.Add(new InstructionOffset(signatureReader2.ReadInt32()));
						collection4.Add(new InstructionOffset(signatureReader2.ReadInt32()));
						collection5.Add(GetMethodDefinition(signatureReader2.ReadCompressedUInt32()));
					}
					AsyncMethodBodyDebugInformation asyncMethodBodyDebugInformation = new AsyncMethodBodyDebugInformation(catchHandler);
					asyncMethodBodyDebugInformation.yields = collection3;
					asyncMethodBodyDebugInformation.resumes = collection4;
					asyncMethodBodyDebugInformation.resume_methods = collection5;
					collection.Add(asyncMethodBodyDebugInformation);
				}
				else if (array[i].Col1 == EmbeddedSourceDebugInformation.KindIdentifier)
				{
					SignatureReader signatureReader3 = ReadSignature(array[i].Col2);
					int num2 = signatureReader3.ReadInt32();
					uint length = signatureReader3.sig_length - 4;
					CustomDebugInformation item = null;
					if (num2 == 0)
					{
						item = new EmbeddedSourceDebugInformation(signatureReader3.ReadBytes((int)length), false);
					}
					else if (num2 > 0)
					{
						MemoryStream stream = new MemoryStream(signatureReader3.ReadBytes((int)length));
						byte[] array2 = new byte[num2];
						MemoryStream destination = new MemoryStream(array2);
						using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress, true))
						{
							deflateStream.CopyTo(destination);
						}
						item = new EmbeddedSourceDebugInformation(array2, true);
					}
					else if (num2 < 0)
					{
						item = new BinaryCustomDebugInformation(array[i].Col1, ReadBlob(array[i].Col2));
					}
					collection.Add(item);
				}
				else if (array[i].Col1 == SourceLinkDebugInformation.KindIdentifier)
				{
					collection.Add(new SourceLinkDebugInformation(Encoding.UTF8.GetString(ReadBlob(array[i].Col2))));
				}
				else
				{
					collection.Add(new BinaryCustomDebugInformation(array[i].Col1, ReadBlob(array[i].Col2)));
				}
				collection[i].token = new MetadataToken(TokenType.CustomDebugInformation, array[i].Col3);
			}
			return collection;
		}
	}
}
