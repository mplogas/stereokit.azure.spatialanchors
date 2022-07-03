using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StereoKit;

namespace Stereokit.Demo.ASA.Services
{
    internal class MockSpatialAnchorsWrapper : ISpatialAnchorsWrapper
    {
        public event EventHandler<SpatialAnchorLocatedEventArgs> SpatialAnchorLocated;
        public event EventHandler<AsaSessionUpdateEventArgs> ASASessionUpdate;
        public event EventHandler<AsaLogEventArgs> ASALogEvent;

        private bool isSessionStarted;

        public MockSpatialAnchorsWrapper()
        {
            
        }


        public void StartSession()
        {
            if (this.isSessionStarted) return;
            this.isSessionStarted = true;
            ASASessionUpdate?.Invoke(this, new AsaSessionUpdateEventArgs {IsRunning = isSessionStarted});

            Task.Run(LogEventEmitter);
        }

        public void EndSession()
        {
            if (!this.isSessionStarted) return;
            this.isSessionStarted = false;
            ASASessionUpdate?.Invoke(this, new AsaSessionUpdateEventArgs { IsRunning = isSessionStarted });
        }

        public void StartLocatingAnchors(int maxResults = 5, float distance = 10)
        {
            throw new NotImplementedException();
        }

        public void StartLocatingAnchors(string[] anchorIds)
        {
            throw new NotImplementedException();
        }

        public void StopLocatingAnchors()
        {
            throw new NotImplementedException();
        }

        public void CreateCloudAnchor(Pose pose)
        {
            throw new NotImplementedException();
        }
        

        private async Task LogEventEmitter()
        {
            var r = new Random();

            while (isSessionStarted)
            {
                var logEvent = new AsaLogEventArgs
                {
                    LogLevel = r.Next(0, 3) % 2 == 0 ? LogLevel.Diagnostic : LogLevel.Error,
                    LogMessage = "log message"
                };

                ASALogEvent?.Invoke(this, logEvent);

                var delay = r.Next(2000, 10000);

                await Task.Delay(delay);
            }
        }
    }
}
