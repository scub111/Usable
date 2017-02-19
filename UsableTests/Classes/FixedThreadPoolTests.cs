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
            TestConditions result = new TestConditions();

            FixedThreadPool pool = new FixedThreadPool(1);
            DateTime started1 = DateTime.MinValue;
            DateTime started2 = DateTime.MinValue;
            DateTime started3 = DateTime.MinValue;

            DateTime t0 = DateTime.Now;
            TimeSpan t1, t2, t3;
            if (!pool.Execute(() => { started1 = DateTime.Now; Console.WriteLine("Task 1"); Thread.Sleep(1000); }))
                result.Add(() => false, () => "Void Execute return FALSE unexpectedly");

            if (!pool.Execute(() => { started2 = DateTime.Now; Console.WriteLine("Task 2"); Thread.Sleep(1000); }))
                result.Add(() => false, () => "Void Execute return FALSE unexpectedly");

            Thread.Sleep(2500);

            if (!pool.Execute(() => { started3 = DateTime.Now; Console.WriteLine("Task 3"); Thread.Sleep(100); }))
                result.Add(() => false, () => "Void Execute return FALSE unexpectedly");

            Thread.Sleep(500);

            t1 = started1 - t0;
            t2 = started2 - t0;
            t3 = started3 - t0;

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
            TestConditions result = new TestConditions();

            FixedThreadPool pool = new FixedThreadPool(2);
            DateTime started1 = DateTime.MinValue;
            DateTime started2 = DateTime.MinValue;
            DateTime started3 = DateTime.MinValue;

            DateTime t0 = DateTime.Now;
            TimeSpan t1, t2, t3;
            if (!pool.Execute(() => { started1 = DateTime.Now; Console.WriteLine("Task 1"); Thread.Sleep(1000); }))
                result.Add(() => false, () => "Void Execute return FALSE unexpectedly");

            if (!pool.Execute(() => { started2 = DateTime.Now; Console.WriteLine("Task 2"); Thread.Sleep(1000); }))
                result.Add(() => false, () => "Void Execute return FALSE unexpectedly");

            Thread.Sleep(2500);

            if (!pool.Execute(() => { started3 = DateTime.Now; Thread.Sleep(100); Console.WriteLine("Task 3"); }))
                result.Add(() => false, () => "Void Execute return FALSE unexpectedly");

            Thread.Sleep(500);

            t1 = started1 - t0;
            t2 = started2 - t0;
            t3 = started3 - t0;


            result.Add(() => 0 <= t1.TotalMilliseconds && t1.TotalMilliseconds < 500,
                () => string.Format("t1 = {0} != [0; 500)", t1.TotalMilliseconds));

            result.Add(() => 0 <= t2.TotalMilliseconds && t2.TotalMilliseconds < 500,
                () => string.Format("t2 = {0} != [0; 500)", t2.TotalMilliseconds));

            result.Add(() => 2500 < t3.TotalMilliseconds && t3.TotalMilliseconds < 3000,
                () => string.Format("t3 = {0} != (2500; 3000)", t3.TotalMilliseconds));

            result.Print();

            Assert.IsTrue(result.Calculate());
        }

        /// <summary>
        /// Структура сбора статистики выполнения задач.
        /// </summary>
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

        /// <summary>
        /// Метод тестрирования под различным количеством потоквов.
        /// </summary>
        /// <param name="poolCount"></param>
        public void ExecuteTest_AnyPoolWithPriority(int poolCount)
        {
            TestConditions result = new TestConditions();

            // перенос из стека в память.
            int poolCountCopy = (int)(object)poolCount;

            FixedThreadPool pool = new FixedThreadPool(poolCountCopy);

            Collection<TaskInfo> infoInit = new Collection<TaskInfo>();
            Collection<TaskInfo> infoAfter = new Collection<TaskInfo>();

            Random rnd = new Random();
            int pr;

            const int attemps = 100;
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
                if (!pool.Execute(
                    () =>
                    {
                        infoAfter.Add(new TaskInfo(copyI, priority, DateTime.Now));
                        Console.WriteLine(string.Format("Task {0} {1}", copyI, priority));
                        Thread.Sleep(30);
                    },
                        priority))
                    result.Add(() => false, () => "Void Execute return FALSE unexpectedly");

            }

            Thread.Sleep((int)(attemps * 30 / (double)poolCountCopy) + 1000);

            int noNormalCount = 0;
            int normalStep = 0;
            TriggerT<int> trgNormal = new TriggerT<int>();
            for (int i = 0; i < attemps; i++)
            {
                int copyI = i;
                if (i < poolCountCopy)
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
                        {
                            if (normalStep == 0)
                                noNormalCount++;
                        }

                        if (noNormalCount > 5)
                            result.Add(() => false, () => string.Format("{0} - NORMAL doesn't exist to long", copyI));

                        int normalStCp = normalStep;

                        if (normalStCp < 30)
                            if (trgNormal.Calculate(normalStep))
                            {
                                result.Add(() =>
                                {
                                    if (infoAfter[normalStCp + 1].Priority == TaskPriorityEx.HIGH &&
                                        infoAfter[normalStCp + 2].Priority == TaskPriorityEx.HIGH &&
                                        infoAfter[normalStCp + 3].Priority == TaskPriorityEx.HIGH &&
                                        infoAfter[normalStCp + 4].Priority == TaskPriorityEx.NORMAL)
                                        return true;
                                    else
                                        return false;
                                }, () => string.Format("{0} - Not accurate orders of HIGH and NORMAL", copyI));

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

        /// <summary>
        /// Тест порядка выполения задач с различными приоритетами и одним потоком в пуле.
        /// </summary>
        [TestMethod()]
        public void ExecuteTest_OnePoolWithPriority()
        {
            var poolCount = 1;
            ExecuteTest_AnyPoolWithPriority(poolCount);
        }

        /// <summary>
        /// Тест порядка выполения задач с различными приоритетами и четырьмя потоками в пуле.
        /// </summary>
        [TestMethod()]
        public void ExecuteTest_FourPoolWithPriority()
        {
            var poolCount = 4;
            ExecuteTest_AnyPoolWithPriority(poolCount);
        }

        /// <summary>
        /// Тест порядка выполения задач с различными приоритетами и десятью потоками в пуле.
        /// </summary>
        [TestMethod()]
        public void ExecuteTest_TenPoolWithPriority()
        {
            var poolCount = 4;
            ExecuteTest_AnyPoolWithPriority(poolCount);
        }

        /// <summary>
        /// Тест порядка выполения задач при остановки работы пула потоков при одно потоке в пуле.
        /// </summary>
        [TestMethod()]
        public void ExecuteTest_OnePoolWithStop()
        {
            TestConditions result = new TestConditions();

            FixedThreadPool pool = new FixedThreadPool(1);
            DateTime started1 = DateTime.MinValue;
            DateTime started2 = DateTime.MinValue;
            DateTime started3 = DateTime.MinValue;

            DateTime t0 = DateTime.Now;
            TimeSpan t1, t2, t3;
            if (!pool.Execute(() => { started1 = DateTime.Now; Console.WriteLine("Task 1"); Thread.Sleep(1000); }))
                result.Add(() => false, () => "Void Execute return FALSE unexpectedly");

            if (!pool.Execute(() => { started2 = DateTime.Now; Console.WriteLine("Task 2"); Thread.Sleep(1000); }))
                result.Add(() => false, () => "Void Execute return FALSE unexpectedly");

            pool.Stop();

            Thread.Sleep(2500);

            if (pool.Execute(() => { started3 = DateTime.Now; Console.WriteLine("Task 3"); Thread.Sleep(100); }))
                result.Add(() => false, () => "Void Execute return TRUE unexpectedly");

            Thread.Sleep(500);

            t1 = started1 - t0;
            t2 = started2 - t0;
            t3 = started3 - t0;

            result.Add(() => 0 <= t1.TotalMilliseconds && t1.TotalMilliseconds < 500,
                () => string.Format("t1 = {0} != [0; 500)", t1.TotalMilliseconds));

            result.Add(() => 500 < t2.TotalMilliseconds && t2.TotalMilliseconds < 1500,
                () => string.Format("t2 = {0} != (500; 1500)", t2.TotalMilliseconds));


            result.Print();

            Assert.IsTrue(result.Calculate());
        }

        /// <summary>
        /// Тест порядка выполения задач при остановки работы пула потоков при одно потоке в пуле.
        /// </summary>
        [TestMethod()]
        public void ExecuteTest_TwoPoolWithStop()
        {
            TestConditions result = new TestConditions();

            FixedThreadPool pool = new FixedThreadPool(2);
            DateTime started1 = DateTime.MinValue;
            DateTime started2 = DateTime.MinValue;
            DateTime started3 = DateTime.MinValue;

            DateTime t0 = DateTime.Now;
            TimeSpan t1, t2, t3;
            if (!pool.Execute(() => { started1 = DateTime.Now; Console.WriteLine("Task 1"); Thread.Sleep(1000); }))
                result.Add(() => false, () => "Void Execute return FALSE unexpectedly");

            if (!pool.Execute(() => { started2 = DateTime.Now; Console.WriteLine("Task 2"); Thread.Sleep(1000); }))
                result.Add(() => false, () => "Void Execute return FALSE unexpectedly");

            pool.Stop();

            Thread.Sleep(2500);

            if (pool.Execute(() => { started3 = DateTime.Now; Console.WriteLine("Task 3"); Thread.Sleep(100); }))
                result.Add(() => false, () => "Void Execute return TRUE unexpectedly");

            Thread.Sleep(500);

            t1 = started1 - t0;
            t2 = started2 - t0;
            t3 = started3 - t0;

            result.Add(() => 0 <= t1.TotalMilliseconds && t1.TotalMilliseconds < 500,
                () => string.Format("t1 = {0} != [0; 500)", t1.TotalMilliseconds));

            result.Add(() => 0 <= t2.TotalMilliseconds && t2.TotalMilliseconds < 500,
                () => string.Format("t2 = {0} != [0; 500)", t2.TotalMilliseconds));


            result.Print();

            Assert.IsTrue(result.Calculate());
        }
    }
}