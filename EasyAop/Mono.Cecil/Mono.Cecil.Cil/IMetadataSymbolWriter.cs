using System;

namespace Mono.Cecil.Cil
{
	internal interface IMetadataSymbolWriter : ISymbolWriter, IDisposable
	{
		void SetMetadata(MetadataBuilder metadata);

		void WriteModule();
	}
}
