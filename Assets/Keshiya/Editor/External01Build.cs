using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Keshiya.Editor {
 public static class External01Build {
  public static void Run(){ExternalRegression.Run();UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Keshiya/Scenes/Prototype.unity");ExternalArtworkChecks.Run();Build();}
  public static void Build(){PlayerSettings.productName="消し屋 External Test";PlayerSettings.bundleVersion="External-Test-01";PlayerSettings.usePlayerLog=false;
   var result=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Builds/Windows-ExternalTest01/Keshiya.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
   if(result.summary.result!=BuildResult.Succeeded){EditorApplication.Exit(1);return;}Debug.Log("KESHIYA_EXTERNAL01_BUILD_OK");}
 }
}
