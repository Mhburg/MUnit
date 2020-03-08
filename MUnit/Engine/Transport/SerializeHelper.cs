// <copyright file="SerializeHelper.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace MUnit.Transport
{
    /// <summary>
    /// Helper for serialization.
    /// </summary>
    internal static class SerializeHelper
    {
        /// <summary>
        /// Serizlize object using <see cref="XmlDictionaryWriter.CreateBinaryWriter(Stream)"/>.
        /// </summary>
        /// <typeparam name="T"> Type of object that will be serialized.</typeparam>
        /// <param name="entity"> Object to be serialized.</param>
        /// <param name="eof"> Serialized byte array of EOF marking. </param>
        /// <returns>Byte array with serialized data.</returns>
        public static byte[] BinarySerialize<T>(T entity, byte[] eof = null)
        {
            NetDataContractSerializer dataContractSerializer = new NetDataContractSerializer();
            using (MemoryStream stream = new MemoryStream())
            {
                XmlDictionaryWriter binaryDictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream);
                dataContractSerializer.WriteObject(binaryDictionaryWriter, entity);
                binaryDictionaryWriter.Flush();

                if (eof != null)
                    stream.Write(eof, 0, eof.Length);

                binaryDictionaryWriter.Close();
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserialize byte array serialized with <see cref="XmlDictionaryWriter.CreateBinaryWriter(Stream)"/>.
        /// </summary>
        /// <typeparam name="T"> Expected type of object which is serialized in <paramref name="stream"/>. </typeparam>
        /// <param name="stream"> <see cref="MemoryStream"/> that holds serialized data. </param>
        /// <returns> Returns a deserialized object of type <typeparamref name="T"/>. </returns>
        public static T BinaryRead<T>(MemoryStream stream)
        {
            using (XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(stream, new XmlDictionaryReaderQuotas()))
            {
                NetDataContractSerializer ser = new NetDataContractSerializer();

                return (T)ser.ReadObject(reader);
            }
        }
    }
}
