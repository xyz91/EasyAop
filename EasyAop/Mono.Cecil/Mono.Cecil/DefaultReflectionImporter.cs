using Mono.Cecil.Metadata;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mono.Cecil
{
	public class DefaultReflectionImporter : IReflectionImporter
	{
		private enum ImportGenericKind
		{
			Definition,
			Open
		}

		protected readonly ModuleDefinition module;

		private static readonly Dictionary<Type, ElementType> type_etype_mapping = new Dictionary<Type, ElementType>(18)
		{
			{
				typeof(void),
				ElementType.Void
			},
			{
				typeof(bool),
				ElementType.Boolean
			},
			{
				typeof(char),
				ElementType.Char
			},
			{
				typeof(sbyte),
				ElementType.I1
			},
			{
				typeof(byte),
				ElementType.U1
			},
			{
				typeof(short),
				ElementType.I2
			},
			{
				typeof(ushort),
				ElementType.U2
			},
			{
				typeof(int),
				ElementType.I4
			},
			{
				typeof(uint),
				ElementType.U4
			},
			{
				typeof(long),
				ElementType.I8
			},
			{
				typeof(ulong),
				ElementType.U8
			},
			{
				typeof(float),
				ElementType.R4
			},
			{
				typeof(double),
				ElementType.R8
			},
			{
				typeof(string),
				ElementType.String
			},
			{
				typeof(TypedReference),
				ElementType.TypedByRef
			},
			{
				typeof(IntPtr),
				ElementType.I
			},
			{
				typeof(UIntPtr),
				ElementType.U
			},
			{
				typeof(object),
				ElementType.Object
			}
		};

		public DefaultReflectionImporter(ModuleDefinition module)
		{
			Mixin.CheckModule(module);
			this.module = module;
		}

		private TypeReference ImportType(Type type, ImportGenericContext context)
		{
			return ImportType(type, context, ImportGenericKind.Open);
		}

		private TypeReference ImportType(Type type, ImportGenericContext context, ImportGenericKind import_kind)
		{
			if (!IsTypeSpecification(type) && !ImportOpenGenericType(type, import_kind))
			{
				TypeReference typeReference = new TypeReference(string.Empty, type.Name, module, ImportScope(type), type.IsValueType());
				typeReference.etype = ImportElementType(type);
				if (IsNestedType(type))
				{
					typeReference.DeclaringType = ImportType(type.DeclaringType, context, import_kind);
				}
				else
				{
					typeReference.Namespace = (type.Namespace ?? string.Empty);
				}
				if (type.IsGenericType())
				{
					ImportGenericParameters(typeReference, type.GetGenericArguments());
				}
				return typeReference;
			}
			return ImportTypeSpecification(type, context);
		}

		protected virtual IMetadataScope ImportScope(Type type)
		{
			return ImportScope(type.Assembly());
		}

		private static bool ImportOpenGenericType(Type type, ImportGenericKind import_kind)
		{
			if (type.IsGenericType() && type.IsGenericTypeDefinition())
			{
				return import_kind == ImportGenericKind.Open;
			}
			return false;
		}

		private static bool ImportOpenGenericMethod(MethodBase method, ImportGenericKind import_kind)
		{
			if (method.IsGenericMethod && method.IsGenericMethodDefinition)
			{
				return import_kind == ImportGenericKind.Open;
			}
			return false;
		}

		private static bool IsNestedType(Type type)
		{
			return type.IsNested;
		}

		private TypeReference ImportTypeSpecification(Type type, ImportGenericContext context)
		{
			if (type.IsByRef)
			{
				return new ByReferenceType(ImportType(type.GetElementType(), context));
			}
			if (type.IsPointer)
			{
				return new PointerType(ImportType(type.GetElementType(), context));
			}
			if (type.IsArray)
			{
				return new ArrayType(ImportType(type.GetElementType(), context), type.GetArrayRank());
			}
			if (type.IsGenericType())
			{
				return ImportGenericInstance(type, context);
			}
			if (type.IsGenericParameter)
			{
				return ImportGenericParameter(type, context);
			}
			throw new NotSupportedException(type.FullName);
		}

		private static TypeReference ImportGenericParameter(Type type, ImportGenericContext context)
		{
			if (context.IsEmpty)
			{
				throw new InvalidOperationException();
			}
			if (type.DeclaringMethod() != (MethodBase)null)
			{
				return context.MethodParameter(NormalizeMethodName(type.DeclaringMethod()), type.GenericParameterPosition);
			}
			if (type.DeclaringType != (Type)null)
			{
				return context.TypeParameter(NormalizeTypeFullName(type.DeclaringType), type.GenericParameterPosition);
			}
			throw new InvalidOperationException();
		}

		private static string NormalizeMethodName(MethodBase method)
		{
			return NormalizeTypeFullName(method.DeclaringType) + "." + method.Name;
		}

		private static string NormalizeTypeFullName(Type type)
		{
			if (IsNestedType(type))
			{
				return NormalizeTypeFullName(type.DeclaringType) + "/" + type.Name;
			}
			return type.FullName;
		}

		private TypeReference ImportGenericInstance(Type type, ImportGenericContext context)
		{
			TypeReference typeReference = ImportType(type.GetGenericTypeDefinition(), context, ImportGenericKind.Definition);
			GenericInstanceType genericInstanceType = new GenericInstanceType(typeReference);
			Type[] genericArguments = type.GetGenericArguments();
			Collection<TypeReference> genericArguments2 = genericInstanceType.GenericArguments;
			context.Push(typeReference);
			try
			{
				for (int i = 0; i < genericArguments.Length; i++)
				{
					genericArguments2.Add(ImportType(genericArguments[i], context));
				}
				return genericInstanceType;
			}
			finally
			{
				context.Pop();
			}
		}

		private static bool IsTypeSpecification(Type type)
		{
			if (!type.HasElementType && !IsGenericInstance(type))
			{
				return type.IsGenericParameter;
			}
			return true;
		}

		private static bool IsGenericInstance(Type type)
		{
			if (type.IsGenericType())
			{
				return !type.IsGenericTypeDefinition();
			}
			return false;
		}

		private static ElementType ImportElementType(Type type)
		{
			if (!type_etype_mapping.TryGetValue(type, out ElementType result))
			{
				return ElementType.None;
			}
			return result;
		}

		protected AssemblyNameReference ImportScope(Assembly assembly)
		{
			return ImportReference(assembly.GetName());
		}

		public virtual AssemblyNameReference ImportReference(AssemblyName name)
		{
			Mixin.CheckName(name);
			if (TryGetAssemblyNameReference(name, out AssemblyNameReference assemblyNameReference))
			{
				return assemblyNameReference;
			}
			assemblyNameReference = new AssemblyNameReference(name.Name, name.Version)
			{
				PublicKeyToken = name.GetPublicKeyToken(),
				Culture = name.CultureInfo.Name,
				HashAlgorithm =(AssemblyHashAlgorithm)(int)name.HashAlgorithm
			};
			module.AssemblyReferences.Add(assemblyNameReference);
			return assemblyNameReference;
		}

		private bool TryGetAssemblyNameReference(AssemblyName name, out AssemblyNameReference assembly_reference)
		{
			Collection<AssemblyNameReference> assemblyReferences = module.AssemblyReferences;
			for (int i = 0; i < assemblyReferences.Count; i++)
			{
				AssemblyNameReference assemblyNameReference = assemblyReferences[i];
				if (!(name.FullName != assemblyNameReference.FullName))
				{
					assembly_reference = assemblyNameReference;
					return true;
				}
			}
			assembly_reference = null;
			return false;
		}

		private FieldReference ImportField(FieldInfo field, ImportGenericContext context)
		{
			TypeReference typeReference = ImportType(field.DeclaringType, context);
			if (IsGenericInstance(field.DeclaringType))
			{
				field = ResolveFieldDefinition(field);
			}
			context.Push(typeReference);
			try
			{
				return new FieldReference
				{
					Name = field.Name,
					DeclaringType = typeReference,
					FieldType = ImportType(field.FieldType, context)
				};
			}
			finally
			{
				context.Pop();
			}
		}

		private static FieldInfo ResolveFieldDefinition(FieldInfo field)
		{
			return field.Module.ResolveField(field.MetadataToken);
		}

		private static MethodBase ResolveMethodDefinition(MethodBase method)
		{
			return method.Module.ResolveMethod(method.MetadataToken);
		}

		private MethodReference ImportMethod(MethodBase method, ImportGenericContext context, ImportGenericKind import_kind)
		{
			if (!IsMethodSpecification(method) && !ImportOpenGenericMethod(method, import_kind))
			{
				TypeReference declaringType = ImportType(method.DeclaringType, context);
				if (IsGenericInstance(method.DeclaringType))
				{
					method = ResolveMethodDefinition(method);
				}
				MethodReference methodReference = new MethodReference
				{
					Name = method.Name,
					HasThis = HasCallingConvention(method, CallingConventions.HasThis),
					ExplicitThis = HasCallingConvention(method, CallingConventions.ExplicitThis),
					DeclaringType = ImportType(method.DeclaringType, context, ImportGenericKind.Definition)
				};
				if (HasCallingConvention(method, CallingConventions.VarArgs))
				{
					methodReference.CallingConvention &= MethodCallingConvention.VarArg;
				}
				if (method.IsGenericMethod)
				{
					ImportGenericParameters(methodReference, method.GetGenericArguments());
				}
				context.Push(methodReference);
				try
				{
					MethodInfo methodInfo = method as MethodInfo;
					methodReference.ReturnType = ((methodInfo != (MethodInfo)null) ? ImportType(methodInfo.ReturnType, context) : ImportType(typeof(void), default(ImportGenericContext)));
					ParameterInfo[] parameters = method.GetParameters();
					Collection<ParameterDefinition> parameters2 = methodReference.Parameters;
					for (int i = 0; i < parameters.Length; i++)
					{
						parameters2.Add(new ParameterDefinition(ImportType(parameters[i].ParameterType, context)));
					}
					methodReference.DeclaringType = declaringType;
					return methodReference;
				}
				finally
				{
					context.Pop();
				}
			}
			return ImportMethodSpecification(method, context);
		}

		private static void ImportGenericParameters(IGenericParameterProvider provider, Type[] arguments)
		{
			Collection<GenericParameter> genericParameters = provider.GenericParameters;
			for (int i = 0; i < arguments.Length; i++)
			{
				genericParameters.Add(new GenericParameter(arguments[i].Name, provider));
			}
		}

		private static bool IsMethodSpecification(MethodBase method)
		{
			if (method.IsGenericMethod)
			{
				return !method.IsGenericMethodDefinition;
			}
			return false;
		}

		private MethodReference ImportMethodSpecification(MethodBase method, ImportGenericContext context)
		{
			MethodInfo methodInfo = method as MethodInfo;
			if (methodInfo == (MethodInfo)null)
			{
				throw new InvalidOperationException();
			}
			MethodReference methodReference = ImportMethod(methodInfo.GetGenericMethodDefinition(), context, ImportGenericKind.Definition);
			GenericInstanceMethod genericInstanceMethod = new GenericInstanceMethod(methodReference);
			Type[] genericArguments = method.GetGenericArguments();
			Collection<TypeReference> genericArguments2 = genericInstanceMethod.GenericArguments;
			context.Push(methodReference);
			try
			{
				for (int i = 0; i < genericArguments.Length; i++)
				{
					genericArguments2.Add(ImportType(genericArguments[i], context));
				}
				return genericInstanceMethod;
			}
			finally
			{
				context.Pop();
			}
		}

		private static bool HasCallingConvention(MethodBase method, CallingConventions conventions)
		{
			return (method.CallingConvention & conventions) != (CallingConventions)0;
		}

		public virtual TypeReference ImportReference(Type type, IGenericParameterProvider context)
		{
			Mixin.CheckType(type);
			return ImportType(type, ImportGenericContext.For(context), (context != null) ? ImportGenericKind.Open : ImportGenericKind.Definition);
		}

		public virtual FieldReference ImportReference(FieldInfo field, IGenericParameterProvider context)
		{
			Mixin.CheckField(field);
			return ImportField(field, ImportGenericContext.For(context));
		}

		public virtual MethodReference ImportReference(MethodBase method, IGenericParameterProvider context)
		{
			Mixin.CheckMethod(method);
			return ImportMethod(method, ImportGenericContext.For(context), (context != null) ? ImportGenericKind.Open : ImportGenericKind.Definition);
		}
	}
}
