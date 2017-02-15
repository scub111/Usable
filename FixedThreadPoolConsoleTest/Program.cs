using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Usable;

namespace FixedThreadPoolConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            FixedThreadPool pool = new FixedThreadPool(1);

            pool.Execute(() => { Console.WriteLine("test"); Thread.Sleep(1000); });

            Thread.Sleep(10000);

            Console.ReadLine();
        }
    }
}
