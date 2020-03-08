using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using MUnit.Engine;
using MUnit.Transport;
using MUnit.Utilities;

using UTF = MUnit.Framework;

namespace MUnit.TestAdapter
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class AssemblyPrep
    {
        public static string Source = Assembly.GetExecutingAssembly().Location;
    }
}
