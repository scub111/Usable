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
            trigger.Calculate(true);
            Assert.IsTrue(trigger.Calculate(false));
        }

        [TestMethod()]
        public void CalculateRet_Int()
        {
            TriggerT<int> trigger = new TriggerT<int>();
            trigger.Calculate(6);
            Assert.IsTrue(trigger.Calculate(5));
        }
    }
}