using System;

namespace Mono.Cecil.Cil
{
	public sealed class Document : DebugInformation
	{
		private string url;

		private Guid type;

		private Guid hash_algorithm;

		private Guid language;

		private Guid language_vendor;

		private byte[] hash;

		private byte[] embedded_source;

		public string Url
		{
			get
			{
				return url;
			}
			set
			{
				url = value;
			}
		}

		public DocumentType Type
		{
			get
			{
				return type.ToType();
			}
			set
			{
				type = value.ToGuid();
			}
		}

		public Guid TypeGuid
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
			}
		}

		public DocumentHashAlgorithm HashAlgorithm
		{
			get
			{
				return hash_algorithm.ToHashAlgorithm();
			}
			set
			{
				hash_algorithm = value.ToGuid();
			}
		}

		public Guid HashAlgorithmGuid
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

		public DocumentLanguage Language
		{
			get
			{
				return language.ToLanguage();
			}
			set
			{
				language = value.ToGuid();
			}
		}

		public Guid LanguageGuid
		{
			get
			{
				return language;
			}
			set
			{
				language = value;
			}
		}

		public DocumentLanguageVendor LanguageVendor
		{
			get
			{
				return language_vendor.ToVendor();
			}
			set
			{
				language_vendor = value.ToGuid();
			}
		}

		public Guid LanguageVendorGuid
		{
			get
			{
				return language_vendor;
			}
			set
			{
				language_vendor = value;
			}
		}

		public byte[] Hash
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

		public byte[] EmbeddedSource
		{
			get
			{
				return embedded_source;
			}
			set
			{
				embedded_source = value;
			}
		}

		public Document(string url)
		{
			this.url = url;
			hash = Empty<byte>.Array;
			embedded_source = Empty<byte>.Array;
			base.token = new MetadataToken(TokenType.Document);
		}
	}
}
