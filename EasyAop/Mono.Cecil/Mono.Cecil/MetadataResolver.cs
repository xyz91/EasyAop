using Mono.Collections.Generic;
using System;

namespace Mono.Cecil
{
	public class MetadataResolver : IMetadataResolver
	{
		private readonly IAssemblyResolver assembly_resolver;

		public IAssemblyResolver AssemblyResolver => assembly_resolver;

		public MetadataResolver(IAssemblyResolver assemblyResolver)
		{
			if (assemblyResolver == null)
			{
				throw new ArgumentNullException("assemblyResolver");
			}
			assembly_resolver = assemblyResolver;
		}

		public virtual TypeDefinition Resolve(TypeReference type)
		{
			Mixin.CheckType(type);
			type = type.GetElementType();
			IMetadataScope scope = type.Scope;
			if (scope == null)
			{
				return null;
			}
			switch (scope.MetadataScopeType)
			{
			case MetadataScopeType.AssemblyNameReference:
			{
				AssemblyDefinition assemblyDefinition = assembly_resolver.Resolve((AssemblyNameReference)scope);
				if (assemblyDefinition == null)
				{
					return null;
				}
				return GetType(assemblyDefinition.MainModule, type);
			}
			case MetadataScopeType.ModuleDefinition:
				return GetType((ModuleDefinition)scope, type);
			case MetadataScopeType.ModuleReference:
			{
				Collection<ModuleDefinition> modules = type.Module.Assembly.Modules;
				ModuleReference moduleReference = (ModuleReference)scope;
				for (int i = 0; i < modules.Count; i++)
				{
					ModuleDefinition moduleDefinition = modules[i];
					if (moduleDefinition.Name == moduleReference.Name)
					{
						return GetType(moduleDefinition, type);
					}
				}
				break;
			}
			}
			throw new NotSupportedException();
		}

		private static TypeDefinition GetType(ModuleDefinition module, TypeReference reference)
		{
			TypeDefinition typeDefinition = GetTypeDefinition(module, reference);
			if (typeDefinition != null)
			{
				return typeDefinition;
			}
			if (!module.HasExportedTypes)
			{
				return null;
			}
			Collection<ExportedType> exportedTypes = module.ExportedTypes;
			for (int i = 0; i < exportedTypes.Count; i++)
			{
				ExportedType exportedType = exportedTypes[i];
				if (!(exportedType.Name != reference.Name) && !(exportedType.Namespace != reference.Namespace))
				{
					return exportedType.Resolve();
				}
			}
			return null;
		}

		private static TypeDefinition GetTypeDefinition(ModuleDefinition module, TypeReference type)
		{
			if (!type.IsNested)
			{
				return module.GetType(type.Namespace, type.Name);
			}
			TypeDefinition typeDefinition = type.DeclaringType.Resolve();
			return typeDefinition?.GetNestedType(type.TypeFullName());
		}

		public virtual FieldDefinition Resolve(FieldReference field)
		{
			Mixin.CheckField(field);
			TypeDefinition typeDefinition = Resolve(field.DeclaringType);
			if (typeDefinition == null)
			{
				return null;
			}
			if (!typeDefinition.HasFields)
			{
				return null;
			}
			return GetField(typeDefinition, field);
		}

		private FieldDefinition GetField(TypeDefinition type, FieldReference reference)
		{
			while (type != null)
			{
				FieldDefinition field = GetField(type.Fields, reference);
				if (field != null)
				{
					return field;
				}
				if (type.BaseType == null)
				{
					return null;
				}
				type = Resolve(type.BaseType);
			}
			return null;
		}

		private static FieldDefinition GetField(Collection<FieldDefinition> fields, FieldReference reference)
		{
			for (int i = 0; i < fields.Count; i++)
			{
				FieldDefinition fieldDefinition = fields[i];
				if (!(fieldDefinition.Name != reference.Name) && AreSame(fieldDefinition.FieldType, reference.FieldType))
				{
					return fieldDefinition;
				}
			}
			return null;
		}

		public virtual MethodDefinition Resolve(MethodReference method)
		{
			Mixin.CheckMethod(method);
			TypeDefinition typeDefinition = Resolve(method.DeclaringType);
			if (typeDefinition == null)
			{
				return null;
			}
			method = method.GetElementMethod();
			if (!typeDefinition.HasMethods)
			{
				return null;
			}
			return GetMethod(typeDefinition, method);
		}

