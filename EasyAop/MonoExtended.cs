using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace EasyAop.Core
{
    static class MonoExtended
    {
        public static void AppendArr(this ILProcessor il, Instruction[] ins)
        {
            foreach (var item in ins)
            {
                il.Append(item);
            }
        }
        public static MethodReference CreateMethod<T>(this ModuleDefinition module, string methodName, params Type[] types)
        {
            if (null == types)
            {
                types = new Type[] { };
            }
            return module.ImportReference(typeof(T).GetMethod(methodName, types));
        }
        public static MethodDefinition Clone(this MethodDefinition method)
        {
            var newmethod = new MethodDefinition(method.Name + Guid.NewGuid().ToString("N"), method.Attributes, method.ReturnType);
            method.Parameters.ToList().ForEach(a => { newmethod.Parameters.Add(a); });
            method.GenericParameters.ToList().ForEach(a => { newmethod.GenericParameters.Add(a); });
            //method.CustomAttributes.ToList().ForEach(a => { newmethod.CustomAttributes.Add(a); });
            method.Body.Instructions.ToList().ForEach(a => { newmethod.Body.Instructions.Add(a); });
            method.Body.Variables.ToList().ForEach(a => { newmethod.Body.Variables.Add(a); });
            method.Body.ExceptionHandlers.ToList().ForEach(a => { newmethod.Body.ExceptionHandlers.Add(a); });
            newmethod.Body.InitLocals = method.Body.InitLocals;
            newmethod.Body.LocalVarToken = method.Body.LocalVarToken;
            newmethod.IsPrivate = true;
            newmethod.IsStatic = method.IsStatic;
            return newmethod;
        }
    }
}
