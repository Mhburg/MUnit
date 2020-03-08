using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MUnit.Transport;

namespace MUnit.Test.Transport
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestReceivingMessage
    {
        static TCPClientWorker _clientWorker;
        static byte[] firstHaflEOFBinary;
        static byte[] secondHaflEOFBinary;
        static MethodInfo _isEOF = typeof(TCPTransporter).GetMethod("IsEOF", BindingFlags.NonPublic | BindingFlags.Instance);

        [ClassInitialize]
        public static void ClassInitialized(TestContext testContext)
        {
            _clientWorker = new TCPClientWorker();

            firstHaflEOFBinary = AssemblyPrep.EOFBinary.Take(AssemblyPrep.EOFBinary.Length / 2).ToArray();
            secondHaflEOFBinary = AssemblyPrep.EOFBinary.Skip(AssemblyPrep.EOFBinary.Length / 2).ToArray();
        }

        [DataTestMethod]
        [DynamicData("MessageData", DynamicDataSourceType.Method)]
        public void TestIsEOF(byte[] buffer, int offset, int length, bool expectedBoolean, int expectedIndex)
        {
            object[] parameters = new object[] { buffer, offset, length, -1 };
            bool returnedBoolean = (bool)_isEOF.Invoke(_clientWorker, parameters);

            Assert.AreEqual(expectedBoolean, returnedBoolean);
            Assert.AreEqual(expectedIndex, parameters[3]);
        }

        public static IEnumerable<object[]> MessageData()
        {
            byte[] combo;
            yield return new object[] { AssemblyPrep.EOFBinary, 0, AssemblyPrep.EOFBinary.Length, true, -1 };
            yield return new object[] { firstHaflEOFBinary, 0, firstHaflEOFBinary.Length, false, -1 };
            yield return new object[] { secondHaflEOFBinary, 0, secondHaflEOFBinary.Length, false, -1 };

            combo = firstHaflEOFBinary.Concat(AssemblyPrep.EOFBinary).ToArray();
            yield return new object[] { combo, 0, combo.Length, true, firstHaflEOFBinary.Length - 1 };

            combo = secondHaflEOFBinary.Concat(AssemblyPrep.EOFBinary).ToArray();
            yield return new object[] { combo, 0, combo.Length, true, secondHaflEOFBinary.Length - 1 };
            yield return new object[] { combo, secondHaflEOFBinary.Length, combo.Length - secondHaflEOFBinary.Length, true, secondHaflEOFBinary.Length - 1 };
        }
    }
}
