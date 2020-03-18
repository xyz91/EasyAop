using Mono.Collections.Generic;
using System;

namespace Mono.Cecil
{
	internal struct ImportGenericContext
	{
		private Collection<IGenericParameterProvider> stack;

		public bool IsEmpty => stack == null;

		public ImportGenericContext(IGenericParameterProvider provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}
			stack = null;
			Push(provider);
		}

		public void Push(IGenericParameterProvider provider)
		{
			if (stack == null)
			{
				stack = new Collection<IGenericParameterProvider>(1)
				{
					provider
				};
			}
			else
			{
				stack.Add(provider);
			}
		}

		public void Pop()
		{
			stack.RemoveAt(stack.Count - 1);
		}

		public TypeReference MethodParameter(string method, int position)
		{
			for (int num = stack.Count - 1; num >= 0; num--)
			{
				MethodReference methodReference = stack[num] as MethodReference;
				if (methodReference != null && !(method != NormalizeMethodName(methodReference)))
				{
					return methodReference.GenericParameters[position];
				}
			}
			throw new InvalidOperationException();
		}

		public string NormalizeMethodName(MethodReference method)
		{
			return method.DeclaringType.GetElementType().FullName + "." + method.Name;
		}

		public TypeReference TypeParameter(string type, int position)
		{
			for (int num = stack.Count - 1; num >= 0; num--)
			{
				TypeReference typeReference = GenericTypeFor(stack[num]);
				if (!(typeReference.FullName != type))
				{
					return typeReference.GenericParameters[position];
				}
			}
			throw new InvalidOperationException();
		}

		private static TypeReference GenericTypeFor(IGenericParameterProvider context)
		{
			TypeReference typeReference = context as TypeReference;
			if (typeReference != null)
			{
				return typeReference.GetElementType();
			}
			MethodReference methodReference = context as MethodReference;
			if (methodReference != null)
			{
				return methodReference.DeclaringType.GetElementType();
			}
			throw new InvalidOperationException();
		}

		public static ImportGenericContext For(IGenericParameterProvider context)
		{
			if (context == null)
			{
				return default(ImportGenericContext);
			}
			return new ImportGenericContext(context);
		}
	}
}
