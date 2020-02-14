// <copyright file="SerializeUtilites.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AtlasModTestAdapter.Utilities
{
    public static class SerializeUtilites
    {
        /// <summary>
        /// Serizlize object using <see cref="XmlDictionaryWriter.CreateBinaryWriter(Stream)"/>.
        /// </summary>
        /// <typeparam name="T"> Type of object that will be serialized.</typeparam>
        /// <param name="entity"> Object to be serialized.</param>
        /// <returns>Byte array with serialized data.</returns>
        public static byte[] BinarySerialize<T>(T entity)
        {
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                XmlDictionaryWriter binaryDictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream);
                dataContractSerializer.WriteObject(binaryDictionaryWriter, entity);
                binaryDictionaryWriter.Flush();
                stream.Position = 0;
                binaryDictionaryWriter.Dispose();
                return stream.ToArray();
            }
        }
    }
}
