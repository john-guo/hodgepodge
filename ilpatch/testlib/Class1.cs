using System;
using System.Collections.Generic;
using System.Collections;
using testlib;

namespace ilpatchlib
{
    [Attr]
    public class Class1 : Interface1
    {
        public class NC
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

            public void testNC1()
            {
                Console.WriteLine("NC1");
            }
        }

        [Attr]
        Class2 c2;
        Struct2[] s2;
        Struct2[,,] s3;
        Dictionary<Struct2[], Dictionary<Class2, List<Struct2[,]>>>[] c3;
        NC.NC2 o; 
        public Class1()
        {
            new NC();
            c2 = new Class2();
            f1();
            f3<Class3>("World");
            new comm.Class1();
            TestM();

            e += Class1_e;
        }

        private void Class1_e()
        {
            throw new NotImplementedException();
        }

        void TestM()
        {
            var d = new NC();
            d.testNC1();
            Console.WriteLine("patched");
        }

        [Attr]
        [return:Attr]
        Class2 f1()
        {
            Console.WriteLine("patched f1 hello");
            return c2;
        }


        void f2<T>(Class2 d, T p)
        {
            d.test();
            Console.WriteLine("patched f2 hello {0}", p);
        }

        void f3<Class3>(string world)
        {
            Console.WriteLine("patched f3 hello {0}", world);
            Class2 a = c2;
            f2(a, 1);
        }

        Class2 C
        {
            get; set;
        }

        Class3 this[Class2 a] { get { return null; }  set { } }


        event eh e;

        private IEnumerator Test()
        {
            int i = 0;
            yield return null;
            i++;
            yield return i;
            i++;
            yield break;
        }
    }
}
