using Mono.Cecil.PE;
using System;

namespace Mono.Cecil.Cil
{
	public sealed class PortablePdbReader : ISymbolReader, IDisposable
	{
		private readonly Image image;

		private readonly ModuleDefinition module;

		private readonly MetadataReader reader;

		private readonly MetadataReader debug_reader;

		private bool IsEmbedded => reader.image == debug_reader.image;

		internal PortablePdbReader(Image image, ModuleDefinition module)
		{
			this.image = image;
			this.module = module;
			reader = module.reader;
			debug_reader = new MetadataReader(image, module, reader);
		}

		public ISymbolWriterProvider GetWriterProvider()
		{
			return new PortablePdbWriterProvider();
		}

		public bool ProcessDebugHeader(ImageDebugHeader header)
		{
			if (image == module.Image)
			{
				return true;
			}
			ImageDebugHeaderEntry codeViewEntry = header.GetCodeViewEntry();
			if (codeViewEntry == null)
			{
				return false;
			}
			byte[] data = codeViewEntry.Data;
			if (data.Length < 24)
			{
				return false;
			}
			if (ReadInt32(data, 0) != 1396986706)
			{
				return false;
			}
			byte[] array = new byte[16];
			Buffer.BlockCopy(data, 4, array, 0, 16);
			Guid a = new Guid(array);
			Buffer.BlockCopy(image.PdbHeap.Id, 0, array, 0, 16);
			Guid b = new Guid(array);
			if (a != b)
			{
				return false;
			}
			ReadModule();
			return true;
		}

		private static int ReadInt32(byte[] bytes, int start)
		{
			return bytes[start] | bytes[start + 1] << 8 | bytes[start + 2] << 16 | bytes[start + 3] << 24;
		}

		private void ReadModule()
		{
			module.custom_infos = debug_reader.GetCustomDebugInformation(module);
		}

		public MethodDebugInformation Read(MethodDefinition method)
		{
			MethodDebugInformation methodDebugInformation = new MethodDebugInformation(method);
			ReadSequencePoints(methodDebugInformation);
			ReadScope(methodDebugInformation);
			ReadStateMachineKickOffMethod(methodDebugInformation);
			ReadCustomDebugInformations(methodDebugInformation);
			return methodDebugInformation;
		}

		private void ReadSequencePoints(MethodDebugInformation method_info)
		{
			method_info.sequence_points = debug_reader.ReadSequencePoints(method_info.method);
		}

		private void ReadScope(MethodDebugInformation method_info)
		{
			method_info.scope = debug_reader.ReadScope(method_info.method);
		}

		private void ReadStateMachineKickOffMethod(MethodDebugInformation method_info)
		{
			method_info.kickoff_method = debug_reader.ReadStateMachineKickoffMethod(method_info.method);
		}

		private void ReadCustomDebugInformations(MethodDebugInformation info)
		{
			info.method.custom_infos = debug_reader.GetCustomDebugInformation(info.method);
		}

		public void Dispose()
		{
			if (!IsEmbedded)
			{
				image.Dispose();
			}
		}
	}
}
