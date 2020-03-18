using Mono.Cecil.Metadata;
using Mono.Collections.Generic;
using System;
using System.Text;

namespace Mono.Cecil
{
	internal class TypeParser
	{
		private class Type
		{
			public const int Ptr = -1;

			public const int ByRef = -2;

			public const int SzArray = -3;

			public string type_fullname;

			public string[] nested_names;

			public int arity;

			public int[] specs;

			public Type[] generic_arguments;

			public string assembly;
		}

		private readonly string fullname;

		private readonly int length;

		private int position;

		private TypeParser(string fullname)
		{
			this.fullname = fullname;
			length = fullname.Length;
		}

		private Type ParseType(bool fq_name)
		{
			Type type = new Type();
			type.type_fullname = ParsePart();
			type.nested_names = ParseNestedNames();
			if (TryGetArity(type))
			{
				type.generic_arguments = ParseGenericArguments(type.arity);
			}
			type.specs = ParseSpecs();
			if (fq_name)
			{
				type.assembly = ParseAssemblyName();
			}
			return type;
		}

		private static bool TryGetArity(Type type)
		{
			int num = 0;
			TryAddArity(type.type_fullname, ref num);
			string[] nested_names = type.nested_names;
			if (!nested_names.IsNullOrEmpty())
			{
				for (int i = 0; i < nested_names.Length; i++)
				{
					TryAddArity(nested_names[i], ref num);
				}
			}
			type.arity = num;
			return num > 0;
		}

		private static bool TryGetArity(string name, out int arity)
		{
			arity = 0;
			int num = name.LastIndexOf('`');
			if (num == -1)
			{
				return false;
			}
			return ParseInt32(name.Substring(num + 1), out arity);
		}

		private static bool ParseInt32(string value, out int result)
		{
			return int.TryParse(value, out result);
		}

		private static void TryAddArity(string name, ref int arity)
		{
			if (TryGetArity(name, out int num))
			{
				arity += num;
			}
		}

		private string ParsePart()
		{
			StringBuilder stringBuilder = new StringBuilder();
			while (position < length && !IsDelimiter(fullname[position]))
			{
				if (fullname[position] == '\\')
				{
					position++;
				}
				stringBuilder.Append(fullname[position++]);
			}
			return stringBuilder.ToString();
		}

		private static bool IsDelimiter(char chr)
		{
			return "+,[]*&".IndexOf(chr) != -1;
		}

		private void TryParseWhiteSpace()
		{
			while (position < length && char.IsWhiteSpace(fullname[position]))
			{
				position++;
			}
		}

		private string[] ParseNestedNames()
		{
			string[] result = null;
			while (TryParse('+'))
			{
				Add(ref result, ParsePart());
			}
			return result;
		}

		private bool TryParse(char chr)
		{
			if (position < length && fullname[position] == chr)
			{
				position++;
				return true;
			}
			return false;
		}

		private static void Add<T>(ref T[] array, T item)
		{
			array = array.Add(item);
		}

		private int[] ParseSpecs()
		{
			int[] result = null;
			while (position < length)
			{
				switch (fullname[position])
				{
				case '*':
					position++;
					Add(ref result, -1);
					break;
				case '&':
					position++;
					Add(ref result, -2);
					break;
				case '[':
					position++;
					switch (fullname[position])
					{
					case ']':
						position++;
						Add(ref result, -3);
						break;
					case '*':
						position++;
						Add(ref result, 1);
						break;
					default:
					{
						int num = 1;
						while (TryParse(','))
						{
							num++;
						}
						Add(ref result, num);
						TryParse(']');
						break;
					}
					}
					break;
				default:
					return result;
				}
			}
			return result;
		}

		private Type[] ParseGenericArguments(int arity)
		{
			Type[] result = null;
			if (position != length && fullname[position] == '[')
			{
				TryParse('[');
				for (int i = 0; i < arity; i++)
				{
					bool flag = TryParse('[');
					Add(ref result, ParseType(flag));
					if (flag)
					{
						TryParse(']');
					}
					TryParse(',');
					TryParseWhiteSpace();
				}
				TryParse(']');
				return result;
			}
			return result;
		}

		private string ParseAssemblyName()
		{
			if (!TryParse(','))
			{
				return string.Empty;
			}
			TryParseWhiteSpace();
			int num = position;
			while (position < length)
			{
				char c = fullname[position];
				if (c == '[')
				{
					break;
				}
				if (c == ']')
				{
					break;
				}
				position++;
			}
			return fullname.Substring(num, position - num);
		}

