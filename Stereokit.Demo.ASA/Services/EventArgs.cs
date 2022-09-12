using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StereoKit;

namespace Stereokit.Azure.SpatialAnchors.Services
{
    internal class SpatialAnchorLocatedEventArgs : EventArgs
    {
        internal string Id;
        internal Pose Anchor;
    }

    internal class AsaSessionUpdateEventArgs : EventArgs
    {
        internal bool IsRunning;
    }

    internal class AsaLogEventArgs : EventArgs
    {
        internal LogLevel LogLevel = LogLevel.Error;
        internal string LogMessage = string.Empty;
    }
}
