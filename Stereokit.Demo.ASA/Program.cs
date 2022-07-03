using StereoKit;
using System;
using Stereokit.Demo.ASA.Services;

namespace Stereokit.Demo.ASA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Initialize StereoKit
            SKSettings settings = new SKSettings
            {
                appName = "Stereokit.Demo.ASA",
                assetsFolder = "Assets",
#if DEBUG
                blendPreference = DisplayBlend.Opaque, //2d screen
                displayPreference = DisplayMode.Flatscreen,
#else 
                blendPreference = DisplayBlend.AnyTransparent, // we're doing this demo for HL
                displayPreference = DisplayMode.MixedReality,
#endif
                logFilter = LogLevel.Diagnostic //I have no idea what I'm doing so please show me everything :D
            };
            if (!SK.Initialize(settings))
                Environment.Exit(1);

            ISpatialAnchorsWrapper asaService;
#if DEBUG
            asaService = new MockSpatialAnchorsWrapper();
#else
            asaService = new SpatialAnchorsWrapper(Configuration.AccountId, Configuration.AccountKey, Configuration.Domain);
#endif
            var logging = new LogWindow(asaService);
            var mainScene = new MainScene(asaService);
            SK.AddStepper(logging);
            SK.AddStepper(mainScene);

            Matrix floorTransform = Matrix.TS(0, -1.5f, 0, new Vec3(30, 0.1f, 30));
            Material floorMaterial = new Material(Shader.FromFile("floor.hlsl"));
            floorMaterial.Transparency = Transparency.Blend;

            while (SK.Step(() =>
            {
                if (SK.System.displayType == Display.Opaque)
                    Default.MeshCube.Draw(floorMaterial, floorTransform);

                //UI.Handle("Cube", ref cubePose, cube.Bounds);
                //cube.Draw(cubePose.ToMatrix());
            }));
            SK.Shutdown();
        }
    }
}
