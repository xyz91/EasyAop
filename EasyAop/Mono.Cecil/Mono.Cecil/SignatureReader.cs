using Mono.Cecil.Cil;
using Mono.Cecil.Metadata;
using Mono.Cecil.PE;
using Mono.Collections.Generic;
using System;
using System.Text;

namespace Mono.Cecil
{
	internal sealed class SignatureReader : ByteBuffer
	{
		private readonly MetadataReader reader;

		internal readonly uint start;

		internal readonly uint sig_length;

		private TypeSystem TypeSystem => reader.module.TypeSystem;

		public SignatureReader(uint blob, MetadataReader reader)
			: base(reader.image.BlobHeap.data)
		{
			this.reader = reader;
			base.position = (int)blob;
			sig_length = base.ReadCompressedUInt32();
			start = (uint)base.position;
		}

		private MetadataToken ReadTypeTokenSignature()
		{
			return CodedIndex.TypeDefOrRef.GetMetadataToken(base.ReadCompressedUInt32());
		}

		private GenericParameter GetGenericParameter(GenericParameterType type, uint var)
		{
			IGenericContext context = reader.context;
			if (context == null)
			{
				return GetUnboundGenericParameter(type, (int)var);
			}
			IGenericParameterProvider genericParameterProvider;
			switch (type)
			{
			case GenericParameterType.Type:
				genericParameterProvider = context.Type;
				break;
			case GenericParameterType.Method:
				genericParameterProvider = context.Method;
				break;
			default:
				throw new NotSupportedException();
			}
			if (!context.IsDefinition)
			{
				CheckGenericContext(genericParameterProvider, (int)var);
			}
			if ((int)var >= genericParameterProvider.GenericParameters.Count)
			{
				return GetUnboundGenericParameter(type, (int)var);
			}
			return genericParameterProvider.GenericParameters[(int)var];
		}

		private GenericParameter GetUnboundGenericParameter(GenericParameterType type, int index)
		{
			return new GenericParameter(index, type, reader.module);
		}

		private static void CheckGenericContext(IGenericParameterProvider owner, int index)
		{
			Collection<GenericParameter> genericParameters = owner.GenericParameters;
			for (int i = genericParameters.Count; i <= index; i++)
			{
				genericParameters.Add(new GenericParameter(owner));
			}
		}

		public void ReadGenericInstanceSignature(IGenericParameterProvider provider, IGenericInstance instance)
		{
			uint num = base.ReadCompressedUInt32();
			if (!provider.IsDefinition)
			{
				CheckGenericContext(provider, (int)(num - 1));
			}
			Collection<TypeReference> genericArguments = instance.GenericArguments;
			for (int i = 0; i < num; i++)
			{
				genericArguments.Add(ReadTypeSignature());
			}
		}

		private ArrayType ReadArrayTypeSignature()
		{
			ArrayType arrayType = new ArrayType(ReadTypeSignature());
			uint num = base.ReadCompressedUInt32();
			uint[] array = new uint[base.ReadCompressedUInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = base.ReadCompressedUInt32();
			}
			int[] array2 = new int[base.ReadCompressedUInt32()];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = base.ReadCompressedInt32();
			}
			arrayType.Dimensions.Clear();
			for (int k = 0; k < num; k++)
			{
				int? nullable = null;
				int? upperBound = null;
				if (k < array2.Length)
				{
					nullable = array2[k];
				}
				if (k < array.Length)
				{
					upperBound = nullable + (int)array[k] - 1;
				}
				arrayType.Dimensions.Add(new ArrayDimension(nullable, upperBound));
			}
			return arrayType;
		}

		private TypeReference GetTypeDefOrRef(MetadataToken token)
		{
			return reader.GetTypeDefOrRef(token);
		}

		public TypeReference ReadTypeSignature()
		{
			return ReadTypeSignature((ElementType)base.ReadByte());
		}

		public TypeReference ReadTypeToken()
		{
			return GetTypeDefOrRef(ReadTypeTokenSignature());
		}

