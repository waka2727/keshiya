using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
namespace Keshiya.Editor {
 public static class PrototypeBuilder {
  [MenuItem("Keshiya/Create or open Prototype 0.8")]
  public static void Setup(){
   Directory.CreateDirectory("Assets/Keshiya/Resources");Directory.CreateDirectory("Assets/Keshiya/Scenes");
   var config=AssetDatabase.LoadAssetAtPath<PrototypeConfig>("Assets/Keshiya/Resources/PrototypeConfig.asset");if(config==null){config=ScriptableObject.CreateInstance<PrototypeConfig>();AssetDatabase.CreateAsset(config,"Assets/Keshiya/Resources/PrototypeConfig.asset");}
   var eraser=AssetDatabase.LoadAssetAtPath<EraserDefinition>("Assets/Keshiya/Resources/OrdinaryEraser.asset");if(eraser==null){eraser=ScriptableObject.CreateInstance<EraserDefinition>();AssetDatabase.CreateAsset(eraser,"Assets/Keshiya/Resources/OrdinaryEraser.asset");}
   var feel=AssetDatabase.LoadAssetAtPath<FeelConfig>("Assets/Keshiya/Resources/FeelConfig.asset");if(feel==null){feel=ScriptableObject.CreateInstance<FeelConfig>();AssetDatabase.CreateAsset(feel,"Assets/Keshiya/Resources/FeelConfig.asset");}
   var profile=AssetDatabase.LoadAssetAtPath<EraserContactProfile>("Assets/Keshiya/Resources/BroadFace.asset");if(profile==null){profile=ScriptableObject.CreateInstance<EraserContactProfile>();AssetDatabase.CreateAsset(profile,"Assets/Keshiya/Resources/BroadFace.asset");}
   if(eraser.contactProfile==null){eraser.contactProfile=profile;EditorUtility.SetDirty(eraser);}
   const string scenePath="Assets/Keshiya/Scenes/Prototype.unity";
   if(!File.Exists(scenePath)){var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var game=new GameObject("Keshiya Prototype 0.1").AddComponent<PrototypeGame>();game.Config=config;game.Eraser=eraser;EditorSceneManager.SaveScene(scene,scenePath);}else EditorSceneManager.OpenScene(scenePath);
   var root=UnityEngine.Object.FindFirstObjectByType<PrototypeGame>();root.gameObject.name="Keshiya Prototype 0.8";root.Feel=feel;root.ObjectShader=Shader.Find("Standard");root.PaperShader=Shader.Find("Unlit/Texture");root.OverlayShader=Shader.Find("Keshiya/FeelOverlay");Prototype02Setup.Apply(root);Prototype021Setup.Apply(root);Prototype03Setup.Apply(root);Prototype04Setup.Apply(root);Prototype05Setup.Apply();Prototype06Setup.Apply(root);Prototype061Setup.Apply(root);Prototype07Setup.Apply(root);Prototype08Setup.Apply(root);EditorUtility.SetDirty(root);EditorSceneManager.SaveScene(root.gameObject.scene);
   EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scenePath,true)};
   PlayerSettings.companyName="Keshiya Studio";PlayerSettings.productName="消し屋 Prototype 0.8";PlayerSettings.bundleVersion="0.8";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=800;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=false;
   PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
   AssetDatabase.SaveAssets();
  }
  public static void VerifyAndBuild(){try{Setup();PrototypeChecks.Run();FeelChecks.Run();ToolChecks.Run();RoleChecks.Run();EconomyChecks.Run();ZigzagChecks.Run();WorkChecks.Run();SkillChecks.Run();ShopChecks.Run();BalanceChecks.Run();SpecialistChecks.Run();WorldChecks.Run();Directory.CreateDirectory("Builds/Windows-0.8");var report=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Builds/Windows-0.8/Keshiya.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);Debug.Log("KESHIYA_BUILD_OK");}catch(Exception ex){Debug.LogException(ex);EditorApplication.Exit(1);}}
 }
}

