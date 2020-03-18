using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Mono.Cecil
{
	internal sealed class WindowsRuntimeProjections
	{
		private struct ProjectionInfo
		{
			public readonly string WinRTNamespace;

			public readonly string ClrNamespace;

			public readonly string ClrName;

			public readonly string ClrAssembly;

			public readonly bool Attribute;

			public readonly bool Disposable;

			public ProjectionInfo(string winrt_namespace, string clr_namespace, string clr_name, string clr_assembly, bool attribute = false, bool disposable = false)
			{
				WinRTNamespace = winrt_namespace;
				ClrNamespace = clr_namespace;
				ClrName = clr_name;
				ClrAssembly = clr_assembly;
				Attribute = attribute;
				Disposable = disposable;
			}
		}

		private static readonly Version version = new Version(4, 0, 0, 0);

		private static readonly byte[] contract_pk_token = new byte[8]
		{
			176,
			63,
			95,
			127,
			17,
			213,
			10,
			58
		};

		private static readonly byte[] contract_pk = new byte[160]
		{
			0,
			36,
			0,
			0,
			4,
			128,
			0,
			0,
			148,
			0,
			0,
			0,
			6,
			2,
			0,
			0,
			0,
			36,
			0,
			0,
			82,
			83,
			65,
			49,
			0,
			4,
			0,
			0,
			1,
			0,
			1,
			0,
			7,
			209,
			250,
			87,
			196,
			174,
			217,
			240,
			163,
			46,
			132,
			170,
			15,
			174,
			253,
			13,
			233,
			232,
			253,
			106,
			236,
			143,
			135,
			251,
			3,
			118,
			108,
			131,
			76,
			153,
			146,
			30,
			178,
			59,
			231,
			154,
			217,
			213,
			220,
			193,
			221,
			154,
			210,
			54,
			19,
			33,
			2,
			144,
			11,
			114,
			60,
			249,
			128,
			149,
			127,
			196,
			225,
			119,
			16,
			143,
			198,
			7,
			119,
			79,
			41,
			232,
			50,
			14,
			146,
			234,
			5,
			236,
			228,
			232,
			33,
			192,
			165,
			239,
			232,
			241,
			100,
			92,
			76,
			12,
			147,
			193,
			171,
			153,
			40,
			93,
			98,
			44,
			170,
			101,
			44,
			29,
			250,
			214,
			61,
			116,
			93,
			111,
			45,
			229,
			241,
			126,
			94,
			175,
			15,
			196,
			150,
			61,
			38,
			28,
			138,
			18,
			67,
			101,
			24,
			32,
			109,
			192,
			147,
			52,
			77,
			90,
			210,
			147
		};

		private static Dictionary<string, ProjectionInfo> projections;

		private readonly ModuleDefinition module;

		private Version corlib_version = new Version(255, 255, 255, 255);

		private AssemblyNameReference[] virtual_references;

		private static Dictionary<string, ProjectionInfo> Projections
		{
			get
			{
				if (projections != null)
				{
					return projections;
				}
				return projections = new Dictionary<string, ProjectionInfo>
				{
					{
						"AttributeTargets",
						new ProjectionInfo("Windows.Foundation.Metadata", "System", "AttributeTargets", "System.Runtime", false, false)
					},
					{
						"AttributeUsageAttribute",
						new ProjectionInfo("Windows.Foundation.Metadata", "System", "AttributeUsageAttribute", "System.Runtime", true, false)
					},
					{
						"Color",
						new ProjectionInfo("Windows.UI", "Windows.UI", "Color", "System.Runtime.WindowsRuntime", false, false)
					},
					{
						"CornerRadius",
						new ProjectionInfo("Windows.UI.Xaml", "Windows.UI.Xaml", "CornerRadius", "System.Runtime.WindowsRuntime.UI.Xaml", false, false)
					},
					{
						"DateTime",
						new ProjectionInfo("Windows.Foundation", "System", "DateTimeOffset", "System.Runtime", false, false)
					},
					{
						"Duration",
						new ProjectionInfo("Windows.UI.Xaml", "Windows.UI.Xaml", "Duration", "System.Runtime.WindowsRuntime.UI.Xaml", false, false)
					},
					{
						"DurationType",
						new ProjectionInfo("Windows.UI.Xaml", "Windows.UI.Xaml", "DurationType", "System.Runtime.WindowsRuntime.UI.Xaml", false, false)
					},
					{
						"EventHandler`1",
						new ProjectionInfo("Windows.Foundation", "System", "EventHandler`1", "System.Runtime", false, false)
					},
					{
						"EventRegistrationToken",
						new ProjectionInfo("Windows.Foundation", "System.Runtime.InteropServices.WindowsRuntime", "EventRegistrationToken", "System.Runtime.InteropServices.WindowsRuntime", false, false)
					},
					{
						"GeneratorPosition",
						new ProjectionInfo("Windows.UI.Xaml.Controls.Primitives", "Windows.UI.Xaml.Controls.Primitives", "GeneratorPosition", "System.Runtime.WindowsRuntime.UI.Xaml", false, false)
					},
					{
						"GridLength",
						new ProjectionInfo("Windows.UI.Xaml", "Windows.UI.Xaml", "GridLength", "System.Runtime.WindowsRuntime.UI.Xaml", false, false)
					},
					{
						"GridUnitType",
						new ProjectionInfo("Windows.UI.Xaml", "Windows.UI.Xaml", "GridUnitType", "System.Runtime.WindowsRuntime.UI.Xaml", false, false)
					},
					{
						"HResult",
						new ProjectionInfo("Windows.Foundation", "System", "Exception", "System.Runtime", false, false)
					},
					{
						"IBindableIterable",
						new ProjectionInfo("Windows.UI.Xaml.Interop", "System.Collections", "IEnumerable", "System.Runtime", false, false)
					},
					{
						"IBindableVector",
						new ProjectionInfo("Windows.UI.Xaml.Interop", "System.Collections", "IList", "System.Runtime", false, false)
					},
					{
						"IClosable",
						new ProjectionInfo("Windows.Foundation", "System", "IDisposable", "System.Runtime", false, true)
					},
					{
						"ICommand",
						new ProjectionInfo("Windows.UI.Xaml.Input", "System.Windows.Input", "ICommand", "System.ObjectModel", false, false)
					},
					{
						"IIterable`1",
						new ProjectionInfo("Windows.Foundation.Collections", "System.Collections.Generic", "IEnumerable`1", "System.Runtime", false, false)
					},
					{
						"IKeyValuePair`2",
						new ProjectionInfo("Windows.Foundation.Collections", "System.Collections.Generic", "KeyValuePair`2", "System.Runtime", false, false)
					},
					{
						"IMapView`2",
						new ProjectionInfo("Windows.Foundation.Collections", "System.Collections.Generic", "IReadOnlyDictionary`2", "System.Runtime", false, false)
					},
					{
						"IMap`2",
						new ProjectionInfo("Windows.Foundation.Collections", "System.Collections.Generic", "IDictionary`2", "System.Runtime", false, false)
					},
					{
						"INotifyCollectionChanged",
						new ProjectionInfo("Windows.UI.Xaml.Interop", "System.Collections.Specialized", "INotifyCollectionChanged", "System.ObjectModel", false, false)
					},
					{
						"INotifyPropertyChanged",
						new ProjectionInfo("Windows.UI.Xaml.Data", "System.ComponentModel", "INotifyPropertyChanged", "System.ObjectModel", false, false)
					},
					{
						"IReference`1",
						new ProjectionInfo("Windows.Foundation", "System", "Nullable`1", "System.Runtime", false, false)
					},
					{
						"IVectorView`1",
						new ProjectionInfo("Windows.Foundation.Collections", "System.Collections.Generic", "IReadOnlyList`1", "System.Runtime", false, false)
					},
					{
						"IVector`1",
						new ProjectionInfo("Windows.Foundation.Collections", "System.Collections.Generic", "IList`1", "System.Runtime", false, false)
					},
					{
						"KeyTime",
						new ProjectionInfo("Windows.UI.Xaml.Media.Animation", "Windows.UI.Xaml.Media.Animation", "KeyTime", "System.Runtime.WindowsRuntime.UI.Xaml", false, false)
					},
					{
						"Matrix",
						new ProjectionInfo("Windows.UI.Xaml.Media", "Windows.UI.Xaml.Media", "Matrix", "System.Runtime.WindowsRuntime.UI.Xaml", false, false)
					},
					{
						"Matrix3D",
						new ProjectionInfo("Windows.UI.Xaml.Media.Media3D", "Windows.UI.Xaml.Media.Media3D", "Matrix3D", "System.Runtime.WindowsRuntime.UI.Xaml", false, false)
					},
					{
						"Matrix3x2",
						new ProjectionInfo("Windows.Foundation.Numerics", "System.Numerics", "Matrix3x2", "System.Numerics.Vectors", false, false)
					},
					{
						"Matrix4x4",
						new ProjectionInfo("Windows.Foundation.Numerics", "System.Numerics", "Matrix4x4", "System.Numerics.Vectors", false, false)
					},
					{
						"NotifyCollectionChangedAction",
						new ProjectionInfo("Windows.UI.Xaml.Interop", "System.Collections.Specialized", "NotifyCollectionChangedAction", "System.ObjectModel", false, false)
					},
					{
						"NotifyCollectionChangedEventArgs",
						new ProjectionInfo("Windows.UI.Xaml.Interop", "System.Collections.Specialized", "NotifyCollectionChangedEventArgs", "System.ObjectModel", false, false)
					},
					{
						"NotifyCollectionChangedEventHandler",
						new ProjectionInfo("Windows.UI.Xaml.Interop", "System.Collections.Specialized", "NotifyCollectionChangedEventHandler", "System.ObjectModel", false, false)
					},
					{
						"Plane",
						new ProjectionInfo("Windows.Foundation.Numerics", "System.Numerics", "Plane", "System.Numerics.Vectors", false, false)
					},
					{
						"Point",
						new ProjectionInfo("Windows.Foundation", "Windows.Foundation", "Point", "System.Runtime.WindowsRuntime", false, false)
					},
					{
						"PropertyChangedEventArgs",
						new ProjectionInfo("Windows.UI.Xaml.Data", "System.ComponentModel", "PropertyChangedEventArgs", "System.ObjectModel", false, false)
					},
					{
						"PropertyChangedEventHandler",
						new ProjectionInfo("Windows.UI.Xaml.Data", "System.ComponentModel", "PropertyChangedEventHandler", "System.ObjectModel", false, false)
					},
					{
						"Quaternion",
						new ProjectionInfo("Windows.Foundation.Numerics", "System.Numerics", "Quaternion", "System.Numerics.Vectors", false, false)
					},
					{
						"Rect",
						new ProjectionInfo("Windows.Foundation", "Windows.Foundation", "Rect", "System.Runtime.WindowsRuntime", false, false)
					},
					{
						"RepeatBehavior",
						new ProjectionInfo("Windows.UI.Xaml.Media.Animation", "Windows.UI.Xaml.Media.Animation", "RepeatBehavior", "System.Runtime.WindowsRuntime.UI.Xaml", false, false)
					},
					{
						"RepeatBehaviorType",
						new ProjectionInfo("Windows.UI.Xaml.Media.Animation", "Windows.UI.Xaml.Media.Animation", "RepeatBehaviorType", "System.Runtime.WindowsRuntime.UI.Xaml", false, false)
					},
					{
						"Size",
						new ProjectionInfo("Windows.Foundation", "Windows.Foundation", "Size", "System.Runtime.WindowsRuntime", false, false)
					},
					{
						"Thickness",
						new ProjectionInfo("Windows.UI.Xaml", "Windows.UI.Xaml", "Thickness", "System.Runtime.WindowsRuntime.UI.Xaml", false, false)
					},
					{
						"TimeSpan",
						new ProjectionInfo("Windows.Foundation", "System", "TimeSpan", "System.Runtime", false, false)
					},
					{
						"TypeName",
						new ProjectionInfo("Windows.UI.Xaml.Interop", "System", "Type", "System.Runtime", false, false)
					},
					{
						"Uri",
						new ProjectionInfo("Windows.Foundation", "System", "Uri", "System.Runtime", false, false)
					},
					{
						"Vector2",
						new ProjectionInfo("Windows.Foundation.Numerics", "System.Numerics", "Vector2", "System.Numerics.Vectors", false, false)
					},
					{
						"Vector3",
						new ProjectionInfo("Windows.Foundation.Numerics", "System.Numerics", "Vector3", "System.Numerics.Vectors", false, false)
					},
					{
						"Vector4",
						new ProjectionInfo("Windows.Foundation.Numerics", "System.Numerics", "Vector4", "System.Numerics.Vectors", false, false)
					}
				};
			}
		}

		private AssemblyNameReference[] VirtualReferences
		{
			get
			{
				if (virtual_references == null)
				{
					Mixin.Read(module.AssemblyReferences);
				}
				return virtual_references;
			}
		}

		public WindowsRuntimeProjections(ModuleDefinition module)
		{
			this.module = module;
		}

		public static void Project(TypeDefinition type)
		{
			TypeDefinitionTreatment typeDefinitionTreatment = TypeDefinitionTreatment.None;
			MetadataKind metadataKind = type.Module.MetadataKind;
			if (type.IsWindowsRuntime)
			{
				switch (metadataKind)
				{
				case MetadataKind.WindowsMetadata:
				{
					typeDefinitionTreatment = GetWellKnownTypeDefinitionTreatment(type);
					if (typeDefinitionTreatment != 0)
					{
						ApplyProjection(type, new TypeDefinitionProjection(type, typeDefinitionTreatment));
						return;
					}
					TypeReference baseType = type.BaseType;
					typeDefinitionTreatment = ((baseType == null || !IsAttribute(baseType)) ? TypeDefinitionTreatment.NormalType : TypeDefinitionTreatment.NormalAttribute);
					break;
				}
				case MetadataKind.ManagedWindowsMetadata:
					if (NeedsWindowsRuntimePrefix(type))
					{
						typeDefinitionTreatment = TypeDefinitionTreatment.PrefixWindowsRuntimeName;
					}
					break;
				}
				if ((typeDefinitionTreatment == TypeDefinitionTreatment.PrefixWindowsRuntimeName || typeDefinitionTreatment == TypeDefinitionTreatment.NormalType) && !type.IsInterface && HasAttribute(type, "Windows.UI.Xaml", "TreatAsAbstractComposableClassAttribute"))
				{
					typeDefinitionTreatment |= TypeDefinitionTreatment.Abstract;
				}
			}
			else if (metadataKind == MetadataKind.ManagedWindowsMetadata && IsClrImplementationType(type))
			{
				typeDefinitionTreatment = TypeDefinitionTreatment.UnmangleWindowsRuntimeName;
			}
			if (typeDefinitionTreatment != 0)
			{
				ApplyProjection(type, new TypeDefinitionProjection(type, typeDefinitionTreatment));
			}
		}

		private static TypeDefinitionTreatment GetWellKnownTypeDefinitionTreatment(TypeDefinition type)
		{
			if (!Projections.TryGetValue(type.Name, out ProjectionInfo projectionInfo))
			{
				return TypeDefinitionTreatment.None;
			}
			TypeDefinitionTreatment typeDefinitionTreatment = projectionInfo.Attribute ? TypeDefinitionTreatment.RedirectToClrAttribute : TypeDefinitionTreatment.RedirectToClrType;
			if (type.Namespace == projectionInfo.ClrNamespace)
			{
				return typeDefinitionTreatment;
			}
			if (type.Namespace == projectionInfo.WinRTNamespace)
			{
				return typeDefinitionTreatment | TypeDefinitionTreatment.Internal;
			}
			return TypeDefinitionTreatment.None;
		}

		private static bool NeedsWindowsRuntimePrefix(TypeDefinition type)
		{
			if ((type.Attributes & (TypeAttributes.VisibilityMask | TypeAttributes.ClassSemanticMask)) != TypeAttributes.Public)
			{
				return false;
			}
			TypeReference baseType = type.BaseType;
			if (baseType != null && baseType.MetadataToken.TokenType == TokenType.TypeRef)
			{
				if (baseType.Namespace == "System")
				{
					switch (baseType.Name)
					{
					case "Attribute":
					case "MulticastDelegate":
					case "ValueType":
						return false;
					}
				}
				return true;
			}
			return false;
		}

		private static bool IsClrImplementationType(TypeDefinition type)
		{
			if ((type.Attributes & (TypeAttributes.VisibilityMask | TypeAttributes.SpecialName)) != TypeAttributes.SpecialName)
			{
				return false;
			}
			return type.Name.StartsWith("<CLR>");
		}

		public static void ApplyProjection(TypeDefinition type, TypeDefinitionProjection projection)
		{
			if (projection != null)
			{
				TypeDefinitionTreatment treatment = projection.Treatment;
				switch (treatment & TypeDefinitionTreatment.KindMask)
				{
				case TypeDefinitionTreatment.NormalType:
					type.Attributes |= (TypeAttributes.Import | TypeAttributes.WindowsRuntime);
					break;
				case TypeDefinitionTreatment.NormalAttribute:
					type.Attributes |= (TypeAttributes.Sealed | TypeAttributes.WindowsRuntime);
					break;
				case TypeDefinitionTreatment.UnmangleWindowsRuntimeName:
					type.Attributes = (TypeAttributes)(((int)type.Attributes & -1025) | 1);
					type.Name = type.Name.Substring("<CLR>".Length);
					break;
				case TypeDefinitionTreatment.PrefixWindowsRuntimeName:
					type.Attributes = (TypeAttributes)(((int)type.Attributes & -2) | 0x1000);
					type.Name = "<WinRT>" + type.Name;
					break;
				case TypeDefinitionTreatment.RedirectToClrType:
					type.Attributes = (TypeAttributes)(((int)type.Attributes & -2) | 0x1000);
					break;
				case TypeDefinitionTreatment.RedirectToClrAttribute:
					type.Attributes &= ~TypeAttributes.Public;
					break;
				}
				if ((treatment & TypeDefinitionTreatment.Abstract) != 0)
				{
					type.Attributes |= TypeAttributes.Abstract;
				}
				if ((treatment & TypeDefinitionTreatment.Internal) != 0)
				{
					type.Attributes &= ~TypeAttributes.Public;
				}
				type.WindowsRuntimeProjection = projection;
			}
		}

		public static TypeDefinitionProjection RemoveProjection(TypeDefinition type)
		{
			if (!type.IsWindowsRuntimeProjection)
			{
				return null;
			}
			TypeDefinitionProjection windowsRuntimeProjection = type.WindowsRuntimeProjection;
			type.WindowsRuntimeProjection = null;
			type.Attributes = windowsRuntimeProjection.Attributes;
			type.Name = windowsRuntimeProjection.Name;
			return windowsRuntimeProjection;
		}

		public static void Project(TypeReference type)
		{
			ProjectionInfo projectionInfo;
			TypeReferenceTreatment typeReferenceTreatment = (!Projections.TryGetValue(type.Name, out projectionInfo) || !(projectionInfo.WinRTNamespace == type.Namespace)) ? GetSpecialTypeReferenceTreatment(type) : TypeReferenceTreatment.UseProjectionInfo;
			if (typeReferenceTreatment != 0)
			{
				ApplyProjection(type, new TypeReferenceProjection(type, typeReferenceTreatment));
			}
		}

		private static TypeReferenceTreatment GetSpecialTypeReferenceTreatment(TypeReference type)
		{
			if (type.Namespace == "System")
			{
				if (type.Name == "MulticastDelegate")
				{
					return TypeReferenceTreatment.SystemDelegate;
				}
				if (type.Name == "Attribute")
				{
					return TypeReferenceTreatment.SystemAttribute;
				}
			}
			return TypeReferenceTreatment.None;
		}

		private static bool IsAttribute(TypeReference type)
		{
			if (type.MetadataToken.TokenType != TokenType.TypeRef)
			{
				return false;
			}
			if (type.Name == "Attribute")
			{
				return type.Namespace == "System";
			}
			return false;
		}

		private static bool IsEnum(TypeReference type)
		{
			if (type.MetadataToken.TokenType != TokenType.TypeRef)
			{
				return false;
			}
			if (type.Name == "Enum")
			{
				return type.Namespace == "System";
			}
			return false;
		}

		public static void ApplyProjection(TypeReference type, TypeReferenceProjection projection)
		{
			if (projection != null)
			{
				switch (projection.Treatment)
				{
				case TypeReferenceTreatment.SystemDelegate:
				case TypeReferenceTreatment.SystemAttribute:
					type.Scope = type.Module.Projections.GetAssemblyReference("System.Runtime");
					break;
				case TypeReferenceTreatment.UseProjectionInfo:
				{
					ProjectionInfo projectionInfo = Projections[type.Name];
					type.Name = projectionInfo.ClrName;
					type.Namespace = projectionInfo.ClrNamespace;
					type.Scope = type.Module.Projections.GetAssemblyReference(projectionInfo.ClrAssembly);
					break;
				}
				}
				type.WindowsRuntimeProjection = projection;
			}
		}

		public static TypeReferenceProjection RemoveProjection(TypeReference type)
		{
			if (!type.IsWindowsRuntimeProjection)
			{
				return null;
			}
			TypeReferenceProjection windowsRuntimeProjection = type.WindowsRuntimeProjection;
			type.WindowsRuntimeProjection = null;
			type.Name = windowsRuntimeProjection.Name;
			type.Namespace = windowsRuntimeProjection.Namespace;
			type.Scope = windowsRuntimeProjection.Scope;
			return windowsRuntimeProjection;
		}

		public static void Project(MethodDefinition method)
		{
			MethodDefinitionTreatment methodDefinitionTreatment = MethodDefinitionTreatment.None;
			bool flag = false;
			TypeDefinition declaringType = method.DeclaringType;
			MetadataToken metadataToken;
			if (declaringType.IsWindowsRuntime)
			{
				if (IsClrImplementationType(declaringType))
				{
					methodDefinitionTreatment = MethodDefinitionTreatment.None;
				}
				else if (declaringType.IsNested)
				{
					methodDefinitionTreatment = MethodDefinitionTreatment.None;
				}
				else if (declaringType.IsInterface)
				{
					methodDefinitionTreatment = (MethodDefinitionTreatment.Runtime | MethodDefinitionTreatment.InternalCall);
				}
				else if (declaringType.Module.MetadataKind == MetadataKind.ManagedWindowsMetadata && !method.IsPublic)
				{
					methodDefinitionTreatment = MethodDefinitionTreatment.None;
				}
				else
				{
					flag = true;
					TypeReference baseType = declaringType.BaseType;
					if (baseType != null)
					{
						metadataToken = baseType.MetadataToken;
						if (metadataToken.TokenType == TokenType.TypeRef)
						{
							switch (GetSpecialTypeReferenceTreatment(baseType))
							{
							case TypeReferenceTreatment.SystemDelegate:
								methodDefinitionTreatment = (MethodDefinitionTreatment.Public | MethodDefinitionTreatment.Runtime);
								flag = false;
								break;
							case TypeReferenceTreatment.SystemAttribute:
								methodDefinitionTreatment = (MethodDefinitionTreatment.Runtime | MethodDefinitionTreatment.InternalCall);
								flag = false;
								break;
							}
						}
					}
				}
			}
			if (flag)
			{
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				foreach (MethodReference @override in method.Overrides)
				{
					metadataToken = @override.MetadataToken;
					if (metadataToken.TokenType == TokenType.MemberRef && ImplementsRedirectedInterface(@override, out flag4))
					{
						flag2 = true;
						if (flag4)
						{
							break;
						}
					}
					else
					{
						flag3 = true;
					}
				}
				if (flag4)
				{
					methodDefinitionTreatment = MethodDefinitionTreatment.Dispose;
					flag = false;
				}
				else if (flag2 && !flag3)
				{
					methodDefinitionTreatment = (MethodDefinitionTreatment.Private | MethodDefinitionTreatment.Runtime | MethodDefinitionTreatment.InternalCall);
					flag = false;
				}
			}
			if (flag)
			{
				methodDefinitionTreatment |= GetMethodDefinitionTreatmentFromCustomAttributes(method);
			}
			if (methodDefinitionTreatment != 0)
			{
				ApplyProjection(method, new MethodDefinitionProjection(method, methodDefinitionTreatment));
			}
		}

		private static MethodDefinitionTreatment GetMethodDefinitionTreatmentFromCustomAttributes(MethodDefinition method)
		{
			MethodDefinitionTreatment methodDefinitionTreatment = MethodDefinitionTreatment.None;
			foreach (CustomAttribute customAttribute in method.CustomAttributes)
			{
				TypeReference attributeType = customAttribute.AttributeType;
				if (!(attributeType.Namespace != "Windows.UI.Xaml"))
				{
					if (attributeType.Name == "TreatAsPublicMethodAttribute")
					{
						methodDefinitionTreatment |= MethodDefinitionTreatment.Public;
					}
					else if (attributeType.Name == "TreatAsAbstractMethodAttribute")
					{
						methodDefinitionTreatment |= MethodDefinitionTreatment.Abstract;
					}
				}
			}
			return methodDefinitionTreatment;
		}

		public static void ApplyProjection(MethodDefinition method, MethodDefinitionProjection projection)
		{
			if (projection != null)
			{
				MethodDefinitionTreatment treatment = projection.Treatment;
				if ((treatment & MethodDefinitionTreatment.Dispose) != 0)
				{
					method.Name = "Dispose";
				}
				if ((treatment & MethodDefinitionTreatment.Abstract) != 0)
				{
					method.Attributes |= MethodAttributes.Abstract;
				}
				if ((treatment & MethodDefinitionTreatment.Private) != 0)
				{
					method.Attributes = ((method.Attributes & ~MethodAttributes.MemberAccessMask) | MethodAttributes.Private);
				}
				if ((treatment & MethodDefinitionTreatment.Public) != 0)
				{
					method.Attributes = ((method.Attributes & ~MethodAttributes.MemberAccessMask) | MethodAttributes.Public);
				}
				if ((treatment & MethodDefinitionTreatment.Runtime) != 0)
				{
					method.ImplAttributes |= MethodImplAttributes.CodeTypeMask;
				}
				if ((treatment & MethodDefinitionTreatment.InternalCall) != 0)
				{
					method.ImplAttributes |= MethodImplAttributes.InternalCall;
				}
				method.WindowsRuntimeProjection = projection;
			}
		}

		public static MethodDefinitionProjection RemoveProjection(MethodDefinition method)
		{
			if (!method.IsWindowsRuntimeProjection)
			{
				return null;
			}
			MethodDefinitionProjection windowsRuntimeProjection = method.WindowsRuntimeProjection;
			method.WindowsRuntimeProjection = null;
			method.Attributes = windowsRuntimeProjection.Attributes;
			method.ImplAttributes = windowsRuntimeProjection.ImplAttributes;
			method.Name = windowsRuntimeProjection.Name;
			return windowsRuntimeProjection;
		}

		public static void Project(FieldDefinition field)
		{
			FieldDefinitionTreatment fieldDefinitionTreatment = FieldDefinitionTreatment.None;
			TypeDefinition declaringType = field.DeclaringType;
			if (declaringType.Module.MetadataKind == MetadataKind.WindowsMetadata && field.IsRuntimeSpecialName && field.Name == "value__")
			{
				TypeReference baseType = declaringType.BaseType;
				if (baseType != null && IsEnum(baseType))
				{
					fieldDefinitionTreatment = FieldDefinitionTreatment.Public;
				}
			}
			if (fieldDefinitionTreatment != 0)
			{
				ApplyProjection(field, new FieldDefinitionProjection(field, fieldDefinitionTreatment));
			}
		}

		public static void ApplyProjection(FieldDefinition field, FieldDefinitionProjection projection)
		{
			if (projection != null)
			{
				if (projection.Treatment == FieldDefinitionTreatment.Public)
				{
					field.Attributes = ((field.Attributes & ~FieldAttributes.FieldAccessMask) | FieldAttributes.Public);
				}
				field.WindowsRuntimeProjection = projection;
			}
		}

		public static FieldDefinitionProjection RemoveProjection(FieldDefinition field)
		{
			if (!field.IsWindowsRuntimeProjection)
			{
				return null;
			}
			FieldDefinitionProjection windowsRuntimeProjection = field.WindowsRuntimeProjection;
			field.WindowsRuntimeProjection = null;
			field.Attributes = windowsRuntimeProjection.Attributes;
			return windowsRuntimeProjection;
		}

		public static void Project(MemberReference member)
		{
			if (ImplementsRedirectedInterface(member, out bool flag) && flag)
			{
				ApplyProjection(member, new MemberReferenceProjection(member, MemberReferenceTreatment.Dispose));
			}
		}

		private static bool ImplementsRedirectedInterface(MemberReference member, out bool disposable)
		{
			disposable = false;
			TypeReference declaringType = member.DeclaringType;
			MetadataToken metadataToken = declaringType.MetadataToken;
			TypeReference typeReference;
			switch (metadataToken.TokenType)
			{
			case TokenType.TypeRef:
				typeReference = declaringType;
				break;
			case TokenType.TypeSpec:
				if (!declaringType.IsGenericInstance)
				{
					return false;
				}
				typeReference = ((TypeSpecification)declaringType).ElementType;
				if (typeReference.MetadataType == MetadataType.Class)
				{
					metadataToken = typeReference.MetadataToken;
					if (metadataToken.TokenType == TokenType.TypeRef)
					{
						break;
					}
				}
				return false;
			default:
				return false;
			}
			TypeReferenceProjection projection = RemoveProjection(typeReference);
			bool result = false;
			if (Projections.TryGetValue(typeReference.Name, out ProjectionInfo projectionInfo) && typeReference.Namespace == projectionInfo.WinRTNamespace)
			{
				disposable = projectionInfo.Disposable;
				result = true;
			}
			ApplyProjection(typeReference, projection);
			return result;
		}

		public static void ApplyProjection(MemberReference member, MemberReferenceProjection projection)
		{
			if (projection != null)
			{
				if (projection.Treatment == MemberReferenceTreatment.Dispose)
				{
					member.Name = "Dispose";
				}
				member.WindowsRuntimeProjection = projection;
			}
		}

		public static MemberReferenceProjection RemoveProjection(MemberReference member)
		{
			if (!member.IsWindowsRuntimeProjection)
			{
				return null;
			}
			MemberReferenceProjection windowsRuntimeProjection = member.WindowsRuntimeProjection;
			member.WindowsRuntimeProjection = null;
			member.Name = windowsRuntimeProjection.Name;
			return windowsRuntimeProjection;
		}

		public void AddVirtualReferences(Collection<AssemblyNameReference> references)
		{
			AssemblyNameReference coreLibrary = GetCoreLibrary(references);
			corlib_version = coreLibrary.Version;
			coreLibrary.Version = version;
			if (virtual_references == null)
			{
				AssemblyNameReference[] assemblyReferences = GetAssemblyReferences(coreLibrary);
				Interlocked.CompareExchange(ref virtual_references, assemblyReferences, null);
			}
			AssemblyNameReference[] array = virtual_references;
			foreach (AssemblyNameReference item in array)
			{
				references.Add(item);
			}
		}

		public void RemoveVirtualReferences(Collection<AssemblyNameReference> references)
		{
			GetCoreLibrary(references).Version = corlib_version;
			AssemblyNameReference[] virtualReferences = VirtualReferences;
			foreach (AssemblyNameReference item in virtualReferences)
			{
				references.Remove(item);
			}
		}

		private static AssemblyNameReference[] GetAssemblyReferences(AssemblyNameReference corlib)
		{
			AssemblyNameReference assemblyNameReference = new AssemblyNameReference("System.Runtime", version);
			AssemblyNameReference assemblyNameReference2 = new AssemblyNameReference("System.Runtime.InteropServices.WindowsRuntime", version);
			AssemblyNameReference assemblyNameReference3 = new AssemblyNameReference("System.ObjectModel", version);
			AssemblyNameReference assemblyNameReference4 = new AssemblyNameReference("System.Runtime.WindowsRuntime", version);
			AssemblyNameReference assemblyNameReference5 = new AssemblyNameReference("System.Runtime.WindowsRuntime.UI.Xaml", version);
			AssemblyNameReference assemblyNameReference6 = new AssemblyNameReference("System.Numerics.Vectors", version);
			if (corlib.HasPublicKey)
			{
				AssemblyNameReference assemblyNameReference7 = assemblyNameReference4;
				AssemblyNameReference assemblyNameReference8 = assemblyNameReference5;
				byte[] array2 = assemblyNameReference7.PublicKey = (assemblyNameReference8.PublicKey = corlib.PublicKey);
				AssemblyNameReference assemblyNameReference9 = assemblyNameReference;
				AssemblyNameReference assemblyNameReference10 = assemblyNameReference2;
				AssemblyNameReference assemblyNameReference11 = assemblyNameReference3;
				AssemblyNameReference assemblyNameReference12 = assemblyNameReference6;
				byte[] array4 = assemblyNameReference12.PublicKey = contract_pk;
				byte[] array6 = assemblyNameReference11.PublicKey = array4;
				array2 = (assemblyNameReference9.PublicKey = (assemblyNameReference10.PublicKey = array6));
			}
			else
			{
				AssemblyNameReference assemblyNameReference13 = assemblyNameReference4;
				AssemblyNameReference assemblyNameReference14 = assemblyNameReference5;
				byte[] array2 = assemblyNameReference13.PublicKeyToken = (assemblyNameReference14.PublicKeyToken = corlib.PublicKeyToken);
				AssemblyNameReference assemblyNameReference15 = assemblyNameReference;
				AssemblyNameReference assemblyNameReference16 = assemblyNameReference2;
				AssemblyNameReference assemblyNameReference17 = assemblyNameReference3;
				AssemblyNameReference assemblyNameReference18 = assemblyNameReference6;
				byte[] array4 = assemblyNameReference18.PublicKeyToken = contract_pk_token;
				byte[] array6 = assemblyNameReference17.PublicKeyToken = array4;
				array2 = (assemblyNameReference15.PublicKeyToken = (assemblyNameReference16.PublicKeyToken = array6));
			}
			return new AssemblyNameReference[6]
			{
				assemblyNameReference,
				assemblyNameReference2,
				assemblyNameReference3,
				assemblyNameReference4,
				assemblyNameReference5,
				assemblyNameReference6
			};
		}

		private static AssemblyNameReference GetCoreLibrary(Collection<AssemblyNameReference> references)
		{
			foreach (AssemblyNameReference reference in references)
			{
				if (reference.Name == "mscorlib")
				{
					return reference;
				}
			}
			throw new BadImageFormatException("Missing mscorlib reference in AssemblyRef table.");
		}

		private AssemblyNameReference GetAssemblyReference(string name)
		{
			AssemblyNameReference[] virtualReferences = VirtualReferences;
			foreach (AssemblyNameReference assemblyNameReference in virtualReferences)
			{
				if (assemblyNameReference.Name == name)
				{
					return assemblyNameReference;
				}
			}
			throw new Exception();
		}

		public static void Project(ICustomAttributeProvider owner, CustomAttribute attribute)
		{
			if (IsWindowsAttributeUsageAttribute(owner, attribute))
			{
				CustomAttributeValueTreatment customAttributeValueTreatment = CustomAttributeValueTreatment.None;
				TypeDefinition typeDefinition = (TypeDefinition)owner;
				if (typeDefinition.Namespace == "Windows.Foundation.Metadata")
				{
					if (typeDefinition.Name == "VersionAttribute")
					{
						customAttributeValueTreatment = CustomAttributeValueTreatment.VersionAttribute;
					}
					else if (typeDefinition.Name == "DeprecatedAttribute")
					{
						customAttributeValueTreatment = CustomAttributeValueTreatment.DeprecatedAttribute;
					}
				}
				if (customAttributeValueTreatment == CustomAttributeValueTreatment.None)
				{
					customAttributeValueTreatment = ((!HasAttribute(typeDefinition, "Windows.Foundation.Metadata", "AllowMultipleAttribute")) ? CustomAttributeValueTreatment.AllowSingle : CustomAttributeValueTreatment.AllowMultiple);
				}
				if (customAttributeValueTreatment != 0)
				{
					AttributeTargets targets = (AttributeTargets)attribute.ConstructorArguments[0].Value;
					ApplyProjection(attribute, new CustomAttributeValueProjection(targets, customAttributeValueTreatment));
				}
			}
		}

		private static bool IsWindowsAttributeUsageAttribute(ICustomAttributeProvider owner, CustomAttribute attribute)
		{
			MetadataToken metadataToken = owner.MetadataToken;
			if (metadataToken.TokenType != TokenType.TypeDef)
			{
				return false;
			}
			MethodReference constructor = attribute.Constructor;
			metadataToken = constructor.MetadataToken;
			if (metadataToken.TokenType != TokenType.MemberRef)
			{
				return false;
			}
			TypeReference declaringType = constructor.DeclaringType;
			metadataToken = declaringType.MetadataToken;
			if (metadataToken.TokenType != TokenType.TypeRef)
			{
				return false;
			}
			if (declaringType.Name == "AttributeUsageAttribute")
			{
				return declaringType.Namespace == "System";
			}
			return false;
		}

		private static bool HasAttribute(TypeDefinition type, string @namespace, string name)
		{
			foreach (CustomAttribute customAttribute in type.CustomAttributes)
			{
				TypeReference attributeType = customAttribute.AttributeType;
				if (attributeType.Name == name && attributeType.Namespace == @namespace)
				{
					return true;
				}
			}
			return false;
		}

		public static void ApplyProjection(CustomAttribute attribute, CustomAttributeValueProjection projection)
		{
			if (projection != null)
			{
				bool flag;
				bool flag2;
				switch (projection.Treatment)
				{
				case CustomAttributeValueTreatment.AllowSingle:
					flag = false;
					flag2 = false;
					break;
				case CustomAttributeValueTreatment.AllowMultiple:
					flag = false;
					flag2 = true;
					break;
				case CustomAttributeValueTreatment.VersionAttribute:
				case CustomAttributeValueTreatment.DeprecatedAttribute:
					flag = true;
					flag2 = true;
					break;
				default:
					throw new ArgumentException();
				}
				CustomAttributeArgument customAttributeArgument = attribute.ConstructorArguments[0];
				AttributeTargets attributeTargets = (AttributeTargets)customAttributeArgument.Value;
				if (flag)
				{
					attributeTargets |= (AttributeTargets.Constructor | AttributeTargets.Property);
				}
				Collection<CustomAttributeArgument> constructorArguments = attribute.ConstructorArguments;
				customAttributeArgument = attribute.ConstructorArguments[0];
				constructorArguments[0] = new CustomAttributeArgument(customAttributeArgument.Type, attributeTargets);
				attribute.Properties.Add(new CustomAttributeNamedArgument("AllowMultiple", new CustomAttributeArgument(attribute.Module.TypeSystem.Boolean, flag2)));
				attribute.projection = projection;
			}
		}

		public static CustomAttributeValueProjection RemoveProjection(CustomAttribute attribute)
		{
			if (attribute.projection == null)
			{
				return null;
			}
			CustomAttributeValueProjection projection = attribute.projection;
			attribute.projection = null;
			attribute.ConstructorArguments[0] = new CustomAttributeArgument(attribute.ConstructorArguments[0].Type, projection.Targets);
			attribute.Properties.Clear();
			return projection;
		}
	}
}
