namespace Keshiya {
 public sealed class Job {
  readonly PrototypeConfig config; public float Seconds {get;private set;}public bool Started {get;private set;}public bool Completed {get;private set;} public RewardResult Result {get;private set;}
  public float ChallengeMultiplier=1;
  public float SupplyProgress {get;private set;}
  public readonly System.Collections.Generic.List<ToolUsage> Usage=new System.Collections.Generic.List<ToolUsage>();
  public void RecordSupply(float erased){SupplyProgress=UnityEngine.Mathf.Clamp01(SupplyProgress+UnityEngine.Mathf.Max(0,erased)/UnityEngine.Mathf.Max(.01f,Definition.requiredErasure));}
  public void RecordUse(OwnedEraser item,bool supplied,StrokeResult stroke,ContactMode mode,float distance,float speed,int pixels){var r=Usage.Find(x=>x.instanceId==item.instanceId);if(r==null){r=new ToolUsage{instanceId=item.instanceId,definitionId=item.definitionId,supplied=supplied};Usage.Add(r);}r.Add(stroke,mode,distance,speed,pixels);}
  public JobDefinition Definition {get;private set;}
  public Job(PrototypeConfig c,JobDefinition definition=null){config=c;Definition=definition;}
  public void StartWork(){Started=true;} public void Tick(float delta){if(Started&&!Completed)Seconds+=delta;}
  public bool CanComplete(Paper p)=>!Completed&&(Definition==null||Definition.suppliedTool==null||SupplyProgress>=.9999f)&&p.Drawing.Erased>=(Definition!=null?Definition.requiredErasure:config.completionThreshold);
  public bool Complete(Paper p){if(!CanComplete(p))return false;Result=Reward.ApplyProtection(Reward.Calculate(config,p.Drawing.Erased,p.AverageDamage,p.PeakDamage,Seconds),p.Protection,Definition);if(Definition!=null){var result=Result;result.qualitySensitivity=Definition.paper!=null?Definition.paper.qualitySensitivity:1;
result.finish=UnityEngine.Mathf.RoundToInt(result.finish*UnityEngine.Mathf.Clamp01(1-result.damage*8*(result.qualitySensitivity-1)));
result.basic=Definition.baseReward;result.challenge=UnityEngine.Mathf.RoundToInt(Definition.baseReward*UnityEngine.Mathf.Max(0,ChallengeMultiplier-1));result.speed=UnityEngine.Mathf.RoundToInt(config.speedBonus*UnityEngine.Mathf.Clamp01(1-Seconds/UnityEngine.Mathf.Max(.01f,Definition.referenceSeconds)));Result=result;}Completed=true;return true;}
 }
}
