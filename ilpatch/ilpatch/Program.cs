using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace ilpatch
{
    class Program
    {
#if false
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("ilpatch file_for_patch file_to_patch");
                return;
            }

            var src = args[0];
            var dest = args[1];
            //var className = args[2];

            try
            {
                var result = Helper.CopyClass(src, dest);
                //var result = Helper.ReplaceClass(src, dest, className);
                var bak = dest + ".bak";

                if (!File.Exists(bak))
                    File.Move(dest, dest + ".bak");
                File.Delete(dest);
                File.Move(result, dest);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
#else
        static void Main(string[] args)
        {
            //Assembly-CSharp_G.dll Assembly-CSharp.dll SteamVR_ExternalCamera
            if (args.Length != 3)
            {
                Console.WriteLine("ilpatch file_for_patch file_to_patch className");
                return;
            }

            var src = args[0];
            var dest = args[1];
            var className = args[2];

            try
            {
                //var result = Helper.CopyClass(src, dest);
                var result = Helper.ReplaceClass(src, dest, className);
                var bak = dest + ".bak";

                if (!File.Exists(bak))
                    File.Move(dest, dest + ".bak");
                File.Delete(dest);
                File.Move(result, dest);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
#endif

    }

    class MyResolver : IResolver
    {
        public IMemberForwarded Resolve(MemberRef memberRef)
        {
            return memberRef.Resolve();
        }

        public TypeDef Resolve(TypeRef typeRef, ModuleDef sourceModule)
        {
            return typeRef.Resolve(sourceModule);
        }
    }

    class MyAppResolver : IAssemblyResolver
    {
        public bool AddToCache(AssemblyDef asm)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Remove(AssemblyDef asm)
        {
            throw new NotImplementedException();
        }

        public AssemblyDef Resolve(IAssembly assembly, ModuleDef sourceModule)
        {
            throw new NotImplementedException();
        }
    }

    static class Helper
    {
        public static string ReplaceClass(string src, string dest, string className)
        {
            var destAssembly = AssemblyDef.Load(dest);
            var srcAssembly = AssemblyDef.Load(src);

            var destMod = ModuleDefMD.Load(dest);
            var srcMod = ModuleDefMD.Load(src);


            //DupRid(destAssembly, srcAssembly);

            var query = from mod in srcAssembly.Modules
                        from type in mod.Types
                        where type.Name == className
                        select new { mod, type };

            var srcTarget = query.FirstOrDefault();

            if (srcTarget == null)
                throw new Exception("srcTarget");

            query = from mod in destAssembly.Modules
                    from type in mod.Types
                    where type.Name == className
                    select new { mod, type };

            var destTarget = query.FirstOrDefault();

            if (destTarget == null)
                throw new Exception("destTarget");

            //var index = destTarget.mod.Types.IndexOf(destTarget.type);
            //srcTarget.mod.Types.Remove(srcTarget.type);
            //destTarget.mod.Types[index] = srcTarget.type;
            //destTarget.mod.Types.Remove(destTarget.type);



            ReReference(srcTarget.type, destAssembly, srcAssembly);
            DupType(srcTarget.type, destTarget.type);

            var result = "patch." + dest;
            destAssembly.Write(result);

            foreach (var mod in destAssembly.Modules)
            {
                mod.Dispose();
            }

            return result;
        }

        private static TypeSig ReferenceType(TypeSig type, AssemblyDef assembly)
        {
            if (type == null)
                return null;

            if (type.IsSZArray)
            {
                var szar = type.ToSZArraySig();
                var eleType = ReferenceType(szar.Next, assembly);
                if (eleType == null)
                    return null;
                return new SZArraySig(eleType);
            }
            
            if (type.IsArray)
            {
                var ar = type.ToArraySig();
                var eleType = ReferenceType(ar.Next, assembly);
                if (eleType == null)
                    return null;
                return new ArraySig(eleType, ar.Rank, ar.Sizes, ar.LowerBounds);
            }

            if (type.IsGenericInstanceType)
            {
                var g = type.ToGenericInstSig();

                var gtype = assembly.Find(g.GenericType.FullName, false);
                ClassOrValueTypeSig ngt;
                if (gtype == null)
                    ngt = g.GenericType;
                else  
                    ngt = gtype.TryGetClassOrValueTypeSig();

                TypeSig[] genericArgs = new TypeSig[g.GenericArguments.Count];
                for (int i = 0; i < g.GenericArguments.Count; ++i)
                {
                    genericArgs[i] = ReferenceType(g.GenericArguments[i], assembly);
                }

                return new GenericInstSig(ngt, genericArgs);
            }

            var targetType = assembly.Find(type.FullName, false);
            if (targetType == null)
                return null;

            return targetType.ToTypeSig();
        }

        private static void ReCustomAttributes(IHasCustomAttribute type, AssemblyDef assembly)
        {

            for (int i = 0; i < type.CustomAttributes.Count; ++i)
            {
                var newattr = assembly.Find(type.CustomAttributes[i].TypeFullName, false);
                if (newattr == null)
                    continue;

                type.CustomAttributes[i] = new CustomAttribute(newattr.FindDefaultConstructor());
            }
        }
        
        private static void ReMethodSig(MethodSig sig, AssemblyDef assembly)
        {
            var retType = sig.RetType;
            var ts = ReferenceType(sig.RetType, assembly);
            if (ts != null)
                sig.RetType = ts;

            TypeSig[] newparams = new TypeSig[sig.Params.Count];
            for (var i = 0; i < sig.Params.Count; ++i)
            {
                ts = ReferenceType(sig.Params[i], assembly);
                if (ts == null)
                    continue;

                sig.Params[i] = ts;
            }
        }

        private static void ReMethodDef(MethodDef method, AssemblyDef assembly)
        {
            var newtype = ReferenceType(method.ReturnType, assembly);
            if (newtype != null)
                method.ReturnType = newtype;

            for (int i = 0; i < method.Parameters.Count; ++i)
            {
                var newparam = ReferenceType(method.Parameters[i].Type, assembly);
                if (newtype == null)
                    continue;

                method.Parameters[i].Type = newparam;
            }

            for (int i = 0; i < method.GenericParameters.Count; ++i)
            {
                var genep = method.GenericParameters[i];
                method.GenericParameters[i] = new GenericParamUser(genep.Number, genep.Flags, genep.Name);
            }

            ReMethodSig(method.MethodSig, assembly);
        }

        //private static MethodDef ReMethod(string typeName, string methodName, AssemblyDef assembly)
        //{
        //    var type = assembly.Find(typeName, false);
        //    return type.FindMethod(methodName);
        //}

        //private static EventDef ReEvent(string typeName, string eventName, AssemblyDef assembly)
        //{
        //    var type = assembly.Find(typeName, false);
        //    return type.FindEvent(eventName);
        //}

        private static void ReMethodBody(TypeDef type, MethodDef method, AssemblyDef assembly)
        {
            for (int i = 0; i < method.Body.Instructions.Count; ++i)
            {
                if (method.Body.Instructions[i].Operand == null)
                    continue;

                TypeSig ts;
                switch (method.Body.Instructions[i].OpCode.OperandType)
                {
                    case OperandType.InlineField:
                        var field = method.Body.Instructions[i].Operand as FieldDef;
                        ts = field.FieldType;
                        ts = ReferenceType(ts, assembly);
                        if (ts != null)
                            method.Body.Instructions[i].Operand = new FieldDefUser(field.Name, new FieldSig(ts));
                        break;
                    case OperandType.InlineMethod:
                        var m = method.Body.Instructions[i].Operand as MemberRef;
                        if (m != null)
                        {
                            var module = type.Module;
                            if (m.DeclaringType != null)
                            {
                                ts = ReferenceType(m.DeclaringType.ToTypeSig(), assembly);
                                if (ts != null)
                                    module = ts.Module;
                            }

                            if (m.IsFieldRef)
                            {
                                throw new Exception("????" + method.Body.Instructions[i].OpCode.OperandType.ToString());
                                //method.Body.Instructions[i].Operand = new MemberRefUser(ts.Module, m.Name, new FieldSig(ts));
                            }
                            else if (m.IsMethodRef)
                            {
                                var retType = m.MethodSig.RetType;
                                ts = ReferenceType(m.MethodSig.RetType, assembly);
                                if (ts != null)
                                    retType = ts;

                                TypeSig[] newparams = new TypeSig[m.MethodSig.Params.Count];
                                for (var j = 0; j < m.MethodSig.Params.Count; ++j)
                                {
                                    ts = ReferenceType(m.MethodSig.Params[j], assembly);
                                    if (ts == null)
                                        newparams[j] = m.MethodSig.Params[j];
                                    else
                                        newparams[j] = ts;
                                }

                                var msig = new MethodSig(
                                    m.MethodSig.CallingConvention,
                                    m.MethodSig.GenParamCount,
                                    ts,
                                    newparams
                                    );
                                method.Body.Instructions[i].Operand = new MemberRefUser(module, m.Name, msig);
                            }

                            break;
                        }

                        var md = method.Body.Instructions[i].Operand as MethodDef;
                        if (md != null)
                        {
                            ts = ReferenceType(md.DeclaringType.ToTypeSig(), assembly);
                            if (ts != null)
                                md.DeclaringType = ts.TryGetTypeDef();

                            ReMethodDef(md, assembly);
                            break;
                        }

                        var ms = method.Body.Instructions[i].Operand as MethodSpec;
                        if (ms != null)
                        {

                            var mru = new MemberRefUser(type.Module, ms.Method.Name, ms.Method.MethodSig);
                            for (int j = 0; j < ms.GenericInstMethodSig.GenericArguments.Count; ++j)
                            {
                                var ga = ReferenceType(ms.GenericInstMethodSig.GenericArguments[j], assembly);
                                if (ga == null)
                                    continue;

                                ms.GenericInstMethodSig.GenericArguments[j] = ga;
                            }
                            method.Body.Instructions[i].Operand = new MethodSpecUser(mru, ms.GenericInstMethodSig);
                            break;
                        }

                        throw new Exception("XXXX");

                        break;
                    case OperandType.InlineString:
                        break;
                    case OperandType.InlineType:
                        var it = method.Body.Instructions[i].Operand as TypeDef;
                        ts = ReferenceType(it.ToTypeSig(), assembly);
                        method.Body.Instructions[i].Operand = ts.TryGetTypeDef();
                        //Console.WriteLine("{0}", (method.Body.Instructions[i].Operand).ToString());
                        break;
                    default:
                        if (method.Body.Instructions[i].Operand is Instruction)
                        {
                            Console.WriteLine("{0}", (method.Body.Instructions[i].Operand as Instruction).Operand);
                            continue;
                        }
                        throw new Exception(method.Body.Instructions[i].OpCode.OperandType.ToString());
                }


            }
        }

        private static void ReReference(TypeDef type, AssemblyDef assembly, AssemblyDef srcAssembly)
        {
            ReCustomAttributes(type, assembly);

            var baseType = type.BaseType.ToTypeSig();
            if (baseType != null)
            {
                baseType = ReferenceType(baseType, assembly);
                if (baseType != null)
                    type.BaseType = baseType.ToTypeDefOrRef();
            }

            for (int i = 0; i < type.Interfaces.Count; ++i)
            {
                var intype = type.Interfaces[i].Interface.ToTypeSig();
                intype = ReferenceType(intype, assembly);
                if (intype == null)
                    continue;

                type.Interfaces[i].Interface = intype.ToTypeDefOrRef();
            }


            for (int i = 0; i < type.Fields.Count; ++i)
            {
                var field = type.Fields[i];
                ReCustomAttributes(field, assembly);

                var newtype = ReferenceType(field.FieldType, assembly);
                if (newtype == null)
                    continue;

                field.FieldType = newtype;
            }

            for (int i = 0; i < type.Events.Count; ++i)
            {
                var e = type.Events[i];
                ReMethodDef(e.AddMethod, assembly);
                ReMethodDef(e.RemoveMethod, assembly);
                ReCustomAttributes(e, assembly);

                var et = ReferenceType(e.EventType.ToTypeSig(), assembly);
                if (et == null)
                    continue;

                e.EventType = et.ToTypeDefOrRef();
                var ne = new EventDefUser(e.Name, et.ToTypeDefOrRef(), e.Attributes);
                ne.AddMethod = new MethodDefUser(e.AddMethod.Name, e.AddMethod.MethodSig, e.AddMethod.ImplAttributes, e.AddMethod.Attributes);
                ne.AddMethod.MethodBody = e.AddMethod.MethodBody;

                ne.RemoveMethod = new MethodDefUser(e.RemoveMethod.Name, e.RemoveMethod.MethodSig, e.RemoveMethod.ImplAttributes, e.RemoveMethod.Attributes);
                ne.RemoveMethod.MethodBody = e.RemoveMethod.MethodBody;

                type.Events[i] = ne;
            }


            for (int i = 0; i < type.Properties.Count; ++i)
            {
                var p = type.Properties[i];
                ReCustomAttributes(p, assembly);
                ReMethodDef(p.GetMethod, assembly);
                ReMethodDef(p.SetMethod, assembly);
            }

            for (int k = 0; k < type.Methods.Count; ++k)
            {
                var method = type.Methods[k];
                ReCustomAttributes(method, assembly);

                ReMethodDef(method, assembly);

                ReMethodBody(type, method, assembly);   
            }
        }

        private static void DupType(TypeDef src, TypeDef dest)
        {
            dest.Attributes = src.Attributes;
            dest.ClassLayout = src.ClassLayout;
            dest.ClassSize = src.ClassSize;
            dest.Visibility = src.Visibility;

            dest.CustomAttributes.Clear();
            foreach (var attr in src.CustomAttributes)
            {
                dest.CustomAttributes.Add(attr);
            }

            dest.BaseType = src.BaseType;

            dest.Interfaces.Clear();
            foreach (var i in src.Interfaces)
            {
                dest.Interfaces.Add(i);
            }

            dest.Fields.Clear();
            for (int i = 0; src.Fields.Count > 0; )
            {
                var f = src.Fields[i];
                f.DeclaringType = null;
                dest.Fields.Add(f);
            }

            dest.Methods.Clear();
            for (int i = 0; src.Methods.Count > 0; )
            {
                var m = src.Methods[i];
                m.DeclaringType = null;
                dest.Methods.Add(m);
            }

            dest.Properties.Clear();
            for (int i = 0; src.Properties.Count > 0; )
            {
                var p = src.Properties[i];
                p.DeclaringType = null;
                dest.Properties.Add(p);
            }

            dest.Events.Clear();
            for (int i = 0; src.Events.Count > 0; )
            {
                var e = src.Events[i];
                e.DeclaringType = null;
                dest.Events.Add(e);
            }

            dest.NestedTypes.Clear();
            for (int i = 0; src.NestedTypes.Count > 0; )
            {
                var n = src.NestedTypes[i];
                n.DeclaringType = null;
                dest.NestedTypes.Add(n);
            }
        }

        public static string CopyClass(string src, string dest)
        {
            var srcAssembly = AssemblyDef.Load(src);
            var destAssembly = AssemblyDef.Load(dest);

            var query =
            from srcM in srcAssembly.Modules
            from srcT in srcM.Types
            from destM in destAssembly.Modules
            from destT in destM.Types
            where srcT.FullName == destT.FullName
            select new { srcT, srcM, destT, destM };

            var array = query.ToArray();

            foreach (var pair in array)
            {
                var index =  pair.destM.Types.IndexOf(pair.destT);
                pair.srcM.Types.Remove(pair.srcT);
                pair.destM.Types[index] = pair.srcT;
            }

            var result = "patch." + dest;
            destAssembly.Write(result);

            foreach (var mod in destAssembly.Modules)
            {
                mod.Dispose();
            }

            return result;
        }
    }
}
