using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace Usable.Tests
{
    [TestClass()]
    public class ThreadTimerTests
    {
        [TestMethod()]
        public void Run_WorkCount_Empty()
        {
            int realWorkCount = 0;
            EventHandler workChangedHandler = (sender, e) =>
            {
                realWorkCount++;
            };

            ThreadTimer threadTimer = new ThreadTimer();
            threadTimer.Period = 100;
            threadTimer.Delay = 10;
            threadTimer.WorkChanged += workChangedHandler;
            threadTimer.Run();

            Thread.Sleep(1000);
            Console.WriteLine(string.Format("realWorkCount = {0}; threadTimer.WorkCount = {1}, cycleCount = {2}",
                                realWorkCount, threadTimer.WorkCount, threadTimer.CycleCount));
            Assert.IsTrue(realWorkCount != 0 && 
                realWorkCount == threadTimer.WorkCount && 
                9 <= realWorkCount && realWorkCount <= 11 &&
                threadTimer.CycleCount >= 60);
        }

        [TestMethod()]
        public void Run_WorkCount_Busy()
        {
            int realWorkCount = 0;
            EventHandler workChangedHandler = (sender, e) =>
            {
                realWorkCount++;
                Thread.Sleep(50);
            };

            ThreadTimer threadTimer = new ThreadTimer();
            threadTimer.Period = 100;
            threadTimer.Delay = 10;
            threadTimer.WorkChanged += workChangedHandler;
            threadTimer.Run();

            Thread.Sleep(1000);
            Console.WriteLine(string.Format("realWorkCount = {0}; threadTimer.WorkCount = {1}, cycleCount = {2}",
                                realWorkCount, threadTimer.WorkCount, threadTimer.CycleCount));
            Assert.IsTrue(realWorkCount != 0 &&
                realWorkCount == threadTimer.WorkCount &&
                9 <= realWorkCount && realWorkCount <= 11 &&
                threadTimer.CycleCount >= 20);
        }

        [TestMethod()]
        public void Run_WorkCount_ExtraBusy()
        {
            int realWorkCount = 0;
            EventHandler workChangedHandler = (sender, e) =>
            {
                realWorkCount++;
                Thread.Sleep(200);
            };

            ThreadTimer threadTimer = new ThreadTimer();
            threadTimer.Period = 100;
            threadTimer.Delay = 10;
            threadTimer.WorkChanged += workChangedHandler;
            threadTimer.Run();

            Thread.Sleep(1000);
            Console.WriteLine(string.Format("realWorkCount = {0}; threadTimer.WorkCount = {1}, cycleCount = {2}",
                                realWorkCount, threadTimer.WorkCount, threadTimer.CycleCount));
            Assert.IsTrue(realWorkCount > 4 && 
                threadTimer.WorkCount > 4 && 
                threadTimer.CycleCount > 4);
        }

        [TestMethod()]
        public void Run_WorkCount_ExtraInterfaceBusy()
        {
            int realWorkCount = 0;
            EventHandler workChangedHandler = (sender, e) =>
            {
                realWorkCount++;
                Thread.Sleep(200);
            };

            int realInterfaceCount = 0;
            EventHandler interfaceChangedHandler = (sender, e) =>
            {
                realInterfaceCount++;
                Thread.Sleep(1500);
            };

            ThreadTimer threadTimer = new ThreadTimer();
            threadTimer.Period = 10;
            threadTimer.Delay = 1;
            threadTimer.WorkChanged += workChangedHandler;
            threadTimer.InterfaceChanged += interfaceChangedHandler;
            threadTimer.Run();

            Thread.Sleep(10000);
            Console.WriteLine(string.Format("realWorkCount = {0}; threadTimer.WorkCount = {1}, cycleCount = {2}, realInterfaceCount = {3}",
                                realWorkCount, threadTimer.WorkCount, threadTimer.CycleCount, realInterfaceCount));
            Assert.IsTrue(realWorkCount > 40 &&
                threadTimer.WorkCount > 40 &&
                threadTimer.CycleCount > 40 &&
                realInterfaceCount < 8);
        }
    }
}