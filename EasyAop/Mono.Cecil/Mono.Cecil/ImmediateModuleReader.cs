using Mono.Cecil.Cil;
using Mono.Cecil.PE;
using Mono.Collections.Generic;

namespace Mono.Cecil
{
	internal sealed class ImmediateModuleReader : ModuleReader
	{
		private bool resolve_attributes;

		public ImmediateModuleReader(Image image)
			: base(image, ReadingMode.Immediate)
		{
		}

		protected override void ReadModule()
		{
			base.module.Read(base.module, delegate(ModuleDefinition module, MetadataReader reader)
			{
				base.ReadModuleManifest(reader);
				ReadModule(module, true);
			});
		}

		public void ReadModule(ModuleDefinition module, bool resolve_attributes)
		{
			this.resolve_attributes = resolve_attributes;
			if (module.HasAssemblyReferences)
			{
				Mixin.Read(module.AssemblyReferences);
			}
			if (module.HasResources)
			{
				Mixin.Read(module.Resources);
			}
			if (module.HasModuleReferences)
			{
				Mixin.Read(module.ModuleReferences);
			}
			if (module.HasTypes)
			{
				ReadTypes(module.Types);
			}
			if (module.HasExportedTypes)
			{
				Mixin.Read(module.ExportedTypes);
			}
			ReadCustomAttributes(module);
			AssemblyDefinition assembly = module.Assembly;
			if (assembly != null)
			{
				ReadCustomAttributes(assembly);
				ReadSecurityDeclarations(assembly);
			}
		}

		private void ReadTypes(Collection<TypeDefinition> types)
		{
			for (int i = 0; i < types.Count; i++)
			{
				ReadType(types[i]);
			}
		}

		private void ReadType(TypeDefinition type)
		{
			ReadGenericParameters(type);
			if (type.HasInterfaces)
			{
				ReadInterfaces(type);
			}
			if (type.HasNestedTypes)
			{
				ReadTypes(type.NestedTypes);
			}
			if (type.HasLayoutInfo)
			{
				Mixin.Read(type.ClassSize);
			}
			if (type.HasFields)
			{
				ReadFields(type);
			}
			if (type.HasMethods)
			{
				ReadMethods(type);
			}
			if (type.HasProperties)
			{
				ReadProperties(type);
			}
			if (type.HasEvents)
			{
				ReadEvents(type);
			}
			ReadSecurityDeclarations(type);
			ReadCustomAttributes(type);
		}

		private void ReadInterfaces(TypeDefinition type)
		{
			Collection<InterfaceImplementation> interfaces = type.Interfaces;
			for (int i = 0; i < interfaces.Count; i++)
			{
				ReadCustomAttributes(interfaces[i]);
			}
		}

		private void ReadGenericParameters(IGenericParameterProvider provider)
		{
			if (provider.HasGenericParameters)
			{
				Collection<GenericParameter> genericParameters = provider.GenericParameters;
				for (int i = 0; i < genericParameters.Count; i++)
				{
					GenericParameter genericParameter = genericParameters[i];
					if (genericParameter.HasConstraints)
					{
						Mixin.Read(genericParameter.Constraints);
					}
					ReadCustomAttributes(genericParameter);
				}
			}
		}

		private void ReadSecurityDeclarations(ISecurityDeclarationProvider provider)
		{
			if (provider.HasSecurityDeclarations)
			{
				Collection<SecurityDeclaration> securityDeclarations = provider.SecurityDeclarations;
				if (resolve_attributes)
				{
					for (int i = 0; i < securityDeclarations.Count; i++)
					{
						Mixin.Read(securityDeclarations[i].SecurityAttributes);
					}
				}
			}
		}

		private void ReadCustomAttributes(ICustomAttributeProvider provider)
		{
			if (provider.HasCustomAttributes)
			{
				Collection<CustomAttribute> customAttributes = provider.CustomAttributes;
				if (resolve_attributes)
				{
					for (int i = 0; i < customAttributes.Count; i++)
					{
						Mixin.Read(customAttributes[i].ConstructorArguments);
					}
				}
			}
		}

