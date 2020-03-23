namespace DataContractSerializerExample
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    // You must apply a DataContractAttribute or SerializableAttribute
    // to a class to have it serialized by the DataContractSerializer.
    [DataContract]
    public class Person : IExtensibleDataObject
    {
        [DataMember]
        public string FirstName;
        [DataMember]
        public string LastName;
        [DataMember]
        public int ID;

        public Person(string newfName, string newLName, int newID)
        {
            FirstName = newfName;
            LastName = newLName;
            ID = newID;
        }

        private ExtensionDataObject extensionData_Value;

        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionData_Value;
            }
            set
            {
                extensionData_Value = value;
            }
        }
    }

    public sealed class Test
    {
        private Test() { }

        public static void Main()
        {
            try
            {
                OnlyConnect();
                //WriteObject();
                //ReadObject("DataContractSerializerExample.xml");
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
                //Console.WriteLine("Press <Enter> to exit....");
                Console.ReadLine();
            }
        }

        public static void OnlyConnect()
        {
            Socket sender = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], 10001));
            Console.WriteLine("Connected");
            Console.ReadLine();
            sender.Close();
        }

        public static void WriteObject()
        {
            MemoryStream stream = new MemoryStream();
            Socket sender = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Thread.Sleep(2000);
                Console.WriteLine(
                "Creating a Person object and serializing it.");
                Person p1 = new Person("Zighetti", "Barbara", 101);
                Person p2 = new Person("Zira", "David", 102);
                List<Person> list = new List<Person>() { p1, p2 };
                DataContractSerializer ser =
                        new DataContractSerializer(typeof(List<Person>));
                XmlDictionaryWriter binaryDictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream);
                ser.WriteObject(binaryDictionaryWriter, list);
                binaryDictionaryWriter.Flush();
                stream.Position = 0;
                XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateBinaryReader(stream, new XmlDictionaryReaderQuotas());
                List<Person> list2 = (List<Person>)ser.ReadObject(xmlDictionaryReader);

                sender.Connect(new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], 10000));

                stream.Position = 0;
                byte[] bytes = stream.ToArray();
                int byteSent = sender.Send(bytes);
                Console.WriteLine("Bytes Sent: {0}", byteSent);
                Console.WriteLine("String sent: {0}", System.Text.Encoding.UTF8.GetString(bytes));
            }
            catch (SocketException exp)
            {
                Console.WriteLine("SocketException: {0}", exp.Message + Environment.NewLine + exp.StackTrace);
            }
            catch (SerializationException exp)
            {
                Console.WriteLine("SerializationException: {0}", exp.Message + Environment.NewLine + exp.StackTrace);
            }
            finally
            {
                stream.Close();
                sender.Dispose();
            }
        }

        public static void ReadObject(string fileName)
        {
            Console.WriteLine("Deserializing an instance of the object.");
            FileStream fs = new FileStream(fileName,
            FileMode.Open);
            XmlDictionaryReader reader =
                XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
            DataContractSerializer ser = new DataContractSerializer(typeof(Person));

            // Deserialize the data and read it from the instance.
            Person deserializedPerson =
                (Person)ser.ReadObject(reader, true);
            reader.Close();
            fs.Close();
            Console.WriteLine(String.Format("{0} {1}, ID: {2}",
            deserializedPerson.FirstName, deserializedPerson.LastName,
            deserializedPerson.ID));
        }
    }
}