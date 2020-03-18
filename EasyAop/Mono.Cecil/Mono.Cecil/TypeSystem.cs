using Mono.Cecil.Metadata;
using Mono.Collections.Generic;
using System;

namespace Mono.Cecil
{
	public abstract class TypeSystem
	{
		private sealed class CoreTypeSystem : TypeSystem
		{
			public CoreTypeSystem(ModuleDefinition module)
				: base(module)
			{
			}

			internal override TypeReference LookupType(string @namespace, string name)
			{
				TypeReference typeReference = LookupTypeDefinition(@namespace, name) ?? LookupTypeForwarded(@namespace, name);
				if (typeReference != null)
				{
					return typeReference;
				}
				throw new NotSupportedException();
			}

			private TypeReference LookupTypeDefinition(string @namespace, string name)
			{
				if (base.module.MetadataSystem.Types == null)
				{
					Initialize(base.module.Types);
				}
				return base.module.Read(new Row<string, string>(@namespace, name), delegate(Row<string, string> row, MetadataReader reader)
				{
					TypeDefinition[] types = reader.metadata.Types;
					for (int i = 0; i < types.Length; i++)
					{
						if (types[i] == null)
						{
							types[i] = reader.GetTypeDefinition((uint)(i + 1));
						}
						TypeDefinition typeDefinition = types[i];
						if (typeDefinition.Name == row.Col2 && typeDefinition.Namespace == row.Col1)
						{
							return typeDefinition;
						}
					}
					return null;
				});
			}

			private TypeReference LookupTypeForwarded(string @namespace, string name)
			{
				if (!base.module.HasExportedTypes)
				{
					return null;
				}
				Collection<ExportedType> exportedTypes = base.module.ExportedTypes;
				for (int i = 0; i < exportedTypes.Count; i++)
				{
					ExportedType exportedType = exportedTypes[i];
					if (exportedType.Name == name && exportedType.Namespace == @namespace)
					{
						return exportedType.CreateReference();
					}
				}
				return null;
			}

			private static void Initialize(object obj)
			{
			}
		}

		private sealed class CommonTypeSystem : TypeSystem
		{
			private AssemblyNameReference core_library;

			public CommonTypeSystem(ModuleDefinition module)
				: base(module)
			{
			}

			internal override TypeReference LookupType(string @namespace, string name)
			{
				return CreateTypeReference(@namespace, name);
			}

			public AssemblyNameReference GetCoreLibraryReference()
			{
				if (core_library != null)
				{
					return core_library;
				}
				if (base.module.TryGetCoreLibraryReference(out core_library))
				{
					return core_library;
				}
				core_library = new AssemblyNameReference
				{
					Name = "mscorlib",
					Version = GetCorlibVersion(),
					PublicKeyToken = new byte[8]
					{
						183,
						122,
						92,
						86,
						25,
						52,
						224,
						137
					}
				};
				base.module.AssemblyReferences.Add(core_library);
				return core_library;
			}

			private Version GetCorlibVersion()
			{
				switch (base.module.Runtime)
				{
				case TargetRuntime.Net_1_0:
				case TargetRuntime.Net_1_1:
					return new Version(1, 0, 0, 0);
				case TargetRuntime.Net_2_0:
					return new Version(2, 0, 0, 0);
				case TargetRuntime.Net_4_0:
					return new Version(4, 0, 0, 0);
				default:
					throw new NotSupportedException();
				}
			}

			private TypeReference CreateTypeReference(string @namespace, string name)
			{
				return new TypeReference(@namespace, name, base.module, GetCoreLibraryReference());
			}
		}

		private readonly ModuleDefinition module;

		private TypeReference type_object;

		private TypeReference type_void;

		private TypeReference type_bool;

		private TypeReference type_char;

		private TypeReference type_sbyte;

		private TypeReference type_byte;

		private TypeReference type_int16;

		private TypeReference type_uint16;

		private TypeReference type_int32;

		private TypeReference type_uint32;

		private TypeReference type_int64;

		private TypeReference type_uint64;

