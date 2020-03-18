using Mono.Cecil.Cil;
using Mono.Cecil.Metadata;
using Mono.Collections.Generic;
using Mono.Security.Cryptography;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Cecil
{
	internal static class Mixin
	{
		public enum Argument
		{
			name,
			fileName,
			fullName,
			stream,
			type,
			method,
			field,
			parameters,
			module,
			modifierType,
			eventType,
			fieldType,
			declaringType,
			returnType,
			propertyType,
			interfaceType
		}

		public static Version ZeroVersion = new Version(0, 0, 0, 0);

		public const int NotResolvedMarker = -2;

		public const int NoDataMarker = -1;

		internal static object NoValue = new object();

		internal static object NotResolved = new object();

		public const string mscorlib = "mscorlib";

		public const string system_runtime = "System.Runtime";

		public const string system_private_corelib = "System.Private.CoreLib";

		public const string netstandard = "netstandard";

		public const int TableCount = 58;

		public const int CodedIndexCount = 14;

		public static bool IsNullOrEmpty<T>(this T[] self)
		{
			if (self != null)
			{
				return self.Length == 0;
			}
			return true;
		}

		public static bool IsNullOrEmpty<T>(this Collection<T> self)
		{
			if (self != null)
			{
				return self.size == 0;
			}
			return true;
		}

		public static T[] Resize<T>(this T[] self, int length)
		{
			Array.Resize(ref self, length);
			return self;
		}

		public static T[] Add<T>(this T[] self, T item)
		{
			if (self == null)
			{
				self = new T[1]
				{
					item
				};
				return self;
			}
			self = self.Resize(self.Length + 1);
			self[self.Length - 1] = item;
			return self;
		}

		public static Version CheckVersion(Version version)
		{
			if (version == (Version)null)
			{
				return ZeroVersion;
			}
			if (version.Build == -1)
			{
				return new Version(version.Major, version.Minor, 0, 0);
			}
			if (version.Revision == -1)
			{
				return new Version(version.Major, version.Minor, version.Build, 0);
			}
			return version;
		}

		public static bool TryGetUniqueDocument(this MethodDebugInformation info, out Document document)
		{
			document = info.SequencePoints[0].Document;
			for (int i = 1; i < info.SequencePoints.Count; i++)
			{
				if (info.SequencePoints[i].Document != document)
				{
					return false;
				}
			}
			return true;
		}

		public static void ResolveConstant(this IConstantProvider self, ref object constant, ModuleDefinition module)
		{
			if (module == null)
			{
				constant = NoValue;
			}
			else
			{
				lock (module.SyncRoot)
				{
					if (constant == NotResolved)
					{
						if (module.HasImage())
						{
							constant = module.Read(self, (IConstantProvider provider, MetadataReader reader) => reader.ReadConstant(provider));
						}
						else
						{
							constant = NoValue;
						}
					}
				}
			}
		}

		public static bool GetHasCustomAttributes(this ICustomAttributeProvider self, ModuleDefinition module)
		{
			if (module.HasImage())
			{
				return module.Read(self, (ICustomAttributeProvider provider, MetadataReader reader) => reader.HasCustomAttributes(provider));
			}
			return false;
		}

		public static Collection<CustomAttribute> GetCustomAttributes(this ICustomAttributeProvider self, ref Collection<CustomAttribute> variable, ModuleDefinition module)
		{
			if (!module.HasImage())
			{
				return variable = new Collection<CustomAttribute>();
			}
			return module.Read(ref variable, self, (ICustomAttributeProvider provider, MetadataReader reader) => reader.ReadCustomAttributes(provider));
		}

		public static bool ContainsGenericParameter(this IGenericInstance self)
		{
			Collection<TypeReference> genericArguments = self.GenericArguments;
			for (int i = 0; i < genericArguments.Count; i++)
			{
				if (genericArguments[i].ContainsGenericParameter)
				{
					return true;
				}
			}
			return false;
		}

		public static void GenericInstanceFullName(this IGenericInstance self, StringBuilder builder)
		{
			builder.Append("<");
			Collection<TypeReference> genericArguments = self.GenericArguments;
			for (int i = 0; i < genericArguments.Count; i++)
			{
				if (i > 0)
				{
					builder.Append(",");
				}
				builder.Append(genericArguments[i].FullName);
			}
			builder.Append(">");
		}

		public static bool GetHasGenericParameters(this IGenericParameterProvider self, ModuleDefinition module)
		{
			if (module.HasImage())
			{
				return module.Read(self, (IGenericParameterProvider provider, MetadataReader reader) => reader.HasGenericParameters(provider));
			}
			return false;
		}

		public static Collection<GenericParameter> GetGenericParameters(this IGenericParameterProvider self, ref Collection<GenericParameter> collection, ModuleDefinition module)
		{
			if (!module.HasImage())
			{
				return collection = new GenericParameterCollection(self);
			}
			return module.Read(ref collection, self, (IGenericParameterProvider provider, MetadataReader reader) => reader.ReadGenericParameters(provider));
		}

		public static bool GetHasMarshalInfo(this IMarshalInfoProvider self, ModuleDefinition module)
		{
			if (module.HasImage())
			{
				return module.Read(self, (IMarshalInfoProvider provider, MetadataReader reader) => reader.HasMarshalInfo(provider));
			}
			return false;
		}

		public static MarshalInfo GetMarshalInfo(this IMarshalInfoProvider self, ref MarshalInfo variable, ModuleDefinition module)
		{
			if (!module.HasImage())
			{
				return null;
			}
			return module.Read(ref variable, self, (IMarshalInfoProvider provider, MetadataReader reader) => reader.ReadMarshalInfo(provider));
		}

		public static bool GetAttributes(this uint self, uint attributes)
		{
			return (self & attributes) != 0;
		}

		public static uint SetAttributes(this uint self, uint attributes, bool value)
		{
			if (value)
			{
				return self | attributes;
			}
			return self & ~attributes;
		}

		public static bool GetMaskedAttributes(this uint self, uint mask, uint attributes)
		{
			return (self & mask) == attributes;
		}

		public static uint SetMaskedAttributes(this uint self, uint mask, uint attributes, bool value)
		{
			if (value)
			{
				self &= ~mask;
				return self | attributes;
			}
			return self & ~(mask & attributes);
		}

		public static bool GetAttributes(this ushort self, ushort attributes)
		{
			return (self & attributes) != 0;
		}

		public static ushort SetAttributes(this ushort self, ushort attributes, bool value)
		{
			if (value)
			{
				return (ushort)(self | attributes);
			}
			return (ushort)(self & ~attributes);
		}

		public static bool GetMaskedAttributes(this ushort self, ushort mask, uint attributes)
		{
			return (self & mask) == attributes;
		}

		public static ushort SetMaskedAttributes(this ushort self, ushort mask, uint attributes, bool value)
		{
			if (value)
			{
				self = (ushort)(self & ~mask);
				return (ushort)(self | attributes);
			}
			return (ushort)(self & ~(mask & attributes));
		}

		public static bool HasImplicitThis(this IMethodSignature self)
		{
			if (self.HasThis)
			{
				return !self.ExplicitThis;
			}
			return false;
		}

		public static void MethodSignatureFullName(this IMethodSignature self, StringBuilder builder)
		{
			builder.Append("(");
			if (self.HasParameters)
			{
				Collection<ParameterDefinition> parameters = self.Parameters;
				for (int i = 0; i < parameters.Count; i++)
				{
					ParameterDefinition parameterDefinition = parameters[i];
					if (i > 0)
					{
						builder.Append(",");
					}
					if (parameterDefinition.ParameterType.IsSentinel)
					{
						builder.Append("...,");
					}
					builder.Append(parameterDefinition.ParameterType.FullName);
				}
			}
			builder.Append(")");
		}

		public static void CheckModule(ModuleDefinition module)
		{
			if (module != null)
			{
				return;
			}
			throw new ArgumentNullException(8.ToString());
		}

		public static bool TryGetAssemblyNameReference(this ModuleDefinition module, AssemblyNameReference name_reference, out AssemblyNameReference assembly_reference)
		{
			Collection<AssemblyNameReference> assemblyReferences = module.AssemblyReferences;
			for (int i = 0; i < assemblyReferences.Count; i++)
			{
				AssemblyNameReference assemblyNameReference = assemblyReferences[i];
				if (Equals(name_reference, assemblyNameReference))
				{
					assembly_reference = assemblyNameReference;
					return true;
				}
			}
			assembly_reference = null;
			return false;
		}

		private static bool Equals(byte[] a, byte[] b)
		{
			if (a == b)
			{
				return true;
			}
			if (a == null)
			{
				return false;
			}
			if (a.Length != b.Length)
			{
				return false;
			}
			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] != b[i])
				{
					return false;
				}
			}
			return true;
		}

		private static bool Equals<T>(T a, T b) where T : class, IEquatable<T>
		{
			if (a == b)
			{
				return true;
			}
			return ((IEquatable<T>)a)?.Equals(b) ?? false;
		}

		private static bool Equals(AssemblyNameReference a, AssemblyNameReference b)
		{
			if (a == b)
			{
				return true;
			}
			if (a.Name != b.Name)
			{
				return false;
			}
			if (!Equals(a.Version, b.Version))
			{
				return false;
			}
			if (a.Culture != b.Culture)
			{
				return false;
			}
			if (!Equals(a.PublicKeyToken, b.PublicKeyToken))
			{
				return false;
			}
			return true;
		}

		public static ParameterDefinition GetParameter(this Mono.Cecil.Cil.MethodBody self, int index)
		{
			MethodDefinition method = self.method;
			if (method.HasThis)
			{
				if (index == 0)
				{
					return self.ThisParameter;
				}
				index--;
			}
			Collection<ParameterDefinition> parameters = method.Parameters;
			if (index >= 0 && index < parameters.size)
			{
				return parameters[index];
			}
			return null;
		}

		public static VariableDefinition GetVariable(this Mono.Cecil.Cil.MethodBody self, int index)
		{
			Collection<VariableDefinition> variables = self.Variables;
			if (index >= 0 && index < variables.size)
			{
				return variables[index];
			}
			return null;
		}

		public static bool GetSemantics(this MethodDefinition self, MethodSemanticsAttributes semantics)
		{
			return (self.SemanticsAttributes & semantics) != MethodSemanticsAttributes.None;
		}

		public static void SetSemantics(this MethodDefinition self, MethodSemanticsAttributes semantics, bool value)
		{
			if (value)
			{
				self.SemanticsAttributes |= semantics;
			}
			else
			{
				self.SemanticsAttributes &= (MethodSemanticsAttributes)(ushort)(~(uint)semantics);
			}
		}

		public static bool IsVarArg(this IMethodSignature self)
		{
			return (self.CallingConvention & MethodCallingConvention.VarArg) != MethodCallingConvention.Default;
		}

		public static int GetSentinelPosition(this IMethodSignature self)
		{
			if (!self.HasParameters)
			{
				return -1;
			}
			Collection<ParameterDefinition> parameters = self.Parameters;
			for (int i = 0; i < parameters.Count; i++)
			{
				if (parameters[i].ParameterType.IsSentinel)
				{
					return i;
				}
			}
			return -1;
		}

		public static void CheckName(object name)
		{
			if (name != null)
			{
				return;
			}
			throw new ArgumentNullException(0.ToString());
		}

		public static void CheckName(string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				return;
			}
			throw new ArgumentNullOrEmptyException(0.ToString());
		}

		public static void CheckFileName(string fileName)
		{
			if (!string.IsNullOrEmpty(fileName))
			{
				return;
			}
			throw new ArgumentNullOrEmptyException(1.ToString());
		}

		public static void CheckFullName(string fullName)
		{
			if (!string.IsNullOrEmpty(fullName))
			{
				return;
			}
			throw new ArgumentNullOrEmptyException(2.ToString());
		}

		public static void CheckStream(object stream)
		{
			if (stream != null)
			{
				return;
			}
			throw new ArgumentNullException(3.ToString());
		}

		public static void CheckWriteSeek(Stream stream)
		{
			if (stream.CanWrite && stream.CanSeek)
			{
				return;
			}
			throw new ArgumentException("Stream must be writable and seekable.");
		}

		public static void CheckReadSeek(Stream stream)
		{
			if (stream.CanRead && stream.CanSeek)
			{
				return;
			}
			throw new ArgumentException("Stream must be readable and seekable.");
		}

		public static void CheckType(object type)
		{
			if (type != null)
			{
				return;
			}
			throw new ArgumentNullException(4.ToString());
		}

		public static void CheckType(object type, Argument argument)
		{
			if (type != null)
			{
				return;
			}
			throw new ArgumentNullException(argument.ToString());
		}

		public static void CheckField(object field)
		{
			if (field != null)
			{
				return;
			}
			throw new ArgumentNullException(6.ToString());
		}

		public static void CheckMethod(object method)
		{
			if (method != null)
			{
				return;
			}
			throw new ArgumentNullException(5.ToString());
		}

		public static void CheckParameters(object parameters)
		{
			if (parameters != null)
			{
				return;
			}
			throw new ArgumentNullException(7.ToString());
		}

		public static uint GetTimestamp()
		{
			return (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
		}

		public static bool HasImage(this ModuleDefinition self)
		{
			return self?.HasImage ?? false;
		}

		public static string GetFileName(this Stream self)
		{
			FileStream fileStream = self as FileStream;
			if (fileStream == null)
			{
				return string.Empty;
			}
			return Path.GetFullPath(fileStream.Name);
		}

		public static TargetRuntime ParseRuntime(this string self)
		{
			if (string.IsNullOrEmpty(self))
			{
				return TargetRuntime.Net_4_0;
			}
			switch (self[1])
			{
			case '1':
				if (self[3] != '0')
				{
					return TargetRuntime.Net_1_1;
				}
				return TargetRuntime.Net_1_0;
			case '2':
				return TargetRuntime.Net_2_0;
			default:
				return TargetRuntime.Net_4_0;
			}
		}

		public static string RuntimeVersionString(this TargetRuntime runtime)
		{
			switch (runtime)
			{
			case TargetRuntime.Net_1_0:
				return "v1.0.3705";
			case TargetRuntime.Net_1_1:
				return "v1.1.4322";
			case TargetRuntime.Net_2_0:
				return "v2.0.50727";
			default:
				return "v4.0.30319";
			}
		}

		public static bool IsWindowsMetadata(this ModuleDefinition module)
		{
			return module.MetadataKind != MetadataKind.Ecma335;
		}

		public static byte[] ReadAll(this Stream self)
		{
			MemoryStream memoryStream = new MemoryStream((int)self.Length);
			byte[] array = new byte[1024];
			int count;
			while ((count = self.Read(array, 0, array.Length)) != 0)
			{
				memoryStream.Write(array, 0, count);
			}
			return memoryStream.ToArray();
		}

		public static void Read(object o)
		{
		}

		public static bool GetHasSecurityDeclarations(this ISecurityDeclarationProvider self, ModuleDefinition module)
		{
			if (module.HasImage())
			{
				return module.Read(self, (ISecurityDeclarationProvider provider, MetadataReader reader) => reader.HasSecurityDeclarations(provider));
			}
			return false;
		}

		public static Collection<SecurityDeclaration> GetSecurityDeclarations(this ISecurityDeclarationProvider self, ref Collection<SecurityDeclaration> variable, ModuleDefinition module)
		{
			if (!module.HasImage())
			{
				return variable = new Collection<SecurityDeclaration>();
			}
			return module.Read(ref variable, self, (ISecurityDeclarationProvider provider, MetadataReader reader) => reader.ReadSecurityDeclarations(provider));
		}

		public static TypeReference GetEnumUnderlyingType(this TypeDefinition self)
		{
			Collection<FieldDefinition> fields = self.Fields;
			for (int i = 0; i < fields.Count; i++)
			{
				FieldDefinition fieldDefinition = fields[i];
				if (!fieldDefinition.IsStatic)
				{
					return fieldDefinition.FieldType;
				}
			}
			throw new ArgumentException();
		}

		public static TypeDefinition GetNestedType(this TypeDefinition self, string fullname)
		{
			if (!self.HasNestedTypes)
			{
				return null;
			}
			Collection<TypeDefinition> nestedTypes = self.NestedTypes;
			for (int i = 0; i < nestedTypes.Count; i++)
			{
				TypeDefinition typeDefinition = nestedTypes[i];
				if (typeDefinition.TypeFullName() == fullname)
				{
					return typeDefinition;
				}
			}
			return null;
		}

		public static bool IsPrimitive(this ElementType self)
		{
			switch (self)
			{
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
				return true;
			default:
				return false;
			}
		}

		public static string TypeFullName(this TypeReference self)
		{
			if (!string.IsNullOrEmpty(self.Namespace))
			{
				return self.Namespace + "." + self.Name;
			}
			return self.Name;
		}

		public static bool IsTypeOf(this TypeReference self, string @namespace, string name)
		{
			if (self.Name == name)
			{
				return self.Namespace == @namespace;
			}
			return false;
		}

		public static bool IsTypeSpecification(this TypeReference type)
		{
			switch (type.etype)
			{
			case ElementType.Ptr:
			case ElementType.ByRef:
			case ElementType.Var:
			case ElementType.Array:
			case ElementType.GenericInst:
			case ElementType.FnPtr:
			case ElementType.SzArray:
			case ElementType.MVar:
			case ElementType.CModReqD:
			case ElementType.CModOpt:
			case ElementType.Sentinel:
			case ElementType.Pinned:
				return true;
			default:
				return false;
			}
		}

		public static TypeDefinition CheckedResolve(this TypeReference self)
		{
			TypeDefinition typeDefinition = self.Resolve();
			if (typeDefinition == null)
			{
				throw new ResolutionException(self);
			}
			return typeDefinition;
		}

		public static bool TryGetCoreLibraryReference(this ModuleDefinition module, out AssemblyNameReference reference)
		{
			Collection<AssemblyNameReference> assemblyReferences = module.AssemblyReferences;
			for (int i = 0; i < assemblyReferences.Count; i++)
			{
				reference = assemblyReferences[i];
				if (IsCoreLibrary(reference))
				{
					return true;
				}
			}
			reference = null;
			return false;
		}

		public static bool IsCoreLibrary(this ModuleDefinition module)
		{
			if (module.Assembly == null)
			{
				return false;
			}
			if (!IsCoreLibrary(module.Assembly.Name))
			{
				return false;
			}
			if (module.HasImage && module.Read(module, (ModuleDefinition m, MetadataReader reader) => reader.image.GetTableLength(Table.AssemblyRef) > 0))
			{
				return false;
			}
			return true;
		}

		public static void KnownValueType(this TypeReference type)
		{
			if (!type.IsDefinition)
			{
				type.IsValueType = true;
			}
		}

		private static bool IsCoreLibrary(AssemblyNameReference reference)
		{
			string name = reference.Name;
			switch (name)
			{
			default:
				return name == "netstandard";
			case "mscorlib":
			case "System.Runtime":
			case "System.Private.CoreLib":
				return true;
			}
		}

		public static ImageDebugHeaderEntry GetCodeViewEntry(this ImageDebugHeader header)
		{
			return header.GetEntry(ImageDebugType.CodeView);
		}

		public static ImageDebugHeaderEntry GetDeterministicEntry(this ImageDebugHeader header)
		{
			return header.GetEntry(ImageDebugType.Deterministic);
		}

		public static ImageDebugHeader AddDeterministicEntry(this ImageDebugHeader header)
		{
			ImageDebugDirectory directory = default(ImageDebugDirectory);
			directory.Type = ImageDebugType.Deterministic;
			ImageDebugHeaderEntry imageDebugHeaderEntry = new ImageDebugHeaderEntry(directory, Empty<byte>.Array);
			if (header == null)
			{
				return new ImageDebugHeader(imageDebugHeaderEntry);
			}
			ImageDebugHeaderEntry[] array = new ImageDebugHeaderEntry[header.Entries.Length + 1];
			Array.Copy(header.Entries, array, header.Entries.Length);
			array[array.Length - 1] = imageDebugHeaderEntry;
			return new ImageDebugHeader(array);
		}

		public static ImageDebugHeaderEntry GetEmbeddedPortablePdbEntry(this ImageDebugHeader header)
		{
			return header.GetEntry(ImageDebugType.EmbeddedPortablePdb);
		}

		private static ImageDebugHeaderEntry GetEntry(this ImageDebugHeader header, ImageDebugType type)
		{
			if (!header.HasEntries)
			{
				return null;
			}
			for (int i = 0; i < header.Entries.Length; i++)
			{
				ImageDebugHeaderEntry imageDebugHeaderEntry = header.Entries[i];
				if (imageDebugHeaderEntry.Directory.Type == type)
				{
					return imageDebugHeaderEntry;
				}
			}
			return null;
		}

		public static string GetPdbFileName(string assemblyFileName)
		{
			return Path.ChangeExtension(assemblyFileName, ".pdb");
		}

		public static string GetMdbFileName(string assemblyFileName)
		{
			return assemblyFileName + ".mdb";
		}

		public static bool IsPortablePdb(string fileName)
		{
			using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				return IsPortablePdb(stream);
			}
		}

		public static bool IsPortablePdb(Stream stream)
		{
			if (stream.Length < 4)
			{
				return false;
			}
			long position = stream.Position;
			try
			{
				return new BinaryReader(stream).ReadUInt32() == 1112167234;
			}
			finally
			{
				stream.Position = position;
			}
		}

		public static uint ReadCompressedUInt32(this byte[] data, ref int position)
		{
			uint result;
			if ((data[position] & 0x80) == 0)
			{
				result = data[position];
				position++;
			}
			else if ((data[position] & 0x40) == 0)
			{
				result = (uint)((data[position] & -129) << 8);
				result |= data[position + 1];
				position += 2;
			}
			else
			{
				result = (uint)((data[position] & -193) << 24);
				result = (uint)((int)result | data[position + 1] << 16);
				result = (uint)((int)result | data[position + 2] << 8);
				result |= data[position + 3];
				position += 4;
			}
			return result;
		}

		public static MetadataToken GetMetadataToken(this CodedIndex self, uint data)
		{
			uint rid;
			TokenType type;
			switch (self)
			{
			case CodedIndex.TypeDefOrRef:
				rid = data >> 2;
				switch (data & 3)
				{
				case 0u:
					break;
				case 1u:
					goto IL_006d;
				case 2u:
					goto IL_0078;
				default:
					goto end_IL_0001;
				}
				type = TokenType.TypeDef;
				goto IL_05b3;
			case CodedIndex.HasConstant:
				rid = data >> 2;
				switch (data & 3)
				{
				case 0u:
					break;
				case 1u:
					goto IL_00ad;
				case 2u:
					goto IL_00b8;
				default:
					goto end_IL_0001;
				}
				type = TokenType.Field;
				goto IL_05b3;
			case CodedIndex.HasCustomAttribute:
				rid = data >> 5;
				switch (data & 0x1F)
				{
				case 0u:
					break;
				case 1u:
					goto IL_013a;
				case 2u:
					goto IL_0145;
				case 3u:
					goto IL_0150;
				case 4u:
					goto IL_015b;
				case 5u:
					goto IL_0166;
				case 6u:
					goto IL_0171;
				case 7u:
					goto IL_017c;
				case 8u:
					goto IL_0183;
				case 9u:
					goto IL_018e;
				case 10u:
					goto IL_0199;
				case 11u:
					goto IL_01a4;
				case 12u:
					goto IL_01af;
				case 13u:
					goto IL_01ba;
				case 14u:
					goto IL_01c5;
				case 15u:
					goto IL_01d0;
				case 16u:
					goto IL_01db;
				case 17u:
					goto IL_01e6;
				case 18u:
					goto IL_01f1;
				case 19u:
					goto IL_01fc;
				case 20u:
					goto IL_0207;
				case 21u:
					goto IL_0212;
				default:
					goto end_IL_0001;
				}
				type = TokenType.Method;
				goto IL_05b3;
			case CodedIndex.HasFieldMarshal:
				rid = data >> 1;
				switch (data & 1)
				{
				case 0u:
					break;
				case 1u:
					goto IL_023c;
				default:
					goto end_IL_0001;
				}
				type = TokenType.Field;
				goto IL_05b3;
			case CodedIndex.HasDeclSecurity:
				rid = data >> 2;
				switch (data & 3)
				{
				case 0u:
					break;
				case 1u:
					goto IL_0271;
				case 2u:
					goto IL_027c;
				default:
					goto end_IL_0001;
				}
				type = TokenType.TypeDef;
				goto IL_05b3;
			case CodedIndex.MemberRefParent:
				rid = data >> 3;
				switch (data & 7)
				{
				case 0u:
					break;
				case 1u:
					goto IL_02b9;
				case 2u:
					goto IL_02c4;
				case 3u:
					goto IL_02cf;
				case 4u:
					goto IL_02da;
				default:
					goto end_IL_0001;
				}
				type = TokenType.TypeDef;
				goto IL_05b3;
			case CodedIndex.HasSemantics:
				rid = data >> 1;
				switch (data & 1)
				{
				case 0u:
					break;
				case 1u:
					goto IL_0304;
				default:
					goto end_IL_0001;
				}
				type = TokenType.Event;
				goto IL_05b3;
			case CodedIndex.MethodDefOrRef:
				rid = data >> 1;
				switch (data & 1)
				{
				case 0u:
					break;
				case 1u:
					goto IL_032e;
				default:
					goto end_IL_0001;
				}
				type = TokenType.Method;
				goto IL_05b3;
			case CodedIndex.MemberForwarded:
				rid = data >> 1;
				switch (data & 1)
				{
				case 0u:
					break;
				case 1u:
					goto IL_0358;
				default:
					goto end_IL_0001;
				}
				type = TokenType.Field;
				goto IL_05b3;
			case CodedIndex.Implementation:
				rid = data >> 2;
				switch (data & 3)
				{
				case 0u:
					break;
				case 1u:
					goto IL_038d;
				case 2u:
					goto IL_0398;
				default:
					goto end_IL_0001;
				}
				type = TokenType.File;
				goto IL_05b3;
			case CodedIndex.CustomAttributeType:
				rid = data >> 3;
				switch (data & 7)
				{
				case 2u:
					break;
				case 3u:
					goto IL_03c3;
				default:
					goto end_IL_0001;
				}
				type = TokenType.Method;
				goto IL_05b3;
			case CodedIndex.ResolutionScope:
				rid = data >> 2;
				switch (data & 3)
				{
				case 0u:
					break;
				case 1u:
					goto IL_03f8;
				case 2u:
					goto IL_0403;
				case 3u:
					goto IL_040e;
				default:
					goto end_IL_0001;
				}
				type = TokenType.Module;
				goto IL_05b3;
			case CodedIndex.TypeOrMethodDef:
				rid = data >> 1;
				switch (data & 1)
				{
				case 0u:
					break;
				case 1u:
					goto IL_0438;
				default:
					goto end_IL_0001;
				}
				type = TokenType.TypeDef;
				goto IL_05b3;
			case CodedIndex.HasCustomDebugInformation:
				{
					rid = data >> 5;
					switch (data & 0x1F)
					{
					case 0u:
						break;
					case 1u:
						goto IL_04ce;
					case 2u:
						goto IL_04d9;
					case 3u:
						goto IL_04e4;
					case 4u:
						goto IL_04ef;
					case 5u:
						goto IL_04fa;
					case 6u:
						goto IL_0505;
					case 7u:
						goto IL_0510;
					case 8u:
						goto IL_0517;
					case 9u:
						goto IL_0522;
					case 10u:
						goto IL_052d;
					case 11u:
						goto IL_0535;
					case 12u:
						goto IL_053d;
					case 13u:
						goto IL_0545;
					case 14u:
						goto IL_054d;
					case 15u:
						goto IL_0555;
					case 16u:
						goto IL_055d;
					case 17u:
						goto IL_0565;
					case 18u:
						goto IL_056d;
					case 19u:
						goto IL_0575;
					case 20u:
						goto IL_057d;
					case 21u:
						goto IL_0585;
					case 22u:
						goto IL_058d;
					case 23u:
						goto IL_0595;
					case 24u:
						goto IL_059d;
					case 25u:
						goto IL_05a5;
					case 26u:
						goto IL_05ad;
					default:
						goto end_IL_0001;
					}
					type = TokenType.Method;
					goto IL_05b3;
				}
				IL_05ad:
				type = TokenType.ImportScope;
				goto IL_05b3;
				IL_05a5:
				type = TokenType.LocalConstant;
				goto IL_05b3;
				IL_059d:
				type = TokenType.LocalVariable;
				goto IL_05b3;
				IL_0595:
				type = TokenType.LocalScope;
				goto IL_05b3;
				IL_058d:
				type = TokenType.Document;
				goto IL_05b3;
				IL_0585:
				type = TokenType.MethodSpec;
				goto IL_05b3;
				IL_057d:
				type = TokenType.GenericParamConstraint;
				goto IL_05b3;
				IL_0575:
				type = TokenType.GenericParam;
				goto IL_05b3;
				IL_056d:
				type = TokenType.ManifestResource;
				goto IL_05b3;
				IL_0565:
				type = TokenType.ExportedType;
				goto IL_05b3;
				IL_055d:
				type = TokenType.File;
				goto IL_05b3;
				IL_0555:
				type = TokenType.AssemblyRef;
				goto IL_05b3;
				IL_054d:
				type = TokenType.Assembly;
				goto IL_05b3;
				IL_0545:
				type = TokenType.TypeSpec;
				goto IL_05b3;
				IL_053d:
				type = TokenType.ModuleRef;
				goto IL_05b3;
				IL_0535:
				type = TokenType.Signature;
				goto IL_05b3;
				IL_052d:
				type = TokenType.Event;
				goto IL_05b3;
				IL_0522:
				type = TokenType.Property;
				goto IL_05b3;
				IL_0517:
				type = TokenType.Permission;
				goto IL_05b3;
				IL_0510:
				type = TokenType.Module;
				goto IL_05b3;
				IL_0505:
				type = TokenType.MemberRef;
				goto IL_05b3;
				IL_04fa:
				type = TokenType.InterfaceImpl;
				goto IL_05b3;
				IL_04ef:
				type = TokenType.Param;
				goto IL_05b3;
				IL_04e4:
				type = TokenType.TypeDef;
				goto IL_05b3;
				IL_04d9:
				type = TokenType.TypeRef;
				goto IL_05b3;
				IL_04ce:
				type = TokenType.Field;
				goto IL_05b3;
				IL_015b:
				type = TokenType.Param;
				goto IL_05b3;
				IL_0438:
				type = TokenType.Method;
				goto IL_05b3;
				IL_0150:
				type = TokenType.TypeDef;
				goto IL_05b3;
				IL_040e:
				type = TokenType.TypeRef;
				goto IL_05b3;
				IL_0403:
				type = TokenType.AssemblyRef;
				goto IL_05b3;
				IL_03f8:
				type = TokenType.ModuleRef;
				goto IL_05b3;
				IL_0145:
				type = TokenType.TypeRef;
				goto IL_05b3;
				IL_03c3:
				type = TokenType.MemberRef;
				goto IL_05b3;
				IL_013a:
				type = TokenType.Field;
				goto IL_05b3;
				IL_0398:
				type = TokenType.ExportedType;
				goto IL_05b3;
				IL_038d:
				type = TokenType.AssemblyRef;
				goto IL_05b3;
				IL_006d:
				type = TokenType.TypeRef;
				goto IL_05b3;
				IL_0358:
				type = TokenType.Method;
				goto IL_05b3;
				IL_032e:
				type = TokenType.MemberRef;
				goto IL_05b3;
				IL_00b8:
				type = TokenType.Property;
				goto IL_05b3;
				IL_0304:
				type = TokenType.Property;
				goto IL_05b3;
				IL_00ad:
				type = TokenType.Param;
				goto IL_05b3;
				IL_02da:
				type = TokenType.TypeSpec;
				goto IL_05b3;
				IL_02cf:
				type = TokenType.Method;
				goto IL_05b3;
				IL_02c4:
				type = TokenType.ModuleRef;
				goto IL_05b3;
				IL_02b9:
				type = TokenType.TypeRef;
				goto IL_05b3;
				IL_05b3:
				return new MetadataToken(type, rid);
				IL_027c:
				type = TokenType.Assembly;
				goto IL_05b3;
				IL_0271:
				type = TokenType.Method;
				goto IL_05b3;
				IL_023c:
				type = TokenType.Param;
				goto IL_05b3;
				IL_0078:
				type = TokenType.TypeSpec;
				goto IL_05b3;
				IL_0212:
				type = TokenType.MethodSpec;
				goto IL_05b3;
				IL_0207:
				type = TokenType.GenericParamConstraint;
				goto IL_05b3;
				IL_01fc:
				type = TokenType.GenericParam;
				goto IL_05b3;
				IL_01f1:
				type = TokenType.ManifestResource;
				goto IL_05b3;
				IL_01e6:
				type = TokenType.ExportedType;
				goto IL_05b3;
				IL_01db:
				type = TokenType.File;
				goto IL_05b3;
				IL_01d0:
				type = TokenType.AssemblyRef;
				goto IL_05b3;
				IL_01c5:
				type = TokenType.Assembly;
				goto IL_05b3;
				IL_01ba:
				type = TokenType.TypeSpec;
				goto IL_05b3;
				IL_01af:
				type = TokenType.ModuleRef;
				goto IL_05b3;
				IL_01a4:
				type = TokenType.Signature;
				goto IL_05b3;
				IL_0199:
				type = TokenType.Event;
				goto IL_05b3;
				IL_018e:
				type = TokenType.Property;
				goto IL_05b3;
				IL_0183:
				type = TokenType.Permission;
				goto IL_05b3;
				IL_017c:
				type = TokenType.Module;
				goto IL_05b3;
				IL_0171:
				type = TokenType.MemberRef;
				goto IL_05b3;
				IL_0166:
				type = TokenType.InterfaceImpl;
				goto IL_05b3;
				end_IL_0001:
				break;
			}
			return MetadataToken.Zero;
		}

		public static uint CompressMetadataToken(this CodedIndex self, MetadataToken token)
		{
			uint result = 0u;
			if (token.RID == 0)
			{
				return result;
			}
			switch (self)
			{
			case CodedIndex.TypeDefOrRef:
				result = token.RID << 2;
				switch (token.TokenType)
				{
				case TokenType.TypeDef:
					return result | 0;
				case TokenType.TypeRef:
					return result | 1;
				case TokenType.TypeSpec:
					return result | 2;
				}
				break;
			case CodedIndex.HasConstant:
				result = token.RID << 2;
				switch (token.TokenType)
				{
				case TokenType.Field:
					return result | 0;
				case TokenType.Param:
					return result | 1;
				case TokenType.Property:
					return result | 2;
				}
				break;
			case CodedIndex.HasCustomAttribute:
				result = token.RID << 5;
				switch (token.TokenType)
				{
				case TokenType.Method:
					return result | 0;
				case TokenType.Field:
					return result | 1;
				case TokenType.TypeRef:
					return result | 2;
				case TokenType.TypeDef:
					return result | 3;
				case TokenType.Param:
					return result | 4;
				case TokenType.InterfaceImpl:
					return result | 5;
				case TokenType.MemberRef:
					return result | 6;
				case TokenType.Module:
					return result | 7;
				case TokenType.Permission:
					return result | 8;
				case TokenType.Property:
					return result | 9;
				case TokenType.Event:
					return result | 0xA;
				case TokenType.Signature:
					return result | 0xB;
				case TokenType.ModuleRef:
					return result | 0xC;
				case TokenType.TypeSpec:
					return result | 0xD;
				case TokenType.Assembly:
					return result | 0xE;
				case TokenType.AssemblyRef:
					return result | 0xF;
				case TokenType.File:
					return result | 0x10;
				case TokenType.ExportedType:
					return result | 0x11;
				case TokenType.ManifestResource:
					return result | 0x12;
				case TokenType.GenericParam:
					return result | 0x13;
				case TokenType.GenericParamConstraint:
					return result | 0x14;
				case TokenType.MethodSpec:
					return result | 0x15;
				}
				break;
			case CodedIndex.HasFieldMarshal:
				result = token.RID << 1;
				switch (token.TokenType)
				{
				case TokenType.Field:
					return result | 0;
				case TokenType.Param:
					return result | 1;
				}
				break;
			case CodedIndex.HasDeclSecurity:
				result = token.RID << 2;
				switch (token.TokenType)
				{
				case TokenType.TypeDef:
					return result | 0;
				case TokenType.Method:
					return result | 1;
				case TokenType.Assembly:
					return result | 2;
				}
				break;
			case CodedIndex.MemberRefParent:
				result = token.RID << 3;
				switch (token.TokenType)
				{
				case TokenType.TypeDef:
					return result | 0;
				case TokenType.TypeRef:
					return result | 1;
				case TokenType.ModuleRef:
					return result | 2;
				case TokenType.Method:
					return result | 3;
				case TokenType.TypeSpec:
					return result | 4;
				}
				break;
			case CodedIndex.HasSemantics:
				result = token.RID << 1;
				switch (token.TokenType)
				{
				case TokenType.Event:
					return result | 0;
				case TokenType.Property:
					return result | 1;
				}
				break;
			case CodedIndex.MethodDefOrRef:
				result = token.RID << 1;
				switch (token.TokenType)
				{
				case TokenType.Method:
					return result | 0;
				case TokenType.MemberRef:
					return result | 1;
				}
				break;
			case CodedIndex.MemberForwarded:
				result = token.RID << 1;
				switch (token.TokenType)
				{
				case TokenType.Field:
					return result | 0;
				case TokenType.Method:
					return result | 1;
				}
				break;
			case CodedIndex.Implementation:
				result = token.RID << 2;
				switch (token.TokenType)
				{
				case TokenType.File:
					return result | 0;
				case TokenType.AssemblyRef:
					return result | 1;
				case TokenType.ExportedType:
					return result | 2;
				}
				break;
			case CodedIndex.CustomAttributeType:
				result = token.RID << 3;
				switch (token.TokenType)
				{
				case TokenType.Method:
					return result | 2;
				case TokenType.MemberRef:
					return result | 3;
				}
				break;
			case CodedIndex.ResolutionScope:
				result = token.RID << 2;
				switch (token.TokenType)
				{
				case TokenType.Module:
					return result | 0;
				case TokenType.ModuleRef:
					return result | 1;
				case TokenType.AssemblyRef:
					return result | 2;
				case TokenType.TypeRef:
					return result | 3;
				}
				break;
			case CodedIndex.TypeOrMethodDef:
				result = token.RID << 1;
				switch (token.TokenType)
				{
				case TokenType.TypeDef:
					return result | 0;
				case TokenType.Method:
					return result | 1;
				}
				break;
			case CodedIndex.HasCustomDebugInformation:
				result = token.RID << 5;
				switch (token.TokenType)
				{
				case TokenType.Method:
					return result | 0;
				case TokenType.Field:
					return result | 1;
				case TokenType.TypeRef:
					return result | 2;
				case TokenType.TypeDef:
					return result | 3;
				case TokenType.Param:
					return result | 4;
				case TokenType.InterfaceImpl:
					return result | 5;
				case TokenType.MemberRef:
					return result | 6;
				case TokenType.Module:
					return result | 7;
				case TokenType.Permission:
					return result | 8;
				case TokenType.Property:
					return result | 9;
				case TokenType.Event:
					return result | 0xA;
				case TokenType.Signature:
					return result | 0xB;
				case TokenType.ModuleRef:
					return result | 0xC;
				case TokenType.TypeSpec:
					return result | 0xD;
				case TokenType.Assembly:
					return result | 0xE;
				case TokenType.AssemblyRef:
					return result | 0xF;
				case TokenType.File:
					return result | 0x10;
				case TokenType.ExportedType:
					return result | 0x11;
				case TokenType.ManifestResource:
					return result | 0x12;
				case TokenType.GenericParam:
					return result | 0x13;
				case TokenType.GenericParamConstraint:
					return result | 0x14;
				case TokenType.MethodSpec:
					return result | 0x15;
				case TokenType.Document:
					return result | 0x16;
				case TokenType.LocalScope:
					return result | 0x17;
				case TokenType.LocalVariable:
					return result | 0x18;
				case TokenType.LocalConstant:
					return result | 0x19;
				case TokenType.ImportScope:
					return result | 0x1A;
				}
				break;
			}
			throw new ArgumentException();
		}

		public static int GetSize(this CodedIndex self, Func<Table, int> counter)
		{
			int num;
			Table[] array;
			switch (self)
			{
			case CodedIndex.TypeDefOrRef:
				num = 2;
				array = new Table[3]
				{
					Table.TypeDef,
					Table.TypeRef,
					Table.TypeSpec
				};
				break;
			case CodedIndex.HasConstant:
				num = 2;
				array = new Table[3]
				{
					Table.Field,
					Table.Param,
					Table.Property
				};
				break;
			case CodedIndex.HasCustomAttribute:
				num = 5;
				array = new Table[22]
				{
					Table.Method,
					Table.Field,
					Table.TypeRef,
					Table.TypeDef,
					Table.Param,
					Table.InterfaceImpl,
					Table.MemberRef,
					Table.Module,
					Table.DeclSecurity,
					Table.Property,
					Table.Event,
					Table.StandAloneSig,
					Table.ModuleRef,
					Table.TypeSpec,
					Table.Assembly,
					Table.AssemblyRef,
					Table.File,
					Table.ExportedType,
					Table.ManifestResource,
					Table.GenericParam,
					Table.GenericParamConstraint,
					Table.MethodSpec
				};
				break;
			case CodedIndex.HasFieldMarshal:
				num = 1;
				array = new Table[2]
				{
					Table.Field,
					Table.Param
				};
				break;
			case CodedIndex.HasDeclSecurity:
				num = 2;
				array = new Table[3]
				{
					Table.TypeDef,
					Table.Method,
					Table.Assembly
				};
				break;
			case CodedIndex.MemberRefParent:
				num = 3;
				array = new Table[5]
				{
					Table.TypeDef,
					Table.TypeRef,
					Table.ModuleRef,
					Table.Method,
					Table.TypeSpec
				};
				break;
			case CodedIndex.HasSemantics:
				num = 1;
				array = new Table[2]
				{
					Table.Event,
					Table.Property
				};
				break;
			case CodedIndex.MethodDefOrRef:
				num = 1;
				array = new Table[2]
				{
					Table.Method,
					Table.MemberRef
				};
				break;
			case CodedIndex.MemberForwarded:
				num = 1;
				array = new Table[2]
				{
					Table.Field,
					Table.Method
				};
				break;
			case CodedIndex.Implementation:
				num = 2;
				array = new Table[3]
				{
					Table.File,
					Table.AssemblyRef,
					Table.ExportedType
				};
				break;
			case CodedIndex.CustomAttributeType:
				num = 3;
				array = new Table[2]
				{
					Table.Method,
					Table.MemberRef
				};
				break;
			case CodedIndex.ResolutionScope:
				num = 2;
				array = new Table[4]
				{
					Table.Module,
					Table.ModuleRef,
					Table.AssemblyRef,
					Table.TypeRef
				};
				break;
			case CodedIndex.TypeOrMethodDef:
				num = 1;
				array = new Table[2]
				{
					Table.TypeDef,
					Table.Method
				};
				break;
			case CodedIndex.HasCustomDebugInformation:
				num = 5;
				array = new Table[27]
				{
					Table.Method,
					Table.Field,
					Table.TypeRef,
					Table.TypeDef,
					Table.Param,
					Table.InterfaceImpl,
					Table.MemberRef,
					Table.Module,
					Table.DeclSecurity,
					Table.Property,
					Table.Event,
					Table.StandAloneSig,
					Table.ModuleRef,
					Table.TypeSpec,
					Table.Assembly,
					Table.AssemblyRef,
					Table.File,
					Table.ExportedType,
					Table.ManifestResource,
					Table.GenericParam,
					Table.GenericParamConstraint,
					Table.MethodSpec,
					Table.Document,
					Table.LocalScope,
					Table.LocalVariable,
					Table.LocalConstant,
					Table.ImportScope
				};
				break;
			default:
				throw new ArgumentException();
			}
			int num2 = 0;
			for (int i = 0; i < array.Length; i++)
			{
				num2 = Math.Max(counter(array[i]), num2);
			}
			if (num2 >= 1 << 16 - num)
			{
				return 4;
			}
			return 2;
		}

		public static RSA CreateRSA(this StrongNameKeyPair key_pair)
		{
			if (!TryGetKeyContainer(key_pair, out byte[] blob, out string keyContainerName))
			{
				return CryptoConvert.FromCapiKeyBlob(blob);
			}
			return new RSACryptoServiceProvider(new CspParameters
			{
				Flags = CspProviderFlags.UseMachineKeyStore,
				KeyContainerName = keyContainerName,
				KeyNumber = 2
			});
		}

		private static bool TryGetKeyContainer(ISerializable key_pair, out byte[] key, out string key_container)
		{
			SerializationInfo serializationInfo = new SerializationInfo(typeof(StrongNameKeyPair), new FormatterConverter());
			key_pair.GetObjectData(serializationInfo, default(StreamingContext));
			key = (byte[])serializationInfo.GetValue("_keyPairArray", typeof(byte[]));
			key_container = serializationInfo.GetString("_keyPairContainer");
			return key_container != null;
		}
	}
}
