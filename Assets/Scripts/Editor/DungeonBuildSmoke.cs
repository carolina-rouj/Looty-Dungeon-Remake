using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class DungeonBuildSmoke
{
    public static void Run()
    {
        try
        {
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/LevelScene.unity" },
                locationPathName = "Builds/Linux/VJ3D.x86_64",
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException("Build failed with result " + summary.result + " and " + summary.totalErrors + " errors.");
            }

            Debug.Log("[DungeonBuildSmoke] OK size=" + summary.totalSize + " bytes");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonBuildSmoke] FAILED\n" + exception);
            EditorApplication.Exit(1);
        }
    }
}
