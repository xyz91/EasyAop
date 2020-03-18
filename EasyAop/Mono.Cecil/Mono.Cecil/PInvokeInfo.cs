namespace Mono.Cecil
{
	public sealed class PInvokeInfo
	{
		private ushort attributes;

		private string entry_point;

		private ModuleReference module;

		public PInvokeAttributes Attributes
		{
			get
			{
				return (PInvokeAttributes)attributes;
			}
			set
			{
				attributes = (ushort)value;
			}
		}

		public string EntryPoint
		{
			get
			{
				return entry_point;
			}
			set
			{
				entry_point = value;
			}
		}

		public ModuleReference Module
		{
			get
			{
				return module;
			}
			set
			{
				module = value;
			}
		}

		public bool IsNoMangle
		{
			get
			{
				return attributes.GetAttributes(1);
			}
			set
			{
				attributes = attributes.SetAttributes(1, value);
			}
		}

		public bool IsCharSetNotSpec
		{
			get
			{
				return attributes.GetMaskedAttributes(6, 0u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(6, 0u, value);
			}
		}

		public bool IsCharSetAnsi
		{
			get
			{
				return attributes.GetMaskedAttributes(6, 2u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(6, 2u, value);
			}
		}

		public bool IsCharSetUnicode
		{
			get
			{
				return attributes.GetMaskedAttributes(6, 4u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(6, 4u, value);
			}
		}

		public bool IsCharSetAuto
		{
			get
			{
				return attributes.GetMaskedAttributes(6, 6u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(6, 6u, value);
			}
		}

		public bool SupportsLastError
		{
			get
			{
				return attributes.GetAttributes(64);
			}
			set
			{
				attributes = attributes.SetAttributes(64, value);
			}
		}

		public bool IsCallConvWinapi
		{
			get
			{
				return attributes.GetMaskedAttributes(1792, 256u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(1792, 256u, value);
			}
		}

		public bool IsCallConvCdecl
		{
			get
			{
				return attributes.GetMaskedAttributes(1792, 512u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(1792, 512u, value);
			}
		}

		public bool IsCallConvStdCall
		{
			get
			{
				return attributes.GetMaskedAttributes(1792, 768u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(1792, 768u, value);
			}
		}

		public bool IsCallConvThiscall
		{
			get
			{
				return attributes.GetMaskedAttributes(1792, 1024u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(1792, 1024u, value);
			}
		}

		public bool IsCallConvFastcall
		{
			get
			{
				return attributes.GetMaskedAttributes(1792, 1280u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(1792, 1280u, value);
			}
		}

		public bool IsBestFitEnabled
		{
			get
			{
				return attributes.GetMaskedAttributes(48, 16u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(48, 16u, value);
			}
		}

		public bool IsBestFitDisabled
		{
			get
			{
				return attributes.GetMaskedAttributes(48, 32u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(48, 32u, value);
			}
		}

		public bool IsThrowOnUnmappableCharEnabled
		{
			get
			{
				return attributes.GetMaskedAttributes(12288, 4096u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(12288, 4096u, value);
			}
		}

		public bool IsThrowOnUnmappableCharDisabled
		{
			get
			{
				return attributes.GetMaskedAttributes(12288, 8192u);
			}
			set
			{
				attributes = attributes.SetMaskedAttributes(12288, 8192u, value);
			}
		}

		public PInvokeInfo(PInvokeAttributes attributes, string entryPoint, ModuleReference module)
		{
			this.attributes = (ushort)attributes;
			entry_point = entryPoint;
			this.module = module;
		}
	}
}
