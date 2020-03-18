using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Cecil
{
	public class AssemblyNameReference : IMetadataScope, IMetadataTokenProvider
	{
		private string name;

		private string culture;

		private Version version;

		private uint attributes;

		private byte[] public_key;

		private byte[] public_key_token;

		private AssemblyHashAlgorithm hash_algorithm;

		private byte[] hash;

		internal MetadataToken token;

		private string full_name;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
				full_name = null;
			}
		}

		public string Culture
		{
			get
			{
				return culture;
			}
			set
			{
				culture = value;
				full_name = null;
			}
		}

		public Version Version
		{
			get
			{
				return version;
			}
			set
			{
				version = Mixin.CheckVersion(value);
				full_name = null;
			}
		}

		public AssemblyAttributes Attributes
		{
			get
			{
				return (AssemblyAttributes)attributes;
			}
			set
			{
				attributes = (uint)value;
			}
		}

		public bool HasPublicKey
		{
			get
			{
				return attributes.GetAttributes(1u);
			}
			set
			{
				attributes = attributes.SetAttributes(1u, value);
			}
		}

		public bool IsSideBySideCompatible
		{
			get
			{
				return attributes.GetAttributes(0u);
			}
			set
			{
				attributes = attributes.SetAttributes(0u, value);
			}
		}

		public bool IsRetargetable
		{
			get
			{
				return attributes.GetAttributes(256u);
			}
			set
			{
				attributes = attributes.SetAttributes(256u, value);
			}
		}

		public bool IsWindowsRuntime
		{
			get
			{
				return attributes.GetAttributes(512u);
			}
			set
			{
				attributes = attributes.SetAttributes(512u, value);
			}
		}

		public byte[] PublicKey
		{
			get
			{
				return public_key ?? Empty<byte>.Array;
			}
			set
			{
				public_key = value;
				HasPublicKey = !public_key.IsNullOrEmpty();
				public_key_token = Empty<byte>.Array;
				full_name = null;
			}
		}

		public byte[] PublicKeyToken
		{
			get
			{
				if (public_key_token.IsNullOrEmpty() && !public_key.IsNullOrEmpty())
				{
					byte[] array = HashPublicKey();
					byte[] array2 = new byte[8];
					Array.Copy(array, array.Length - 8, array2, 0, 8);
					Array.Reverse(array2, 0, 8);
					public_key_token = array2;
				}
				return public_key_token ?? Empty<byte>.Array;
			}
			set
			{
				public_key_token = value;
				full_name = null;
			}
		}

		public virtual MetadataScopeType MetadataScopeType => MetadataScopeType.AssemblyNameReference;

		public string FullName
		{
			get
			{
				if (full_name != null)
				{
					return full_name;
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(name);
				stringBuilder.Append(", ");
				stringBuilder.Append("Version=");
				stringBuilder.Append(version.ToString(4));
				stringBuilder.Append(", ");
				stringBuilder.Append("Culture=");
				stringBuilder.Append(string.IsNullOrEmpty(culture) ? "neutral" : culture);
				stringBuilder.Append(", ");
				stringBuilder.Append("PublicKeyToken=");
				byte[] publicKeyToken = PublicKeyToken;
				if (!publicKeyToken.IsNullOrEmpty() && publicKeyToken.Length != 0)
				{
					for (int i = 0; i < publicKeyToken.Length; i++)
					{
						stringBuilder.Append(publicKeyToken[i].ToString("x2"));
					}
				}
				else
				{
					stringBuilder.Append("null");
				}
				if (IsRetargetable)
				{
					stringBuilder.Append(", ");
					stringBuilder.Append("Retargetable=Yes");
				}
				return full_name = stringBuilder.ToString();
			}
		}

		public AssemblyHashAlgorithm HashAlgorithm
		{
			get
			{
				return hash_algorithm;
			}
			set
			{
				hash_algorithm = value;
			}
		}

		public virtual byte[] Hash
		{
			get
			{
				return hash;
			}
			set
			{
				hash = value;
			}
		}

		public MetadataToken MetadataToken
		{
			get
			{
				return token;
			}
			set
			{
				token = value;
			}
		}

		private byte[] HashPublicKey()
		{
			AssemblyHashAlgorithm assemblyHashAlgorithm = hash_algorithm;
			HashAlgorithm hashAlgorithm = (assemblyHashAlgorithm != AssemblyHashAlgorithm.Reserved) ? ((HashAlgorithm)SHA1.Create()) : ((HashAlgorithm)MD5.Create());
			using (hashAlgorithm)
			{
				return hashAlgorithm.ComputeHash(public_key);
			}
		}

		public static AssemblyNameReference Parse(string fullName)
		{
			if (fullName == null)
			{
				throw new ArgumentNullException("fullName");
			}
			if (fullName.Length == 0)
			{
				throw new ArgumentException("Name can not be empty");
			}
			AssemblyNameReference assemblyNameReference = new AssemblyNameReference();
			string[] array = fullName.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].Trim();
				if (i == 0)
				{
					assemblyNameReference.Name = text;
				}
				else
				{
					string[] array2 = text.Split('=');
					if (array2.Length != 2)
					{
						throw new ArgumentException("Malformed name");
					}
					switch (array2[0].ToLowerInvariant())
					{
					case "version":
						assemblyNameReference.Version = new Version(array2[1]);
						break;
					case "culture":
						assemblyNameReference.Culture = ((array2[1] == "neutral") ? "" : array2[1]);
						break;
					case "publickeytoken":
					{
						string text2 = array2[1];
						if (!(text2 == "null"))
						{
							assemblyNameReference.PublicKeyToken = new byte[text2.Length / 2];
							for (int j = 0; j < assemblyNameReference.PublicKeyToken.Length; j++)
							{
								assemblyNameReference.PublicKeyToken[j] = byte.Parse(text2.Substring(j * 2, 2), NumberStyles.HexNumber);
							}
						}
						break;
					}
					}
				}
			}
			return assemblyNameReference;
		}

		internal AssemblyNameReference()
		{
			version = Mixin.ZeroVersion;
			token = new MetadataToken(TokenType.AssemblyRef);
		}

		public AssemblyNameReference(string name, Version version)
		{
			Mixin.CheckName(name);
			this.name = name;
			this.version = Mixin.CheckVersion(version);
			hash_algorithm = AssemblyHashAlgorithm.None;
			token = new MetadataToken(TokenType.AssemblyRef);
		}

		public override string ToString()
		{
			return FullName;
		}
	}
}
