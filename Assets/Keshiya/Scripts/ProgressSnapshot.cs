using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Keshiya {
 [Serializable] public sealed class HistoryEntry {public string jobId;public JobRecord record;}
 [Serializable] public sealed class WorkSnapshot {public int completedCount,qualityPoints;public HistoryEntry[] history;public long[] recorded;}
 [Serializable] public sealed class ProgressSnapshot {public string playerJson;public WorkSnapshot work;public long[] formed,collected,completed;}
 [Serializable] public sealed class EconomySnapshot {
  public CrumbPiece[] inventory;public float loose,produced,sold,discarded,best,longest,bonus,ball,ballLifetime,overflowMass,overflowLongest;
  public long jobId,nextId,jobSales,overflowPrice;public int overflowCount;
 }
 public sealed partial class WorkSession {
  internal WorkSnapshot ExportSave()=>new WorkSnapshot{completedCount=CompletedCount,qualityPoints=QualityPoints,history=history.Select(x=>new HistoryEntry{jobId=x.Key,record=x.Value}).ToArray(),recorded=recorded.ToArray()};
  internal void ImportSave(WorkSnapshot s){history.Clear();foreach(var h in s.history)history.Add(h.jobId,h.record);recorded.Clear();recorded.UnionWith(s.recorded);CompletedCount=s.completedCount;QualityPoints=s.qualityPoints;Selected=0;ShowBoard();}
 }
 public sealed partial class PlayerProgress {
  public ProgressSnapshot ExportSave(){if(work.Phase!=WorkPhase.Board)throw new InvalidOperationException("Save only from the job board");return new ProgressSnapshot{playerJson=JsonUtility.ToJson(this),work=work.ExportSave(),formed=formed.ToArray(),collected=collected.ToArray(),completed=completed.ToArray()};}
  public void ImportSave(ProgressSnapshot s){var restored=JsonUtility.FromJson<PlayerProgress>(s.playerJson);JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(restored.wallet),wallet);JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(restored.tools),tools);banks=restored.banks;levels=restored.levels;statistics=restored.statistics;work.ImportSave(s.work);formed.Clear();formed.UnionWith(s.formed);collected.Clear();collected.UnionWith(s.collected);completed.Clear();completed.UnionWith(s.completed);BeginJob();notification="";notificationRevision++;}
 }
 public sealed partial class CrumbEconomy {
  public EconomySnapshot ExportSave(){if(paper.Count>0||blown.Count>0||PaperFineGrams>0||blownFine>0||Growing!=null)throw new InvalidOperationException("Collect or settle crumbs before saving");return new EconomySnapshot{inventory=inventory.Select(p=>p.Copy()).ToArray(),loose=LooseGrams,produced=ProducedGrams,sold=SoldGrams,discarded=DiscardedGrams,best=BestLength,longest=JobLongest,bonus=ConversionBonusGrams,ball=Ball.Grams,ballLifetime=Ball.LifetimeAddedGrams,overflowMass=overflowMass,overflowLongest=OverflowLongest,jobId=JobId,nextId=nextId,jobSales=JobSales,overflowPrice=OverflowPrice,overflowCount=OverflowCount};}
  public void ImportSave(EconomySnapshot s){paper.Clear();blown.Clear();inventory.Clear();inventory.AddRange(s.inventory.Select(p=>p.Copy()));Growing=null;PaperFineGrams=blownFine=now=rescueUntil=graphiteCredit=materialCredit=0;lastStroke=-100;priorTool=null;LooseGrams=s.loose;ProducedGrams=s.produced;SoldGrams=s.sold;DiscardedGrams=s.discarded;BestLength=s.best;JobLongest=s.longest;ConversionBonusGrams=s.bonus;Ball.RestoreSave(s.ball,s.ballLifetime);overflowMass=s.overflowMass;OverflowLongest=s.overflowLongest;JobId=s.jobId;nextId=s.nextId;JobSales=s.jobSales;OverflowPrice=s.overflowPrice;OverflowCount=s.overflowCount;Motion.Reset();}
 }
}
