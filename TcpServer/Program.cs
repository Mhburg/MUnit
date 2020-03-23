using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;
using System.IO;
using DataContractSerializerExample;
using System.Xml;

namespace TcpServer
{
    class Program
    {
        static ManualResetEvent resetEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            Socket listener = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], 10001));
                listener.Listen(10);


                AcceptAsync(listener);
                resetEvent.WaitOne();
                Console.WriteLine("Wait finished.");

                //Socket worker = listener.Accept();

                //byte[] buffer = new byte[1024];

                //int byteReceived = worker.Receive(buffer);
                //MemoryStream memoryStream = new MemoryStream();
                //memoryStream.Write(buffer, 0, byteReceived);
                //memoryStream.Position = 0;
                //Console.WriteLine("ByteReceived: {0}", byteReceived);
                ////Console.WriteLine("string received: {0}", System.Text.Encoding.UTF8.GetString(buffer));
                //XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(memoryStream, new XmlDictionaryReaderQuotas());

                //DataContractSerializer ser =
                //        new DataContractSerializer(typeof(List<Person>));

                //List<Person> person = ser.ReadObject(reader) as List<Person>;
                //Console.WriteLine("Person read: {0} {1}, ID: {2}", person[0].FirstName, person[0].LastName, person[0].ID);
                //Console.WriteLine("Person read: {0} {1}, ID: {2}", person[1].FirstName, person[1].LastName, person[1].ID);
            }
            catch (SerializationException serExc)
            {
                Console.WriteLine("Serialization Failed");
                Console.WriteLine(serExc.Message);
            }
            catch (Exception exc)
            {
                Console.WriteLine(
                "The serialization operation failed: {0} StackTrace: {1}",
                exc.Message, exc.StackTrace);
            }

            finally
            {
                listener.Dispose();
                Console.WriteLine("Press <Enter> to exit....");
                Console.ReadLine();
            }
        }

        static void AcceptAsync(Socket socket)
        {
            Console.WriteLine("Process ID: " + Process.GetCurrentProcess().Id);
            Console.WriteLine("Managed Thread ID: " + Thread.CurrentThread.ManagedThreadId);
            socket.BeginAccept(AcceptCallback, socket);
            Console.WriteLine("After BeginAccept.");

            for (int i = 0; i < 10; i++)
            {
                socket.BeginAccept(AcceptCallback, socket);
            }
        }

        static void AcceptCallback(IAsyncResult asyncResult)
        {
            Socket listener = (Socket)asyncResult.AsyncState;
            Socket handler = listener.EndAccept(asyncResult);
            Console.WriteLine("Process ID: " + Process.GetCurrentProcess().Id);
            Console.WriteLine("Managed Thread ID: " + Thread.CurrentThread.ManagedThreadId);

            resetEvent.Set();
            handler.Close();


            while (true)
            {

            }
        }

        static async void AwaitOp()
        {
            Console.WriteLine("Process ID: " + Process.GetCurrentProcess().Id);
            Console.WriteLine("Managed Thread ID: " + Thread.CurrentThread.ManagedThreadId);
            await Waitop();
            Console.WriteLine("Following await operator");
            Console.WriteLine("Process ID: " + Process.GetCurrentProcess().Id);
            Console.WriteLine("Managed Thread ID: " + Thread.CurrentThread.ManagedThreadId);
            Console.ReadLine();
        }

        static async Task<bool> Waitop()
        {
            await Task.Run(() => Thread.Sleep(2000));
            return true;
        }



        //[DataContract(Name = "Customer", Namespace = "http://www.contoso.com")]
        //class Person : IExtensibleDataObject
        //{
        //    [DataMember()]
        //    public string FirstName;
        //    [DataMember]
        //    public string LastName;
        //    [DataMember()]
        //    public int ID;

        //    public Person(string newfName, string newLName, int newID)
        //    {
        //        FirstName = newfName;
        //        LastName = newLName;
        //        ID = newID;
        //    }

        //    private ExtensionDataObject extensionData_Value;

        //    public ExtensionDataObject ExtensionData
        //    {
        //        get
        //        {
        //            return extensionData_Value;
        //        }
        //        set
        //        {
        //            extensionData_Value = value;
        //        }
        //    }
        //}
    }
}
