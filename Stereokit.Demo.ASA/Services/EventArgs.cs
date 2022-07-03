using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StereoKit;

namespace Stereokit.Demo.ASA.Services
{
    internal class SpatialAnchorLocatedEventArgs : EventArgs
    {

    }

    internal class AsaSessionUpdateEventArgs : EventArgs
    {
        internal bool IsRunning;
    }

    internal class AsaLogEventArgs : EventArgs
    {
        internal LogLevel LogLevel;
        internal string LogMessage;
    }
}
