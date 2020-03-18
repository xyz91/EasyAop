using Mono.Collections.Generic;

namespace Mono.Cecil
{
	public sealed class EventDefinition : EventReference, IMemberDefinition, ICustomAttributeProvider, IMetadataTokenProvider
	{
		private ushort attributes;

		private Collection<CustomAttribute> custom_attributes;

		internal MethodDefinition add_method;

		internal MethodDefinition invoke_method;

		internal MethodDefinition remove_method;

		internal Collection<MethodDefinition> other_methods;

		public EventAttributes Attributes
		{
			get
			{
				return (EventAttributes)attributes;
			}
			set
			{
				attributes = (ushort)value;
			}
		}

		public MethodDefinition AddMethod
		{
			get
			{
				if (add_method != null)
				{
					return add_method;
				}
				InitializeMethods();
				return add_method;
			}
			set
			{
				add_method = value;
			}
		}

		public MethodDefinition InvokeMethod
		{
			get
			{
				if (invoke_method != null)
				{
					return invoke_method;
				}
				InitializeMethods();
				return invoke_method;
			}
			set
			{
				invoke_method = value;
			}
		}

		public MethodDefinition RemoveMethod
		{
			get
			{
				if (remove_method != null)
				{
					return remove_method;
				}
				InitializeMethods();
				return remove_method;
			}
			set
			{
				remove_method = value;
			}
		}

		public bool HasOtherMethods
		{
			get
			{
				if (other_methods != null)
				{
					return other_methods.Count > 0;
				}
				InitializeMethods();
				return !other_methods.IsNullOrEmpty();
			}
		}

		public Collection<MethodDefinition> OtherMethods
		{
			get
			{
				if (other_methods != null)
				{
					return other_methods;
				}
				InitializeMethods();
				if (other_methods != null)
				{
					return other_methods;
				}
				return other_methods = new Collection<MethodDefinition>();
			}
		}

		public bool HasCustomAttributes
		{
			get
			{
				if (custom_attributes != null)
				{
					return custom_attributes.Count > 0;
				}
				return this.GetHasCustomAttributes(Module);
			}
		}

		public Collection<CustomAttribute> CustomAttributes => custom_attributes ?? this.GetCustomAttributes(ref custom_attributes, Module);

		public bool IsSpecialName
		{
			get
			{
				return attributes.GetAttributes(512);
			}
			set
			{
				attributes = attributes.SetAttributes(512, value);
			}
		}

		public bool IsRuntimeSpecialName
		{
			get
			{
				return attributes.GetAttributes(1024);
			}
			set
			{
				attributes = attributes.SetAttributes(1024, value);
			}
		}

		public new TypeDefinition DeclaringType
		{
			get
			{
				return (TypeDefinition)base.DeclaringType;
			}
			set
			{
				base.DeclaringType = value;
			}
		}

		public override bool IsDefinition => true;

		public EventDefinition(string name, EventAttributes attributes, TypeReference eventType)
			: base(name, eventType)
		{
			this.attributes = (ushort)attributes;
			base.token = new MetadataToken(TokenType.Event);
		}

		private void InitializeMethods()
		{
			ModuleDefinition module = Module;
			if (module != null)
			{
				lock (module.SyncRoot)
				{
					if (add_method == null && invoke_method == null && remove_method == null && module.HasImage())
					{
						module.Read(this, delegate(EventDefinition @event, MetadataReader reader)
						{
							reader.ReadMethods(@event);
						});
					}
				}
			}
		}

		public override EventDefinition Resolve()
		{
			return this;
		}
	}
}
