namespace Mono.Cecil.PE
{
	internal struct DataDirectory
	{
		public readonly uint VirtualAddress;

		public readonly uint Size;

		public bool IsZero
		{
			get
			{
				if (VirtualAddress == 0)
				{
					return Size == 0;
				}
				return false;
			}
		}

		public DataDirectory(uint rva, uint size)
		{
			VirtualAddress = rva;
			Size = size;
		}
	}
}
