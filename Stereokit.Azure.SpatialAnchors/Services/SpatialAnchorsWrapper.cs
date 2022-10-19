using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.SpatialAnchors;
using Windows.Perception.Spatial;
using StereoKit;

namespace Stereokit.Azure.SpatialAnchors.Services
{
    //max 1 watcher, max 35 anchors per watcher
    //https://docs.microsoft.com/en-us/azure/spatial-anchors/how-tos/create-locate-anchors-unity#locate-a-cloud-spatial-anchor

    public class SpatialAnchorsWrapper : ISpatialAnchorsWrapper
    {
        public event EventHandler<SpatialAnchorLocatedEventArgs> SpatialAnchorLocated;
        public event EventHandler<AsaSessionUpdateEventArgs> ASASessionUpdate;
        public event EventHandler<AsaLogEventArgs> ASALogEvent;

        private readonly CloudSpatialAnchorSession cloudSession;

        public SpatialAnchorsWrapper(string accountId, string accountKey, string domain)
        {
            this.cloudSession = new CloudSpatialAnchorSession();
            this.cloudSession.Configuration.AccountId = accountId;
            this.cloudSession.Configuration.AccountKey = accountKey;
            this.cloudSession.Configuration.AccountDomain = domain;

            this.cloudSession.LogLevel = SessionLogLevel.All; //show me what you got!
            //TODO: provide external event handlers
            this.cloudSession.OnLogDebug += CloudSessionOnOnLogDebug;
            this.cloudSession.Error += CloudSessionOnError;
            this.cloudSession.AnchorLocated += CloudSessionOnAnchorLocated;
            this.cloudSession.LocateAnchorsCompleted += CloudSessionOnLocateAnchorsCompleted;
            this.cloudSession.SessionUpdated += CloudSessionOnSessionUpdated;

            this.cloudSession.LocationProvider = BuildCoarseLocationProvider();
        }
        public void StartSession()
        {
            if (!HasActiveSession())
            {
                this.cloudSession.Start();
            }
        }

        public void EndSession()
        {
            if (HasActiveSession())
            {
                StopLocatingAnchors();
                this.cloudSession.Stop();
            }
        }

        public void StartLocatingAnchors(int maxResults = 5, float distance = 10)
        {
            //maxresults: 35
            StopLocatingAnchors();

            var deviceCriteria = new NearDeviceCriteria
            {
                DistanceInMeters = distance,
                MaxResultCount = maxResults
            };

            var criteria = new AnchorLocateCriteria
            {
                NearDevice = deviceCriteria
            };
            this.cloudSession.CreateWatcher(criteria);
        }

        public void StartLocatingAnchors(string[] anchorIds)
        {
            StopLocatingAnchors();

            // criteria in detail: https://docs.microsoft.com/en-us/azure/spatial-anchors/concepts/anchor-locate-strategy
            var criteria = new AnchorLocateCriteria
            {
                Identifiers = anchorIds
            };
            this.cloudSession.CreateWatcher(criteria);
        }

        public void StopLocatingAnchors()
        {
            //just one watchersession is supported, despite the name of the API
            //but this way we make sure we'Re not running into an NRE
           foreach (var activeWatcher in this.cloudSession.GetActiveWatchers())
           {
               activeWatcher.Stop();
            }
        }

        public void CreateCloudAnchor(Pose pose)
        {
            var locator = SpatialLocator.GetDefault();
            var reference = locator.CreateStationaryFrameOfReferenceAtCurrentLocation();
            var anchor = SpatialAnchor.TryCreateRelativeTo(reference.CoordinateSystem);
            var cloudAnchor = new CloudSpatialAnchor
            {
                LocalAnchor = anchor
            };

            //TODO: does this work?
            SpatialAnchor.TryCreateRelativeTo(reference.CoordinateSystem, pose.position);
        }

