using Microsoft.VisualStudio.TestTools.UnitTesting;
using Usable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;

namespace Usable.Tests
{
    [TestClass()]
    public class FixedThreadPoolTests
    {
        /// <summary>
        /// Тест на поочередное выполнение задач при одном потоке в пуле.
        /// </summary>
        [TestMethod()]
        public void ExecuteTest_OnePoolNoPriority()
        {
            FixedThreadPool pool = new FixedThreadPool(1);
            DateTime started1 = DateTime.MinValue;
            DateTime started2 = DateTime.MinValue;
            DateTime started3 = DateTime.MinValue;

            DateTime t0 = DateTime.Now;
            TimeSpan t1, t2, t3;
            pool.Execute(() => { started1 = DateTime.Now; Console.WriteLine("Task 1"); Thread.Sleep(1000); });
            pool.Execute(() => { started2 = DateTime.Now; Console.WriteLine("Task 2"); Thread.Sleep(1000); });

            Thread.Sleep(2500);

            pool.Execute(() => { started3 = DateTime.Now; Console.WriteLine("Task 3"); Thread.Sleep(100); });

            Thread.Sleep(500);

            t1 = started1 - t0;
            t2 = started2 - t0;
            t3 = started3 - t0;

            TestConditions result = new TestConditions();

            result.Add(() => 0 < t1.TotalMilliseconds && t1.TotalMilliseconds < 500,
                () => string.Format("t1 = {0} != (0; 500)", t1.TotalMilliseconds));

            result.Add(() => 500 < t2.TotalMilliseconds && t2.TotalMilliseconds < 1500,
                () => string.Format("t2 = {0} != (500; 1500)", t2.TotalMilliseconds));

            result.Add(() => 2500 < t3.TotalMilliseconds && t3.TotalMilliseconds < 3000,
                () => string.Format("t3 = {0} != (2500; 3000)", t3.TotalMilliseconds));

            result.Print();

            Assert.IsTrue(result.Calculate());
        }

        /// <summary>
        /// Тест на выполнение задач при двух потоках в пуле.
        /// </summary>
        [TestMethod()]
        public void ExecuteTest_TwoPoolNoPriority()
        {
            FixedThreadPool pool = new FixedThreadPool(2);
            DateTime started1 = DateTime.MinValue;
            DateTime started2 = DateTime.MinValue;
            DateTime started3 = DateTime.MinValue;

            DateTime t0 = DateTime.Now;
            TimeSpan t1, t2, t3;
            pool.Execute(() => { started1 = DateTime.Now; Console.WriteLine("Task 1"); Thread.Sleep(1000); });
            pool.Execute(() => { started2 = DateTime.Now; Console.WriteLine("Task 2"); Thread.Sleep(1000); });

            Thread.Sleep(2500);

            pool.Execute(() => { started3 = DateTime.Now; Thread.Sleep(100); Console.WriteLine("Task 3"); });

            Thread.Sleep(500);

            t1 = started1 - t0;
            t2 = started2 - t0;
            t3 = started3 - t0;

            TestConditions result = new TestConditions();

            result.Add(() => 0 < t1.TotalMilliseconds && t1.TotalMilliseconds < 500,
                () => string.Format("t1 = {0} != (0; 500)", t1.TotalMilliseconds));

            result.Add(() => 0 < t2.TotalMilliseconds && t2.TotalMilliseconds < 500,
                () => string.Format("t2 = {0} != (0; 500)", t2.TotalMilliseconds));

            result.Add(() => 2500 < t3.TotalMilliseconds && t3.TotalMilliseconds < 3000,
                () => string.Format("t3 = {0} != (2500; 3000)", t3.TotalMilliseconds));

            result.Print();

            Assert.IsTrue(result.Calculate());
        }

        public struct TaskInfo
        {
            public TaskInfo(int id, TaskPriorityEx priority, DateTime time)
            {
                Id = id;
                Priority = priority;
                Time = time;
            }
            public int Id { get; private set; }

            public TaskPriorityEx Priority { get; private set; }

            public DateTime Time { get; private set; }
        }

        [TestMethod()]
        public void ExecuteTest_OnePoolWithPriority()
        {
            int poolCount = 5;

            FixedThreadPool pool = new FixedThreadPool(poolCount);

            Collection<TaskInfo> infoInit = new Collection<TaskInfo>();
            Collection<TaskInfo> infoAfter = new Collection<TaskInfo>();

            Random rnd = new Random();
            int pr;

            int attemps = 100;
            for (int i = 0; i < attemps; i++)
            {
                TaskPriorityEx priority;
                pr = rnd.Next(0, 3);

                switch (pr)
                {
                    case 0:
                        priority = TaskPriorityEx.LOW;
                        break;
                    case 1:
                        priority = TaskPriorityEx.NORMAL;
                        break;
                    case 2:
                        priority = TaskPriorityEx.HIGH;
                        break;
                    default:
                        priority = TaskPriorityEx.LOW;
                        break;
                }

                int copyI = i;
                infoInit.Add(new TaskInfo(copyI, priority, DateTime.Now));
                pool.Execute(
                    () =>
                        {
                            infoAfter.Add(new TaskInfo(copyI, priority, DateTime.Now));
                            Console.WriteLine(string.Format("Task {0} {1}", copyI, priority));
                            Thread.Sleep(30);
                        },
                        priority);
            }

            Thread.Sleep((int)(attemps * 30 / (double)poolCount) + 100);

            TestConditions result = new TestConditions();

            int noNormalCount = 0;
            int normalStep = 0;
            for (int i = 0; i < attemps; i++)
            {
                int copyI = i;
                if (i < poolCount)
                {
                    IEnumerable<TaskInfo> infors = infoInit.Where(item => item.Id == infoAfter[i].Id);
                    if (infors.Count() > 0)
                        infoInit.Remove(infors.First());
                    else
                        result.Add(() => false, () => string.Format("{0} is bad", copyI));
                }
                else
                {
                    IEnumerable<TaskInfo> infors = infoInit.Where(item => item.Id == infoAfter[i].Id);
                    if (infors.Count() > 0)
                    {
                        if (infors.First().Priority == TaskPriorityEx.NORMAL)
                            normalStep = i;
                        else
                            noNormalCount++;
                        
                        if (noNormalCount > 5)
                        {
                            IEnumerable<TaskInfo> normals = infoInit.Where(item => item.Priority == TaskPriorityEx.NORMAL);
                            if (normals.Count() > 0)
                                result.Add(() => false, () => string.Format("{0} - doesn't exist NORMAL", copyI));
                        }

                        infoInit.Remove(infors.First());
                    }
                    else
                        result.Add(() => false, () => string.Format("{0} - is bad", copyI));
                }
            }

            result.Print();
            Assert.IsTrue(result.Calculate());
        }
    }
}