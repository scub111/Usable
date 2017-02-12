using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Usable.Tests
{
    [TestClass()]
    public class TriggerTTests
    {
        [TestMethod()]
        public void CalculateRet_Bool()
        {
            TriggerT<bool> trigger = new TriggerT<bool>();
            trigger.CalculateRet(true);
            Assert.IsTrue(trigger.CalculateRet(false));
        }

        [TestMethod()]
        public void CalculateRet_Int()
        {
            TriggerT<int> trigger = new TriggerT<int>();
            trigger.CalculateRet(6);
            Assert.IsTrue(trigger.CalculateRet(5));
        }
    }
}