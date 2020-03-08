using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUnit.Framework
{
    /// <summary>
    /// A cancellation token passed to test cycles.
    /// </summary>
    public class CancellationBit
    {
        /// <summary>
        /// Indicates if cancellation has been signaled.
        /// </summary>
        public volatile bool IsCancel = false;
    }
}
