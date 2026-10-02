using System;
using System.Collections.Generic;
using UnityEngine;
namespace Keshiya {
 [Serializable] public sealed class OwnedEraser {
  public string instanceId,definitionId;public EraserState state=new EraserState{allowExhaustion=true};
  public bool loan,favorite;public int jobsUsed;public long lastJob;public float longestCm;
  public string Condition=>state.Exhausted?"使い切り":state.Travel==0&&state.UsedUnits==0&&state.CornerSharpness>=1?"新品":state.CornerSharpness<.5f?"角が丸い":"使いかけ";
 }
 [Serializable] public sealed class ToolPurchase {public string instanceId,definitionId;public int paid;public bool debug;}
 [Serializable] public sealed class ToolRecord {public string definitionId;public bool discovered,purchased;public int uses;public long lastJob;public float longestCm;public string bestGrade="—";}
 // Save-friendly IDs and lists; no scene objects in individual inventory records.
 [Serializable] public sealed class ToolInventory {
  public List<OwnedEraser> items=new List<OwnedEraser>();
  public List<ToolPurchase> purchases=new List<ToolPurchase>();
  public List<ToolRecord> records=new List<ToolRecord>();
  public ToolLoadout loadout=new ToolLoadout();
  public string selectedId;public int revision;
  public string Discovery(string id){var r=Record(id);return r.uses>0?"使用済み":r.purchased?"購入済み":r.discovered?"発見済み":"未発見";}
  public int CollectedSeries(ToolCatalog catalog,string series){int count=0;foreach(var d in catalog.tools)if(d.series==series&&Record(d.id).purchased)count++;return count;}
  public ToolRecord Record(string id){var r=records.Find(x=>x.definitionId==id);if(r==null){r=new ToolRecord{definitionId=id};records.Add(r);}return r;}
  public OwnedEraser Find(string id)=>items.Find(x=>x.instanceId==id);
  public OwnedEraser Selected=>Find(selectedId);
  public bool AnyUsable=>items.Exists(x=>!x.state.Exhausted);
  public OwnedEraser Add(EraserDefinition d,bool loan=false){var item=new OwnedEraser{instanceId=Guid.NewGuid().ToString("N"),definitionId=d.id,loan=loan};items.Add(item);Record(d.id).discovered=true;loadout.Add(item);revision++;if(selectedId==null)selectedId=item.instanceId;return item;}
  public void Initialize(ToolCatalog catalog){foreach(var d in catalog.tools)Record(d.id);if(items.Count==0)for(int i=0;i<Math.Min(3,catalog.tools.Length);i++)Add(catalog.tools[i]);}
  public bool Select(string id){var item=Find(id);if(item==null||item.state.Exhausted)return false;selectedId=id;return true;}
  public OwnedEraser OfType(string id){if(Selected!=null&&Selected.definitionId==id&&!Selected.state.Exhausted)return Selected;return items.Find(x=>x.definitionId==id&&!x.state.Exhausted);}
  public bool Purchase(EraserDefinition d,Wallet wallet,out OwnedEraser item){item=null;if(d==null||d.questOnly||string.IsNullOrEmpty(d.id)||d.price<=0||!wallet.TrySpend(d.price))return false;item=Add(d);Record(d.id).purchased=true;purchases.Add(new ToolPurchase{definitionId=d.id,instanceId=item.instanceId,paid=d.price});return true;}
  public void RecordUse(OwnedEraser item,long job,float length){var r=Record(item.definitionId);if(item.lastJob!=job){item.lastJob=job;item.jobsUsed++;}if(r.lastJob!=job){r.lastJob=job;r.uses++;}item.longestCm=Mathf.Max(item.longestCm,length);r.longestCm=Mathf.Max(r.longestCm,length);}
  public void RecordLength(string definitionId,string instanceId,float length){if(string.IsNullOrEmpty(definitionId))return;var r=Record(definitionId);r.longestCm=Mathf.Max(r.longestCm,length);var item=Find(instanceId);if(item!=null)item.longestCm=Mathf.Max(item.longestCm,length);}
  public void Complete(long job,string grade){foreach(var item in items)if(item.lastJob==job){var r=Record(item.definitionId);if(r.bestGrade=="—"||"SABC".IndexOf(grade)<"SABC".IndexOf(r.bestGrade))r.bestGrade=grade;}}
  public OwnedEraser Rescue(EraserDefinition ordinary){if(AnyUsable)return null;var item=items.Find(x=>x.loan);if(item==null)item=Add(ordinary,true);else item.state=new EraserState{allowExhaustion=true};Select(item.instanceId);revision++;return item;}
  public void DebugFresh(){foreach(var item in items)item.state=new EraserState{allowExhaustion=true};revision++;}
  public void DebugAll(ToolCatalog catalog){foreach(var d in catalog.tools)if(!d.questOnly&&!items.Exists(x=>x.definitionId==d.id)){var item=Add(d);purchases.Add(new ToolPurchase{definitionId=d.id,instanceId=item.instanceId,debug=true});}}
  public void DebugReset(ToolCatalog catalog){items.Clear();loadout.selected.Clear();selectedId=null;Initialize(catalog);revision++;}
 }
}
