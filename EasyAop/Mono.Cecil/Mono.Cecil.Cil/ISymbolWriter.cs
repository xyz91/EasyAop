using System;

namespace Mono.Cecil.Cil
{
	public interface ISymbolWriter : IDisposable
	{
		ISymbolReaderProvider GetReaderProvider();

		ImageDebugHeader GetDebugHeader();

		void Write(MethodDebugInformation info);
	}
}
