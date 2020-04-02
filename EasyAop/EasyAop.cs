using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace EasyAop.Core
{
    public class EasyAop
    {
        /// <summary>
        /// 修改dll中有注解为BaseAopAttribute的方法属性构造函数
        /// </summary>
        /// <param name="dllFilePath">dll路径</param>
        /// <param name="outpath">查找引用dll的目录</param>
        public static void Work(string dllFilePath, string outpath)
        {
            try
            {
                using (AssemblyDefinition ass = AssemblyDefinition.ReadAssembly(dllFilePath))
                {
                    foreach (var type in ass.MainModule.Types)
                    {
                        if (null != outpath)
                        {                                                                  
                            type.Module.AssemblyResolver.AddSearchDirectory(outpath);
                        }                        
                        List<MethodDefinition> methods = new List<MethodDefinition>();
                        List<CustomAttribute> typeatts = type.CustomAttributes.Where(a => CheckAttribute(a.AttributeType)).ToList();
                        typeatts.ForEach(a=>type.CustomAttributes.Remove(a));
                        List<PropertyDefinition> properties = new List<PropertyDefinition>();
                        foreach (var method in type.Methods)
                        {
                            IEnumerable<CustomAttribute> methodatts = method.CustomAttributes.Where(a => CheckAttribute(a.AttributeType));
                            
                            var newatts = new List<CustomAttribute>();
                            if (method.IsConstructor)
                            {
                                var ctor = FilterAttribute(typeatts, AopType.Ctor, false);
                                newatts.AddRange(ctor);                               
                            }
                            else if (method.IsGetter)
                            {
                                var getpro = type.Properties.SingleOrDefault(a => a.Name == method.Name.Substring(4));
                                if (getpro != null)
                                {
                                    var getatts = getpro.CustomAttributes.Where(a => CheckAttribute(a.AttributeType));
                                    newatts.AddRange(FilterAttribute(getatts, AopType.Get, true));
                                    if (!properties.Contains(getpro))
                                    {
                                        properties.Add(getpro);
                                    }
                                }
                                newatts.AddRange(FilterAttribute(typeatts, AopType.Get, false));
                            }
                            else if (method.IsSetter)
                            {
                                var setpro = type.Properties.SingleOrDefault(a => a.Name == method.Name.Substring(4));
                                if (setpro != null)
                                {
                                    var setatts = setpro.CustomAttributes.Where(a => CheckAttribute(a.AttributeType));
                                    newatts.AddRange(FilterAttribute(setatts, AopType.Set, true));
                                    if (!properties.Contains(setpro))
                                    {
                                        properties.Add(setpro);
                                    }
                                }
                                newatts.AddRange(FilterAttribute(typeatts, AopType.Set, false));
                            }
                            else
                            {
                                newatts.AddRange(FilterAttribute(typeatts, AopType.Method, true));
                            }
                            properties.ForEach(a=> {
                                a.CustomAttributes.Where(b => CheckAttribute(b.AttributeType)).ToList().ForEach(c => a.CustomAttributes.Remove(c));
                            });
                            newatts.AddRange(methodatts);
                            methodatts.ToList().ForEach(a => method.CustomAttributes.Remove(a));
                            newatts = newatts.OrderBy(a =>
                            {
                                var op = a.Properties.SingleOrDefault(b => b.Name == "Order");
                                var opv = op.Argument.Value;
                                return Convert.ToInt32(opv);
                            }).ToList();
                            if (newatts.Count > 0)
                            {
                                var m = EditMethod(method, newatts,ass.MainModule);
                                if (m != null)
                                {
                                    methods.Add(m);
                                }
                            }
                        }
                        methods.ForEach(a => type.Methods.Add(a));
                    }
                    ass.Write(dllFilePath + ".temp");
                }
                //File.Copy(dllFilePath, dllFilePath + ".old", true);
                File.Delete(dllFilePath);
                File.Copy(dllFilePath + ".temp", dllFilePath, true);
                File.Delete(dllFilePath + ".temp");
            }
            catch (Exception e) { throw new Exception("result:" + e.Message + e.StackTrace); }
        }
        /// <summary>
        ///  修改dll中有注解为BaseAopAttribute的方法属性构造函数
        /// </summary>
        /// <param name="dllFilePath">dll路径，查找目录为当前目录或bin目录</param>
        public static void Work(string dllFilePath) {
             Work(dllFilePath,null);                               
        }

        private static bool CheckAttribute(TypeReference type)
        {
            if (type.Name == "BaseAopAttribute")
            {
                return true;
            }
            else if (type.Name == "Attribute")
            {
                return false;
            }
            else
            {
                return CheckAttribute(type.Resolve().BaseType);
            }
        }

        /// <summary>
        /// 修改dll里所有符合条件的方法
        /// </summary>
        /// <typeparam name="T">注入的方法类</typeparam>
        /// <param name="dllFilePath">dll路径</param>
        /// <param name="aoptype">修改方法的类型</param>
        public static void Work<T>(string dllFilePath, AopType aoptype)   where T:BaseAopAttribute,new()
        {
            using (AssemblyDefinition ass = AssemblyDefinition.ReadAssembly(dllFilePath)) {
                var typere = ass.MainModule.ImportReference(typeof(T));
                foreach (var type in ass.MainModule.Types) {  
                    IEnumerable <MethodDefinition>  meths  = type.Methods.Where(a=>(((AopType)(a.IsGetter?4:(a.IsSetter?8:(a.IsConstructor?2:1))))|aoptype)==aoptype);
                    List<MethodDefinition> methods = new List<MethodDefinition>();
                    foreach (var meth in type.Methods.Where(a => (((AopType)(a.IsGetter ? 4 : (a.IsSetter ? 8 : (a.IsConstructor ? 2 : 1)))) | aoptype) == aoptype))
                    {
                       var newmeth = EditMethod(meth, new List<TypeReference> { typere }, ass.MainModule);
                        if (newmeth !=null)
                        {
                            methods.Add(newmeth);
                        }
                    }
                    methods.ForEach(m=>type.Methods.Add(m));
                }
                ass.Write(dllFilePath + ".temp");
            }
            File.Delete(dllFilePath);
            File.Copy(dllFilePath + ".temp", dllFilePath, true);
            File.Delete(dllFilePath + ".temp");
        }
        private static IEnumerable<CustomAttribute> FilterAttribute(IEnumerable<CustomAttribute> attributes, AopType type, bool isDefault)
        {
            return attributes.Where(a =>
            {
                var op = a.Properties.SingleOrDefault(b => b.Name == "Type");
                if (null == op.Name )
                {
                    return isDefault || false;
                }
                var opv = op.Argument.Value;
                int i = Convert.ToInt32(opv);
                AopType aop = ((AopType)i) & type;
                return aop == type;
            });
        }

        private static MethodDefinition EditMethod(MethodDefinition method,MethodReference typector ,MethodReference reference) {
            ILProcessor il = method.Body.GetILProcessor();         
            MethodDefinition newmethod = method.Clone();
            VariableDefinition returnvalue = null;

            var returnvoid = method.ReturnType.FullName == "System.Void";
            if (!returnvoid)
            {
                returnvalue = new VariableDefinition(method.Module.ImportReference(method.ReturnType));
                method.Body.Variables.Add(returnvalue);
            }

            method.Body.Instructions.Clear();
            if (!method.IsStatic)
            {
                il.Append(il.Create(OpCodes.Ldarg_0));
            }
            foreach (var p in method.Parameters)
            {
                il.Append(il.Create(OpCodes.Ldarg_S, p));
            }

            var methbase = new VariableDefinition(reference.DeclaringType);
            method.Body.Variables.Add(methbase);
            il.Append(il.Create(OpCodes.Newobj, method.Module.ImportReference(typector)));
            il.Append(il.Create(OpCodes.Stloc_S, methbase));
            il.Append(il.Create(OpCodes.Ldloc_S, methbase));
            il.Append(il.Create(OpCodes.Call, reference));

            il.Append(il.Create(OpCodes.Call, newmethod));

            if (!returnvoid)
            {
                il.Append(il.Create(OpCodes.Stloc_S, returnvalue));
                il.Append(il.Create(OpCodes.Ldloc_S, returnvalue));

            }
            il.Append(il.Create(OpCodes.Ret));
            return newmethod;
        }
        private static MethodDefinition EditMethod(MethodDefinition method, List<CustomAttribute> atts,ModuleDefinition main)
        {
            if (null == method  ||null == atts  || atts.Count == 0)
            {
                return null;
            }                                                                             
            atts = atts.Distinct().ToList();
            atts.ForEach(a => method.CustomAttributes.Remove(a));
            return EditMethod(method, atts.Select(a => a.AttributeType).ToList(),main);
        }

        private static MethodDefinition EditMethod(MethodDefinition method,IList<TypeReference> atts, ModuleDefinition mainmodule) {
            if (null == method || null == atts || atts.Count() == 0)
            {
                return null;
            }
            ILProcessor il = method.Body.GetILProcessor();
            MethodDefinition newmethod = method.Clone();
            method.Body.Instructions.Clear();
            if (!method.IsConstructor)
            {
                method.Body.Variables.Clear();
                method.Body.ExceptionHandlers.Clear();
            }
            var methbase = new VariableDefinition(method.Module.ImportReference(typeof(ExceEventArg)));
            method.Body.Variables.Add(methbase);

            var exception = new VariableDefinition(method.Module.ImportReference(typeof(System.Exception)));
            method.Body.Variables.Add(exception);

            var ps = new VariableDefinition(method.Module.ImportReference(typeof(List<object>)));
            method.Body.Variables.Add(ps);

            VariableDefinition returnvalue = null;

            var returnvoid = method.ReturnType.FullName == "System.Void";
            if (!returnvoid)
            {
                returnvalue = new VariableDefinition(method.Module.ImportReference(method.ReturnType));
                method.Body.Variables.Add(returnvalue);
            }

            var curmethod = method.Module.ImportReference(typeof(MethodBase).GetMethod("GetCurrentMethod"));

            il.AppendArr(new[] {
                                il.Create(OpCodes.Newobj, method.Module.ImportReference(typeof(ExceEventArg).GetConstructor(new Type[] { }))),
                                il.Create(OpCodes.Stloc_S,methbase),
                                il.Create(OpCodes.Ldloc_S,methbase),
                                il.Create(OpCodes.Call, curmethod),
                                il.Create(OpCodes.Callvirt,method.Module.CreateMethod<ExceEventArg>("set_MethodInfo",typeof(MethodBase))),
                        });

            if (method.Parameters.Count > 0)
            {
                il.AppendArr(new[] {
                                il.Create(OpCodes.Newobj,method.Module.ImportReference(typeof(List<object>).GetConstructor(new Type[]{ }))),
                                il.Create(OpCodes.Stloc_S,ps),
                            });
                foreach (var p in method.Parameters)
                {
                    il.Append(il.Create(OpCodes.Ldloc_S, ps));
                    il.Append(il.Create(OpCodes.Ldarg_S, p));
                    il.Append(il.Create(OpCodes.Box, method.Module.ImportReference(p.ParameterType)));
                    il.Append(il.Create(OpCodes.Call, method.Module.CreateMethod<List<object>>("Add", typeof(object))));
                }

                il.AppendArr(new[] {
                                il.Create(OpCodes.Ldloc_S, methbase),
                                il.Create(OpCodes.Ldloc_S, ps),
                                il.Create(OpCodes.Callvirt, method.Module.CreateMethod<ExceEventArg>("set_Parameters", typeof(List<object>))),
                        });
            }

            List<TypeDefinition> typeDefinitions = new List<TypeDefinition>();
            List<VariableDefinition> variables = new List<VariableDefinition>();
            Dictionary<TypeReference, Dictionary<string, Instruction>> excehandler = new Dictionary<TypeReference, Dictionary<string, Instruction>>();
            for (int i = 0; i < atts.Count(); i++)
            {
                var excedic = new Dictionary<string, Instruction>
                {
                    { "TryStart", il.Create(OpCodes.Nop) },
                    { "TryEnd", il.Create(OpCodes.Stloc_S, exception) },
                    { "HandlerStart", il.Create(OpCodes.Nop) },
                    { "HandlerEnd", il.Create(OpCodes.Nop) }
                };
                excehandler.Add(atts[i], excedic);
                var w = new ExceptionHandler(ExceptionHandlerType.Catch)
                {
                    CatchType = method.Module.ImportReference(typeof(Exception)),
                    TryStart = excehandler[atts[i]]["TryStart"],
                    TryEnd = excehandler[atts[i]]["TryEnd"],
                    HandlerStart = excehandler[atts[i]]["TryEnd"],
                    HandlerEnd = excehandler[atts[i]]["HandlerEnd"]
                };
                method.Body.ExceptionHandlers.Add(w);
                var re = atts[i].Resolve();
                typeDefinitions.Add(re);
                var begin =mainmodule.ImportReference(SearchMethod(re, "Before"));

                VariableDefinition log = new VariableDefinition(mainmodule.ImportReference(atts[i]));
                method.Body.Variables.Add(log);
                variables.Add(log);
                il.Append( il.Create(OpCodes.Newobj,mainmodule.ImportReference(SearchMethod(re, ".ctor"))));

               il.Append( il.Create(OpCodes.Stloc_S, log));
                if (begin != null)
                {
                    il.AppendArr(new[] {
                                il.Create(OpCodes.Ldloc_S,log),
                                il.Create(OpCodes.Ldloc_S,methbase),
                                il.Create(OpCodes.Call,begin),
                        });
                }
            }
            for (int i = atts.Count - 1; i >= 0; i--)
            {
                il.Append(excehandler[atts[i]]["TryStart"]);
            }
            if (method.IsConstructor)
            {
                newmethod.Body.Instructions.RemoveAt(newmethod.Body.Instructions.Count - 1);
                il.AppendArr(newmethod.Body.Instructions.ToArray());
            }
            else
            {
                if (!method.IsStatic)
                {
                    il.Append(il.Create(OpCodes.Ldarg_0));
                }
                foreach (var p in method.Parameters)
                {
                    il.Append(il.Create(OpCodes.Ldarg_S, p));
                }
                il.Append(il.Create(OpCodes.Call, newmethod));
            }
            if (!returnvoid)
            {
                il.AppendArr(new[] {
                                il.Create(OpCodes.Stloc_S, returnvalue),
                            il.Create(OpCodes.Ldloc_S, methbase),
                            il.Create(OpCodes.Ldloc_S, returnvalue),
                            il.Create(OpCodes.Box,method.Module.ImportReference(method.ReturnType)),
                            il.Create(OpCodes.Callvirt,method.Module.CreateMethod<ExceEventArg>("set_ReturnValue",method.ReturnType.GetType())),
                            });
            }
            for (int i = 0; i < atts.Count(); i++)
            {
                var exce = SearchMethod(typeDefinitions[i], "Exception");
                il.AppendArr(new[] {

                             il.Create(OpCodes.Leave_S,excehandler[atts[i]]["HandlerEnd"]),
                                     
                            excehandler[atts[i]]["TryEnd"],
                            //il.Create(OpCodes.Nop),

                               il.Create(OpCodes.Ldloc_S,methbase),
                            il.Create(OpCodes.Ldloc_S,exception),

                            il.Create(OpCodes.Callvirt,method.Module.CreateMethod<ExceEventArg>("set_Error",typeof(Exception))),

                            il.Create(OpCodes.Ldloc_S,variables[i]),
                                il.Create(OpCodes.Ldloc_S,methbase),
                                il.Create(OpCodes.Call,mainmodule.ImportReference( exce)),

                            il.Create(OpCodes.Nop),
                            il.Create(OpCodes.Leave_S,excehandler[atts[i]]["HandlerEnd"]),
                            excehandler[atts[i]]["HandlerEnd"],
                        });

                var after = SearchMethod(typeDefinitions[i], "After");

                if (after != null)
                {
                    il.AppendArr(new[] {
                            il.Create(OpCodes.Ldloc_S,variables[i]),
                                il.Create(OpCodes.Ldloc_S,methbase),
                                il.Create(OpCodes.Call,mainmodule.ImportReference( after)),
                        });
                }
            }
            if (!returnvoid)
            {
                il.Append(il.Create(OpCodes.Ldloc_S, returnvalue));
            }
            il.Append(il.Create(OpCodes.Ret));
            if (method.IsConstructor)
            {
                return null;
            }
            return newmethod;
        }
        private static MethodDefinition SearchMethod(TypeDefinition definition, string name)
        {

            var method = definition.Methods.FirstOrDefault(a => a.Name == name);
            if (null == method)
            {
                return SearchMethod(definition.BaseType.Resolve(), name);
            }
            return method;
        }
    }
    
    
   
  
    

}
