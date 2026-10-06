using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEditor;
namespace Keshiya.Editor {
 public static class Feedback082AdjustmentChecks {
  [Serializable] class Baseline {public RewardRow[] rewards;public ToolRow[] tools;public MangaHashes mangaHashes;}
  [Serializable] class RewardRow {public string id,path;public int old,@new;}
  [Serializable] class ToolRow {public string id,path,sha256;}
  [Serializable] class MangaHashes {public string Protected,ProtectMask,CompletePreview,Paper;}
  static int count;static readonly List<string> log=new List<string>();
  static void Check(bool ok,string message){count++;log.Add((ok?"PASS ":"FAIL ")+message);if(!ok)throw new Exception(message);}
  static string Hash(string path){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
  public static void Run(){try{
   Feedback082Build.Import();Checks();FoundationChecks.Run();ExternalRegression.Run();ExternalArtworkChecks.Run();Feedback082Build.Checks();Feedback082Build.BuildAdjustment();
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
  public static void Checks(){count=0;log.Clear();Directory.CreateDirectory("TestResults-Feedback082-Adjustment");try{
   var baseline=JsonUtility.FromJson<Baseline>(File.ReadAllText("Tools/Feedback082/Adjustment/baseline.json"));
   var config=Resources.Load<PrototypeConfig>("PrototypeConfig");var catalog=Resources.Load<ToolCatalog>("ToolCatalog");
   var jobs=Resources.LoadAll<JobDefinition>("");Check(jobs.Length==baseline.rewards.Length,"All runtime jobs included; DEV excluded");
   foreach(var row in baseline.rewards){var j=AssetDatabase.LoadAssetAtPath<JobDefinition>(row.path);
    Check(j!=null&&j.id==row.id&&j.baseReward==row.@new&&j.baseReward==(row.old+1)/2,"Half base reward / "+row.id);
    using(var p=new Paper(config,.1f,j)){for(int i=0;i<p.Drawing.Ink.Length;i++)p.Drawing.Erase(i,p.Drawing.Ink[i]);var work=new Job(config,j);work.StartWork();work.Tick(100000);if(j.suppliedTool!=null)work.RecordSupply(100);
     Check(work.Complete(p)&&work.Result.basic==row.@new&&work.Result.speed==0,"Result uses same base as listing / "+row.id);
     var wallet=new Wallet();Check(wallet.CreditJob(1,work.Result.Total)&&wallet.Balance==work.Result.Total&&!wallet.CreditJob(1,work.Result.Total),"One reward credit / "+row.id);
    }
   }
   foreach(var t in baseline.tools.Where(t=>t.id!="budget-soft"))Check(Hash(t.path)==t.sha256,"Other tool asset unchanged / "+t.id);
   var d=catalog.Find("budget-soft");var m=new PerformanceModifiers();new PlayerProgress(Resources.Load<SkillCatalog>("SkillCatalog"),Resources.Load<ProgressionConfig>("ProgressionConfig")).Apply(m);var inv=new ToolInventory();var cash=new Wallet();
   Check(d.price==0&&d.erasePower==.40f&&d.radius==.20f,"Free piece price/power/area");
   Check(d.wearRate==.90f&&d.cornerWearRate==.065f&&d.longCrumbPotential==.60f&&d.crumbAmount==.65f,"Disposable wear and limited byproduct");
   Check(d.description.Contains("無料")&&d.description.Contains("交換")&&d.description.Contains("消しゴム片"),"Honest piece description");
   Check(inv.Purchase(d,cash,out var one)&&cash.Balance==0&&cash.ShopSpent==0,"Receive with zero wallet");
   Check(Mathf.Abs(one.state.Remaining(d,m)-.35f)<.0001f&&one.state.CornerSharpness==.30f&&one.Condition!="新品","Damaged starting condition");
   Check(inv.Purchase(d,cash,out var two)&&two.instanceId!=one.instanceId,"Multiple separate pieces");
   one.state.Use(8,ContactMode.Corner,d,m);Check(one.state.Remaining(d,m)<two.state.Remaining(d,m)&&one.state.CornerSharpness<two.state.CornerSharpness,"Copies wear separately");
   var clone=JsonUtility.FromJson<ToolInventory>(JsonUtility.ToJson(inv));Check(clone.items[0].state.UsedUnits==one.state.UsedUnits&&clone.items[1].state.CornerSharpness==two.state.CornerSharpness,"Piece state survives serialization");
   one.state.Use(100,ContactMode.Face,d,m);Check(one.state.Exhausted&&!inv.Select(one.instanceId)&&inv.Select(two.instanceId),"Exhaustion requires switching");
   Check(cash.JobIncome==0&&cash.CrumbIncome==0&&cash.ShopSpent==0&&inv.purchases.All(x=>x.paid==0),"Receiving grants no money or sale material");
   var blank=new CrumbEconomy(Resources.Load<CrumbEconomyConfig>("CrumbEconomyConfig"));ZigzagTestPath.Trace(blank,d,graphitePerUnit:0);blank.EndStroke();Check(blank.ProducedGrams==0&&blank.Wallet.CrumbIncome==0,"Free tool rubbing blank paper generates no sale material");
   var invalid=UnityEngine.Object.Instantiate(d);invalid.price=-1;Check(!inv.Purchase(invalid,cash,out _),"Negative price rejected");UnityEngine.Object.DestroyImmediate(invalid);
   Check(!inv.Purchase(catalog.Find("ordinary"),cash,out _),"Paid tool still needs funds");
   // With replacement pieces, the free tool can actually remove a normal job at Lv0.
   var training=jobs.First(j=>j.id=="child");var state=d.CreateInitialState();int replacements=0;
   using(var paper=new Paper(config,.1f,training)){for(int pass=0;pass<180&&paper.Drawing.Erased<.95f;pass++)for(float y=-paper.Size.y*.5f+.08f;y<paper.Size.y*.5f;y+=.20f){
     if(state.Exhausted){state=d.CreateInitialState();replacements++;}var a=new Vector2(-paper.Size.x*.5f,y);var b=new Vector2(paper.Size.x*.5f,y);paper.Stroke(a,b,2,pass,d,m,d.Contact(m,ContactMode.Face,state,0),state);state.Use(Vector2.Distance(a,b),ContactMode.Face,d,m);
    }Check(paper.Drawing.Erased>=.95f&&!paper.Severe&&replacements>0,"Free pieces complete work with manual replacements");}
   string folder="Assets/Keshiya/ExternalArt/TEST_003/";
   Check(Hash(folder+"Protected.png")==baseline.mangaHashes.Protected,"Completed ink unchanged");Check(Hash(folder+"ProtectMask.png")==baseline.mangaHashes.ProtectMask,"Visible protected lines still protected");Check(Hash(folder+"CompletePreview.png")==baseline.mangaHashes.CompletePreview,"Completion preview unchanged");Check(Hash(folder+"Paper.png")==baseline.mangaHashes.Paper,"Paper unchanged");
   var art=jobs.First(j=>j.id=="TEST_003").artwork;var mask=art.eraseMask.GetPixels32();var protect=art.protectMask.GetPixels32();int w=art.eraseMask.width,h=art.eraseMask.height;long total=0,hand=0;int ink=0;
   for(int y=0;y<h;y++)for(int x=0;x<w;x++){int i=y*w+x;total+=mask[i].r;int top=h-1-y;if(x>=825&&x<=1155&&top>=520&&top<=910){hand+=mask[i].r;if(protect[i].r>0)ink++;}}
   Check(hand==0&&ink>100,"Hand/strap panel has no required graphite but retains protected ink");Check(total>0&&art.Valid,"Manga layers/masks remain valid");
  }finally{File.WriteAllLines("TestResults-Feedback082-Adjustment/checks.txt",log.Concat(new[]{"Checks="+count+"; Failures="+log.Count(x=>x.StartsWith("FAIL"))}));}Debug.Log("FEEDBACK082_ADJUSTMENT_OK "+count);}
 }
}