		public static TypeReference ParseType(ModuleDefinition module, string fullname, bool typeDefinitionOnly = false)
		{
			if (string.IsNullOrEmpty(fullname))
			{
				return null;
			}
			TypeParser typeParser = new TypeParser(fullname);
			return GetTypeReference(module, typeParser.ParseType(true), typeDefinitionOnly);
		}

		private static TypeReference GetTypeReference(ModuleDefinition module, Type type_info, bool type_def_only)
		{
			if (!TryGetDefinition(module, type_info, out TypeReference type))
			{
				if (type_def_only)
				{
					return null;
				}
				type = CreateReference(type_info, module, GetMetadataScope(module, type_info));
			}
			return CreateSpecs(type, type_info);
		}

		private static TypeReference CreateSpecs(TypeReference type, Type type_info)
		{
			type = TryCreateGenericInstanceType(type, type_info);
			int[] specs = type_info.specs;
			if (specs.IsNullOrEmpty())
			{
				return type;
			}
			for (int i = 0; i < specs.Length; i++)
			{
				switch (specs[i])
				{
				case -1:
					type = new PointerType(type);
					break;
				case -2:
					type = new ByReferenceType(type);
					break;
				case -3:
					type = new ArrayType(type);
					break;
				default:
				{
					ArrayType arrayType = new ArrayType(type);
					arrayType.Dimensions.Clear();
					for (int j = 0; j < specs[i]; j++)
					{
						arrayType.Dimensions.Add(default(ArrayDimension));
					}
					type = arrayType;
					break;
				}
				}
			}
			return type;
		}

		private static TypeReference TryCreateGenericInstanceType(TypeReference type, Type type_info)
		{
			Type[] generic_arguments = type_info.generic_arguments;
			if (generic_arguments.IsNullOrEmpty())
			{
				return type;
			}
			GenericInstanceType genericInstanceType = new GenericInstanceType(type);
			Collection<TypeReference> genericArguments = genericInstanceType.GenericArguments;
			for (int i = 0; i < generic_arguments.Length; i++)
			{
				genericArguments.Add(GetTypeReference(type.Module, generic_arguments[i], false));
			}
			return genericInstanceType;
		}

		public static void SplitFullName(string fullname, out string @namespace, out string name)
		{
			int num = fullname.LastIndexOf('.');
			if (num == -1)
			{
				@namespace = string.Empty;
				name = fullname;
			}
			else
			{
				@namespace = fullname.Substring(0, num);
				name = fullname.Substring(num + 1);
			}
		}

		private static TypeReference CreateReference(Type type_info, ModuleDefinition module, IMetadataScope scope)
		{
			SplitFullName(type_info.type_fullname, out string @namespace, out string name);
			TypeReference typeReference = new TypeReference(@namespace, name, module, scope);
			MetadataSystem.TryProcessPrimitiveTypeReference(typeReference);
			AdjustGenericParameters(typeReference);
			string[] nested_names = type_info.nested_names;
			if (nested_names.IsNullOrEmpty())
			{
				return typeReference;
			}
			for (int i = 0; i < nested_names.Length; i++)
			{
				typeReference = new TypeReference(string.Empty, nested_names[i], module, null)
				{
					DeclaringType = typeReference
				};
				AdjustGenericParameters(typeReference);
			}
			return typeReference;
		}

		private static void AdjustGenericParameters(TypeReference type)
		{
			if (TryGetArity(type.Name, out int num))
			{
				for (int i = 0; i < num; i++)
				{
					type.GenericParameters.Add(new GenericParameter(type));
				}
			}
		}

		private static IMetadataScope GetMetadataScope(ModuleDefinition module, Type type_info)
		{
			if (string.IsNullOrEmpty(type_info.assembly))
			{
				return module.TypeSystem.CoreLibrary;
			}
			AssemblyNameReference assemblyNameReference = AssemblyNameReference.Parse(type_info.assembly);
			if (!module.TryGetAssemblyNameReference(assemblyNameReference, out AssemblyNameReference result))
			{
				return assemblyNameReference;
			}
			return result;
		}

