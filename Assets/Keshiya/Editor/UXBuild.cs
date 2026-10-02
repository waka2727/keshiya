using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Keshiya.Editor {
 public static class UXBuild {
  public static void BuildOnly(){var r=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Builds/Windows-ExternalTest-UX/Keshiya.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);if(r.summary.result!=BuildResult.Succeeded)EditorApplication.Exit(1);Debug.Log("KESHIYA_UX_BUILD_OK");}
  public static void Run(){try{ExternalArtworkImporter.Setup();ExternalArtworkChecks.Run();ExternalRegression.Run();var r=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Builds/Windows-ExternalTest-UX/Keshiya.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);if(r.summary.result!=BuildResult.Succeeded)throw new Exception("UX build failed");Debug.Log("KESHIYA_UX_BUILD_OK");}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 }
}
