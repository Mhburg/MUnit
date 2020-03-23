using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace ConsoleApp1
{
    [DataContract]
    public class MockClass
    {
        [DataMember]
        public object Array { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            byte[] bytes;

            MockClass mockClass = new MockClass() { Array = new List<int>() { 1, 2 } };
            MockClass deserialize;

            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(MockClass), new List<Type>() { typeof(List<int>) });
            using (MemoryStream stream = new MemoryStream())
            {
                XmlDictionaryWriter binaryDictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream);
                dataContractSerializer.WriteObject(binaryDictionaryWriter, mockClass);
                binaryDictionaryWriter.Flush();

                binaryDictionaryWriter.Close();
                bytes = stream.ToArray();
            }

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                using (XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(stream, new XmlDictionaryReaderQuotas()))
                {
                    DataContractSerializer ser = new DataContractSerializer(typeof(MockClass), new List<Type>() { typeof(List<int>) });

                     deserialize = (MockClass)ser.ReadObject(reader);
                }
            }

            Console.WriteLine(deserialize);
        }
    }
}
