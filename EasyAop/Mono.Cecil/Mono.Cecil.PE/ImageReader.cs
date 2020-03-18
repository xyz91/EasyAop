using Mono.Cecil.Cil;
using Mono.Cecil.Metadata;
using System;
using System.IO;

namespace Mono.Cecil.PE
{
	internal sealed class ImageReader : BinaryStreamReader
	{
		private readonly Image image;

		private DataDirectory cli;

		private DataDirectory metadata;

		private uint table_heap_offset;

		public ImageReader(Disposable<Stream> stream, string file_name)
			: base(stream.value)
		{
			image = new Image();
			image.Stream = stream;
			image.FileName = file_name;
		}

		private void MoveTo(DataDirectory directory)
		{
			BaseStream.Position = image.ResolveVirtualAddress(directory.VirtualAddress);
		}

		private void ReadImage()
		{
			if (BaseStream.Length < 128)
			{
				throw new BadImageFormatException();
			}
			if (ReadUInt16() != 23117)
			{
				throw new BadImageFormatException();
			}
			base.Advance(58);
			base.MoveTo(ReadUInt32());
			if (ReadUInt32() != 17744)
			{
				throw new BadImageFormatException();
			}
			image.Architecture = ReadArchitecture();
			ushort count = ReadUInt16();
			image.Timestamp = ReadUInt32();
			base.Advance(10);
			ushort characteristics = ReadUInt16();
			ReadOptionalHeaders(out ushort subsystem, out ushort characteristics2, out ushort linkerVersion);
			ReadSections(count);
			ReadCLIHeader();
			ReadMetadata();
			ReadDebugHeader();
			image.Kind = GetModuleKind(characteristics, subsystem);
			image.Characteristics = (ModuleCharacteristics)characteristics2;
			image.LinkerVersion = linkerVersion;
		}

		private TargetArchitecture ReadArchitecture()
		{
			return (TargetArchitecture)ReadUInt16();
		}

		private static ModuleKind GetModuleKind(ushort characteristics, ushort subsystem)
		{
			if ((characteristics & 0x2000) != 0)
			{
				return ModuleKind.Dll;
			}
			if (subsystem != 2 && subsystem != 9)
			{
				return ModuleKind.Console;
			}
			return ModuleKind.Windows;
		}

		private void ReadOptionalHeaders(out ushort subsystem, out ushort dll_characteristics, out ushort linker)
		{
			bool flag = ReadUInt16() == 523;
			linker = ReadUInt16();
			base.Advance(64);
			subsystem = ReadUInt16();
			dll_characteristics = ReadUInt16();
			base.Advance(flag ? 56 : 40);
			image.Win32Resources = base.ReadDataDirectory();
			base.Advance(24);
			image.Debug = base.ReadDataDirectory();
			base.Advance(56);
			cli = base.ReadDataDirectory();
			if (cli.IsZero)
			{
				throw new BadImageFormatException();
			}
			base.Advance(8);
		}

		private string ReadAlignedString(int length)
		{
			int num = 0;
			char[] array = new char[length];
			while (num < length)
			{
				byte b = ReadByte();
				if (b == 0)
				{
					break;
				}
				array[num++] = (char)b;
			}
			base.Advance(-1 + (num + 4 & -4) - num);
			return new string(array, 0, num);
		}

		private string ReadZeroTerminatedString(int length)
		{
			int num = 0;
			char[] array = new char[length];
			byte[] array2 = ReadBytes(length);
			while (num < length)
			{
				byte b = array2[num];
				if (b == 0)
				{
					break;
				}
				array[num++] = (char)b;
			}
			return new string(array, 0, num);
		}

		private void ReadSections(ushort count)
		{
			Section[] array = new Section[count];
			for (int i = 0; i < count; i++)
			{
				Section section = new Section();
				section.Name = ReadZeroTerminatedString(8);
				base.Advance(4);
				section.VirtualAddress = ReadUInt32();
				section.SizeOfRawData = ReadUInt32();
				section.PointerToRawData = ReadUInt32();
				base.Advance(16);
				array[i] = section;
			}
			image.Sections = array;
		}

		private void ReadCLIHeader()
		{
			MoveTo(cli);
			base.Advance(8);
			metadata = base.ReadDataDirectory();
			image.Attributes = (ModuleAttributes)ReadUInt32();
			image.EntryPointToken = ReadUInt32();
			image.Resources = base.ReadDataDirectory();
			image.StrongName = base.ReadDataDirectory();
		}

