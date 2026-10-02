using System;
using System.IO;
using UnityEngine;
namespace Keshiya.Editor {
 public static class PrototypeChecks {
  static int checks;static void Check(bool ok,string message){if(!ok)throw new Exception("FAIL: "+message);checks++;Debug.Log("PASS: "+message);}
  public static void Run(){checks=0;var c=ScriptableObject.CreateInstance<PrototypeConfig>();c.resolution=256;var e=ScriptableObject.CreateInstance<EraserDefinition>();var m=new PerformanceModifiers();
   var paper=new Paper(c);Check(paper.Drawing.Erased==0,"Drawing starts at 0% erased");var job=new Job(c);Check(!job.Complete(paper),"Cannot submit unfinished job");
   var p=new Vector2(-.3f,-.1f);paper.Stroke(p,p,30,0,e,m);Check(paper.Drawing.Erased==0&&paper.PeakDamage==0,"Stationary contact does not erase or damage");
   paper.Stroke(new Vector2(-3,0),new Vector2(3,0),3,1,e,m);Check(paper.Drawing.Erased>0&&paper.Drawing.Erased<.5f,"One pass partially erases only its path");
   for(int pass=0;pass<15&&!job.CanComplete(paper);pass++)for(float y=-2.4f;y<=2.4f;y+=.2f)paper.Stroke(new Vector2(-3.4f,y),new Vector2(3.4f,y),3,pass*100+y+3,e,m);
   Check(job.CanComplete(paper),"Safe rubbing reaches completion threshold");Check(paper.PeakDamage==0,"Repeated slow rubbing never damages paper");job.StartWork();job.Tick(99999);Check(job.Complete(paper),"No time limit, very slow job completes");Check(job.Result.basic==c.baseReward&&job.Result.speed==0,"Slow work keeps full base reward and zero speed bonus");Check(!job.Complete(paper),"Completion cannot be duplicated");paper.Dispose();
   var slow=Reward.Calculate(c,1,0,0,1000);var fast=Reward.Calculate(c,1,0,0,10);Check(fast.Total>slow.Total&&fast.basic==slow.basic,"Speed is additive bonus only");Check(slow.pristine==c.pristineBonus,"Pristine bonus is granted");
   var damaged=new Paper(c);for(int i=0;i<80;i++)damaged.Stroke(new Vector2(-1,0),new Vector2(1,0),30,i*.08f,e,m);Check(damaged.PeakDamage>0&&damaged.Severe,"Aggressive rubbing causes localized severe damage");Check(Reward.Calculate(c,1,damaged.AverageDamage,damaged.PeakDamage,10).pristine==0,"Damage removes pristine bonus");damaged.Dispose();
   var a=new Paper(c);var b=new Paper(c);a.Stroke(new Vector2(-3,0),new Vector2(3,0),3,0,e,m);for(int i=0;i<60;i++)b.Stroke(new Vector2(-3+i*.1f,0),new Vector2(-3+(i+1)*.1f,0),3,i/30f,e,m);Check(Mathf.Abs(a.Drawing.Erased-b.Drawing.Erased)<.008f,"Stroke subdivision gives consistent erasure");a.Dispose();b.Dispose();
   Directory.CreateDirectory("TestResults");File.WriteAllText("TestResults/editor-checks.txt",$"PASS {checks} checks\nUnity {Application.unityVersion}\n");UnityEngine.Object.DestroyImmediate(c);UnityEngine.Object.DestroyImmediate(e);
  }
 }
}
