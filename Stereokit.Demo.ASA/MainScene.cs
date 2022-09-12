using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StereoKit;
using Stereokit.Azure.SpatialAnchors.Services;
using StereoKit.Framework;

namespace Stereokit.Azure.SpatialAnchors
{
    internal class MainScene : IStepper
    {
        private readonly ISpatialAnchorsWrapper service;
        private Pose windowPose;
        private string sessionState = "unknown";
        private Dictionary<string, Pose> anchors = new Dictionary<string, Pose>();
        private Model model = Model.FromFile("retro_ufo.glb");
        private string watcherState = "stopped";

        public MainScene(ISpatialAnchorsWrapper asaService)
        {
            this.service = asaService;
            this.service.ASASessionUpdate += ServiceOnASASessionUpdate;
            this.service.SpatialAnchorLocated += ServiceOnSpatialAnchorLocated;
            this.windowPose = new Pose(-0.3f, 0.3f, -0.4f, Quat.LookDir(1, 0, 1));
        }


        public bool Initialize()
        {
            return true;
        }

        public void Step()
        {
            UI.WindowBegin("Control", ref windowPose);
            UI.Text($"state: {this.sessionState}", TextAlign.TopRight); 
            if (UI.Button("Start"))
            {
                this.service.StartSession();
                this.sessionState = "started";
            }
            UI.SameLine();
            if (UI.Button("Stop"))
            {
                this.service.EndSession();
                this.sessionState = "stopped";
            }

            UI.Text($"watcher: {this.watcherState}", TextAlign.XRight);
            if (UI.Button("Start"))
            {
                this.service.StartLocatingAnchors();
                this.watcherState = "started";
            }
            UI.SameLine();
            if (UI.Button("Stop"))
            {
                this.service.StopLocatingAnchors();
                this.watcherState = "stopped";
            }
            if (UI.Button("Quit"))
            {
                this.service.EndSession();
                SK.Quit();
            }
            UI.WindowEnd();

            foreach (var anchor in anchors)
            {
                DrawModel(anchor.Value);
            }
        }

        private void DrawModel(Pose anchor)
        {
            //taken from https://stereokit.net/Pages/Reference/Model.html
            var scale = 0.1f;
            UI.HandleBegin("Anchor Handle", ref anchor, this.model.Bounds * scale);
            model.Draw(Matrix.S(scale));
            Mesh.Cube.Draw(Material.UIBox, Matrix.TS(this.model.Bounds.center * scale, this.model.Bounds.dimensions * scale));
            UI.HandleEnd();
        }

        public void Shutdown()
        {
        }

        public bool Enabled => true;

        private void ServiceOnSpatialAnchorLocated(object sender, SpatialAnchorLocatedEventArgs e)
        {
            this.anchors[e.Id] = e.Anchor;

        }

        private void ServiceOnASASessionUpdate(object sender, AsaSessionUpdateEventArgs e)
        {
        }
    }
}
