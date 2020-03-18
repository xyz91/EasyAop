using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mono.Cecil
{
	public abstract class BaseAssemblyResolver : IAssemblyResolver, IDisposable
	{
		private static readonly bool on_mono = Type.GetType("Mono.Runtime") != (Type)null;

		private readonly Collection<string> directories;

		private Collection<string> gac_paths;

		public event AssemblyResolveEventHandler ResolveFailure;

		public void AddSearchDirectory(string directory)
		{
			directories.Add(directory);
		}

		public void RemoveSearchDirectory(string directory)
		{
			directories.Remove(directory);
		}

		public string[] GetSearchDirectories()
		{
			string[] array = new string[directories.size];
			Array.Copy(directories.items, array, array.Length);
			return array;
		}

		protected BaseAssemblyResolver()
		{
			directories = new Collection<string>(2)
			{
				".",
				"bin",                
			};
		}

		private AssemblyDefinition GetAssembly(string file, ReaderParameters parameters)
		{
			if (parameters.AssemblyResolver == null)
			{
				parameters.AssemblyResolver = this;
			}
			return ModuleDefinition.ReadModule(file, parameters).Assembly;
		}

		public virtual AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			return Resolve(name, new ReaderParameters());
		}

		public virtual AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
		{
			Mixin.CheckName(name);
			Mixin.CheckParameters(parameters);
			AssemblyDefinition assemblyDefinition = SearchDirectory(name, directories, parameters);
			if (assemblyDefinition != null)
			{
				return assemblyDefinition;
			}
			if (name.IsRetargetable)
			{
				name = new AssemblyNameReference(name.Name, Mixin.ZeroVersion)
				{
					PublicKeyToken = Empty<byte>.Array
				};
			}
			string directoryName = Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName);
			string[] array = on_mono ? new string[2]
			{
				directoryName,
				Path.Combine(directoryName, "Facades")
			} : new string[1]
			{
				directoryName
			};
			if (IsZero(name.Version))
			{
				assemblyDefinition = SearchDirectory(name, array, parameters);
				if (assemblyDefinition != null)
				{
					return assemblyDefinition;
				}
			}
			if (name.Name == "mscorlib")
			{
				assemblyDefinition = GetCorlib(name, parameters);
				if (assemblyDefinition != null)
				{
					return assemblyDefinition;
				}
			}
			assemblyDefinition = GetAssemblyInGac(name, parameters);
			if (assemblyDefinition != null)
			{
				return assemblyDefinition;
			}
			assemblyDefinition = SearchDirectory(name, array, parameters);
			if (assemblyDefinition != null)
			{
				return assemblyDefinition;
			}
			if (this.ResolveFailure != null)
			{
				assemblyDefinition = this.ResolveFailure(this, name);
				if (assemblyDefinition != null)
				{
					return assemblyDefinition;
				}
			}           
            throw new AssemblyResolutionException(name);
		}

		protected virtual AssemblyDefinition SearchDirectory(AssemblyNameReference name, IEnumerable<string> directories, ReaderParameters parameters)
		{
			string[] array = name.IsWindowsRuntime ? new string[2]
			{
				".winmd",
				".dll"
			} : new string[2]
			{
				".exe",
				".dll"
			};
			foreach (string directory in directories)
			{               
				string[] array2 = array;
				foreach (string str in array2)
				{                   
                    string text = Path.Combine(directory, name.Name + str);
                    if (File.Exists(text))
                    {
                        try
                        {
                            return GetAssembly(text, parameters);
                        }
                        catch (BadImageFormatException)
                        {
                        }
                    }
                    else {
                        
                    }
				}
			}
			return null;
		}

		private static bool IsZero(Version version)
		{
			if (version.Major == 0 && version.Minor == 0 && version.Build == 0)
			{
				return version.Revision == 0;
			}
			return false;
		}

		private AssemblyDefinition GetCorlib(AssemblyNameReference reference, ReaderParameters parameters)
		{
			Version version = reference.Version;
			string fullName;
			if (!(typeof(object).Assembly.GetName().Version == version) && !IsZero(version))
			{
				fullName = Directory.GetParent(Directory.GetParent(typeof(object).Module.FullyQualifiedName).FullName).FullName;
				if (on_mono)
				{
					if (version.Major == 1)
					{
						fullName = Path.Combine(fullName, "1.0");
						goto IL_016c;
					}
					if (version.Major == 2)
					{
						fullName = ((version.MajorRevision != 5) ? Path.Combine(fullName, "2.0") : Path.Combine(fullName, "2.1"));
						goto IL_016c;
					}
					if (version.Major == 4)
					{
						fullName = Path.Combine(fullName, "4.0");
						goto IL_016c;
					}
					throw new NotSupportedException("Version not supported: " + version);
				}
				switch (version.Major)
				{
				case 1:
					fullName = ((version.MajorRevision != 3300) ? Path.Combine(fullName, "v1.0.5000.0") : Path.Combine(fullName, "v1.0.3705"));
					break;
				case 2:
					fullName = Path.Combine(fullName, "v2.0.50727");
					break;
				case 4:
					fullName = Path.Combine(fullName, "v4.0.30319");
					break;
				default:
					throw new NotSupportedException("Version not supported: " + version);
				}
				goto IL_016c;
			}
			return GetAssembly(typeof(object).Module.FullyQualifiedName, parameters);
			IL_016c:
			string text = Path.Combine(fullName, "mscorlib.dll");
			if (File.Exists(text))
			{
				return GetAssembly(text, parameters);
			}
			if (on_mono && Directory.Exists(fullName + "-api"))
			{
				text = Path.Combine(fullName + "-api", "mscorlib.dll");
				if (File.Exists(text))
				{
					return GetAssembly(text, parameters);
				}
			}
			return null;
		}

		private static Collection<string> GetGacPaths()
		{
			if (on_mono)
			{
				return GetDefaultMonoGacPaths();
			}
			Collection<string> collection = new Collection<string>(2);
			string environmentVariable = Environment.GetEnvironmentVariable("WINDIR");
			if (environmentVariable == null)
			{
				return collection;
			}
			collection.Add(Path.Combine(environmentVariable, "assembly"));
			collection.Add(Path.Combine(environmentVariable, Path.Combine("Microsoft.NET", "assembly")));
			return collection;
		}

		private static Collection<string> GetDefaultMonoGacPaths()
		{
			Collection<string> collection = new Collection<string>(1);
			string currentMonoGac = GetCurrentMonoGac();
			if (currentMonoGac != null)
			{
				collection.Add(currentMonoGac);
			}
			string environmentVariable = Environment.GetEnvironmentVariable("MONO_GAC_PREFIX");
			if (string.IsNullOrEmpty(environmentVariable))
			{
				return collection;
			}
			string[] array = environmentVariable.Split(Path.PathSeparator);
			foreach (string text in array)
			{
				if (!string.IsNullOrEmpty(text))
				{
					string text2 = Path.Combine(Path.Combine(Path.Combine(text, "lib"), "mono"), "gac");
					if (Directory.Exists(text2) && !collection.Contains(currentMonoGac))
					{
						collection.Add(text2);
					}
				}
			}
			return collection;
		}

		private static string GetCurrentMonoGac()
		{
			return Path.Combine(Directory.GetParent(Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName)).FullName, "gac");
		}

		private AssemblyDefinition GetAssemblyInGac(AssemblyNameReference reference, ReaderParameters parameters)
		{
			if (reference.PublicKeyToken != null && reference.PublicKeyToken.Length != 0)
			{
				if (gac_paths == null)
				{
					gac_paths = GetGacPaths();
				}
				if (on_mono)
				{
					return GetAssemblyInMonoGac(reference, parameters);
				}
				return GetAssemblyInNetGac(reference, parameters);
			}
			return null;
		}

		private AssemblyDefinition GetAssemblyInMonoGac(AssemblyNameReference reference, ReaderParameters parameters)
		{
			for (int i = 0; i < gac_paths.Count; i++)
			{
				string gac = gac_paths[i];
				string assemblyFile = GetAssemblyFile(reference, string.Empty, gac);
				if (File.Exists(assemblyFile))
				{
					return GetAssembly(assemblyFile, parameters);
				}
			}
			return null;
		}

		private AssemblyDefinition GetAssemblyInNetGac(AssemblyNameReference reference, ReaderParameters parameters)
		{
			string[] array = new string[4]
			{
				"GAC_MSIL",
				"GAC_32",
				"GAC_64",
				"GAC"
			};
			string[] array2 = new string[2]
			{
				string.Empty,
				"v4.0_"
			};
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < array.Length; j++)
				{
					string text = Path.Combine(gac_paths[i], array[j]);
					string assemblyFile = GetAssemblyFile(reference, array2[i], text);
					if (Directory.Exists(text) && File.Exists(assemblyFile))
					{
						return GetAssembly(assemblyFile, parameters);
					}
				}
			}
			return null;
		}

		private static string GetAssemblyFile(AssemblyNameReference reference, string prefix, string gac)
		{
			StringBuilder stringBuilder = new StringBuilder().Append(prefix).Append(reference.Version).Append("__");
			for (int i = 0; i < reference.PublicKeyToken.Length; i++)
			{
				stringBuilder.Append(reference.PublicKeyToken[i].ToString("x2"));
			}
			return Path.Combine(Path.Combine(Path.Combine(gac, reference.Name), stringBuilder.ToString()), reference.Name + ".dll");
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}
	}
}
