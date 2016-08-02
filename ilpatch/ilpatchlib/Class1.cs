using System;
using testlib;

namespace ilpatchlib
{
    public class Class1
    {
        Class2 c2;
        Struct2 s2;
        public Class1()
        {
            //c2 = new Class2();
            //f3();
            new comm.Class1();
            Console.WriteLine("origin");
        }

        //Class2 f1()
        //{
        //    return c2;
        //}

        //void f2(Class2 d)
        //{
        //    d.test();
        //}

        //void f3()
        //{
        //    Class2 a = c2;
        //    f2(a);
        //}

        //Class2 C
        //{
        //    get; set;
        //}
    }
}
