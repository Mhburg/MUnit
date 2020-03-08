using MUnit.Engine;

namespace MUnit.Test.Engine
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestReflectionWorker
    {
        [TestMethod]
        public void TestIsValidClass()
        {
            Assert.IsTrue(new ReflectionHelper(PlatformService.GetServiceManager().ReflectionCache)
                .IsValidTestClass(typeof(MockTestClass1), AssemblyPrep.Logger));
        }
    }
}
