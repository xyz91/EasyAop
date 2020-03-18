using Mono.Cecil.Metadata;
using Mono.Collections.Generic;
using System;

namespace Mono.Cecil
{
	public class DefaultMetadataImporter : IMetadataImporter
	{
		protected readonly ModuleDefinition module;

		public DefaultMetadataImporter(ModuleDefinition module)
		{
			Mixin.CheckModule(module);
			this.module = module;
		}

		private TypeReference ImportType(TypeReference type, ImportGenericContext context)
		{
			if (type.IsTypeSpecification())
			{
				return ImportTypeSpecification(type, context);
			}
			TypeReference typeReference = new TypeReference(type.Namespace, type.Name, module, ImportScope(type), type.IsValueType);
			MetadataSystem.TryProcessPrimitiveTypeReference(typeReference);
			if (type.IsNested)
			{
				typeReference.DeclaringType = ImportType(type.DeclaringType, context);
			}
			if (type.HasGenericParameters)
			{
				ImportGenericParameters(typeReference, type);
			}
			return typeReference;
		}

		protected virtual IMetadataScope ImportScope(TypeReference type)
		{
			return ImportScope(type.Scope);
		}

		protected IMetadataScope ImportScope(IMetadataScope scope)
		{
			switch (scope.MetadataScopeType)
			{
			case MetadataScopeType.AssemblyNameReference:
				return ImportReference((AssemblyNameReference)scope);
			case MetadataScopeType.ModuleDefinition:
				if (scope == module)
				{
					return scope;
				}
				return ImportReference(((ModuleDefinition)scope).Assembly.Name);
			case MetadataScopeType.ModuleReference:
				throw new NotImplementedException();
			default:
				throw new NotSupportedException();
			}
		}

		public virtual AssemblyNameReference ImportReference(AssemblyNameReference name)
		{
			Mixin.CheckName(name);
			if (module.TryGetAssemblyNameReference(name, out AssemblyNameReference assemblyNameReference))
			{
				return assemblyNameReference;
			}
			assemblyNameReference = new AssemblyNameReference(name.Name, name.Version)
			{
				Culture = name.Culture,
				HashAlgorithm = name.HashAlgorithm,
				IsRetargetable = name.IsRetargetable,
				IsWindowsRuntime = name.IsWindowsRuntime
			};
			byte[] array = (!name.PublicKeyToken.IsNullOrEmpty()) ? new byte[name.PublicKeyToken.Length] : Empty<byte>.Array;
			if (array.Length != 0)
			{
				Buffer.BlockCopy(name.PublicKeyToken, 0, array, 0, array.Length);
			}
			assemblyNameReference.PublicKeyToken = array;
			module.AssemblyReferences.Add(assemblyNameReference);
			return assemblyNameReference;
		}

		private static void ImportGenericParameters(IGenericParameterProvider imported, IGenericParameterProvider original)
		{
			Collection<GenericParameter> genericParameters = original.GenericParameters;
			Collection<GenericParameter> genericParameters2 = imported.GenericParameters;
			for (int i = 0; i < genericParameters.Count; i++)
			{
				genericParameters2.Add(new GenericParameter(genericParameters[i].Name, imported));
			}
		}

