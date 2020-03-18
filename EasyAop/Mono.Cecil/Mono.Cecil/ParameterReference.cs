using System;

namespace Mono.Cecil
{
	public abstract class ParameterReference : IMetadataTokenProvider
	{
		private string name;

		internal int index = -1;

		protected TypeReference parameter_type;

		internal MetadataToken token;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public int Index => index;

		public TypeReference ParameterType
		{
			get
			{
				return parameter_type;
			}
			set
			{
				parameter_type = value;
			}
		}

		public MetadataToken MetadataToken
		{
			get
			{
				return token;
			}
			set
			{
				token = value;
			}
		}

		internal ParameterReference(string name, TypeReference parameterType)
		{
			if (parameterType == null)
			{
				throw new ArgumentNullException("parameterType");
			}
			this.name = (name ?? string.Empty);
			parameter_type = parameterType;
		}

		public override string ToString()
		{
			return name;
		}

		public abstract ParameterDefinition Resolve();
	}
}
