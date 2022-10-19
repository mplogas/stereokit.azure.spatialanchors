using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StereoKit;

namespace Stereokit.Azure.SpatialAnchors.Services
{
    public class SpatialAnchorLocatedEventArgs : EventArgs
    {
        public string Id;
        public Pose Anchor;
    }

    public class AsaSessionUpdateEventArgs : EventArgs
    {
        public bool IsRunning;
    }

    public class AsaLogEventArgs : EventArgs
    {
        public LogLevel LogLevel = LogLevel.Error;
        public string LogMessage = string.Empty;
    }
}
