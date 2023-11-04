using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class BuildAndRun : MonoBehaviour
{
    /*[MenuItem("MyMenu/Build and Run")]
    public static void BuildAndRunGame()
    {
        // Build the Unity project
        string buildPath = @"F:\Unity Projects\AutoCadUnity\CreateObjectByCode\Build\";
        BuildPipeline.BuildPlayer(new string[] { "Assets/Scenes/Create3DObjectFromPolylineVertex.unity" }, buildPath, BuildTarget.StandaloneWindows, BuildOptions.None);

        // Run the built .exe
        string exePath = System.IO.Path.Combine(buildPath, "CreateObjectByCode.exe");
        Process.Start(exePath);
    }*/
}
