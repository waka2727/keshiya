using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Keshiya.Editor {
 public static class FoundationBuild {
  public static void Run(){try{FoundationChecks.Run();ExternalRegression.Run();EditorSceneManager.OpenScene("Assets/Keshiya/Scenes/Prototype.unity");ExternalArtworkChecks.Run();Build(false);Build(true);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
  public static void BuildOnly(){Build(false);Build(true);}
  static void Build(bool development){string mode=development?"Development":"Normal";var result=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Builds/Foundation-"+mode+"/Keshiya.exe",BuildTarget.StandaloneWindows64,development?BuildOptions.Development:BuildOptions.None);if(result.summary.result!=BuildResult.Succeeded)throw new Exception(mode+" build failed");var packed=result.packedAssets.SelectMany(p=>p.contents).Select(c=>c.sourceAssetPath).Distinct().ToArray();if(packed.Any(p=>p.Contains("DEV_PIPELINE_001")))throw new Exception("DEV job leaked into "+mode);Directory.CreateDirectory("TestResults-Foundation");File.WriteAllLines("TestResults-Foundation/build-"+mode+".txt",new[]{"PASS "+mode+" Windows build","PASS DEV job excluded from packed assets","Size="+result.summary.totalSize});Debug.Log("FOUNDATION_BUILD_OK "+mode);}
 }
}