		private void ReadMetadata()
		{
			MoveTo(metadata);
			if (ReadUInt32() != 1112167234)
			{
				throw new BadImageFormatException();
			}
			base.Advance(8);
			image.RuntimeVersion = ReadZeroTerminatedString(ReadInt32());
			base.Advance(2);
			ushort num = ReadUInt16();
			Section sectionAtVirtualAddress = image.GetSectionAtVirtualAddress(metadata.VirtualAddress);
			if (sectionAtVirtualAddress == null)
			{
				throw new BadImageFormatException();
			}
			image.MetadataSection = sectionAtVirtualAddress;
			for (int i = 0; i < num; i++)
			{
				ReadMetadataStream(sectionAtVirtualAddress);
			}
			if (image.PdbHeap != null)
			{
				ReadPdbHeap();
			}
			if (image.TableHeap != null)
			{
				ReadTableHeap();
			}
		}

		private void ReadDebugHeader()
		{
			if (image.Debug.IsZero)
			{
				image.DebugHeader = new ImageDebugHeader(Empty<ImageDebugHeaderEntry>.Array);
			}
			else
			{
				MoveTo(image.Debug);
				ImageDebugHeaderEntry[] array = new ImageDebugHeaderEntry[(int)image.Debug.Size / 28];
				for (int i = 0; i < array.Length; i++)
				{
					ImageDebugDirectory imageDebugDirectory = default(ImageDebugDirectory);
					imageDebugDirectory.Characteristics = ReadInt32();
					imageDebugDirectory.TimeDateStamp = ReadInt32();
					imageDebugDirectory.MajorVersion = ReadInt16();
					imageDebugDirectory.MinorVersion = ReadInt16();
					imageDebugDirectory.Type = (ImageDebugType)ReadInt32();
					imageDebugDirectory.SizeOfData = ReadInt32();
					imageDebugDirectory.AddressOfRawData = ReadInt32();
					imageDebugDirectory.PointerToRawData = ReadInt32();
					ImageDebugDirectory imageDebugDirectory2 = imageDebugDirectory;
					if (imageDebugDirectory2.AddressOfRawData == 0)
					{
						array[i] = new ImageDebugHeaderEntry(imageDebugDirectory2, Empty<byte>.Array);
					}
					else
					{
						int position = base.Position;
						try
						{
							base.MoveTo((uint)imageDebugDirectory2.PointerToRawData);
							byte[] data = ReadBytes(imageDebugDirectory2.SizeOfData);
							array[i] = new ImageDebugHeaderEntry(imageDebugDirectory2, data);
						}
						finally
						{
							base.Position = position;
						}
					}
				}
				image.DebugHeader = new ImageDebugHeader(array);
			}
		}

		private void ReadMetadataStream(Section section)
		{
			uint offset = metadata.VirtualAddress - section.VirtualAddress + ReadUInt32();
			uint size = ReadUInt32();
			byte[] data = ReadHeapData(offset, size);
			switch (ReadAlignedString(16))
			{
			case "#~":
			case "#-":
				image.TableHeap = new TableHeap(data);
				table_heap_offset = offset;
				break;
			case "#Strings":
				image.StringHeap = new StringHeap(data);
				break;
			case "#Blob":
				image.BlobHeap = new BlobHeap(data);
				break;
			case "#GUID":
				image.GuidHeap = new GuidHeap(data);
				break;
			case "#US":
				image.UserStringHeap = new UserStringHeap(data);
				break;
			case "#Pdb":
				image.PdbHeap = new PdbHeap(data);
				break;
			}
		}

		private byte[] ReadHeapData(uint offset, uint size)
		{
			long position = BaseStream.Position;
			base.MoveTo(offset + image.MetadataSection.PointerToRawData);
			byte[] result = ReadBytes((int)size);
			BaseStream.Position = position;
			return result;
		}

		private void ReadTableHeap()
		{
			TableHeap tableHeap = image.TableHeap;
			base.MoveTo(table_heap_offset + image.MetadataSection.PointerToRawData);
			base.Advance(6);
			byte sizes = ReadByte();
			base.Advance(1);
			tableHeap.Valid = ReadInt64();
			tableHeap.Sorted = ReadInt64();
			if (image.PdbHeap != null)
			{
				for (int i = 0; i < 58; i++)
				{
					if (image.PdbHeap.HasTable((Table)i))
					{
						tableHeap.Tables[i].Length = image.PdbHeap.TypeSystemTableRows[i];
					}
				}
			}
			for (int j = 0; j < 58; j++)
			{
				if (tableHeap.HasTable((Table)j))
				{
					tableHeap.Tables[j].Length = ReadUInt32();
				}
			}
			SetIndexSize(image.StringHeap, sizes, 1);
			SetIndexSize(image.GuidHeap, sizes, 2);
			SetIndexSize(image.BlobHeap, sizes, 4);
			ComputeTableInformations();
		}

		private static void SetIndexSize(Heap heap, uint sizes, byte flag)
		{
			if (heap != null)
			{
				heap.IndexSize = (((sizes & flag) != 0) ? 4 : 2);
			}
		}

