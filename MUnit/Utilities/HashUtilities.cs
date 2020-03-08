// <copyright file="HashUtilities.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace MUnit.Utilities
{
    public static class HashUtilities
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5350:Do Not Use Weak Cryptographic Algorithms", Justification = "Only for test id.")]
        public static Guid GuidFromString(string data)
        {
            Debug.Assert(data != null, "data should not be null");

            using (HashAlgorithm provider = SHA1.Create())
            {
                byte[] hash = provider.ComputeHash(System.Text.Encoding.Unicode.GetBytes(data));

                // Guid is always 16 bytes
                Debug.Assert(Guid.Empty.ToByteArray().Length == 16, "Expected Guid to be 16 bytes");

                byte[] toGuid = new byte[16];
                Array.Copy(hash, toGuid, 16);

                return new Guid(toGuid);
            }
        }

        public static Guid GuidForTestCycleID(string source, string fullyQualifiedName)
        {
            return GuidFromString(string.Concat(source, fullyQualifiedName));
        }
    }
}
