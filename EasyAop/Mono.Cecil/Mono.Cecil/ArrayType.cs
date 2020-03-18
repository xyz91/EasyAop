using Mono.Cecil.Metadata;
using Mono.Collections.Generic;
using System;
using System.Text;

namespace Mono.Cecil
{
	public sealed class ArrayType : TypeSpecification
	{
		private Collection<ArrayDimension> dimensions;

		public Collection<ArrayDimension> Dimensions
		{
			get
			{
				if (dimensions != null)
				{
					return dimensions;
				}
				dimensions = new Collection<ArrayDimension>();
				dimensions.Add(default(ArrayDimension));
				return dimensions;
			}
		}

		public int Rank
		{
			get
			{
				if (dimensions != null)
				{
					return dimensions.Count;
				}
				return 1;
			}
		}

		public bool IsVector
		{
			get
			{
				if (dimensions == null)
				{
					return true;
				}
				if (dimensions.Count > 1)
				{
					return false;
				}
				return !dimensions[0].IsSized;
			}
		}

		public override bool IsValueType
		{
			get
			{
				return false;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override string Name => base.Name + Suffix;

		public override string FullName => base.FullName + Suffix;

		private string Suffix
		{
			get
			{
				if (IsVector)
				{
					return "[]";
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("[");
				for (int i = 0; i < dimensions.Count; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(",");
					}
					stringBuilder.Append(dimensions[i].ToString());
				}
				stringBuilder.Append("]");
				return stringBuilder.ToString();
			}
		}

		public override bool IsArray => true;

		public ArrayType(TypeReference type)
			: base(type)
		{
			Mixin.CheckType(type);
			base.etype = Mono.Cecil.Metadata.ElementType.Array;
		}

		public ArrayType(TypeReference type, int rank)
			: this(type)
		{
			Mixin.CheckType(type);
			if (rank != 1)
			{
				dimensions = new Collection<ArrayDimension>(rank);
				for (int i = 0; i < rank; i++)
				{
					dimensions.Add(default(ArrayDimension));
				}
				base.etype = Mono.Cecil.Metadata.ElementType.Array;
			}
		}
	}
}
