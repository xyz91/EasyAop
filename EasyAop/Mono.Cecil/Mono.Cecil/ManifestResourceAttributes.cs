using System;

namespace Mono.Cecil
{
	[Flags]
	public enum ManifestResourceAttributes : uint
	{
		VisibilityMask = 0x7,
		Public = 0x1,
		Private = 0x2
	}
}
