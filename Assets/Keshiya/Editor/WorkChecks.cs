using System;
using System.IO;
using System.Text;
using UnityEngine;
namespace Keshiya.Editor {
 public static class WorkChecks {
  static int count;static StringBuilder log;
  static void Check(bool ok,string name){if(!ok)throw new Exception("WORK FAIL: "+name);count++;log.AppendLine("PASS: "+name);}
  public static void Run(){count=0;log=new StringBuilder();var c=Resources.Load<PrototypeConfig>("PrototypeConfig");var ec=Resources.Load<CrumbEconomyConfig>("CrumbEconomyConfig");var tools=Resources.Load<ToolCatalog>("ToolCatalog").tools;
   var old=UnityEngine.Object.Instantiate(tools[1]);old.crumbCohesion=.9f;old.longCrumbPotential=1.6f;
   var previous=new CrumbEconomy(ec);var current=new CrumbEconomy(ec);ZigzagTestPath.Trace(previous,old,irregular:true);ZigzagTestPath.Trace(current,tools[1],irregular:true);
   Check(current.BestLength>previous.BestLength&&current.BreakCount<=previous.BreakCount,"Soft irregular rubbing grows more than 0.3.1");
   Check(tools[1].crumbCohesion>old.crumbCohesion,"Soft amplitude period and direction tolerance increased");
   var still=new CrumbEconomy(ec);ZigzagTestPath.Trace(still,tools[1],advance:0,halves:400);Check(still.BestLength==0,"Soft in-place motion remains bounded");
   var blank=new CrumbEconomy(ec);ZigzagTestPath.Trace(blank,tools[1],graphitePerUnit:0);Check(blank.BestLength==0&&blank.ProducedGrams==0,"Soft cannot farm blank paper");
   var longRun=new CrumbEconomy(ec);ZigzagTestPath.Trace(longRun,tools[1],advance:30,halves:300);Check(longRun.BestLength<=80&&longRun.BreakCount>0,"Soft strands keep the 80cm limit");UnityEngine.Object.DestroyImmediate(old);
   string[] names={"NormalJob","PrecisionJob","DraftJob","CorrectionsJob","PatternJob"};var ids=new System.Collections.Generic.HashSet<string>();float previousMass=0;
   foreach(var name in names){var j=Resources.Load<JobDefinition>(name);Check(j!=null&&!string.IsNullOrEmpty(j.id)&&ids.Add(j.id),"Unique job data: "+name);Check(!string.IsNullOrEmpty(j.client)&&!string.IsNullOrEmpty(j.summary)&&!string.IsNullOrEmpty(j.clientQuote)&&j.baseReward>0&&j.difficulty>=1,"Card metadata: "+name);Check(j.requiredErasure==.95f,"95 percent requirement: "+name);
    using(var paper=new Paper(c,.1f,j)){Check(paper.Drawing.InitialMass>0&&(j.precision==(paper.Protection!=null)),"Artwork and protection loaded: "+name);if(name=="NormalJob")previousMass=paper.Drawing.InitialMass;if(name=="DraftJob")Check(paper.Drawing.InitialMass>previousMass*1.5f,"Large draft contains substantially more graphite");
     var work=new Job(c,j);Check(!work.CanComplete(paper),"Fresh job cannot complete: "+name);for(int i=0;i<paper.Drawing.Ink.Length;i++)paper.Drawing.Erase(i,paper.Drawing.Ink[i]);work.StartWork();work.Tick(100000);Check(work.Complete(paper)&&work.Result.basic==j.baseReward&&work.Result.speed==0,"Slow job keeps full base pay: "+name);Check(work.Result.finish>0&&work.Result.pristine>0&&WorkSession.Grade(work.Result)=="S","Clean slow work earns quality and bonuses: "+name);}
   }
   var session=new WorkSession();Check(session.Phase==WorkPhase.Board&&!session.Begin(),"Initial board cannot start unselected job");Check(!session.Choose(-1,5)&&!session.Choose(5,5),"Invalid board selections rejected");Check(session.Choose(4,5)&&session.Phase==WorkPhase.Detail&&session.Begin(),"Detail then start transitions");
   var job=Resources.Load<JobDefinition>("PatternJob");var result=Reward.Calculate(c,1,0,0,10);Check(result.speed>0&&result.basic==1000,"Speed adds money without base deduction");
   Check(session.Finish(1,job,result)&&session.Phase==WorkPhase.Result,"Result records completion");Check(!session.Finish(1,job,result)&&session.CompletedCount==1,"Repeated completion does not duplicate history");
   Check(session.Record(job.id).bestGrade=="S"&&session.Record(job.id).bestReward==result.Total,"History retains best grade and reward");
   session.ShowBoard();session.Choose(4,5);session.Begin();result.seconds=20;result.peak=.1f;Check(session.Finish(2,job,result)&&session.Record(job.id).completions==2&&session.Record(job.id).fastest==10&&session.Record(job.id).bestGrade=="S","Retry preserves independent best results");
   result.seconds=100000;result.peak=0;Check(WorkSession.Grade(result)=="S","Time alone never lowers grade");result.protectedDamage=.03f;Check(WorkSession.Grade(result)=="A","Small protected error still earns completion grade");result.protectedMajor=true;Check(WorkSession.Grade(result)=="C","Major loss reduces quality grade");
   Directory.CreateDirectory("TestResults");File.WriteAllText("TestResults/work-checks.txt",$"PASS {count} work checks\n"+log);Debug.Log($"WORK_CHECKS_OK {count}");
  }
 }
}