		private void ReadFields(TypeDefinition type)
		{
			Collection<FieldDefinition> fields = type.Fields;
			for (int i = 0; i < fields.Count; i++)
			{
				FieldDefinition fieldDefinition = fields[i];
				if (fieldDefinition.HasConstant)
				{
					Mixin.Read(fieldDefinition.Constant);
				}
				if (fieldDefinition.HasLayoutInfo)
				{
					Mixin.Read(fieldDefinition.Offset);
				}
				if (fieldDefinition.RVA > 0)
				{
					Mixin.Read(fieldDefinition.InitialValue);
				}
				if (fieldDefinition.HasMarshalInfo)
				{
					Mixin.Read(fieldDefinition.MarshalInfo);
				}
				ReadCustomAttributes(fieldDefinition);
			}
		}

		private void ReadMethods(TypeDefinition type)
		{
			Collection<MethodDefinition> methods = type.Methods;
			for (int i = 0; i < methods.Count; i++)
			{
				MethodDefinition methodDefinition = methods[i];
				ReadGenericParameters(methodDefinition);
				if (methodDefinition.HasParameters)
				{
					ReadParameters(methodDefinition);
				}
				if (methodDefinition.HasOverrides)
				{
					Mixin.Read(methodDefinition.Overrides);
				}
				if (methodDefinition.IsPInvokeImpl)
				{
					Mixin.Read(methodDefinition.PInvokeInfo);
				}
				ReadSecurityDeclarations(methodDefinition);
				ReadCustomAttributes(methodDefinition);
				MethodReturnType methodReturnType = methodDefinition.MethodReturnType;
				if (methodReturnType.HasConstant)
				{
					Mixin.Read(methodReturnType.Constant);
				}
				if (methodReturnType.HasMarshalInfo)
				{
					Mixin.Read(methodReturnType.MarshalInfo);
				}
				ReadCustomAttributes(methodReturnType);
			}
		}

		private void ReadParameters(MethodDefinition method)
		{
			Collection<ParameterDefinition> parameters = method.Parameters;
			for (int i = 0; i < parameters.Count; i++)
			{
				ParameterDefinition parameterDefinition = parameters[i];
				if (parameterDefinition.HasConstant)
				{
					Mixin.Read(parameterDefinition.Constant);
				}
				if (parameterDefinition.HasMarshalInfo)
				{
					Mixin.Read(parameterDefinition.MarshalInfo);
				}
				ReadCustomAttributes(parameterDefinition);
			}
		}

		private void ReadProperties(TypeDefinition type)
		{
			Collection<PropertyDefinition> properties = type.Properties;
			for (int i = 0; i < properties.Count; i++)
			{
				PropertyDefinition propertyDefinition = properties[i];
				Mixin.Read(propertyDefinition.GetMethod);
				if (propertyDefinition.HasConstant)
				{
					Mixin.Read(propertyDefinition.Constant);
				}
				ReadCustomAttributes(propertyDefinition);
			}
		}

		private void ReadEvents(TypeDefinition type)
		{
			Collection<EventDefinition> events = type.Events;
			for (int i = 0; i < events.Count; i++)
			{
				EventDefinition eventDefinition = events[i];
				Mixin.Read(eventDefinition.AddMethod);
				ReadCustomAttributes(eventDefinition);
			}
		}

		public override void ReadSymbols(ModuleDefinition module)
		{
			if (module.symbol_reader != null)
			{
				ReadTypesSymbols(module.Types, module.symbol_reader);
			}
		}

		private void ReadTypesSymbols(Collection<TypeDefinition> types, ISymbolReader symbol_reader)
		{
			for (int i = 0; i < types.Count; i++)
			{
				TypeDefinition typeDefinition = types[i];
				if (typeDefinition.HasNestedTypes)
				{
					ReadTypesSymbols(typeDefinition.NestedTypes, symbol_reader);
				}
				if (typeDefinition.HasMethods)
				{
					ReadMethodsSymbols(typeDefinition, symbol_reader);
				}
			}
		}

		private void ReadMethodsSymbols(TypeDefinition type, ISymbolReader symbol_reader)
		{
			Collection<MethodDefinition> methods = type.Methods;
			for (int i = 0; i < methods.Count; i++)
			{
				MethodDefinition methodDefinition = methods[i];
				if (methodDefinition.HasBody && methodDefinition.token.RID != 0 && methodDefinition.debug_info == null)
				{
					methodDefinition.debug_info = symbol_reader.Read(methodDefinition);
				}
			}
		}
	}
}
