using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom.Compiler;
using dnlib.DotNet;
using System.IO;

namespace cspatch
{
    class Program
    {
        static void Process(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("cspatch targetDll source.cs [ref.dll ...]");
                return;
            }

            var targetdll = args[0];
            var src = args[1];

            var refNames = new[] { targetdll }.Union(args.Skip(2)).ToArray();

            var srcdll = Build(src, refNames);
            if (srcdll == null)
                return;

            CopyClass(srcdll, targetdll);
        }

        static void Main(string[] args)
        {
            Process(args);
            Console.ReadLine();
        }

        static string Build(string csfile, params string[] referencedAssemblies)
        {
            var buildDll = Path.ChangeExtension(Path.GetRandomFileName(), "dll");
            var provider = CodeDomProvider.CreateProvider("cs");

            var defaultRefAssemblies = new[]
            {
                "System.dll",
            };

            var result = provider.CompileAssemblyFromFile(
                new CompilerParameters(defaultRefAssemblies.Union(referencedAssemblies).ToArray(), buildDll)
                , csfile);

            if (result.Errors.HasErrors)
            {
                foreach(var err in result.Errors.OfType<CompilerError>())
                {
                    Console.WriteLine(err.ToString());
                }

                return null;
            }

            return result.PathToAssembly;
        }

        static string CopyClass(string src, string dest)
        {
            var destAssembly = AssemblyDef.Load(dest);
            var srcAssembly = AssemblyDef.Load(src);

            var query = 
                from module in srcAssembly.Modules
                from type in module.Types
                where !type.IsGlobalModuleType
                select new { module, type };

            var alltypes = query.ToArray();

            foreach (var type in alltypes)
            {
                var q =
                    from m in destAssembly.Modules
                    from t in m.Types
                    where t.FullName == type.type.FullName
                    select new { m, t };

                type.module.Types.Remove(type.type);

                var target = q.FirstOrDefault();
                if (target == null)
                {
                    destAssembly.ManifestModule.Types.Add(type.type);
                }
                else
                {
                    DupType(type.type, target.t);
                }
            }

            var result = "patch." + dest;
            destAssembly.Write(result);

            foreach (var mod in destAssembly.Modules)
            {
                mod.Dispose();
            }

            return result;
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
            for (int i = 0; src.Fields.Count > 0;)
            {
                var f = src.Fields[i];
                f.DeclaringType = null;
                dest.Fields.Add(f);
            }

            dest.Methods.Clear();
            for (int i = 0; src.Methods.Count > 0;)
            {
                var m = src.Methods[i];
                m.DeclaringType = null;
                dest.Methods.Add(m);
            }

            dest.Properties.Clear();
            for (int i = 0; src.Properties.Count > 0;)
            {
                var p = src.Properties[i];
                p.DeclaringType = null;
                dest.Properties.Add(p);
            }

            dest.Events.Clear();
            for (int i = 0; src.Events.Count > 0;)
            {
                var e = src.Events[i];
                e.DeclaringType = null;
                dest.Events.Add(e);
            }

            dest.NestedTypes.Clear();
            for (int i = 0; src.NestedTypes.Count > 0;)
            {
                var n = src.NestedTypes[i];
                n.DeclaringType = null;
                dest.NestedTypes.Add(n);
            }
        }
    }
}