		private TypeReference ImportTypeSpecification(TypeReference type, ImportGenericContext context)
		{
			switch (type.etype)
			{
			case ElementType.SzArray:
			{
				ArrayType arrayType = (ArrayType)type;
				return new ArrayType(ImportType(arrayType.ElementType, context));
			}
			case ElementType.Ptr:
			{
				PointerType pointerType = (PointerType)type;
				return new PointerType(ImportType(pointerType.ElementType, context));
			}
			case ElementType.ByRef:
			{
				ByReferenceType byReferenceType = (ByReferenceType)type;
				return new ByReferenceType(ImportType(byReferenceType.ElementType, context));
			}
			case ElementType.Pinned:
			{
				PinnedType pinnedType = (PinnedType)type;
				return new PinnedType(ImportType(pinnedType.ElementType, context));
			}
			case ElementType.Sentinel:
			{
				SentinelType sentinelType = (SentinelType)type;
				return new SentinelType(ImportType(sentinelType.ElementType, context));
			}
			case ElementType.FnPtr:
			{
				FunctionPointerType functionPointerType = (FunctionPointerType)type;
				FunctionPointerType functionPointerType2 = new FunctionPointerType
				{
					HasThis = functionPointerType.HasThis,
					ExplicitThis = functionPointerType.ExplicitThis,
					CallingConvention = functionPointerType.CallingConvention,
					ReturnType = ImportType(functionPointerType.ReturnType, context)
				};
				if (!functionPointerType.HasParameters)
				{
					return functionPointerType2;
				}
				for (int j = 0; j < functionPointerType.Parameters.Count; j++)
				{
					functionPointerType2.Parameters.Add(new ParameterDefinition(ImportType(functionPointerType.Parameters[j].ParameterType, context)));
				}
				return functionPointerType2;
			}
			case ElementType.CModOpt:
			{
				OptionalModifierType optionalModifierType = (OptionalModifierType)type;
				return new OptionalModifierType(ImportType(optionalModifierType.ModifierType, context), ImportType(optionalModifierType.ElementType, context));
			}
			case ElementType.CModReqD:
			{
				RequiredModifierType requiredModifierType = (RequiredModifierType)type;
				return new RequiredModifierType(ImportType(requiredModifierType.ModifierType, context), ImportType(requiredModifierType.ElementType, context));
			}
			case ElementType.Array:
			{
				ArrayType arrayType2 = (ArrayType)type;
				ArrayType arrayType3 = new ArrayType(ImportType(arrayType2.ElementType, context));
				if (arrayType2.IsVector)
				{
					return arrayType3;
				}
				Collection<ArrayDimension> dimensions = arrayType2.Dimensions;
				Collection<ArrayDimension> dimensions2 = arrayType3.Dimensions;
				dimensions2.Clear();
				for (int k = 0; k < dimensions.Count; k++)
				{
					ArrayDimension arrayDimension = dimensions[k];
					dimensions2.Add(new ArrayDimension(arrayDimension.LowerBound, arrayDimension.UpperBound));
				}
				return arrayType3;
			}
			case ElementType.GenericInst:
			{
				GenericInstanceType genericInstanceType = (GenericInstanceType)type;
				GenericInstanceType genericInstanceType2 = new GenericInstanceType(ImportType(genericInstanceType.ElementType, context));
				Collection<TypeReference> genericArguments = genericInstanceType.GenericArguments;
				Collection<TypeReference> genericArguments2 = genericInstanceType2.GenericArguments;
				for (int i = 0; i < genericArguments.Count; i++)
				{
					genericArguments2.Add(ImportType(genericArguments[i], context));
				}
				return genericInstanceType2;
			}
			case ElementType.Var:
			{
				GenericParameter genericParameter2 = (GenericParameter)type;
				if (genericParameter2.DeclaringType == null)
				{
					throw new InvalidOperationException();
				}
				return context.TypeParameter(genericParameter2.DeclaringType.FullName, genericParameter2.Position);
			}
			case ElementType.MVar:
			{
				GenericParameter genericParameter = (GenericParameter)type;
				if (genericParameter.DeclaringMethod == null)
				{
					throw new InvalidOperationException();
				}
				return context.MethodParameter(context.NormalizeMethodName(genericParameter.DeclaringMethod), genericParameter.Position);
			}
			default:
				throw new NotSupportedException(type.etype.ToString());
			}
		}

		private FieldReference ImportField(FieldReference field, ImportGenericContext context)
		{
			TypeReference typeReference = ImportType(field.DeclaringType, context);
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

		private MethodReference ImportMethod(MethodReference method, ImportGenericContext context)
		{
			if (method.IsGenericInstance)
			{
				return ImportMethodSpecification(method, context);
			}
			TypeReference declaringType = ImportType(method.DeclaringType, context);
			MethodReference methodReference = new MethodReference
			{
				Name = method.Name,
				HasThis = method.HasThis,
				ExplicitThis = method.ExplicitThis,
				DeclaringType = declaringType,
				CallingConvention = method.CallingConvention
			};
			if (method.HasGenericParameters)
			{
				ImportGenericParameters(methodReference, method);
			}
			context.Push(methodReference);
			try
			{
				methodReference.ReturnType = ImportType(method.ReturnType, context);
				if (!method.HasParameters)
				{
					return methodReference;
				}
				Collection<ParameterDefinition> parameters = method.Parameters;
				ParameterDefinitionCollection parameterDefinitionCollection = methodReference.parameters = new ParameterDefinitionCollection(methodReference, parameters.Count);
				for (int i = 0; i < parameters.Count; i++)
				{
					parameterDefinitionCollection.Add(new ParameterDefinition(ImportType(parameters[i].ParameterType, context)));
				}
				return methodReference;
			}
			finally
			{
				context.Pop();
			}
		}

		private MethodSpecification ImportMethodSpecification(MethodReference method, ImportGenericContext context)
		{
			if (!method.IsGenericInstance)
			{
				throw new NotSupportedException();
			}
			GenericInstanceMethod genericInstanceMethod = (GenericInstanceMethod)method;
			GenericInstanceMethod genericInstanceMethod2 = new GenericInstanceMethod(ImportMethod(genericInstanceMethod.ElementMethod, context));
			Collection<TypeReference> genericArguments = genericInstanceMethod.GenericArguments;
			Collection<TypeReference> genericArguments2 = genericInstanceMethod2.GenericArguments;
			for (int i = 0; i < genericArguments.Count; i++)
			{
				genericArguments2.Add(ImportType(genericArguments[i], context));
			}
			return genericInstanceMethod2;
		}

		public virtual TypeReference ImportReference(TypeReference type, IGenericParameterProvider context)
		{
			Mixin.CheckType(type);
			return ImportType(type, ImportGenericContext.For(context));
		}

		public virtual FieldReference ImportReference(FieldReference field, IGenericParameterProvider context)
		{
			Mixin.CheckField(field);
			return ImportField(field, ImportGenericContext.For(context));
		}

		public virtual MethodReference ImportReference(MethodReference method, IGenericParameterProvider context)
		{
			Mixin.CheckMethod(method);
			return ImportMethod(method, ImportGenericContext.For(context));
		}
	}
}
