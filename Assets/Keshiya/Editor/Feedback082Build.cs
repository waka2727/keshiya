using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Keshiya.Editor {
 public static class Feedback082Build {
  static readonly List<string> lines=new List<string>();static int count;
  static void Check(bool ok,string message){count++;lines.Add((ok?"PASS ":"FAIL ")+message);if(!ok)throw new Exception(message);}
  public static void BuildOnly(){Import();Build(false);Build(true);}
  public static void Verify(){Import();Checks();}
  public static void Run(){try{Import();FoundationChecks.Run();ExternalRegression.Run();ExternalArtworkChecks.Run();Checks();Build(false);Build(true);}catch(Exception e){Directory.CreateDirectory("TestResults-Feedback082");File.WriteAllLines("TestResults-Feedback082/checks.txt",lines);Debug.LogException(e);EditorApplication.Exit(1);}}
  public static void Import(){
   JobContentPipeline.Import();EditorSceneManager.OpenScene("Assets/Keshiya/Scenes/Prototype.unity");
   string path="Assets/Keshiya/ExternalArt/TEST_003/manifest.json";var json=File.ReadAllText(path);var m=JsonUtility.FromJson<ExternalArtworkImporter.Manifest>(json);
   var a=Resources.Load<JobArtworkData>("Artwork-TEST_003");a.testStrokes=m.strokes;a.manifestHash=Hash128.Compute(json).ToString();EditorUtility.SetDirty(a);PlayerSettings.bundleVersion="0.8.2-feedback";AssetDatabase.SaveAssets();
  }
  public static void Checks(){
   lines.Clear();count=0;var game=UnityEngine.Object.FindFirstObjectByType<PrototypeGame>();
   foreach(var job in game.Jobs.Where(j=>j.externalTest)){
    Check(job.reactions.Length>=1,job.id+" reactions registered");
    foreach(var kind in new[]{JobReactionKind.LowQuality,JobReactionKind.HeavyPaperDamage,JobReactionKind.HeavyProtectedDamage,JobReactionKind.VeryLowErase,JobReactionKind.UnexpectedDestruction})Check(!string.IsNullOrEmpty(TextCatalog.Require(job.reactions.First(x=>x.kind==kind).textId)),job.id+" text "+kind);
    Check(JobReactions.Resolve(job,new RewardResult{erased=1})==job.thankYou,job.id+" original success retained");
   }
   var first=game.Jobs.First(j=>j.id=="TEST_001");Check(first.thankYou.Contains("猫")&&!first.thankYou.Contains("犬"),"cat text regression");
   Check(JobReactions.Classify(new RewardResult{severe=true})==JobReactionKind.HeavyPaperDamage,"paper reaction");Check(JobReactions.Classify(new RewardResult{protectedMajor=true})==JobReactionKind.HeavyProtectedDamage,"protected reaction");
   var empty=ScriptableObject.CreateInstance<JobDefinition>();Check(!string.IsNullOrEmpty(JobReactions.Resolve(empty,new RewardResult{severe=true})),"fallback");UnityEngine.Object.DestroyImmediate(empty);
   foreach(var mode in new[]{ContactMode.Face,ContactMode.Edge,ContactMode.Corner})foreach(float angle in new[]{0f,45f,90f}){
    var fp=game.Catalog.tools[0].Contact(new PerformanceModifiers(),mode,new EraserState(),angle);bool boundary=true;
    for(int i=0;i<96;i++){var p=fp.OutlinePoint(i*Mathf.PI*2/96);var relative=p-fp.Offset;boundary&=fp.Weight(p)<.0001f&&fp.Weight(fp.Offset+relative*.99f)>0&&fp.Weight(fp.Offset+relative*1.01f)==0;}
    Check(boundary,mode+" "+angle+" same analytical boundary");
    foreach(float zoom in new[]{1f,2f,4f}){var cameraObject=new GameObject("projection check");var camera=cameraObject.AddComponent<Camera>();camera.targetTexture=new RenderTexture(800,800,0);camera.orthographic=true;camera.orthographicSize=5/zoom;camera.transform.position=new Vector3(0,10,0);camera.transform.rotation=Quaternion.Euler(90,0,0);var point=fp.OutlinePoint(.7f);var world=new Vector3(point.x,0,point.y);camera.projectionMatrix=Matrix4x4.Ortho(-5/zoom,5/zoom,-5/zoom,5/zoom,.1f,100);var matrix=camera.projectionMatrix*camera.worldToCameraMatrix;var clip=matrix.MultiplyPoint(world);Check(Vector3.Distance(matrix.inverse.MultiplyPoint(clip),world)<.0001f,mode+" paper geometry zoom "+zoom);UnityEngine.Object.DestroyImmediate(camera.targetTexture);UnityEngine.Object.DestroyImmediate(cameraObject);}
   }
   using(var paper=new Paper(game.Config,game.Feel.erasureGrain,first)){Check(paper.Texture.filterMode==FilterMode.Point,"live state does not bilinearly bleed across contact cells");Check(paper.Texture.wrapMode==TextureWrapMode.Clamp,"state clamps paper border");Check(paper.Damage.All(x=>x==0),"fresh paper local damage reset");}
   // Exercise the real sampling loop, not just the analytical outline.
   foreach(var mode in new[]{ContactMode.Face,ContactMode.Edge,ContactMode.Corner})foreach(float angle in new[]{0f,45f,90f}){
    using(var p=new Paper(game.Config,game.Feel.erasureGrain,first)){
     var tool=game.Catalog.tools[0];var state=new EraserState();var skills=new PerformanceModifiers();var fp=tool.Contact(skills,mode,state,angle);
     for(int i=0;i<p.Drawing.Ink.Length;i++)p.Drawing.Ink[i]=1;
     var end=new Vector2(.031f,.019f);float distance=Mathf.Min(.002f,fp.SampleSpacing*.5f);p.Stroke(end-Vector2.right*distance,end,2,0,tool,skills,fp,state);
     bool outside=false;int changed=0;for(int y=0;y<p.Height;y++)for(int x=0;x<p.Width;x++){if(p.Drawing.Ink[y*p.Width+x]>=1)continue;changed++;if(fp.PixelWeight(x,y,p.Width,p.Height,p.Size,end)<=0)outside=true;}
     Check(changed>0&&!outside,mode+" "+angle+" actual Stroke only changes shared contact samples");
    }
   }
   for(int stage=0;stage<4;stage++)Check(PaperDamageAppearance.Stage(new[]{0f,.2f,.6f,1.1f}[stage]*.65f,.65f)==stage,"local paper visual stage "+stage);
   var soundObject=new GameObject("sound check");var audio=soundObject.AddComponent<FeedbackAudio>();audio.Initialize(game.Feel);audio.ToggleMute();Check(!audio.TryCompletion(.95f),"95 is not complete cue");Check(audio.TryCompletion(1),"100 once even muted");Check(!audio.TryCompletion(1)&&!audio.TryCompletion(.99f)&&!audio.TryCompletion(1),"no repeat completion");audio.ResetFeedback();Check(!audio.TryCompletion(1),"focus reset retains latch");audio.ResetCompletion();Check(audio.TryCompletion(1),"retry rearms completion");Check(audio.Muted,"mute retained");UnityEngine.Object.DestroyImmediate(soundObject);
   ExportData(game);
   Directory.CreateDirectory("TestResults-Feedback082");lines.Add("Checks="+count+"; Failures=0");File.WriteAllLines("TestResults-Feedback082/checks.txt",lines);Debug.Log("FEEDBACK082_CHECKS_OK "+count);
  }
  static void ExportData(PrototypeGame game){
   var rows=new List<string>{"# 現状データ（0.8.2、経済値変更なし）","","Mask非ゼロ面積 / 紙面積。操作量は消去質量÷普通消しLv0消去力の比較指標であり秒数ではない。Protected密度は難易度全体を表さない。","","|ID|Title|Reward|★|Erase面積|Protect密度|Paper Risk (倍率)|候補道具|操作量指標|","|---|---|---:|---:|---:|---:|---|---|---:|"};
   foreach(var j in game.Jobs){using(var p=new Paper(game.Config,game.Feel.erasureGrain,j)){float area=p.Size.x*p.Size.y*p.Drawing.Ink.Count(v=>v>0)/(p.Width*p.Height);float density=(p.Protection?.Ink.Count(v=>v>0)??0)/(float)(p.Width*p.Height);rows.Add($"|{j.id}|{j.displayName}|{j.baseReward}|{j.difficulty}|{area:F4}|{density:P2}|{j.PaperName}: {j.paper?.DamageScale:F2}|{j.recommended}|{p.Drawing.InitialMass/(game.Catalog.tools[0].erasePower*.9f):F1}|");}}
   rows.AddRange(new[]{"","|Tool|Price|ErasePower|Precision倍率|PaperDamage|Wear|CornerWear|SpecialAbility|","|---|---:|---:|---:|---:|---:|---:|---|"});
   foreach(var t in game.Catalog.tools)rows.Add($"|{t.displayName}|{t.price}|{t.erasePower*t.equipment.erasePower:F3}|{t.equipment.precision:F2}|{t.paperDamageMultiplier*t.equipment.paperDamage:F3}|{t.wearRate:F3}|{t.cornerWearRate:F3}|{t.role.Replace("\n"," / ")} / {t.specialKind}|");
   Directory.CreateDirectory("Documentation/Feedback082");File.WriteAllLines("Documentation/Feedback082/CurrentData.md",rows);
  }
  static void Build(bool development){string mode=development?"Development":"Normal";var result=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Builds/Feedback082-"+mode+"/Keshiya.exe",BuildTarget.StandaloneWindows64,development?BuildOptions.Development:BuildOptions.None);if(result.summary.result!=BuildResult.Succeeded)throw new Exception(mode+" build failed");if(result.packedAssets.SelectMany(p=>p.contents).Any(c=>c.sourceAssetPath.Contains("DEV_PIPELINE_001")))throw new Exception("DEV asset leaked");File.WriteAllText("TestResults-Feedback082/build-"+mode+".txt","PASS "+mode+"; bytes="+result.summary.totalSize);}
 }
}
