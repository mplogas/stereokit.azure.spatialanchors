using System;
using StereoKit;

namespace Stereokit.Demo.ASA.Services
{
    internal interface ISpatialAnchorsWrapper
    {
        event EventHandler<SpatialAnchorLocatedEventArgs> SpatialAnchorLocated;
        event EventHandler<AsaSessionUpdateEventArgs> ASASessionUpdate;
        event EventHandler<AsaLogEventArgs> ASALogEvent;
        void StartSession();
        void EndSession();
        void StartLocatingAnchors(int maxResults = 5, float distance = 10);
        void StartLocatingAnchors(string[] anchorIds);
        void StopLocatingAnchors();
        void CreateCloudAnchor(Pose pose);
    }
}