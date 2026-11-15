using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Menu: Tools > Next Interview > Build WebGL
// Builds the game for the browser into the folder Builds/WebGL (next to Assets).
// Zip the CONTENTS of that folder (index.html must be at the top of the zip) and upload the zip to itch.io.
public static class WebBuild
{
    private const string OutputFolder = "Builds/WebGL";

    [MenuItem("Tools/Next Interview/Build WebGL")]
    public static void Build()
    {
        // Settings that work on itch.io: gzip with the fallback loader, and a 16:9 canvas.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.defaultWebScreenWidth = 1280;
        PlayerSettings.defaultWebScreenHeight = 720;

        var options = new BuildPlayerOptions();
        options.scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        options.locationPathName = OutputFolder;
        options.target = BuildTarget.WebGL;
        options.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log("WebGL build: " + report.summary.result + ". Output folder: " + OutputFolder);
    }
}
