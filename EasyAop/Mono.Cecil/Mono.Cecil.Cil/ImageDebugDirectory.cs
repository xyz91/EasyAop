namespace Mono.Cecil.Cil
{
	public struct ImageDebugDirectory
	{
		public const int Size = 28;

		public int Characteristics;

		public int TimeDateStamp;

		public short MajorVersion;

		public short MinorVersion;

		public ImageDebugType Type;

		public int SizeOfData;

		public int AddressOfRawData;

		public int PointerToRawData;
	}
}