		private int GetTableIndexSize(Table table)
		{
			return image.GetTableIndexSize(table);
		}

		private int GetCodedIndexSize(CodedIndex index)
		{
			return image.GetCodedIndexSize(index);
		}

		private void ComputeTableInformations()
		{
			uint num = (uint)((int)BaseStream.Position - (int)table_heap_offset - (int)image.MetadataSection.PointerToRawData);
			int indexSize = image.StringHeap.IndexSize;
			int num2 = (image.GuidHeap != null) ? image.GuidHeap.IndexSize : 2;
			int num3 = (image.BlobHeap != null) ? image.BlobHeap.IndexSize : 2;
			TableHeap tableHeap = image.TableHeap;
			TableInformation[] tables = tableHeap.Tables;
			for (int i = 0; i < 58; i++)
			{
				Table table = (Table)i;
				if (tableHeap.HasTable(table))
				{
					int num4;
					switch (table)
					{
					case Table.Module:
						num4 = 2 + indexSize + num2 * 3;
						break;
					case Table.TypeRef:
						num4 = GetCodedIndexSize(CodedIndex.ResolutionScope) + indexSize * 2;
						break;
					case Table.TypeDef:
						num4 = 4 + indexSize * 2 + GetCodedIndexSize(CodedIndex.TypeDefOrRef) + GetTableIndexSize(Table.Field) + GetTableIndexSize(Table.Method);
						break;
					case Table.FieldPtr:
						num4 = GetTableIndexSize(Table.Field);
						break;
					case Table.Field:
						num4 = 2 + indexSize + num3;
						break;
					case Table.MethodPtr:
						num4 = GetTableIndexSize(Table.Method);
						break;
					case Table.Method:
						num4 = 8 + indexSize + num3 + GetTableIndexSize(Table.Param);
						break;
					case Table.ParamPtr:
						num4 = GetTableIndexSize(Table.Param);
						break;
					case Table.Param:
						num4 = 4 + indexSize;
						break;
					case Table.InterfaceImpl:
						num4 = GetTableIndexSize(Table.TypeDef) + GetCodedIndexSize(CodedIndex.TypeDefOrRef);
						break;
					case Table.MemberRef:
						num4 = GetCodedIndexSize(CodedIndex.MemberRefParent) + indexSize + num3;
						break;
					case Table.Constant:
						num4 = 2 + GetCodedIndexSize(CodedIndex.HasConstant) + num3;
						break;
					case Table.CustomAttribute:
						num4 = GetCodedIndexSize(CodedIndex.HasCustomAttribute) + GetCodedIndexSize(CodedIndex.CustomAttributeType) + num3;
						break;
					case Table.FieldMarshal:
						num4 = GetCodedIndexSize(CodedIndex.HasFieldMarshal) + num3;
						break;
					case Table.DeclSecurity:
						num4 = 2 + GetCodedIndexSize(CodedIndex.HasDeclSecurity) + num3;
						break;
					case Table.ClassLayout:
						num4 = 6 + GetTableIndexSize(Table.TypeDef);
						break;
					case Table.FieldLayout:
						num4 = 4 + GetTableIndexSize(Table.Field);
						break;
					case Table.StandAloneSig:
						num4 = num3;
						break;
					case Table.EventMap:
						num4 = GetTableIndexSize(Table.TypeDef) + GetTableIndexSize(Table.Event);
						break;
					case Table.EventPtr:
						num4 = GetTableIndexSize(Table.Event);
						break;
					case Table.Event:
						num4 = 2 + indexSize + GetCodedIndexSize(CodedIndex.TypeDefOrRef);
						break;
					case Table.PropertyMap:
						num4 = GetTableIndexSize(Table.TypeDef) + GetTableIndexSize(Table.Property);
						break;
					case Table.PropertyPtr:
						num4 = GetTableIndexSize(Table.Property);
						break;
					case Table.Property:
						num4 = 2 + indexSize + num3;
						break;
					case Table.MethodSemantics:
						num4 = 2 + GetTableIndexSize(Table.Method) + GetCodedIndexSize(CodedIndex.HasSemantics);
						break;
					case Table.MethodImpl:
						num4 = GetTableIndexSize(Table.TypeDef) + GetCodedIndexSize(CodedIndex.MethodDefOrRef) + GetCodedIndexSize(CodedIndex.MethodDefOrRef);
						break;
					case Table.ModuleRef:
						num4 = indexSize;
						break;
					case Table.TypeSpec:
						num4 = num3;
						break;
					case Table.ImplMap:
						num4 = 2 + GetCodedIndexSize(CodedIndex.MemberForwarded) + indexSize + GetTableIndexSize(Table.ModuleRef);
						break;
					case Table.FieldRVA:
						num4 = 4 + GetTableIndexSize(Table.Field);
						break;
					case Table.EncLog:
						num4 = 8;
						break;
					case Table.EncMap:
						num4 = 4;
						break;
					case Table.Assembly:
						num4 = 16 + num3 + indexSize * 2;
						break;
					case Table.AssemblyProcessor:
						num4 = 4;
						break;
					case Table.AssemblyOS:
						num4 = 12;
						break;
					case Table.AssemblyRef:
						num4 = 12 + num3 * 2 + indexSize * 2;
						break;
					case Table.AssemblyRefProcessor:
						num4 = 4 + GetTableIndexSize(Table.AssemblyRef);
						break;
					case Table.AssemblyRefOS:
						num4 = 12 + GetTableIndexSize(Table.AssemblyRef);
						break;
					case Table.File:
						num4 = 4 + indexSize + num3;
						break;
					case Table.ExportedType:
						num4 = 8 + indexSize * 2 + GetCodedIndexSize(CodedIndex.Implementation);
						break;
					case Table.ManifestResource:
						num4 = 8 + indexSize + GetCodedIndexSize(CodedIndex.Implementation);
						break;
					case Table.NestedClass:
						num4 = GetTableIndexSize(Table.TypeDef) + GetTableIndexSize(Table.TypeDef);
						break;
					case Table.GenericParam:
						num4 = 4 + GetCodedIndexSize(CodedIndex.TypeOrMethodDef) + indexSize;
						break;
					case Table.MethodSpec:
						num4 = GetCodedIndexSize(CodedIndex.MethodDefOrRef) + num3;
						break;
					case Table.GenericParamConstraint:
						num4 = GetTableIndexSize(Table.GenericParam) + GetCodedIndexSize(CodedIndex.TypeDefOrRef);
						break;
					case Table.Document:
						num4 = num3 + num2 + num3 + num2;
						break;
					case Table.MethodDebugInformation:
						num4 = GetTableIndexSize(Table.Document) + num3;
						break;
					case Table.LocalScope:
						num4 = GetTableIndexSize(Table.Method) + GetTableIndexSize(Table.ImportScope) + GetTableIndexSize(Table.LocalVariable) + GetTableIndexSize(Table.LocalConstant) + 8;
						break;
					case Table.LocalVariable:
						num4 = 4 + indexSize;
						break;
					case Table.LocalConstant:
						num4 = indexSize + num3;
						break;
					case Table.ImportScope:
						num4 = GetTableIndexSize(Table.ImportScope) + num3;
						break;
					case Table.StateMachineMethod:
						num4 = GetTableIndexSize(Table.Method) + GetTableIndexSize(Table.Method);
						break;
					case Table.CustomDebugInformation:
						num4 = GetCodedIndexSize(CodedIndex.HasCustomDebugInformation) + num2 + num3;
						break;
					default:
						throw new NotSupportedException();
					}
					tables[i].RowSize = (uint)num4;
					tables[i].Offset = num;
					num = (uint)((int)num + num4 * (int)tables[i].Length);
				}
			}
		}

