using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Keshiya.Editor {
 public static class WorldChecks {
  static int n;static StringBuilder log;
  static void Check(bool ok,string name){n++;log.AppendLine((ok?"PASS: ":"FAIL: ")+name);File.WriteAllText("TestResults/world-checks.txt",$"Checks={n}\n"+log);if(!ok)throw new Exception(name);}
  public static void Run(){n=0;log=new StringBuilder();var cat=Resources.Load<ToolCatalog>("ToolCatalog");var t=cat.tools;var inv=new ToolInventory();inv.Initialize(cat);inv.loadout.limited=true;var wallet=new Wallet();wallet.DebugCredit(100000);
   Check(t.Length==17&&t.Count(x=>!x.questOnly)==16,"16 shop tools plus separate supplied tool");
   foreach(var pair in new[]{("パンシリーズ",3),("野菜シリーズ",2),("どうぶつシリーズ",2)})Check(t.Count(x=>x.series==pair.Item1)==pair.Item2,"Series membership "+pair.Item1);
   foreach(var d in t.Skip(7)){Check(!string.IsNullOrEmpty(d.category)&&!string.IsNullOrEmpty(d.description)&&d.contactProfile!=null&&d.edgeProfile!=null&&d.cornerProfile!=null,"Complete data "+d.id);Check(d.Power(new PerformanceModifiers())>0&&d.price>0,"Usable positive values "+d.id);if(!d.questOnly){Check(inv.Discovery(d.id)=="未発見","Undiscovered "+d.id);long cash=wallet.Balance;Check(inv.Purchase(d,wallet,out var item)&&wallet.Balance==cash-d.price,"Buy "+d.id);Check(inv.Discovery(d.id)=="購入済み","Purchase catalog "+d.id);inv.RecordUse(item,1,12);Check(inv.Discovery(d.id)=="使用済み","Use catalog "+d.id);}}
   Check(!inv.Purchase(t[16],wallet,out _),"Supplied item not sold");Check(inv.CollectedSeries(cat,"パンシリーズ")==3,"Bread collection count");Check(inv.loadout.selected.Count==5,"Purchases cannot exceed five packed individuals");Check(!inv.loadout.Add(inv.items.Last()),"Sixth item rejected");inv.loadout.limited=false;Check(inv.loadout.Contains(inv.items.Last().instanceId),"Unlimited admits owned tools");
   var knead=new EraserState();var bread=new EraserState();knead.special.Advance(t[14],.2f,20);bread.special.Advance(t[15],.2f,20);Check(knead.special.graphiteLoad>0&&knead.special.toastLevel==0,"Graphite state independent");Check(bread.special.toastLevel>0&&bread.special.graphiteLoad==0,"Toast state independent");Check(knead.special.Power(t[14])>1&&knead.special.Damage(t[14])==1,"Kneaded graphite conditions absorption without damage boost");Check(bread.special.Power(t[15])>1&&bread.special.Damage(t[15])>1,"Bread hardens with power and risk");Check(knead.special.Color(t[14])!=t[14].bodyColor&&bread.special.Color(t[15])!=t[15].bodyColor,"Both state colors change");
   inv.items[0].state=knead;inv.items[1].state=bread;var copy=JsonUtility.FromJson<ToolInventory>(JsonUtility.ToJson(inv));Check(copy.items[0].state.special.graphiteLoad==knead.special.graphiteLoad&&copy.items[1].state.special.toastLevel==bread.special.toastLevel,"Individual states survive serialization");copy.items[0].state.special.graphiteLoad=0;Check(knead.special.graphiteLoad>0,"Saved states are independent copies");
   var blank=new SpecialToolState();blank.Advance(t[14],0,10000);Check(blank.graphiteLoad==0,"White paper does not dirty kneaded eraser");blank.Advance(t[15],0,100000);Check(blank.toastLevel==1&&blank.Power(t[15])<2&&blank.Damage(t[15])<3,"Bread state capped");Check(t[14].paperDamageMultiplier<t[0].paperDamageMultiplier*.1f&&t[14].erasePower<t[0].erasePower,"Kneaded is slow and safe");
   var config=Resources.Load<PrototypeConfig>("PrototypeConfig");var jobs=Resources.LoadAll<JobDefinition>("");Check(jobs.Length>=20,"Twenty jobs in assets");foreach(var j in jobs.Where(x=>x.suppliedTool!=null)){var job=new Job(config,j);job.RecordSupply(0);Check(job.SupplyProgress==0,"Blank wear not credited");job.RecordSupply(.5f);Check(job.SupplyProgress>0&&job.SupplyProgress<1,"Real erasure advances supplied objective");job.RecordSupply(.45f);Check(job.SupplyProgress==1,"95 percent consumes calibrated supplied remainder");}
   var use=new ToolUsage{instanceId="one",definitionId="ordinary",graphiteArea=.3f};use.modeDistance[2]=7;var cloned=use.Copy();use.modeDistance[2]=0;Check(cloned.modeDistance[2]==7,"Usage modes deep copied");Check(t[5].price==7800,"Premium price preserved");Debug.Log("WORLD_CHECKS_OK "+n);
  }
 }
}
