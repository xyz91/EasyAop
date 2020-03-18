using Mono.Collections.Generic;
using System;
using System.Threading;

namespace Mono.Cecil.Cil
{
	public sealed class MethodBody
	{
		internal readonly MethodDefinition method;

		internal ParameterDefinition this_parameter;

		internal int max_stack_size;

		internal int code_size;

		internal bool init_locals;

		internal MetadataToken local_var_token;

		internal Collection<Instruction> instructions;

		internal Collection<ExceptionHandler> exceptions;

		internal Collection<VariableDefinition> variables;

		public MethodDefinition Method => method;

		public int MaxStackSize
		{
			get
			{
				return max_stack_size;
			}
			set
			{
				max_stack_size = value;
			}
		}

		public int CodeSize => code_size;

		public bool InitLocals
		{
			get
			{
				return init_locals;
			}
			set
			{
				init_locals = value;
			}
		}

		public MetadataToken LocalVarToken
		{
			get
			{
				return local_var_token;
			}
			set
			{
				local_var_token = value;
			}
		}

		public Collection<Instruction> Instructions => instructions ?? (instructions = new InstructionCollection(method));

		public bool HasExceptionHandlers => !exceptions.IsNullOrEmpty();

		public Collection<ExceptionHandler> ExceptionHandlers => exceptions ?? (exceptions = new Collection<ExceptionHandler>());

		public bool HasVariables => !variables.IsNullOrEmpty();

		public Collection<VariableDefinition> Variables => variables ?? (variables = new VariableDefinitionCollection());

		public ParameterDefinition ThisParameter
		{
			get
			{
				if (method != null && method.DeclaringType != null)
				{
					if (!method.HasThis)
					{
						return null;
					}
					if (this_parameter == null)
					{
						Interlocked.CompareExchange(ref this_parameter, CreateThisParameter(method), null);
					}
					return this_parameter;
				}
				throw new NotSupportedException();
			}
		}

		private static ParameterDefinition CreateThisParameter(MethodDefinition method)
		{
			TypeReference typeReference = method.DeclaringType;
			if (typeReference.HasGenericParameters)
			{
				GenericInstanceType genericInstanceType = new GenericInstanceType(typeReference);
				for (int i = 0; i < typeReference.GenericParameters.Count; i++)
				{
					genericInstanceType.GenericArguments.Add(typeReference.GenericParameters[i]);
				}
				typeReference = genericInstanceType;
			}
			if (typeReference.IsValueType || typeReference.IsPrimitive)
			{
				typeReference = new ByReferenceType(typeReference);
			}
			return new ParameterDefinition(typeReference, method);
		}

		public MethodBody(MethodDefinition method)
		{
			this.method = method;
		}

		public ILProcessor GetILProcessor()
		{
			return new ILProcessor(this);
		}
	}
}