		private TypeReference ReadTypeSignature(ElementType etype)
		{
			switch (etype)
			{
			case ElementType.ValueType:
			{
				TypeReference typeDefOrRef2 = GetTypeDefOrRef(ReadTypeTokenSignature());
				typeDefOrRef2.KnownValueType();
				return typeDefOrRef2;
			}
			case ElementType.Class:
				return GetTypeDefOrRef(ReadTypeTokenSignature());
			case ElementType.Ptr:
				return new PointerType(ReadTypeSignature());
			case ElementType.FnPtr:
			{
				FunctionPointerType functionPointerType = new FunctionPointerType();
				ReadMethodSignature(functionPointerType);
				return functionPointerType;
			}
			case ElementType.ByRef:
				return new ByReferenceType(ReadTypeSignature());
			case ElementType.Pinned:
				return new PinnedType(ReadTypeSignature());
			case ElementType.SzArray:
				return new ArrayType(ReadTypeSignature());
			case ElementType.Array:
				return ReadArrayTypeSignature();
			case ElementType.CModOpt:
				return new OptionalModifierType(GetTypeDefOrRef(ReadTypeTokenSignature()), ReadTypeSignature());
			case ElementType.CModReqD:
				return new RequiredModifierType(GetTypeDefOrRef(ReadTypeTokenSignature()), ReadTypeSignature());
			case ElementType.Sentinel:
				return new SentinelType(ReadTypeSignature());
			case ElementType.Var:
				return GetGenericParameter(GenericParameterType.Type, base.ReadCompressedUInt32());
			case ElementType.MVar:
				return GetGenericParameter(GenericParameterType.Method, base.ReadCompressedUInt32());
			case ElementType.GenericInst:
			{
				bool num = base.ReadByte() == 17;
				TypeReference typeDefOrRef = GetTypeDefOrRef(ReadTypeTokenSignature());
				GenericInstanceType genericInstanceType = new GenericInstanceType(typeDefOrRef);
				ReadGenericInstanceSignature(typeDefOrRef, genericInstanceType);
				if (num)
				{
					genericInstanceType.KnownValueType();
					typeDefOrRef.GetElementType().KnownValueType();
				}
				return genericInstanceType;
			}
			case ElementType.Object:
				return TypeSystem.Object;
			case ElementType.Void:
				return TypeSystem.Void;
			case ElementType.TypedByRef:
				return TypeSystem.TypedReference;
			case ElementType.I:
				return TypeSystem.IntPtr;
			case ElementType.U:
				return TypeSystem.UIntPtr;
			default:
				return GetPrimitiveType(etype);
			}
		}

		public void ReadMethodSignature(IMethodSignature method)
		{
			byte b = base.ReadByte();
			if ((b & 0x20) != 0)
			{
				method.HasThis = true;
				b = (byte)(b & -33);
			}
			if ((b & 0x40) != 0)
			{
				method.ExplicitThis = true;
				b = (byte)(b & -65);
			}
			method.CallingConvention = (MethodCallingConvention)b;
			MethodReference methodReference = method as MethodReference;
			if (methodReference != null && !methodReference.DeclaringType.IsArray)
			{
				reader.context = methodReference;
			}
			if ((b & 0x10) != 0)
			{
				uint num = base.ReadCompressedUInt32();
				if (methodReference != null && !methodReference.IsDefinition)
				{
					CheckGenericContext(methodReference, (int)(num - 1));
				}
			}
			uint num2 = base.ReadCompressedUInt32();
			method.MethodReturnType.ReturnType = ReadTypeSignature();
			if (num2 != 0)
			{
				MethodReference methodReference2 = method as MethodReference;
				Collection<ParameterDefinition> collection = (methodReference2 == null) ? method.Parameters : (methodReference2.parameters = new ParameterDefinitionCollection(method, (int)num2));
				for (int i = 0; i < num2; i++)
				{
					collection.Add(new ParameterDefinition(ReadTypeSignature()));
				}
			}
		}

		public object ReadConstantSignature(ElementType type)
		{
			return ReadPrimitiveValue(type);
		}

