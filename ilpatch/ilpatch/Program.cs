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
        static void Main(string[] args)
        {
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
                var result = Helper.MergeClass(src, dest, className);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }

    static class Helper
    {
        public static string MergeClass(string src, string dest, string className)
        {
            var destMod = ModuleDefMD.Load(dest);
            var srcMod = ModuleDefMD.Load(src);

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

                var gtype = FindType(g.GenericType.FullName, module);
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

            var targetType = FindType(type.FullName, module);
            if (targetType == null)
                return null;

            return targetType.ToTypeSig();
        }

        private static void ReCustomAttributes(IHasCustomAttribute type, ModuleDef module)
        {

            for (int i = 0; i < type.CustomAttributes.Count; ++i)
            {
                var newattr = FindType(type.CustomAttributes[i].TypeFullName, module);
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

        private static void ReFieldSig(FieldSig sig, ModuleDef module)
        {
            var ts = ReferenceType(sig.Type, module);
            if (ts != null)
                sig.Type = ts;
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

        private static IEnumerable<TypeDef> AllNestTypes(TypeDef type)
        {
            yield return type;
            if (!type.NestedTypes.Any())
                yield break;

            foreach(var t in type.NestedTypes)
                foreach (var nt in AllNestTypes(t))
                    yield return nt;
        }

        private static TypeDef FindType(string fullName, ModuleDef module)
        {
            var newT = module.Find(fullName, false);
            if (newT != null)
                return newT;

            newT = (from t in module.Types
                        from it in AllNestTypes(t)
                        where it.FullName == fullName
                        select it).FirstOrDefault();

            return newT;
        }

        private static MethodDef FindMethod(string fullName, ModuleDef module)
        {
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

                            break;
                        }
                        var fieldR = method.Body.Instructions[i].Operand as MemberRef;
                        if (fieldR != null)
                        {
                            if (fieldR.IsFieldRef)
                            {
                                ReFieldSig(fieldR.FieldSig, module);
                            }
                            else if (fieldR.IsMethodRef)
                            {
                                throw new NotSupportedException(method.Body.Instructions[i].OpCode.OperandType.ToString());
                            }

                            break;
                        }

                        Console.WriteLine("InlineField {0}", method.Body.Instructions[i].Operand.GetType());
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
                                throw new NotSupportedException(method.Body.Instructions[i].OpCode.OperandType.ToString());
                            }
                            else if (m.IsMethodRef)
                            {
                                ReMethodSig(m.MethodSig, module);
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
                            break;
                        }

                        var ms = method.Body.Instructions[i].Operand as MethodSpec;
                        if (ms != null)
                        {
                            var msm = FindMethod(ms.Method.FullName, module);
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

                        Console.WriteLine("InlineMethod {0}", method.Body.Instructions[i].Operand.GetType());
                        break;
                    case OperandType.InlineType:
                        var it = method.Body.Instructions[i].Operand as TypeDef;
                        if (it != null)
                        {
                            ts = ReferenceType(it.ToTypeSig(), module);
                            if (ts != null)
                                method.Body.Instructions[i].Operand = ts.TryGetTypeDef();
                            break;
                        }

                        var itf = method.Body.Instructions[i].Operand as TypeRef;
                        if (itf != null)
                        {
                            ts = ReferenceType(it.ToTypeSig(), module);
                            if (ts != null)
                                method.Body.Instructions[i].Operand = ts.TryGetTypeRef();
                            break;
                        }

                        var its = method.Body.Instructions[i].Operand as TypeSpec;
                        if (its != null)
                        {
                            ts = ReferenceType(its.TypeSig, module);
                            if (ts != null)
                                its.TypeSig = ts;

                            break;
                        }

                        Console.WriteLine("InlineType {0}", method.Body.Instructions[i].Operand.GetType());
                        break;
                    case OperandType.ShortInlineVar:
                        var local = method.Body.Instructions[i].Operand as Local;
                        if (local == null)
                        {
                            Console.WriteLine("ShortInlineVar {0}", method.Body.Instructions[i].Operand.GetType());
                            break;
                        }
                        ts = ReferenceType(local.Type, module);
                        if (ts != null)
                            local.Type = ts;
                        break;
                    default:
                        break;
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
            dest.BaseType = src.BaseType;

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

            var caArray = src.CustomAttributes.Where(snt => !dest.CustomAttributes.Any(dnt => dnt.TypeFullName == snt.TypeFullName)).ToArray();
            foreach (var attr in caArray)
            {

                dest.CustomAttributes.Add(attr);
            }

            var iArray = src.Interfaces.Where(snt => !dest.Interfaces.Any(dnt => dnt.Interface.FullName == snt.Interface.FullName)).ToArray();
            foreach (var i in iArray)
            {
                dest.Interfaces.Add(i);
            }

            for (int i = 0; i < dest.Fields.Count; ++i)
            {
                var ditem = dest.Fields[i];
                var sitem = src.Fields.FirstOrDefault(item => item.FullName == ditem.FullName);

                if (sitem == null)
                    continue;

                ditem.Constant = sitem.Constant;
                ditem.Attributes = sitem.Attributes;
                ditem.FieldOffset = sitem.FieldOffset;
                ditem.Access = sitem.Access;
                ditem.FieldType = sitem.FieldType;
                ditem.FieldSig = sitem.FieldSig;
                var itemca = sitem.CustomAttributes.Where(snt => !ditem.CustomAttributes.Any(dnt => dnt.TypeFullName == snt.TypeFullName)).ToArray();
                foreach (var attr in itemca)
                {
                    ditem.CustomAttributes.Add(attr);
                }
            }
            var fdArray = src.Fields.Where(snt => !dest.Fields.Any(dnt => dnt.FullName == snt.FullName)).ToArray();
            foreach (var fd in fdArray)
            {
                fd.DeclaringType = dest;
            }



            for (int i = 0; i < dest.Methods.Count; ++i)
            {
                var dmd = dest.Methods[i];
                var smd = src.Methods.FirstOrDefault(md => md.FullName == dmd.FullName);

                if (smd == null)
                    continue;

                dmd.Attributes = smd.Attributes;
                dmd.Access = smd.Access;
                dmd.ReturnType = smd.ReturnType;
                dmd.Body = smd.Body;
                dmd.MethodSig.Params.Clear();
                for (int j = 0; j < smd.MethodSig.Params.Count; ++j)
                {
                    dmd.MethodSig.Params.Add(smd.MethodSig.Params[j]);
                }
                var itemca = dmd.CustomAttributes.Where(snt => !smd.CustomAttributes.Any(dnt => dnt.TypeFullName == snt.TypeFullName)).ToArray();
                foreach (var attr in itemca)
                {
                    dmd.CustomAttributes.Add(attr);
                }
            }
            var mdArray = src.Methods.Where(snt => !dest.Methods.Any(dnt => dnt.FullName == snt.FullName)).ToArray();
            foreach (var md in mdArray)
            {
                md.DeclaringType = dest;
            }

            for (int i = 0; i < dest.Properties.Count; ++i)
            {
                var ditem = dest.Properties[i];
                var sitem = src.Properties.FirstOrDefault(item => item.FullName == ditem.FullName);

                if (sitem == null)
                    continue;

                ditem.Type = sitem.Type;
                ditem.Attributes = sitem.Attributes;
                ditem.PropertySig = sitem.PropertySig;
                var itemca = sitem.CustomAttributes.Where(snt => !ditem.CustomAttributes.Any(dnt => dnt.TypeFullName == snt.TypeFullName)).ToArray();
                foreach (var attr in itemca)
                {
                    ditem.CustomAttributes.Add(attr);
                }
            }
            var ptArray = src.Properties.Where(snt => !dest.Properties.Any(dnt => dnt.FullName == snt.FullName)).ToArray();
            foreach (var pt in ptArray)
            {
                pt.DeclaringType = dest;
            }


            for (int i = 0; i < dest.Events.Count; ++i)
            {
                var ditem = dest.Events[i];
                var sitem = src.Events.FirstOrDefault(item => item.FullName == ditem.FullName);

                if (sitem == null)
                    continue;

                ditem.EventType = sitem.EventType;
                ditem.Attributes = sitem.Attributes;
                var itemca = sitem.CustomAttributes.Where(snt => !ditem.CustomAttributes.Any(dnt => dnt.TypeFullName == snt.TypeFullName)).ToArray();
                foreach (var attr in itemca)
                {
                    ditem.CustomAttributes.Add(attr);
                }
            }
            var evArray = src.Events.Where(snt => !dest.Events.Any(dnt => dnt.FullName == snt.FullName)).ToArray();
            foreach (var ev in evArray)
            {
                ev.DeclaringType = dest;
            }
        }
    }
}
