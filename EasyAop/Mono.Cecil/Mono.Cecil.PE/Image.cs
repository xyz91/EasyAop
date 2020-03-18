using Mono.Cecil.Cil;
using Mono.Cecil.Metadata;
using System;
using System.IO;

namespace Mono.Cecil.PE
{
	internal sealed class Image : IDisposable
	{
		public Disposable<Stream> Stream;

		public string FileName;

		public ModuleKind Kind;

		public string RuntimeVersion;

		public TargetArchitecture Architecture;

		public ModuleCharacteristics Characteristics;

		public ushort LinkerVersion;

		public ImageDebugHeader DebugHeader;

		public Section[] Sections;

		public Section MetadataSection;

		public uint EntryPointToken;

		public uint Timestamp;

		public ModuleAttributes Attributes;

		public DataDirectory Win32Resources;

		public DataDirectory Debug;

		public DataDirectory Resources;

		public DataDirectory StrongName;

		public StringHeap StringHeap;

		public BlobHeap BlobHeap;

		public UserStringHeap UserStringHeap;

		public GuidHeap GuidHeap;

		public TableHeap TableHeap;

		public PdbHeap PdbHeap;

		private readonly int[] coded_index_sizes = new int[14];

		private readonly Func<Table, int> counter;

		public Image()
		{
			counter = GetTableLength;
		}

		public bool HasTable(Table table)
		{
			return GetTableLength(table) > 0;
		}

		public int GetTableLength(Table table)
		{
			return (int)TableHeap[table].Length;
		}

		public int GetTableIndexSize(Table table)
		{
			if (GetTableLength(table) >= 65536)
			{
				return 4;
			}
			return 2;
		}

		public int GetCodedIndexSize(CodedIndex coded_index)
		{
			int num = coded_index_sizes[(int)coded_index];
			if (num != 0)
			{
				return num;
			}
			return coded_index_sizes[(int)coded_index] = coded_index.GetSize(counter);
		}

		public uint ResolveVirtualAddress(uint rva)
		{
			Section sectionAtVirtualAddress = GetSectionAtVirtualAddress(rva);
			if (sectionAtVirtualAddress == null)
			{
				throw new ArgumentOutOfRangeException();
			}
			return ResolveVirtualAddressInSection(rva, sectionAtVirtualAddress);
		}

		public uint ResolveVirtualAddressInSection(uint rva, Section section)
		{
			return rva + section.PointerToRawData - section.VirtualAddress;
		}

		public Section GetSection(string name)
		{
			Section[] sections = Sections;
			foreach (Section section in sections)
			{
				if (section.Name == name)
				{
					return section;
				}
			}
			return null;
		}

		public Section GetSectionAtVirtualAddress(uint rva)
		{
			Section[] sections = Sections;
			foreach (Section section in sections)
			{
				if (rva >= section.VirtualAddress && rva < section.VirtualAddress + section.SizeOfRawData)
				{
					return section;
				}
			}
			return null;
		}

		private BinaryStreamReader GetReaderAt(uint rva)
		{
			Section sectionAtVirtualAddress = GetSectionAtVirtualAddress(rva);
			if (sectionAtVirtualAddress == null)
			{
				return null;
			}
			BinaryStreamReader binaryStreamReader = new BinaryStreamReader(Stream.value);
			binaryStreamReader.MoveTo(ResolveVirtualAddressInSection(rva, sectionAtVirtualAddress));
			return binaryStreamReader;
		}

		public TRet GetReaderAt<TItem, TRet>(uint rva, TItem item, Func<TItem, BinaryStreamReader, TRet> read) where TRet : class
		{
			long position = Stream.value.Position;
			try
			{
				BinaryStreamReader readerAt = GetReaderAt(rva);
				if (readerAt == null)
				{
					return null;
				}
				return read(item, readerAt);
			}
			finally
			{
				Stream.value.Position = position;
			}
		}

		public bool HasDebugTables()
		{
			if (!HasTable(Table.Document) && !HasTable(Table.MethodDebugInformation) && !HasTable(Table.LocalScope) && !HasTable(Table.LocalVariable) && !HasTable(Table.LocalConstant) && !HasTable(Table.StateMachineMethod))
			{
				return HasTable(Table.CustomDebugInformation);
			}
			return true;
		}

		public void Dispose()
		{
			Stream.Dispose();
		}
	}
}
