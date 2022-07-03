﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.SpatialAnchors;
using Windows.Perception.Spatial;
using StereoKit;

namespace Stereokit.Demo.ASA.Services
{
    internal class SpatialAnchorsWrapper : ISpatialAnchorsWrapper
    {
        public event EventHandler<SpatialAnchorLocatedEventArgs> SpatialAnchorLocated;
        public event EventHandler<AsaSessionUpdateEventArgs> ASASessionUpdate;
        public event EventHandler<AsaLogEventArgs> ASALogEvent;

        private readonly CloudSpatialAnchorSession cloudSession;
        private CloudSpatialAnchorWatcher cloudSpatialAnchorWatcher;

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
            //just one watchersession is currently supported TODO: reach out to patrick & confirm
            var activeWatchers = this.cloudSession.GetActiveWatchers();
            foreach (var watcher in activeWatchers)
            {
                watcher.Stop();
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
            throw new NotImplementedException();
        }

        private void CloudSessionOnLocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void CloudSessionOnAnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            //https://stereokit.net/Pages/Reference/World/FromPerceptionAnchor.html

            throw new NotImplementedException();
        }

        private void CloudSessionOnError(object sender, SessionErrorEventArgs args)
        {
            var eventArgs = new AsaLogEventArgs
            {
                LogLevel = LogLevel.Error,
                LogMessage = $"Error code {args.ErrorCode.ToString()}: {args.ErrorMessage}"
            };

            ASALogEvent?.Invoke(this, eventArgs);
        }

        private void CloudSessionOnOnLogDebug(object sender, OnLogDebugEventArgs args)
        {
            var eventArgs = new AsaLogEventArgs
            {
                LogLevel = LogLevel.Diagnostic,
                LogMessage = args.Message
            };

            ASALogEvent?.Invoke(this, eventArgs);
        }
    }
}