		private MethodDefinition GetMethod(TypeDefinition type, MethodReference reference)
		{
			while (type != null)
			{
				MethodDefinition method = GetMethod(type.Methods, reference);
				if (method != null)
				{
					return method;
				}
				if (type.BaseType == null)
				{
					return null;
				}
				type = Resolve(type.BaseType);
			}
			return null;
		}

		public static MethodDefinition GetMethod(Collection<MethodDefinition> methods, MethodReference reference)
		{
			for (int i = 0; i < methods.Count; i++)
			{
				MethodDefinition methodDefinition = methods[i];
				if (!(methodDefinition.Name != reference.Name) && methodDefinition.HasGenericParameters == reference.HasGenericParameters && (!methodDefinition.HasGenericParameters || methodDefinition.GenericParameters.Count == reference.GenericParameters.Count) && AreSame(methodDefinition.ReturnType, reference.ReturnType) && methodDefinition.IsVarArg() == reference.IsVarArg())
				{
					if (methodDefinition.IsVarArg() && IsVarArgCallTo(methodDefinition, reference))
					{
						return methodDefinition;
					}
					if (methodDefinition.HasParameters == reference.HasParameters)
					{
						if (!methodDefinition.HasParameters && !reference.HasParameters)
						{
							return methodDefinition;
						}
						if (AreSame(methodDefinition.Parameters, reference.Parameters))
						{
							return methodDefinition;
						}
					}
				}
			}
			return null;
		}

		private static bool AreSame(Collection<ParameterDefinition> a, Collection<ParameterDefinition> b)
		{
			int count = a.Count;
			if (count != b.Count)
			{
				return false;
			}
			if (count == 0)
			{
				return true;
			}
			for (int i = 0; i < count; i++)
			{
				if (!AreSame(a[i].ParameterType, b[i].ParameterType))
				{
					return false;
				}
			}
			return true;
		}

		private static bool IsVarArgCallTo(MethodDefinition method, MethodReference reference)
		{
			if (method.Parameters.Count >= reference.Parameters.Count)
			{
				return false;
			}
			if (reference.GetSentinelPosition() != method.Parameters.Count)
			{
				return false;
			}
			for (int i = 0; i < method.Parameters.Count; i++)
			{
				if (!AreSame(method.Parameters[i].ParameterType, reference.Parameters[i].ParameterType))
				{
					return false;
				}
			}
			return true;
		}

		private static bool AreSame(TypeSpecification a, TypeSpecification b)
		{
			if (!AreSame(a.ElementType, b.ElementType))
			{
				return false;
			}
			if (a.IsGenericInstance)
			{
				return AreSame((GenericInstanceType)a, (GenericInstanceType)b);
			}
			if (!a.IsRequiredModifier && !a.IsOptionalModifier)
			{
				if (a.IsArray)
				{
					return AreSame((ArrayType)a, (ArrayType)b);
				}
				return true;
			}
			return AreSame((IModifierType)a, (IModifierType)b);
		}

		private static bool AreSame(ArrayType a, ArrayType b)
		{
			if (a.Rank != b.Rank)
			{
				return false;
			}
			return true;
		}

		private static bool AreSame(IModifierType a, IModifierType b)
		{
			return AreSame(a.ModifierType, b.ModifierType);
		}

		private static bool AreSame(GenericInstanceType a, GenericInstanceType b)
		{
			if (a.GenericArguments.Count != b.GenericArguments.Count)
			{
				return false;
			}
			for (int i = 0; i < a.GenericArguments.Count; i++)
			{
				if (!AreSame(a.GenericArguments[i], b.GenericArguments[i]))
				{
					return false;
				}
			}
			return true;
		}

		private static bool AreSame(GenericParameter a, GenericParameter b)
		{
			return a.Position == b.Position;
		}

		private static bool AreSame(TypeReference a, TypeReference b)
		{
			if (a == b)
			{
				return true;
			}
			if (a != null && b != null)
			{
				if (a.etype != b.etype)
				{
					return false;
				}
				if (a.IsGenericParameter)
				{
					return AreSame((GenericParameter)a, (GenericParameter)b);
				}
				if (a.IsTypeSpecification())
				{
					return AreSame((TypeSpecification)a, (TypeSpecification)b);
				}
				if (!(a.Name != b.Name) && !(a.Namespace != b.Namespace))
				{
					return AreSame(a.DeclaringType, b.DeclaringType);
				}
				return false;
			}
			return false;
		}
	}
}
