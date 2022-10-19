using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using StereoKit;

namespace Stereokit.Azure.SpatialAnchors.Services
{
    public class MockSpatialAnchorsWrapper : ISpatialAnchorsWrapper
    {
        public event EventHandler<SpatialAnchorLocatedEventArgs> SpatialAnchorLocated;
        public event EventHandler<AsaSessionUpdateEventArgs> ASASessionUpdate;
        public event EventHandler<AsaLogEventArgs> ASALogEvent;

        private bool isSessionStarted;
        private Random random = new Random();

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

        public async void StartLocatingAnchors(int maxResults = 5, float distance = 10)
        {
            for (var i = 0; i < maxResults; i++)
            {
                var arg = new SpatialAnchorLocatedEventArgs()
                {
                    Anchor = new Pose((float) this.random.NextDouble(), (float) this.random.NextDouble(),
                        (float) this.random.NextDouble(), new Quat()),
                    Id = Guid.NewGuid().ToString()
                };
                
            }
        }

        public void StartLocatingAnchors(string[] anchorIds)
        {
            throw new NotImplementedException();
        }

        public void StopLocatingAnchors()
        {
            throw new NotImplementedException();
        }

        public void CreateCloudAnchor(Vector3 position)
        {
            throw new NotImplementedException();
        }

        public void CreateCloudAnchor(Pose pose)
        {
            throw new NotImplementedException();
        }

        private async Task LogEventEmitter()
        {

            while (this.isSessionStarted)
            {
                var logEvent = new AsaLogEventArgs
                {
                    LogLevel = this.random.Next(0, 3) % 2 == 0 ? LogLevel.Diagnostic : LogLevel.Error,
                    LogMessage = "log message"
                };

                ASALogEvent?.Invoke(this, logEvent);

                var delay = this.random.Next(2000, 10000);

                await Task.Delay(delay);
            }
        }
    }
}
