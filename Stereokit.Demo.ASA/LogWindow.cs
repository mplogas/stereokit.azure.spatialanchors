using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StereoKit;
using Stereokit.Demo.ASA.Services;
using StereoKit.Framework;

namespace Stereokit.Demo.ASA
{
    internal class LogWindow : IStepper
    {
        private readonly ISpatialAnchorsWrapper service;
        private Pose windowPose;
        private string log = string.Empty;

        public LogWindow(ISpatialAnchorsWrapper service)
        {
            this.service = service;
            this.service.ASALogEvent += ServiceOnASALogEvent;
            windowPose = new Pose(0.2f, 0.3f, -0.4f, Quat.LookDir(-1, 0, 1));
        }

        private void ServiceOnASALogEvent(object sender, AsaLogEventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(DateTime.UtcNow.ToLongTimeString());
            sb.Append(" - ");
            sb.Append(e.LogLevel.ToString());
            sb.Append(Environment.NewLine);
            sb.AppendLine(e.LogMessage);
            sb.Append(Environment.NewLine);

            this.log += sb.ToString();
        }

        public bool Initialize()
        {
            this.log = string.Empty; //allows resetting the log window
            return true;
        }

        public void Step()
        {

            //UI.WindowBegin("Log", ref windowPose, UIWin.Body);
            UI.WindowBegin("Log", ref windowPose, new Vec2(0.3f, 0.1f));
            UI.Text(this.log);
            UI.WindowEnd();
        }

        public void Shutdown()
        {
        }

        public bool Enabled => true;
    }
}