		private static bool TryGetDefinition(ModuleDefinition module, Type type_info, out TypeReference type)
		{
			type = null;
			if (!TryCurrentModule(module, type_info))
			{
				return false;
			}
			TypeDefinition typeDefinition = module.GetType(type_info.type_fullname);
			if (typeDefinition == null)
			{
				return false;
			}
			string[] nested_names = type_info.nested_names;
			if (!nested_names.IsNullOrEmpty())
			{
				for (int i = 0; i < nested_names.Length; i++)
				{
					TypeDefinition nestedType = typeDefinition.GetNestedType(nested_names[i]);
					if (nestedType == null)
					{
						return false;
					}
					typeDefinition = nestedType;
				}
			}
			type = typeDefinition;
			return true;
		}

		private static bool TryCurrentModule(ModuleDefinition module, Type type_info)
		{
			if (string.IsNullOrEmpty(type_info.assembly))
			{
				return true;
			}
			if (module.assembly != null && module.assembly.Name.FullName == type_info.assembly)
			{
				return true;
			}
			return false;
		}

		public static string ToParseable(TypeReference type)
		{
			if (type == null)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			AppendType(type, stringBuilder, true, true);
			return stringBuilder.ToString();
		}

		private static void AppendNamePart(string part, StringBuilder name)
		{
			foreach (char c in part)
			{
				if (IsDelimiter(c))
				{
					name.Append('\\');
				}
				name.Append(c);
			}
		}

		private static void AppendType(TypeReference type, StringBuilder name, bool fq_name, bool top_level)
		{
			TypeReference elementType = type.GetElementType();
			TypeReference declaringType = elementType.DeclaringType;
			if (declaringType != null)
			{
				AppendType(declaringType, name, false, top_level);
				name.Append('+');
			}
			string @namespace = type.Namespace;
			if (!string.IsNullOrEmpty(@namespace))
			{
				AppendNamePart(@namespace, name);
				name.Append('.');
			}
			AppendNamePart(elementType.Name, name);
			if (fq_name)
			{
				if (type.IsTypeSpecification())
				{
					AppendTypeSpecification((TypeSpecification)type, name);
				}
				if (RequiresFullyQualifiedName(type, top_level))
				{
					name.Append(", ");
					name.Append(GetScopeFullName(type));
				}
			}
		}

		private static string GetScopeFullName(TypeReference type)
		{
			IMetadataScope scope = type.Scope;
			switch (scope.MetadataScopeType)
			{
			case MetadataScopeType.AssemblyNameReference:
				return ((AssemblyNameReference)scope).FullName;
			case MetadataScopeType.ModuleDefinition:
				return ((ModuleDefinition)scope).Assembly.Name.FullName;
			default:
				throw new ArgumentException();
			}
		}

		private static void AppendTypeSpecification(TypeSpecification type, StringBuilder name)
		{
			if (type.ElementType.IsTypeSpecification())
			{
				AppendTypeSpecification((TypeSpecification)type.ElementType, name);
			}
			switch (type.etype)
			{
			case ElementType.Ptr:
				name.Append('*');
				break;
			case ElementType.ByRef:
				name.Append('&');
				break;
			case ElementType.Array:
			case ElementType.SzArray:
			{
				ArrayType arrayType = (ArrayType)type;
				if (arrayType.IsVector)
				{
					name.Append("[]");
				}
				else
				{
					name.Append('[');
					for (int j = 1; j < arrayType.Rank; j++)
					{
						name.Append(',');
					}
					name.Append(']');
				}
				break;
			}
			case ElementType.GenericInst:
			{
				Collection<TypeReference> genericArguments = ((GenericInstanceType)type).GenericArguments;
				name.Append('[');
				for (int i = 0; i < genericArguments.Count; i++)
				{
					if (i > 0)
					{
						name.Append(',');
					}
					TypeReference typeReference = genericArguments[i];
					bool num = typeReference.Scope != typeReference.Module;
					if (num)
					{
						name.Append('[');
					}
					AppendType(typeReference, name, true, false);
					if (num)
					{
						name.Append(']');
					}
				}
				name.Append(']');
				break;
			}
			}
		}

		private static bool RequiresFullyQualifiedName(TypeReference type, bool top_level)
		{
			if (type.Scope == type.Module)
			{
				return false;
			}
			if (type.Scope.Name == "mscorlib" & top_level)
			{
				return false;
			}
			return true;
		}
	}
}
