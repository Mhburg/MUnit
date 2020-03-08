using System;
using MUnitTestAdapter;

namespace MUnit.TestAdapter
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    [TestClass]
    public class TestTestDiscoverer
    {
        private static MUnitTestDiscoverer _discoverer = new MUnitTestDiscoverer();

        [TestMethod]
        public void DiscoverTests()
        {
        }
    }
}
