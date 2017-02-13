using Microsoft.VisualStudio.TestTools.UnitTesting;
using Usable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Usable.Tests
{
    [TestClass()]
    public class FixedThreadPoolTests
    {
        /// <summary>
        /// Тест на поочередное выполнение задач при одном потоке в пуле.
        /// </summary>
        [TestMethod()]
        public void ExecuteTest_One()
        {
            FixedThreadPool pool = new FixedThreadPool(1);
            DateTime finished1 = DateTime.MinValue;
            DateTime finished2 = DateTime.MinValue;
            DateTime finished3 = DateTime.MinValue;

            DateTime t0 = DateTime.Now;
            TimeSpan t1, t2, t3;
            pool.Execute(new TaskEx(() => { Thread.Sleep(1000); Console.WriteLine("Task 1"); finished1 = DateTime.Now; }));
            pool.Execute(new TaskEx(() => { Thread.Sleep(1000); Console.WriteLine("Task 2"); finished2 = DateTime.Now; }));

            Thread.Sleep(2500);

            pool.Execute(new TaskEx(() => { Thread.Sleep(100); Console.WriteLine("Task 3"); finished3 = DateTime.Now; }));

            Thread.Sleep(500);

            t1 = finished1 - t0;
            t2 = finished2 - t0;
            t3 = finished3 - t0;

            bool result = false;

            if ( 500 < t1.TotalMilliseconds && t1.TotalMilliseconds < 1500 &&
                1500 < t2.TotalMilliseconds && t2.TotalMilliseconds < 2500 &&
                2500 < t3.TotalMilliseconds && t3.TotalMilliseconds < 3000)
                result = true;

            Console.WriteLine(string.Format("finished1 = {0}; t1 = {1:0}; finished2 = {2}; t2 = {3:0}; finished3 = {4}; t3 = {5:0}", 
                finished1, t1.TotalMilliseconds, 
                finished2, t2.TotalMilliseconds,
                finished3, t3.TotalMilliseconds));

            Assert.IsTrue(result);
        }

        /// <summary>
        /// Тест на выполнение задач при двух потоках в пуле.
        /// </summary>
        [TestMethod()]
        public void ExecuteTest_Two()
        {
            FixedThreadPool pool = new FixedThreadPool(2);
            DateTime finished1 = DateTime.MinValue;
            DateTime finished2 = DateTime.MinValue;
            DateTime finished3 = DateTime.MinValue;

            DateTime t0 = DateTime.Now;
            TimeSpan t1, t2, t3;
            pool.Execute(new TaskEx(() => { Thread.Sleep(1000); Console.WriteLine("Task 1"); finished1 = DateTime.Now; }));
            pool.Execute(new TaskEx(() => { Thread.Sleep(1000); Console.WriteLine("Task 2"); finished2 = DateTime.Now; }));

            Thread.Sleep(2500);

            pool.Execute(new TaskEx(() => { Thread.Sleep(100); Console.WriteLine("Task 3"); finished3 = DateTime.Now; }));

            Thread.Sleep(500);

            t1 = finished1 - t0;
            t2 = finished2 - t0;
            t3 = finished3 - t0;

            bool result = false;

            if ( 500 < t1.TotalMilliseconds && t1.TotalMilliseconds < 1500 &&
                 500 < t2.TotalMilliseconds && t2.TotalMilliseconds < 1500 &&
                2500 < t3.TotalMilliseconds && t3.TotalMilliseconds < 3000)
                result = true;

            Console.WriteLine(string.Format("finished1 = {0}; t1 = {1:0}; finished2 = {2}; t2 = {3:0}; finished3 = {4}; t3 = {5:0}",
                finished1, t1.TotalMilliseconds,
                finished2, t2.TotalMilliseconds,
                finished3, t3.TotalMilliseconds));

            Assert.IsTrue(result);
        }
    }
}