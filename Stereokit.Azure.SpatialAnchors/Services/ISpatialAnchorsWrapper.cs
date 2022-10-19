using System;
using System.Numerics;
using StereoKit;

namespace Stereokit.Azure.SpatialAnchors.Services
{
    public interface ISpatialAnchorsWrapper
    {
        event EventHandler<SpatialAnchorLocatedEventArgs> SpatialAnchorLocated;
        event EventHandler<AsaSessionUpdateEventArgs> ASASessionUpdate;
        event EventHandler<AsaLogEventArgs> ASALogEvent;
        void StartSession();
        void EndSession();
        void StartLocatingAnchors(int maxResults = 5, float distance = 10);
        void StartLocatingAnchors(string[] anchorIds);
        void StopLocatingAnchors();



        void CreateCloudAnchor(Vector3 position);
        void CreateCloudAnchor(Pose pose);
    }
}