		public void ReadCustomAttributeConstructorArguments(CustomAttribute attribute, Collection<ParameterDefinition> parameters)
		{
			int count = parameters.Count;
			if (count != 0)
			{
				attribute.arguments = new Collection<CustomAttributeArgument>(count);
				for (int i = 0; i < count; i++)
				{
					attribute.arguments.Add(ReadCustomAttributeFixedArgument(parameters[i].ParameterType));
				}
			}
		}

		private CustomAttributeArgument ReadCustomAttributeFixedArgument(TypeReference type)
		{
			if (type.IsArray)
			{
				return ReadCustomAttributeFixedArrayArgument((ArrayType)type);
			}
			return ReadCustomAttributeElement(type);
		}

		public void ReadCustomAttributeNamedArguments(ushort count, ref Collection<CustomAttributeNamedArgument> fields, ref Collection<CustomAttributeNamedArgument> properties)
		{
			for (int i = 0; i < count; i++)
			{
				if (!CanReadMore())
				{
					break;
				}
				ReadCustomAttributeNamedArgument(ref fields, ref properties);
			}
		}

		private void ReadCustomAttributeNamedArgument(ref Collection<CustomAttributeNamedArgument> fields, ref Collection<CustomAttributeNamedArgument> properties)
		{
			byte b = base.ReadByte();
			TypeReference type = ReadCustomAttributeFieldOrPropType();
			string name = ReadUTF8String();
			Collection<CustomAttributeNamedArgument> customAttributeNamedArgumentCollection;
			switch (b)
			{
			case 83:
				customAttributeNamedArgumentCollection = GetCustomAttributeNamedArgumentCollection(ref fields);
				break;
			case 84:
				customAttributeNamedArgumentCollection = GetCustomAttributeNamedArgumentCollection(ref properties);
				break;
			default:
				throw new NotSupportedException();
			}
			customAttributeNamedArgumentCollection.Add(new CustomAttributeNamedArgument(name, ReadCustomAttributeFixedArgument(type)));
		}

		private static Collection<CustomAttributeNamedArgument> GetCustomAttributeNamedArgumentCollection(ref Collection<CustomAttributeNamedArgument> collection)
		{
			if (collection != null)
			{
				return collection;
			}
			return collection = new Collection<CustomAttributeNamedArgument>();
		}

		private CustomAttributeArgument ReadCustomAttributeFixedArrayArgument(ArrayType type)
		{
			uint num = base.ReadUInt32();
			switch (num)
			{
			case uint.MaxValue:
				return new CustomAttributeArgument(type, null);
			case 0u:
				return new CustomAttributeArgument(type, Empty<CustomAttributeArgument>.Array);
			default:
			{
				CustomAttributeArgument[] array = new CustomAttributeArgument[num];
				TypeReference elementType = type.ElementType;
				for (int i = 0; i < num; i++)
				{
					array[i] = ReadCustomAttributeElement(elementType);
				}
				return new CustomAttributeArgument(type, array);
			}
			}
		}

		private CustomAttributeArgument ReadCustomAttributeElement(TypeReference type)
		{
			if (type.IsArray)
			{
				return ReadCustomAttributeFixedArrayArgument((ArrayType)type);
			}
			return new CustomAttributeArgument(type, (type.etype == ElementType.Object) ? ((object)ReadCustomAttributeElement(ReadCustomAttributeFieldOrPropType())) : ReadCustomAttributeElementValue(type));
		}

		private object ReadCustomAttributeElementValue(TypeReference type)
		{
			ElementType etype = type.etype;
			switch (etype)
			{
			case ElementType.String:
				return ReadUTF8String();
			case ElementType.None:
				if (type.IsTypeOf("System", "Type"))
				{
					return ReadTypeReference();
				}
				return ReadCustomAttributeEnum(type);
			default:
				return ReadPrimitiveValue(etype);
			}
		}

