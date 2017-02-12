using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace Usable.Tests
{
    [TestClass()]
    public class ThreadExTests
    {
        [TestMethod()]
        public void CallTimedOutMethodAsyncTest_FastAction()
        {
            bool isResponse = false;
            bool isDone = false;
            Action callBack = new Action(() => { isResponse = true; });
            ThreadEx.CallTimedOutMethodAsync(() => { Thread.Sleep(500); isDone = true; }, 1000, callBack);
            Thread.Sleep(1200);
            Assert.IsTrue(isResponse && isDone);
        }

        [TestMethod()]
        public void CallTimedOutMethodAsyncTest_SlowAction()
        {
            bool isResponse = false;
            bool isDone = false;
            Action callBack = new Action(() => { /*isResponse = true;*/ });
            ThreadEx.CallTimedOutMethodAsync(() => { Thread.Sleep(5000); isDone = true; }, 1000, callBack);
            Thread.Sleep(1200);
            Assert.IsFalse(isResponse && !isDone);
        }

        [TestMethod()]
        public void CallTimedOutMethodSyncTest_FastAction()
        {
            bool result = ThreadEx.CallTimedOutMethodSync(new Action(() => { Thread.Sleep(500); }), 1000);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void CallTimedOutMethodSyncTest_SlowAction()
        {
            bool result = ThreadEx.CallTimedOutMethodSync(new Action(() => { Thread.Sleep(5000); }), 1000);
            Assert.IsFalse(result);
        }
    }
}