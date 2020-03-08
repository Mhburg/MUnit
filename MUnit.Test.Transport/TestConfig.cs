using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MUnit.Test.Transport
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestConfig
    {
        [TestMethod]
        public void RetrieveServerIPFromConfig()
        {
            byte[] bytes = Dns.GetHostAddresses(Dns.GetHostName())[0].GetAddressBytes();
            byte[] cache = Engine.Service.MUnitConfiguration.ServerIP.GetAddressBytes();

            Assert.AreEqual(bytes.Length, cache.Length);

            for (int i = 0; i < bytes.Length; i++)
            {
                Assert.AreEqual(bytes[i], cache[i]);
            }
        }
    }
}
