using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
namespace Keshiya.Editor {
 public static class ExternalArtworkImporter {
  [Serializable] public class Manifest {public string id,title;public int[] source,runtime;public float[] worldSize;public string[] layers;public JobPath[] strokes;}
  [Serializable] public class Brief {public string client,paper,writing,clientQuote,instruction,thankYou,recommended;public int baseReward,difficulty;public float requiredErasure,referenceSeconds;public bool precision,supply;}
  static T Asset<T>(string name) where T:ScriptableObject {string path="Assets/Keshiya/Resources/"+name+".asset";var value=AssetDatabase.LoadAssetAtPath<T>(path);if(value==null){value=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(value,path);}return value;}
  [MenuItem("Keshiya/External Test/Import artwork and configure")]
  public static void Setup(){PrototypeBuilder.Setup();var game=UnityEngine.Object.FindFirstObjectByType<PrototypeGame>();Apply(game);EditorSceneManager.SaveScene(game.gameObject.scene);AssetDatabase.SaveAssets();}
  public static void Apply(PrototypeGame game){
   var jobs=new List<JobDefinition>(game.Jobs.Where(j=>!j.externalTest));
   foreach(string folder in Directory.GetDirectories("Assets/Keshiya/ExternalArt").OrderBy(x=>x)){
    string manifestPath=Path.Combine(folder,"manifest.json");if(!File.Exists(manifestPath))continue;var m=JsonUtility.FromJson<Manifest>(File.ReadAllText(manifestPath));string number=m.id.Substring(5);var brief=JsonUtility.FromJson<Brief>(File.ReadAllText("Tools/ExternalArt/job-"+number+".json"));
    foreach(string layer in m.layers){string path=(folder+"/"+layer+".png").Replace('\\','/');AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var t=(TextureImporter)AssetImporter.GetAtPath(path);bool mask=layer.EndsWith("Mask");t.textureType=TextureImporterType.Default;t.sRGBTexture=!mask;t.isReadable=mask;t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=4096;t.textureCompression=TextureImporterCompression.Uncompressed;t.filterMode=FilterMode.Bilinear;t.wrapMode=TextureWrapMode.Clamp;t.alphaIsTransparency=!mask;t.SaveAndReimport();}
    Func<string,Texture2D> tex=n=>AssetDatabase.LoadAssetAtPath<Texture2D>((folder+"/"+n+".png").Replace('\\','/'));
    var art=Asset<JobArtworkData>("Artwork-"+m.id);art.paper=tex("Paper");art.protectedImage=tex("Protected");art.erasable=tex("Erasable");art.eraseMask=tex("EraseMask");art.protectMask=tex("ProtectMask");art.completePreview=tex("CompletePreview");art.initialPreview=tex("InitialPreview");art.worldSize=new Vector2(m.worldSize[0],m.worldSize[1]);art.sourceFolder="Tools/ExternalArt/Source/"+m.id;art.manifestHash=Hash128.Compute(File.ReadAllText(manifestPath)).ToString();art.testStrokes=m.strokes;EditorUtility.SetDirty(art);
    var job=Asset<JobDefinition>("External-"+m.id);job.id=m.id;job.displayName=m.title;job.externalTest=true;job.artwork=art;job.client=brief.client;job.summary=brief.instruction;job.clientQuote=brief.clientQuote;job.instruction=brief.instruction;job.thankYou=brief.thankYou;job.recommended=brief.recommended;job.baseReward=brief.baseReward;job.difficulty=brief.difficulty;job.requiredErasure=brief.requiredErasure;job.referenceSeconds=brief.referenceSeconds;job.precision=brief.precision;job.paper=Resources.Load<PaperDefinition>("Paper-"+brief.paper);job.writing=Resources.Load<WritingInstrumentDefinition>("Writing-"+brief.writing);job.category=brief.precision?"精密修正":"全面消去";job.feature="薄い青灰色は消す / 濃い完成線は残す";job.protectionBonus=brief.precision?400:0;job.majorLoss=.2f;job.suppliedTool=brief.supply?game.Catalog.tools.First(x=>x.questOnly):null;job.suppliedRemaining=.12f;EditorUtility.SetDirty(job);jobs.Add(job);
   }
   var nameMat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Keshiya/Resources/SuppliedNameLabel.mat");if(nameMat==null){nameMat=new Material(Shader.Find("Unlit/Transparent"));AssetDatabase.CreateAsset(nameMat,"Assets/Keshiya/Resources/SuppliedNameLabel.mat");}nameMat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Keshiya/ExternalArt/NameLabel.png");EditorUtility.SetDirty(nameMat);
   game.Jobs=jobs.ToArray();game.ArtworkShader=Shader.Find("Keshiya/LayeredPaper");var config=Asset<PlaytestConfig>("PlaytestConfig");EditorUtility.SetDirty(config);EditorUtility.SetDirty(game);
   PlayerSettings.productName="消し屋 External Test";PlayerSettings.bundleVersion="0.8.1-external";
  }
  public static void VerifyAndBuild(){try{Setup();ExternalArtworkChecks.Run();Directory.CreateDirectory("Builds/Windows-ExternalTest");var result=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Builds/Windows-ExternalTest/Keshiya.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed "+result.summary.result);Debug.Log("KESHIYA_EXTERNAL_BUILD_OK");}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 }
}
