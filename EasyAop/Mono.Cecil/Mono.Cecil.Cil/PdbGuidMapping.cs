using System;
using System.Collections.Generic;

namespace Mono.Cecil.Cil
{
	internal static class PdbGuidMapping
	{
		private static readonly Dictionary<Guid, DocumentLanguage> guid_language;

		private static readonly Dictionary<DocumentLanguage, Guid> language_guid;

		private static readonly Guid type_text;

		private static readonly Guid hash_md5;

		private static readonly Guid hash_sha1;

		private static readonly Guid hash_sha256;

		private static readonly Guid vendor_ms;

		static PdbGuidMapping()
		{
			guid_language = new Dictionary<Guid, DocumentLanguage>();
			language_guid = new Dictionary<DocumentLanguage, Guid>();
			type_text = new Guid("5a869d0b-6611-11d3-bd2a-0000f80849bd");
			hash_md5 = new Guid("406ea660-64cf-4c82-b6f0-42d48172a799");
			hash_sha1 = new Guid("ff1816ec-aa5e-4d10-87f7-6f4963833460");
			hash_sha256 = new Guid("8829d00f-11b8-4213-878b-770e8597ac16");
			vendor_ms = new Guid("994b45c4-e6e9-11d2-903f-00c04fa302a1");
			AddMapping(DocumentLanguage.C, new Guid("63a08714-fc37-11d2-904c-00c04fa302a1"));
			AddMapping(DocumentLanguage.Cpp, new Guid("3a12d0b7-c26c-11d0-b442-00a0244a1dd2"));
			AddMapping(DocumentLanguage.CSharp, new Guid("3f5162f8-07c6-11d3-9053-00c04fa302a1"));
			AddMapping(DocumentLanguage.Basic, new Guid("3a12d0b8-c26c-11d0-b442-00a0244a1dd2"));
			AddMapping(DocumentLanguage.Java, new Guid("3a12d0b4-c26c-11d0-b442-00a0244a1dd2"));
			AddMapping(DocumentLanguage.Cobol, new Guid("af046cd1-d0e1-11d2-977c-00a0c9b4d50c"));
			AddMapping(DocumentLanguage.Pascal, new Guid("af046cd2-d0e1-11d2-977c-00a0c9b4d50c"));
			AddMapping(DocumentLanguage.Cil, new Guid("af046cd3-d0e1-11d2-977c-00a0c9b4d50c"));
			AddMapping(DocumentLanguage.JScript, new Guid("3a12d0b6-c26c-11d0-b442-00a0244a1dd2"));
			AddMapping(DocumentLanguage.Smc, new Guid("0d9b9f7b-6611-11d3-bd2a-0000f80849bd"));
			AddMapping(DocumentLanguage.MCpp, new Guid("4b35fde8-07c6-11d3-9053-00c04fa302a1"));
			AddMapping(DocumentLanguage.FSharp, new Guid("ab4f38c9-b6e6-43ba-be3b-58080b2ccce3"));
		}

		private static void AddMapping(DocumentLanguage language, Guid guid)
		{
			guid_language.Add(guid, language);
			language_guid.Add(language, guid);
		}

		public static DocumentType ToType(this Guid guid)
		{
			if (guid == type_text)
			{
				return DocumentType.Text;
			}
			return DocumentType.Other;
		}

		public static Guid ToGuid(this DocumentType type)
		{
			if (type == DocumentType.Text)
			{
				return type_text;
			}
			return default(Guid);
		}

		public static DocumentHashAlgorithm ToHashAlgorithm(this Guid guid)
		{
			if (guid == hash_md5)
			{
				return DocumentHashAlgorithm.MD5;
			}
			if (guid == hash_sha1)
			{
				return DocumentHashAlgorithm.SHA1;
			}
			if (guid == hash_sha256)
			{
				return DocumentHashAlgorithm.SHA256;
			}
			return DocumentHashAlgorithm.None;
		}

		public static Guid ToGuid(this DocumentHashAlgorithm hash_algo)
		{
			switch (hash_algo)
			{
			case DocumentHashAlgorithm.MD5:
				return hash_md5;
			case DocumentHashAlgorithm.SHA1:
				return hash_sha1;
			case DocumentHashAlgorithm.SHA256:
				return hash_sha256;
			default:
				return default(Guid);
			}
		}

		public static DocumentLanguage ToLanguage(this Guid guid)
		{
			if (!guid_language.TryGetValue(guid, out DocumentLanguage result))
			{
				return DocumentLanguage.Other;
			}
			return result;
		}

		public static Guid ToGuid(this DocumentLanguage language)
		{
			if (!language_guid.TryGetValue(language, out Guid result))
			{
				return default(Guid);
			}
			return result;
		}

		public static DocumentLanguageVendor ToVendor(this Guid guid)
		{
			if (guid == vendor_ms)
			{
				return DocumentLanguageVendor.Microsoft;
			}
			return DocumentLanguageVendor.Other;
		}

		public static Guid ToGuid(this DocumentLanguageVendor vendor)
		{
			if (vendor == DocumentLanguageVendor.Microsoft)
			{
				return vendor_ms;
			}
			return default(Guid);
		}
	}
}