		private void ReadPdbHeap()
		{
			PdbHeap pdbHeap = image.PdbHeap;
			ByteBuffer byteBuffer = new ByteBuffer(pdbHeap.data);
			pdbHeap.Id = byteBuffer.ReadBytes(20);
			pdbHeap.EntryPoint = byteBuffer.ReadUInt32();
			pdbHeap.TypeSystemTables = byteBuffer.ReadInt64();
			pdbHeap.TypeSystemTableRows = new uint[58];
			for (int i = 0; i < 58; i++)
			{
				Table table = (Table)i;
				if (pdbHeap.HasTable(table))
				{
					pdbHeap.TypeSystemTableRows[i] = byteBuffer.ReadUInt32();
				}
			}
		}

		public static Image ReadImage(Disposable<Stream> stream, string file_name)
		{
			try
			{
				ImageReader imageReader = new ImageReader(stream, file_name);
				imageReader.ReadImage();
				return imageReader.image;
			}
			catch (EndOfStreamException inner)
			{
				throw new BadImageFormatException(stream.value.GetFileName(), inner);
			}
		}

		public static Image ReadPortablePdb(Disposable<Stream> stream, string file_name)
		{
			try
			{
				ImageReader imageReader = new ImageReader(stream, file_name);
				uint num = (uint)stream.value.Length;
				imageReader.image.Sections = new Section[1]
				{
					new Section
					{
						PointerToRawData = 0,
						SizeOfRawData = num,
						VirtualAddress = 0,
						VirtualSize = num
					}
				};
				imageReader.metadata = new DataDirectory(0u, num);
				imageReader.ReadMetadata();
				return imageReader.image;
			}
			catch (EndOfStreamException inner)
			{
				throw new BadImageFormatException(stream.value.GetFileName(), inner);
			}
		}
	}
}
