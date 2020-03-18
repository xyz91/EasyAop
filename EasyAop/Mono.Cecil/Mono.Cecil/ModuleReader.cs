using Mono.Cecil.Cil;
using Mono.Cecil.PE;
using System;

namespace Mono.Cecil
{
	internal abstract class ModuleReader
	{
		protected readonly ModuleDefinition module;

		protected ModuleReader(Image image, ReadingMode mode)
		{
			module = new ModuleDefinition(image);
			module.ReadingMode = mode;
		}

		protected abstract void ReadModule();

		public abstract void ReadSymbols(ModuleDefinition module);

		protected void ReadModuleManifest(MetadataReader reader)
		{
			reader.Populate(module);
			ReadAssembly(reader);
		}

		private void ReadAssembly(MetadataReader reader)
		{
			AssemblyNameDefinition assemblyNameDefinition = reader.ReadAssemblyNameDefinition();
			if (assemblyNameDefinition == null)
			{
				module.kind = ModuleKind.NetModule;
			}
			else
			{
				AssemblyDefinition assemblyDefinition = new AssemblyDefinition();
				assemblyDefinition.Name = assemblyNameDefinition;
				module.assembly = assemblyDefinition;
				assemblyDefinition.main_module = module;
			}
		}

		public static ModuleDefinition CreateModule(Image image, ReaderParameters parameters)
		{
			ModuleReader moduleReader = CreateModuleReader(image, parameters.ReadingMode);
			ModuleDefinition moduleDefinition = moduleReader.module;
			if (parameters.assembly_resolver != null)
			{
				moduleDefinition.assembly_resolver = Disposable.NotOwned(parameters.assembly_resolver);
			}
			if (parameters.metadata_resolver != null)
			{
				moduleDefinition.metadata_resolver = parameters.metadata_resolver;
			}
			if (parameters.metadata_importer_provider != null)
			{
				moduleDefinition.metadata_importer = parameters.metadata_importer_provider.GetMetadataImporter(moduleDefinition);
			}
			if (parameters.reflection_importer_provider != null)
			{
				moduleDefinition.reflection_importer = parameters.reflection_importer_provider.GetReflectionImporter(moduleDefinition);
			}
			GetMetadataKind(moduleDefinition, parameters);
			moduleReader.ReadModule();
			ReadSymbols(moduleDefinition, parameters);
			moduleReader.ReadSymbols(moduleDefinition);
			if (parameters.ReadingMode == ReadingMode.Immediate)
			{
				moduleDefinition.MetadataSystem.Clear();
			}
			return moduleDefinition;
		}

		private static void ReadSymbols(ModuleDefinition module, ReaderParameters parameters)
		{
			ISymbolReaderProvider symbolReaderProvider = parameters.SymbolReaderProvider;
			if (symbolReaderProvider == null && parameters.ReadSymbols)
			{
				symbolReaderProvider = new DefaultSymbolReaderProvider();
			}
			if (symbolReaderProvider != null)
			{
				module.SymbolReaderProvider = symbolReaderProvider;
				ISymbolReader symbolReader = (parameters.SymbolStream != null) ? symbolReaderProvider.GetSymbolReader(module, parameters.SymbolStream) : symbolReaderProvider.GetSymbolReader(module, module.FileName);
				if (symbolReader != null)
				{
					module.ReadSymbols(symbolReader, parameters.ThrowIfSymbolsAreNotMatching);
				}
			}
			if (module.Image.HasDebugTables())
			{
				module.ReadSymbols(new PortablePdbReader(module.Image, module));
			}
		}

		private static void GetMetadataKind(ModuleDefinition module, ReaderParameters parameters)
		{
			if (!parameters.ApplyWindowsRuntimeProjections)
			{
				module.MetadataKind = MetadataKind.Ecma335;
			}
			else
			{
				string runtimeVersion = module.RuntimeVersion;
				if (!runtimeVersion.Contains("WindowsRuntime"))
				{
					module.MetadataKind = MetadataKind.Ecma335;
				}
				else if (runtimeVersion.Contains("CLR"))
				{
					module.MetadataKind = MetadataKind.ManagedWindowsMetadata;
				}
				else
				{
					module.MetadataKind = MetadataKind.WindowsMetadata;
				}
			}
		}

		private static ModuleReader CreateModuleReader(Image image, ReadingMode mode)
		{
			switch (mode)
			{
			case ReadingMode.Immediate:
				return new ImmediateModuleReader(image);
			case ReadingMode.Deferred:
				return new DeferredModuleReader(image);
			default:
				throw new ArgumentException();
			}
		}
	}
}