		private object ReadPrimitiveValue(ElementType type)
		{
			switch (type)
			{
			case ElementType.Boolean:
				return base.ReadByte() == 1;
			case ElementType.I1:
				return (sbyte)base.ReadByte();
			case ElementType.U1:
				return base.ReadByte();
			case ElementType.Char:
				return (char)base.ReadUInt16();
			case ElementType.I2:
				return base.ReadInt16();
			case ElementType.U2:
				return base.ReadUInt16();
			case ElementType.I4:
				return base.ReadInt32();
			case ElementType.U4:
				return base.ReadUInt32();
			case ElementType.I8:
				return base.ReadInt64();
			case ElementType.U8:
				return base.ReadUInt64();
			case ElementType.R4:
				return base.ReadSingle();
			case ElementType.R8:
				return base.ReadDouble();
			default:
				throw new NotImplementedException(type.ToString());
			}
		}

		private TypeReference GetPrimitiveType(ElementType etype)
		{
			switch (etype)
			{
			case ElementType.Boolean:
				return TypeSystem.Boolean;
			case ElementType.Char:
				return TypeSystem.Char;
			case ElementType.I1:
				return TypeSystem.SByte;
			case ElementType.U1:
				return TypeSystem.Byte;
			case ElementType.I2:
				return TypeSystem.Int16;
			case ElementType.U2:
				return TypeSystem.UInt16;
			case ElementType.I4:
				return TypeSystem.Int32;
			case ElementType.U4:
				return TypeSystem.UInt32;
			case ElementType.I8:
				return TypeSystem.Int64;
			case ElementType.U8:
				return TypeSystem.UInt64;
			case ElementType.R4:
				return TypeSystem.Single;
			case ElementType.R8:
				return TypeSystem.Double;
			case ElementType.String:
				return TypeSystem.String;
			default:
				throw new NotImplementedException(etype.ToString());
			}
		}

		private TypeReference ReadCustomAttributeFieldOrPropType()
		{
			ElementType elementType = (ElementType)base.ReadByte();
			switch (elementType)
			{
			case ElementType.Boxed:
				return TypeSystem.Object;
			case ElementType.SzArray:
				return new ArrayType(ReadCustomAttributeFieldOrPropType());
			case ElementType.Enum:
				return ReadTypeReference();
			case ElementType.Type:
				return TypeSystem.LookupType("System", "Type");
			default:
				return GetPrimitiveType(elementType);
			}
		}

		public TypeReference ReadTypeReference()
		{
			return TypeParser.ParseType(reader.module, ReadUTF8String(), false);
		}

		private object ReadCustomAttributeEnum(TypeReference enum_type)
		{
			TypeDefinition typeDefinition = enum_type.CheckedResolve();
			if (!typeDefinition.IsEnum)
			{
				throw new ArgumentException();
			}
			return ReadCustomAttributeElementValue(typeDefinition.GetEnumUnderlyingType());
		}

		public SecurityAttribute ReadSecurityAttribute()
		{
			SecurityAttribute securityAttribute = new SecurityAttribute(ReadTypeReference());
			base.ReadCompressedUInt32();
			ReadCustomAttributeNamedArguments((ushort)base.ReadCompressedUInt32(), ref securityAttribute.fields, ref securityAttribute.properties);
			return securityAttribute;
		}

