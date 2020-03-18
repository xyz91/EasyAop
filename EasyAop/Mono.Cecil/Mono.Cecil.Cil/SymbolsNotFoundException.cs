using System;
using System.IO;
using System.Runtime.Serialization;

namespace Mono.Cecil.Cil
{
	[Serializable]
	public sealed class SymbolsNotFoundException : FileNotFoundException
	{
		public SymbolsNotFoundException(string message)
			: base(message)
		{
		}

		private SymbolsNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
