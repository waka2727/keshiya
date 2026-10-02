using System.Collections.Generic;
namespace Keshiya {
 public enum WorkPhase { Board, Detail, Work, Result }
 [System.Serializable] public sealed class JobRecord { public System.Collections.Generic.List<ToolUsage> lastUsage=new System.Collections.Generic.List<ToolUsage>();public int completions;public string bestGrade="C";public int bestReward;public float fastest=float.MaxValue; }
 // Session-only progression. A future shop can consume Wallet without changing Job or Paper.
 public sealed class WorkSession {
  public WorkPhase Phase {get;private set;}=WorkPhase.Board;
  public int Selected {get;private set;}
  public int CompletedCount {get;private set;}
  public int QualityPoints {get;private set;}
  readonly Dictionary<string,JobRecord> history=new Dictionary<string,JobRecord>();
  readonly HashSet<long> recorded=new HashSet<long>();
  public JobRecord Record(string id)=>history.TryGetValue(id,out var value)?value:null;
  public void ShowBoard(){Phase=WorkPhase.Board;}
  public bool Choose(int index,int count){if(index<0||index>=count)return false;Selected=index;Phase=WorkPhase.Detail;return true;}
  public bool Begin(){if(Phase!=WorkPhase.Detail)return false;Phase=WorkPhase.Work;return true;}
  public static string Grade(RewardResult r)=>r.severe||r.protectedMajor||r.damage*System.Math.Max(1,r.qualitySensitivity)>.04f||r.protectedDamage>.2f?"C":r.erased>=.98f&&r.peak*System.Math.Max(1,r.qualitySensitivity)<=.01f&&r.protectedDamage<=.005f?"S":r.peak*System.Math.Max(1,r.qualitySensitivity)<=.15f&&r.protectedDamage<=.05f?"A":"B";
  public bool Finish(long run,JobDefinition job,RewardResult result,System.Collections.Generic.List<ToolUsage> usage=null){
   if(Phase!=WorkPhase.Work||!recorded.Add(run))return false;Phase=WorkPhase.Result;
   string id=string.IsNullOrEmpty(job.id)?job.name:job.id,grade=Grade(result);
   if(!history.TryGetValue(id,out var record)){record=new JobRecord();history.Add(id,record);}
   record.lastUsage.Clear();if(usage!=null)foreach(var use in usage)record.lastUsage.Add(use.Copy());
   record.completions++;if(Rank(grade)>Rank(record.bestGrade))record.bestGrade=grade;
   record.bestReward=System.Math.Max(record.bestReward,result.Total);record.fastest=System.Math.Min(record.fastest,result.seconds);
   CompletedCount++;QualityPoints+=Rank(grade);return true;
  }
  static int Rank(string grade)=>grade=="S"?4:grade=="A"?3:grade=="B"?2:1;
 }
}
