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
            //testlib.dll ilpatchlib.dll Class1
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
                var result = Helper.MergeClass(src, dest, className);
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
        static ModuleDef debugMod;

        public static string MergeClass(string src, string dest, string className)
        {
            //var destAssembly = AssemblyDef.Load(dest);
            //var srcAssembly = AssemblyDef.Load(src);

            var destMod = ModuleDefMD.Load(dest);
            var srcMod = ModuleDefMD.Load(src);


            //DupRid(destAssembly, srcAssembly);

            var query = from type in srcMod.Types
                        where type.Name == className
                        select type;

            var srcType = query.FirstOrDefault();

            if (srcType == null)
                throw new Exception("srcType");

            query = from type in destMod.Types
                    where type.Name == className
                    select type;

            var destType = query.FirstOrDefault();

            if (destType == null)
                throw new Exception("destType");

            debugMod = destMod;

            DupType(srcType, destType);
            ReReference(destType, destMod);

            var result = "patch." + dest;
            destMod.Write(result);
            destMod.Dispose();

            return result;
        }

        private static TypeSig ReferenceType(TypeSig type, ModuleDef module)
        {
            if (type == null)
                return null;

            if (type.IsSZArray)
            {
                var szar = type.ToSZArraySig();
                var eleType = ReferenceType(szar.Next, module);
                if (eleType == null)
                    return null;
                return new SZArraySig(eleType);
            }
            
            if (type.IsArray)
            {
                var ar = type.ToArraySig();
                var eleType = ReferenceType(ar.Next, module);
                if (eleType == null)
                    return null;
                return new ArraySig(eleType, ar.Rank, ar.Sizes, ar.LowerBounds);
            }

            if (type.IsGenericInstanceType)
            {
                var g = type.ToGenericInstSig();

                var gtype = module.Find(g.GenericType.FullName, false);
                ClassOrValueTypeSig ngt;
                if (gtype == null)
                    ngt = g.GenericType;
                else  
                    ngt = gtype.TryGetClassOrValueTypeSig();

                TypeSig[] genericArgs = new TypeSig[g.GenericArguments.Count];
                for (int i = 0; i < g.GenericArguments.Count; ++i)
                {
                    var subArg = ReferenceType(g.GenericArguments[i], module);
                    if (subArg != null)
                        genericArgs[i] = subArg;
                    else
                        genericArgs[i] = g.GenericArguments[i];
                }

                return new GenericInstSig(ngt, genericArgs);
            }

            var targetType = module.Find(type.FullName, false);
            if (targetType == null)
                return null;

            return targetType.ToTypeSig();
        }

        private static void ReCustomAttributes(IHasCustomAttribute type, ModuleDef module)
        {

            for (int i = 0; i < type.CustomAttributes.Count; ++i)
            {
                var newattr = module.Find(type.CustomAttributes[i].TypeFullName, false);
                if (newattr == null)
                    continue;

                type.CustomAttributes[i] = new CustomAttribute(newattr.FindDefaultConstructor());
            }
        }
        
        private static void ReMethodSig(MethodSig sig, ModuleDef module)
        {
            var retType = sig.RetType;
            var ts = ReferenceType(sig.RetType, module);
            if (ts != null)
                sig.RetType = ts;

            for (var i = 0; i < sig.Params.Count; ++i)
            {
                ts = ReferenceType(sig.Params[i], module);
                if (ts == null)
                    continue;

                sig.Params[i] = ts;
            }
        }

        private static void ReMethodDef(MethodDef method, ModuleDef module)
        {
            ReCustomAttributes(method, module);

            var newtype = ReferenceType(method.ReturnType, module);
            if (newtype != null)
                method.ReturnType = newtype;

            for (int i = 0; i < method.ParamDefs.Count; ++i)
            {
                var p = method.ParamDefs[i];
                ReCustomAttributes(p, module);
            }

            for (int i = 0; i < method.Parameters.Count; ++i)
            {
                var p = method.Parameters[i];

                if (p.HasParamDef)
                    ReCustomAttributes(p.ParamDef, module);

                var newparam = ReferenceType(p.Type, module);
                if (newtype == null)
                    continue;

                p.Type = newparam;
            }


            for (int i = 0; i < method.GenericParameters.Count; ++i)
            {
                var genep = method.GenericParameters[i];
                ReCustomAttributes(genep, module);
                var negp = new GenericParamUser(genep.Number, genep.Flags, genep.Name);
                foreach (var ca in genep.CustomAttributes)
                {
                    negp.CustomAttributes.Add(ca);
                }

                method.GenericParameters[i] = negp;
            }

            ReMethodSig(method.MethodSig, module);
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

        private static IEnumerable<TypeDef> AllNestTypes(TypeDef type)
        {
            List<TypeDef> types = new List<TypeDef>();
            types.Add(type);

            if (!type.NestedTypes.Any())
                return types;

            foreach(var t in type.NestedTypes)
            {
                types.AddRange(AllNestTypes(t));
            }

            return types;
        }

        private static MethodDef FindMethod(string fullName, ModuleDef module)
        {
            //foreach (var type in module.Types)
            //{
            //    foreach (var nestType in AllNestTypes(type))
            //    {
            //        foreach (var method in nestType.Methods)
            //        {
            //            if (method.FullName == fullName)
            //                return method;
            //        }
            //    }
            //}

            //return null;

            var newM = (from t in module.Types
                        from it in AllNestTypes(t)
                        from im in it.Methods
                        where im.FullName == fullName
                        select im).FirstOrDefault();

            return newM;
        }

        private static FieldDef FindField(string fullName, ModuleDef module)
        {
            var newF = (from t in module.Types
                        from it in AllNestTypes(t)
                        from im in it.Fields
                        where im.FullName == fullName
                        select im).FirstOrDefault();

            return newF;
        }

        private static void ReMethodBody(TypeDef type, MethodDef method, ModuleDef module)
        {
            for (int i = 0; i < method.Body.Variables.Count; ++i)
            {
                var local = method.Body.Variables[i];

                var lt = ReferenceType(local.Type, module);
                if (lt != null)
                    local.Type = lt;
            }

            for (int i = 0; i < method.Body.Instructions.Count; ++i)
            {
                if (method.Body.Instructions[i].Operand == null)
                    continue;

                TypeSig ts;
                switch (method.Body.Instructions[i].OpCode.OperandType)
                {
                    case OperandType.InlineField:
                        var field = method.Body.Instructions[i].Operand as FieldDef;
                        if (field != null)
                        {
                            var newfield = FindField(field.FullName, module);
                            if (newfield != null)
                                method.Body.Instructions[i].Operand = newfield;
                        }
                        var fieldR = method.Body.Instructions[i].Operand as MemberRef;
                        if (fieldR != null)
                        {
                            var nfr = module.GetMemberRefs().FirstOrDefault(mr => mr.FullName == fieldR.FullName);
                            if (nfr != null)
                                method.Body.Instructions[i].Operand = nfr;
                        }
                        break;
                    case OperandType.InlineMethod:
                        var m = method.Body.Instructions[i].Operand as MemberRef;
                        if (m != null)
                        {
                            var tmodule = type.Module;
                            if (m.DeclaringType != null)
                            {
                                ts = ReferenceType(m.DeclaringType.ToTypeSig(), module);
                                if (ts != null)
                                    tmodule = ts.Module;
                            }

                            if (m.IsFieldRef)
                            {
                                throw new Exception("????" + method.Body.Instructions[i].OpCode.OperandType.ToString());
                                //method.Body.Instructions[i].Operand = new MemberRefUser(ts.Module, m.Name, new FieldSig(ts));
                            }
                            else if (m.IsMethodRef)
                            {

                                ReMethodSig(m.MethodSig, module);

                                //var msig = new MethodSig(
                                //    m.MethodSig.CallingConvention,
                                //    m.MethodSig.GenParamCount,
                                //    m.MethodSig.RetType,
                                //    m.MethodSig.Params
                                //    );
                                //method.Body.Instructions[i].Operand = new MemberRefUser(tmodule, m.Name, m.MethodSig);
                            }

                            break;
                        }

                        var md = method.Body.Instructions[i].Operand as MethodDef;
                        if (md != null)
                        {
                            var newM = FindMethod(md.FullName, module);
                            if (newM != null)
                            {
                                method.Body.Instructions[i].Operand = newM;
                            }
                            else
                            {
                                var newMR = module.GetMemberRefs().FirstOrDefault(mr => mr.FullName == md.FullName);
                                if (newMR != null)
                                    method.Body.Instructions[i].Operand = newMR;

                            }

                            break;
                        }

                        var ms = method.Body.Instructions[i].Operand as MethodSpec;
                        if (ms != null)
                        {
                            var msm = module.GetMemberRefs().FirstOrDefault(mr => mr.FullName == ms.Method.FullName);
                            if (msm != null)
                                ms.Method = msm;

                            for (int j = 0; j < ms.GenericInstMethodSig.GenericArguments.Count; ++j)
                            {
                                var ga = ReferenceType(ms.GenericInstMethodSig.GenericArguments[j], module);
                                if (ga == null)
                                    continue;

                                ms.GenericInstMethodSig.GenericArguments[j] = ga;
                            }
                            break;
                        }

                        throw new NotSupportedException();

                        break;
                    case OperandType.InlineType:
                        var it = method.Body.Instructions[i].Operand as TypeDef;
                        if (it == null)
                        {
                            Console.WriteLine("InlineType {0}", (method.Body.Instructions[i].Operand as Instruction).Operand);
                            break;
                        }
                        ts = ReferenceType(it.ToTypeSig(), module);
                        if (ts != null)
                            method.Body.Instructions[i].Operand = ts.TryGetTypeDef();
                        //Console.WriteLine("{0}", (method.Body.Instructions[i].Operand).ToString());
                        break;
                    case OperandType.ShortInlineVar:
                        var local = method.Body.Instructions[i].Operand as Local;
                        if (local == null)
                        {
                            Console.WriteLine("ShortInlineVar {0}", (method.Body.Instructions[i].Operand as Instruction).Operand);
                            break;
                        }
                        ts = ReferenceType(local.Type, module);
                        if (ts != null)
                            local.Type = ts;
                        break;
                    default:
                        if (method.Body.Instructions[i].Operand is Instruction)
                        {
                            //Console.WriteLine("{0}", (method.Body.Instructions[i].Operand as Instruction).Operand);
                            continue;
                        }
                        break;
                        //throw new Exception(method.Body.Instructions[i].OpCode.OperandType.ToString());
                }
            }
        }

        private static void ReReference(TypeDef type, ModuleDef module)
        {
            ReCustomAttributes(type, module);

            var baseType = type.BaseType.ToTypeSig();
            if (baseType != null)
            {
                baseType = ReferenceType(baseType, module);
                if (baseType != null)
                    type.BaseType = baseType.ToTypeDefOrRef();
            }

            for (int i = 0; i < type.Interfaces.Count; ++i)
            {
                var intype = type.Interfaces[i].Interface.ToTypeSig();
                intype = ReferenceType(intype, module);
                if (intype == null)
                    continue;

                type.Interfaces[i].Interface = intype.ToTypeDefOrRef();
            }


            for (int i = 0; i < type.Fields.Count; ++i)
            {
                var field = type.Fields[i];
                ReCustomAttributes(field, module);

                var newtype = ReferenceType(field.FieldType, module);
                if (newtype == null)
                    continue;

                field.FieldType = newtype;
            }

            for (int i = 0; i < type.Events.Count; ++i)
            {
                var e = type.Events[i];
                if (e.AddMethod != null)
                    ReMethodDef(e.AddMethod, module);
                if (e.RemoveMethod != null)
                    ReMethodDef(e.RemoveMethod, module);
                ReCustomAttributes(e, module);

                var et = ReferenceType(e.EventType.ToTypeSig(), module);
                if (et == null)
                    continue;

                e.EventType = et.ToTypeDefOrRef();
                //var ne = new EventDefUser(e.Name, et.ToTypeDefOrRef(), e.Attributes);
                //ne.AddMethod = new MethodDefUser(e.AddMethod.Name, e.AddMethod.MethodSig, e.AddMethod.ImplAttributes, e.AddMethod.Attributes);
                //ne.AddMethod.MethodBody = e.AddMethod.MethodBody;

                //ne.RemoveMethod = new MethodDefUser(e.RemoveMethod.Name, e.RemoveMethod.MethodSig, e.RemoveMethod.ImplAttributes, e.RemoveMethod.Attributes);
                //ne.RemoveMethod.MethodBody = e.RemoveMethod.MethodBody;

                //type.Events[i] = ne;
            }


            for (int i = 0; i < type.Properties.Count; ++i)
            {
                var p = type.Properties[i];
                ReCustomAttributes(p, module);

                var retType = ReferenceType(p.PropertySig.RetType, module);
                if (retType != null)
                    p.PropertySig.RetType = retType;

                for (int j = 0; j < p.PropertySig.Params.Count; ++j)
                {
                    var pp = p.PropertySig.Params[j];
                    pp = ReferenceType(pp, module);
                    if (pp == null)
                        continue;

                    p.PropertySig.Params[j] = pp;
                }

                if (p.GetMethod != null)
                    ReMethodDef(p.GetMethod, module);
                if (p.SetMethod != null)
                    ReMethodDef(p.SetMethod, module);
            }

            for (int i = 0; i < type.Methods.Count; ++i)
            {
                var method = type.Methods[i];
                ReMethodDef(method, module);
                ReMethodBody(type, method, module);   
            }

            for (int i = 0; i < type.NestedTypes.Count; ++i)
            {
                var nest = type.NestedTypes[i];

                ReReference(nest, module);
            }
        }

        private static void DupType(TypeDef src, TypeDef dest)
        {
            dest.Attributes = src.Attributes;
            dest.ClassLayout = src.ClassLayout;
            dest.ClassSize = src.ClassSize;
            dest.Visibility = src.Visibility;

            
            for (int i = 0; i < dest.NestedTypes.Count; ++i)
            {
                var dnt = dest.NestedTypes[i];
                var snt = src.NestedTypes.FirstOrDefault(nt => nt.FullName == dnt.FullName);

                if (snt == null)
                    continue;

                DupType(snt, dnt);
            }

            var ntArray = src.NestedTypes.Where(snt => !dest.NestedTypes.Any(dnt => dnt.FullName == snt.FullName)).ToArray();
            foreach (var nt in ntArray)
            {
                nt.DeclaringType = dest;
            }

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


            var fdArray = src.Fields.Where(snt => !dest.Fields.Any(dnt => dnt.FullName == snt.FullName)).ToArray();
            foreach (var fd in fdArray)
            {
                fd.DeclaringType = dest;
            }

            //dest.Fields.Clear();
            //for (int i = 0; src.Fields.Count > 0; )
            //{
            //    var f = src.Fields[i];
            //    f.DeclaringType = dest;
            //}

            var mdArray = src.Methods.Where(snt => !dest.Methods.Any(dnt => dnt.FullName == snt.FullName)).ToArray();
            foreach (var md in mdArray)
            {
                md.DeclaringType = dest;
            }

            //dest.Methods.Clear();
            //for (int i = 0; src.Methods.Count > 0; )
            //{
            //    var m = src.Methods[i];
            //    m.DeclaringType = dest;
            //}

            var ptArray = src.Properties.Where(snt => !dest.Properties.Any(dnt => dnt.FullName == snt.FullName)).ToArray();
            foreach (var pt in ptArray)
            {
                pt.DeclaringType = dest;
            }

            //dest.Properties.Clear();
            //for (int i = 0; src.Properties.Count > 0; )
            //{
            //    var p = src.Properties[i];
            //    p.DeclaringType = dest;
            //}

            var evArray = src.Events.Where(snt => !dest.Events.Any(dnt => dnt.FullName == snt.FullName)).ToArray();
            foreach (var ev in evArray)
            {
                ev.DeclaringType = dest;
            }

            //dest.Events.Clear();
            //for (int i = 0; src.Events.Count > 0; )
            //{
            //    var e = src.Events[i];
            //    e.DeclaringType = dest;
            //}
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
