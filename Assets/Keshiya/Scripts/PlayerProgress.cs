using System;
using System.Collections.Generic;
using UnityEngine;
namespace Keshiya {
 [Serializable] public sealed class ExperienceBank {public float total,remainder;public int earnedSP,availableSP,debugSP;}
 [Serializable] public sealed class CraftStatistics {public float erasedArea,longestCm;public int precisionSuccesses,pristineJobs,longCollections;}
 [Serializable] public sealed class ExperienceReason {public CraftBranch branch;public string reason;public float amount;}
 // All persistent progression state and reward deduplication lives here, not in UI.
 [Serializable] public sealed class PlayerProgress {
  public Wallet wallet=new Wallet();
  public ToolInventory tools=new ToolInventory();
  public WorkSession work=new WorkSession();
  public ExperienceBank[] banks={new ExperienceBank(),new ExperienceBank(),new ExperienceBank()};
  public int[] levels=new int[11];public CraftStatistics statistics=new CraftStatistics();
  public float[] jobExp=new float[3];public int[] jobSP=new int[3];public List<ExperienceReason> reasons=new List<ExperienceReason>();
  public int notificationRevision;public string notification;
  readonly HashSet<long> formed=new HashSet<long>(),collected=new HashSet<long>(),completed=new HashSet<long>();
  readonly bool[] modes=new bool[3];float jobErased,travel,ballAward,saleAward;bool milestone;
  readonly SkillCatalog catalog;readonly ProgressionConfig config;
  public PlayerProgress(SkillCatalog definitions,ProgressionConfig tuning){catalog=definitions;config=tuning;}
  [Serializable] sealed class RestartData {public ExperienceBank[] banks;public int[] levels;public CraftStatistics statistics;}
  public Action CaptureRestart(){string w=JsonUtility.ToJson(wallet),t=JsonUtility.ToJson(tools),data=JsonUtility.ToJson(new RestartData{banks=banks,levels=levels,statistics=statistics});var f=new List<long>(formed);var c=new List<long>(collected);
   return ()=>{JsonUtility.FromJsonOverwrite(w,wallet);JsonUtility.FromJsonOverwrite(t,tools);var d=JsonUtility.FromJson<RestartData>(data);banks=d.banks;levels=d.levels;statistics=d.statistics;formed.Clear();formed.UnionWith(f);collected.Clear();collected.UnionWith(c);BeginJob();notification="";notificationRevision++;};}
  public int Level(SkillKind kind)=>levels[(int)kind];
  public float Effect(SkillKind kind)=>catalog.Find(kind).Effect(Level(kind));
  public float Next(CraftBranch branch)=>config.Required(banks[(int)branch].earnedSP);
  public void BeginJob(){Array.Clear(jobExp,0,3);Array.Clear(jobSP,0,3);Array.Clear(modes,0,3);reasons.Clear();jobErased=travel=ballAward=saleAward=0;milestone=false;}
  public System.Collections.Generic.List<string> ExperienceLines(){var lines=new System.Collections.Generic.List<string>();for(int i=0;i<3;i++){var branch=(CraftBranch)i;lines.Add($"{SkillCatalog.BranchName(branch)} EXP +{jobExp[i]:0.0} / SP +{jobSP[i]}");bool any=false;foreach(var r in reasons)if((int)r.branch==i){lines.Add($"・{r.reason} +{r.amount:0.0}");any=true;}if(!any)lines.Add("・この仕事ではまだ獲得していません");lines.Add("");}return lines;}
  public void Gain(CraftBranch branch,float amount,string reason){if(float.IsNaN(amount)||float.IsInfinity(amount)||amount<=0)return;var b=banks[(int)branch];b.total+=amount;b.remainder+=amount;jobExp[(int)branch]+=amount;
   var line=reasons.Find(r=>r.branch==branch&&r.reason==reason);if(line==null){line=new ExperienceReason{branch=branch,reason=reason};reasons.Add(line);}line.amount+=amount;
   int gained=0;while(b.remainder>=config.Required(b.earnedSP)){b.remainder-=config.Required(b.earnedSP);b.earnedSP++;b.availableSP++;jobSP[(int)branch]++;gained++;}
   if(gained>0){notification=SkillCatalog.BranchName(branch)+$" SP +{gained}";notificationRevision++;}
  }
  public void Stroke(StrokeResult stroke,ContactMode mode,bool precisionJob,float distance){travel+=Mathf.Max(0,distance);if(stroke.graphiteArea<=0||stroke.erasedFraction<=0)return;
   statistics.erasedArea+=stroke.graphiteArea;jobErased+=stroke.erasedFraction;
   Gain(CraftBranch.Erasing,stroke.graphiteArea*config.erasedAreaExp+stroke.erasedFraction*config.erasedFractionExp,"鉛筆を消した量");
   if(jobErased>=.95f&&!milestone){milestone=true;Gain(CraftBranch.Erasing,config.milestoneExp,"95%まで消去");}
   if(stroke.protectedLoss<=0&&!stroke.Damaged&&stroke.preciseArea>0){float amount=stroke.preciseArea*config.nearProtectionExp+stroke.preciseFraction*(mode==ContactMode.Corner?config.cornerFractionExp:mode==ContactMode.Edge?config.edgeFractionExp:0);Gain(CraftBranch.Precision,amount,"傷つけず精密に消去");
    if(!modes[(int)mode]){modes[(int)mode]=true;Gain(CraftBranch.Precision,config.modePracticeExp,"保護対象の近くで接触面を使用");}}
  }
  public void Complete(long id,RewardResult r,bool precisionJob){if(!completed.Add(id))return;Gain(CraftBranch.Erasing,config.completionExp,"依頼完了");Gain(CraftBranch.Erasing,config.efficiencyExp*Mathf.Clamp01(jobErased/Mathf.Max(.01f,travel)/.025f),"少ない擦り距離で完了");
   if(r.peak<=.00001f){statistics.pristineJobs++;Gain(CraftBranch.Precision,config.safePaperExp+(precisionJob?config.precisionSafeExp:0),"紙ダメージなしで完了");}
   if(precisionJob&&r.protectedDamage<=.000001f){statistics.precisionSuccesses++;Gain(CraftBranch.Precision,config.protectedSafeExp,"保護対象を無傷で完了");}
  }
  public void StrandFormed(long id,float length){if(length<5||!formed.Add(id))return;Gain(CraftBranch.Crumbs,Mathf.Pow(length,config.longExpPower)*config.longExpScale,"長い一本を作成");if(length>statistics.longestCm){Gain(CraftBranch.Crumbs,(length-statistics.longestCm)*config.bestExpPerCm,"最長記録を更新");statistics.longestCm=length;}}
  public void Collected(long id,float length){if(length<5||!collected.Add(id))return;statistics.longCollections++;Gain(CraftBranch.Crumbs,length*config.collectionExpPerCm,"長い一本を回収");}
  public void Rolled(float rawGrams){float gain=Mathf.Min(Mathf.Max(0,rawGrams)*config.ballExpPerGram,Mathf.Max(0,config.ballExpCap-ballAward));ballAward+=gain;Gain(CraftBranch.Crumbs,gain,"細かなカスを玉へ加工");}
  public void Sold(long price){float gain=Mathf.Min(Mathf.Max(0,price)*config.saleExpPerYen,Mathf.Max(0,config.saleExpCap-saleAward));saleAward+=gain;Gain(CraftBranch.Crumbs,gain,"回収品を売却");}
  public bool Buy(SkillKind kind){var d=catalog.Find(kind);var b=banks[(int)d.branch];if(Level(kind)>=Mathf.Min(10,d.maxLevel)||b.availableSP<d.cost)return false;b.availableSP-=d.cost;levels[(int)kind]++;return true;}
  public void ResetSkills(){foreach(var d in catalog.skills){banks[(int)d.branch].availableSP+=Level(d.kind)*d.cost;levels[(int)d.kind]=0;}}
  public void DebugAddSP(int amount){if(amount<=0)return;foreach(var b in banks){b.availableSP+=amount;b.debugSP+=amount;}}
  public void DebugPreset(int level){ResetSkills();level=Mathf.Clamp(level,0,10);foreach(var d in catalog.skills){var b=banks[(int)d.branch];int needed=level*d.cost-b.availableSP;if(needed>0){b.debugSP+=needed;b.availableSP+=needed;}for(int i=0;i<level;i++)Buy(d.kind);}}
  public void Apply(PerformanceModifiers m){m.erasePower=Effect(SkillKind.Efficiency);m.radius=Effect(SkillKind.Range);m.wear=Effect(SkillKind.Wear);m.paperDamage=Effect(SkillKind.PaperCare);m.protectedGlance=Effect(SkillKind.InkCare);m.cornerWear=Effect(SkillKind.CornerCare);m.longGrowth=Effect(SkillKind.LongGrowth);m.crumbTolerance=Effect(SkillKind.Continuity);m.crumbAmount=Effect(SkillKind.CrumbAmount);m.ballYield=Effect(SkillKind.BallYield);}
 }
}
