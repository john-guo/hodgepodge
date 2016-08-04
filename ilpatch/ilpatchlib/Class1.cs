using System;
using testlib;

namespace ilpatchlib
{
    public class Class1
    {
        class NC
        {
            int i;
            public class NC2
            {
                int j;
                void testNC2()
                {
                    Console.WriteLine("NC2");
                }

                public NC2()
                {
                    testNC2();
                }
            }

            public NC()
            {
                testNC1();
                new NC2();
            }

            void testNC1()
            {
                Console.WriteLine("NC1");
            }
        }

        Class2 c2;
        Struct2 s2;
        public Class1()
        {
            f1();
            f2<int, object>(0, null);
            f3<Class1>(1);
            new comm.Class1();
            Console.WriteLine("origin");
        }

        void f1() { Console.WriteLine("f1 hello"); }
        void f2<T1, T2>(T1 a, T2 b) { Console.WriteLine("f2 hello"); }
        void f3<Class1>(int a) { Console.WriteLine("f3 hello"); }

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
