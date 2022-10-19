using StereoKit.Framework;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stereokit.Azure.SpatialAnchors.Services;

namespace Stereokit.Azure.SpatialAnchors.Demo
{
    internal struct LogEntry
    {
        internal DateTime DateTime;
        internal LogLevel LogLevel;
        internal string LogMessage;
    }

    internal class LogWindow : IStepper
    {
        private Pose windowPose;
        private readonly ISpatialAnchorsWrapper service;
        private List<LogEntry> logEntries = new List<LogEntry>();

        public LogWindow(ISpatialAnchorsWrapper asaService)
        {
            this.service = asaService;
            this.service.ASALogEvent += ServiceOnASALogEvent;
            this.service.ASASessionUpdate += ServiceOnASASessionUpdate;
            windowPose = new Pose(0.2f, 0.3f, -0.4f, Quat.LookDir(-1, 0, 1));
        }

        private void ServiceOnASALogEvent(object sender, AsaLogEventArgs e)
        {
            this.logEntries.Add(new LogEntry {DateTime = DateTime.UtcNow, LogLevel = e.LogLevel, LogMessage = e.LogMessage});
        }

        private void ServiceOnASASessionUpdate(object sender, AsaSessionUpdateEventArgs e)
        {
            this.logEntries.Add(new LogEntry {DateTime = DateTime.UtcNow, LogLevel = LogLevel.Info, LogMessage = $"Status (isRunning): {e.IsRunning}"});
        }

        //private void LogEventReceived(LogLevel level, string text)
        //{
        //    this.logEntries.Add(new LogEntry { DateTime = DateTime.UtcNow, LogLevel = level, LogMessage = text });
        //}

        private string BuildLogMessage(LogEntry logEntry)
        {
            var sb = new StringBuilder();
            sb.Append(logEntry.DateTime.ToLongTimeString());
            sb.Append(" - ");
            sb.Append(logEntry.LogLevel.ToString());
            sb.Append(Environment.NewLine);
            sb.AppendLine(logEntry.LogMessage);
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        public bool Initialize()
        {
            //Log.Subscribe(LogEventReceived);
            return true;
        }

        public void Step()
        {
            UI.WindowBegin("Log", ref windowPose, new Vec2(0.3f, 0.1f));

            foreach (var entry in this.logEntries.OrderByDescending(e => e.DateTime).Take(15))
            {
                UI.Text(BuildLogMessage(entry));
            }

            UI.WindowEnd();
        }

        public void Shutdown()
        {
            //Log.Unsubscribe(LogEventReceived);
        }

        public bool Enabled => true;
    }
}
