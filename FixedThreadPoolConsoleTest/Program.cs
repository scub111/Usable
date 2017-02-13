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

            TaskEx task = new TaskEx(() => { Console.WriteLine("test"); Thread.Sleep(1000); });

            pool.Execute(task);

            Thread.Sleep(10000);

            Console.ReadLine();
        }
    }
}
