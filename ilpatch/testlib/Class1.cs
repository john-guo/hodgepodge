using System;
using System.Collections.Generic;
using testlib;

namespace ilpatchlib
{
    //[Attr]
    public class Class1
    {
        //[Attr]
        Class2 c2;
        Struct2[] s2;
        Struct2[,,] s3;
        Dictionary<Struct2[], Dictionary<Class2, List<Struct2[,]>>>[] c3;
        public Class1()
        {
            //c2 = new Class2();
            //f3();
            new comm.Class1();
            Console.WriteLine("patched");
        }

        //[Attr]
        //[return:Attr]
        Class2 f1()
        {
            return c2;
        }

        /*
        void f2(Class2 d)
        {
            d.test();
        }

        void f3<Class3>()
        {
            Class2 a = c2;
            f2(a);
        }

        Class2 C
        {
            get; set;
        }
        */

        //event eh e;
    }
}
