using UnitTest;

namespace Profiler
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.Sleep(3000);

            var test = new Calculate24Test();
            test.Test();
            //test.Test2();
            test.TestFast();

            System.Threading.Thread.Sleep(3000);
        }
    }
}
