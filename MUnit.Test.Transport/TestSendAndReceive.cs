using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MUnit.Transport;

namespace MUnit.Test.Transport
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestSendAndReceive
    {
        TCPClientWorker _clientWorker;
        TCPServerWorker _serverWorker;

        public static IEnumerable<object[]> WireMessages
        {
            get
            {
                yield return new object[] { new WireMessage(WireMessageTypes.Request, CommandType.Cancel, null) };
                yield return new object[] { new WireMessage(WireMessageTypes.Reply, CommandType.RunTests, new List<int>() { 1, 2 }) };
                yield return new object[] { new WireMessage(WireMessageTypes.Reply, CommandType.RunTests, new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() }) };
                yield return new object[] { new WireMessage(WireMessageTypes.Request, CommandType.CallFunction, typeof(TestSendAndReceive).GetMethods()[0], typeof(TestSendAndReceive), (new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() }).OfType<object>().ToArray()) };
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _clientWorker = new TCPClientWorker();
            _serverWorker = new TCPServerWorker();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _clientWorker.Dispose();
            _serverWorker.Dispose();
        }

        [DataTestMethod]
        [DynamicData("WireMessages", DynamicDataSourceType.Property)]
        public void TestBinarySerializationReadAndWrite(WireMessage wireMessage)
        {
            byte[] bytes = SerializeHelper.BinarySerialize(wireMessage);
            WireMessage unWrap = SerializeHelper.BinaryRead<WireMessage>(new System.IO.MemoryStream(bytes));

            Assert.IsTrue(CompareWireMessage(wireMessage, unWrap));
        }

        [DataTestMethod]
        [DynamicData("WireMessages", DynamicDataSourceType.Property)]
        public void TestSendAndParse(WireMessage wireMessage)
        {
            Task task = Task.Run(() =>
            {
                while (!_serverWorker.MessageQueue.Any)
                {
                    _serverWorker.Start();
                }
                _serverWorker.Dispose();
            });
            _clientWorker.Start();
            _clientWorker.Send(wireMessage);
            task.Wait(5000);

            if (_serverWorker.MessageQueue.TryDequeue(out WireMessage message))
            {
                Assert.IsTrue(CompareWireMessage(message, wireMessage));
            }
            else
            {
                Assert.Fail("No asserting executed.");
            }
        }

        [TestMethod]
        public void TestServereStart()
        {
            Assert.AreEqual(TCPServerWorker.Status.Idle, _serverWorker.ServerStatus);

            _serverWorker.Start();
            Assert.AreEqual(TCPServerWorker.Status.Listening, _serverWorker.ServerStatus);

            _serverWorker.Start();
            Assert.AreEqual(TCPServerWorker.Status.Accepting, _serverWorker.ServerStatus);

            _clientWorker.Start();
            _clientWorker.Send(new WireMessage(WireMessageTypes.EndOfReply, CommandType.Cancel, null));
            Task.Delay(100).Wait();
            _serverWorker.Start();
            Assert.AreEqual(TCPServerWorker.Status.Receiving, _serverWorker.ServerStatus);
        }

        private bool CompareWireMessage(WireMessage a, WireMessage b)
        {
            return a.ID == b.ID
                && a.Type == b.Type
                && a.Command == b.Command;
        }
    }
}
