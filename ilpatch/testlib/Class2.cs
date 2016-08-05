using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testlib
{
    public interface Interface1 { }

    public class Class3 { }

    public class Class2 : Class3, Interface1
    {
        public void test()
        {
            ilpatchlib.Class1.NC nc = new ilpatchlib.Class1.NC();
            nc.testNC1();
            Console.WriteLine("Class2");
        }

        public ilpatchlib.Class1 getClass()
        {
            object c3 = new Class3();
            return c3 as ilpatchlib.Class1;
        }
    }

    public struct Struct2
    {

    }

    public delegate void eh();

    [AttributeUsage(AttributeTargets.All)]
    public class Attr: Attribute
    {

    }
}