        public void CreateCloudAnchor(Vector3 position)
        {
            var locator = SpatialLocator.GetDefault();
            var reference = locator.CreateStationaryFrameOfReferenceAtCurrentLocation();
            var anchor = SpatialAnchor.TryCreateRelativeTo(reference.CoordinateSystem);
            var cloudAnchor = new CloudSpatialAnchor
            {
                LocalAnchor = anchor
            };

            //TODO: does this work?
            SpatialAnchor.TryCreateRelativeTo(reference.CoordinateSystem, position);
        }


        private PlatformLocationProvider BuildCoarseLocationProvider()
        {
            //https://docs.microsoft.com/en-us/azure/spatial-anchors/concepts/coarse-reloc#platform-availability
            //if your lens has been paired with an external GPS, hook to the UpdateSensorFingerprintRequired event and submit geolocation
            return new PlatformLocationProvider
            {
                Sensors =
                {
                    BluetoothEnabled = true,
                    WifiEnabled = true
                }
            };
        }


        
        private bool HasActiveSession()
        {
            return !string.IsNullOrEmpty(this.cloudSession.SessionId);
        }

        private void CloudSessionOnSessionUpdated(object sender, SessionUpdatedEventArgs args)
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                $"{nameof(args.Status.RecommendedForCreateProgress)}: {args.Status.RecommendedForCreateProgress}");
            sb.AppendLine($"{nameof(args.Status.ReadyForCreateProgress)}: {args.Status.ReadyForCreateProgress}");
            sb.AppendLine($"{nameof(args.Status.SessionCreateHash)}: {args.Status.SessionCreateHash}");
            sb.AppendLine($"{nameof(args.Status.SessionLocateHash)}: {args.Status.SessionLocateHash}");
            sb.AppendLine($"{args.Status.UserFeedback}: {args.Status.UserFeedback}");

            Log.Write(LogLevel.Diagnostic, sb.ToString());
        }

        private void CloudSessionOnLocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
        {
            var message = args.Cancelled ? $"{args.Watcher.Identifier} cancelled" : $"{args.Watcher.Identifier} stopped";

            Log.Write(LogLevel.Diagnostic, message);
        }

        private void CloudSessionOnAnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            var message = $"Anchor {args.Identifier} returned {args.Status}";
            Log.Write(LogLevel.Diagnostic, message);

            //https://stereokit.net/Pages/Reference/World/FromPerceptionAnchor.html
            switch (args.Status)
            {
                case LocateAnchorStatus.Located:
                    var csa = args.Anchor;
                    if (csa?.LocalAnchor != null)
                    {
                        if (World.FromPerceptionAnchor(csa.LocalAnchor, out var localAnchor))
                        {
                            Log.Write(LogLevel.Info, $"anchor {csa.Identifier} position X:{localAnchor.position.x} Y:{localAnchor.position.y} Z:{localAnchor.position.z}");
                            SpatialAnchorLocated?.Invoke(this, new SpatialAnchorLocatedEventArgs { Anchor = localAnchor, Id = csa.Identifier });
                        }
                    }
                    break;
                //TODO: nottracked scenario for locating anchors by id
                default:
                    break;
            }

        }

        private void CloudSessionOnError(object sender, SessionErrorEventArgs args)
        {
            var eventArgs = new AsaLogEventArgs
            {
                LogLevel = LogLevel.Error,
                LogMessage = $"Error code {args.ErrorCode.ToString()}: {args.ErrorMessage}"
            };

            Log.Write(LogLevel.Error, eventArgs.LogMessage);
            ASALogEvent?.Invoke(this, eventArgs);
        }

        private void CloudSessionOnOnLogDebug(object sender, OnLogDebugEventArgs args)
        {
            var eventArgs = new AsaLogEventArgs
            {
                LogLevel = LogLevel.Diagnostic,
                LogMessage = args.Message
            };

            Log.Write(LogLevel.Diagnostic, eventArgs.LogMessage);
            ASALogEvent?.Invoke(this, eventArgs);
        }
    }
}