		private TypeReference type_single;

		private TypeReference type_double;

		private TypeReference type_intptr;

		private TypeReference type_uintptr;

		private TypeReference type_string;

		private TypeReference type_typedref;

		[Obsolete("Use CoreLibrary")]
		public IMetadataScope Corlib
		{
			get
			{
				return CoreLibrary;
			}
		}

		public IMetadataScope CoreLibrary
		{
			get
			{
				CommonTypeSystem commonTypeSystem = this as CommonTypeSystem;
				if (commonTypeSystem == null)
				{
					return module;
				}
				return commonTypeSystem.GetCoreLibraryReference();
			}
		}

		public TypeReference Object => type_object ?? LookupSystemType(ref type_object, "Object", ElementType.Object);

		public TypeReference Void => type_void ?? LookupSystemType(ref type_void, "Void", ElementType.Void);

		public TypeReference Boolean => type_bool ?? LookupSystemValueType(ref type_bool, "Boolean", ElementType.Boolean);

		public TypeReference Char => type_char ?? LookupSystemValueType(ref type_char, "Char", ElementType.Char);

		public TypeReference SByte => type_sbyte ?? LookupSystemValueType(ref type_sbyte, "SByte", ElementType.I1);

		public TypeReference Byte => type_byte ?? LookupSystemValueType(ref type_byte, "Byte", ElementType.U1);

		public TypeReference Int16 => type_int16 ?? LookupSystemValueType(ref type_int16, "Int16", ElementType.I2);

		public TypeReference UInt16 => type_uint16 ?? LookupSystemValueType(ref type_uint16, "UInt16", ElementType.U2);

		public TypeReference Int32 => type_int32 ?? LookupSystemValueType(ref type_int32, "Int32", ElementType.I4);

		public TypeReference UInt32 => type_uint32 ?? LookupSystemValueType(ref type_uint32, "UInt32", ElementType.U4);

		public TypeReference Int64 => type_int64 ?? LookupSystemValueType(ref type_int64, "Int64", ElementType.I8);

		public TypeReference UInt64 => type_uint64 ?? LookupSystemValueType(ref type_uint64, "UInt64", ElementType.U8);

		public TypeReference Single => type_single ?? LookupSystemValueType(ref type_single, "Single", ElementType.R4);

		public TypeReference Double => type_double ?? LookupSystemValueType(ref type_double, "Double", ElementType.R8);

		public TypeReference IntPtr => type_intptr ?? LookupSystemValueType(ref type_intptr, "IntPtr", ElementType.I);

		public TypeReference UIntPtr => type_uintptr ?? LookupSystemValueType(ref type_uintptr, "UIntPtr", ElementType.U);

		public TypeReference String => type_string ?? LookupSystemType(ref type_string, "String", ElementType.String);

		public TypeReference TypedReference => type_typedref ?? LookupSystemValueType(ref type_typedref, "TypedReference", ElementType.TypedByRef);

		private TypeSystem(ModuleDefinition module)
		{
			this.module = module;
		}

		internal static TypeSystem CreateTypeSystem(ModuleDefinition module)
		{
			if (module.IsCoreLibrary())
			{
				return new CoreTypeSystem(module);
			}
			return new CommonTypeSystem(module);
		}

		internal abstract TypeReference LookupType(string @namespace, string name);

		private TypeReference LookupSystemType(ref TypeReference reference, string name, ElementType element_type)
		{
			lock (module.SyncRoot)
			{
				if (reference != null)
				{
					return reference;
				}
				TypeReference typeReference = LookupType("System", name);
				typeReference.etype = element_type;
				return reference = typeReference;
			}
		}

		private TypeReference LookupSystemValueType(ref TypeReference typeRef, string name, ElementType element_type)
		{
			lock (module.SyncRoot)
			{
				if (typeRef != null)
				{
					return typeRef;
				}
				TypeReference typeReference = LookupType("System", name);
				typeReference.etype = element_type;
				typeReference.KnownValueType();
				return typeRef = typeReference;
			}
		}
	}
}