		public MarshalInfo ReadMarshalInfo()
		{
			NativeType nativeType = ReadNativeType();
			switch (nativeType)
			{
			case NativeType.Array:
			{
				ArrayMarshalInfo arrayMarshalInfo = new ArrayMarshalInfo();
				if (CanReadMore())
				{
					arrayMarshalInfo.element_type = ReadNativeType();
				}
				if (CanReadMore())
				{
					arrayMarshalInfo.size_parameter_index = (int)base.ReadCompressedUInt32();
				}
				if (CanReadMore())
				{
					arrayMarshalInfo.size = (int)base.ReadCompressedUInt32();
				}
				if (CanReadMore())
				{
					arrayMarshalInfo.size_parameter_multiplier = (int)base.ReadCompressedUInt32();
				}
				return arrayMarshalInfo;
			}
			case NativeType.SafeArray:
			{
				SafeArrayMarshalInfo safeArrayMarshalInfo = new SafeArrayMarshalInfo();
				if (CanReadMore())
				{
					safeArrayMarshalInfo.element_type = ReadVariantType();
				}
				return safeArrayMarshalInfo;
			}
			case NativeType.FixedArray:
			{
				FixedArrayMarshalInfo fixedArrayMarshalInfo = new FixedArrayMarshalInfo();
				if (CanReadMore())
				{
					fixedArrayMarshalInfo.size = (int)base.ReadCompressedUInt32();
				}
				if (CanReadMore())
				{
					fixedArrayMarshalInfo.element_type = ReadNativeType();
				}
				return fixedArrayMarshalInfo;
			}
			case NativeType.FixedSysString:
			{
				FixedSysStringMarshalInfo fixedSysStringMarshalInfo = new FixedSysStringMarshalInfo();
				if (CanReadMore())
				{
					fixedSysStringMarshalInfo.size = (int)base.ReadCompressedUInt32();
				}
				return fixedSysStringMarshalInfo;
			}
			case NativeType.CustomMarshaler:
			{
				CustomMarshalInfo customMarshalInfo = new CustomMarshalInfo();
				string text = ReadUTF8String();
				customMarshalInfo.guid = ((!string.IsNullOrEmpty(text)) ? new Guid(text) : Guid.Empty);
				customMarshalInfo.unmanaged_type = ReadUTF8String();
				customMarshalInfo.managed_type = ReadTypeReference();
				customMarshalInfo.cookie = ReadUTF8String();
				return customMarshalInfo;
			}
			default:
				return new MarshalInfo(nativeType);
			}
		}

		private NativeType ReadNativeType()
		{
			return (NativeType)base.ReadByte();
		}

		private VariantType ReadVariantType()
		{
			return (VariantType)base.ReadByte();
		}

		private string ReadUTF8String()
		{
			if (base.buffer[base.position] == 255)
			{
				base.position++;
				return null;
			}
			int num = (int)base.ReadCompressedUInt32();
			if (num == 0)
			{
				return string.Empty;
			}
			string @string = Encoding.UTF8.GetString(base.buffer, base.position, (base.buffer[base.position + num - 1] == 0) ? (num - 1) : num);
			base.position += num;
			return @string;
		}

		public string ReadDocumentName()
		{
			char c = (char)base.buffer[base.position];
			base.position++;
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			while (CanReadMore())
			{
				if (num > 0 && c != 0)
				{
					stringBuilder.Append(c);
				}
				uint num2 = base.ReadCompressedUInt32();
				if (num2 != 0)
				{
					stringBuilder.Append(reader.ReadUTF8StringBlob(num2));
				}
				num++;
			}
			return stringBuilder.ToString();
		}

		public Collection<SequencePoint> ReadSequencePoints(Document document)
		{
			Collection<SequencePoint> collection = new Collection<SequencePoint>();
			base.ReadCompressedUInt32();
			if (document == null)
			{
				document = reader.GetDocument(base.ReadCompressedUInt32());
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			bool flag = true;
			int num4 = 0;
			while (CanReadMore())
			{
				int num5 = (int)base.ReadCompressedUInt32();
				if (num4 > 0 && num5 == 0)
				{
					document = reader.GetDocument(base.ReadCompressedUInt32());
				}
				else
				{
					num += num5;
					int num6 = (int)base.ReadCompressedUInt32();
					int num7 = (num6 == 0) ? ((int)base.ReadCompressedUInt32()) : base.ReadCompressedInt32();
					if (num6 == 0 && num7 == 0)
					{
						collection.Add(new SequencePoint(num, document)
						{
							StartLine = 16707566,
							EndLine = 16707566,
							StartColumn = 0,
							EndColumn = 0
						});
					}
					else
					{
						if (flag)
						{
							num2 = (int)base.ReadCompressedUInt32();
							num3 = (int)base.ReadCompressedUInt32();
						}
						else
						{
							num2 += base.ReadCompressedInt32();
							num3 += base.ReadCompressedInt32();
						}
						collection.Add(new SequencePoint(num, document)
						{
							StartLine = num2,
							StartColumn = num3,
							EndLine = num2 + num6,
							EndColumn = num3 + num7
						});
						flag = false;
					}
				}
				num4++;
			}
			return collection;
		}

		public bool CanReadMore()
		{
			return base.position - start < sig_length;
		}
	}
}
