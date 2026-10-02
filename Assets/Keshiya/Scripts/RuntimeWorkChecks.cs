using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
namespace Keshiya {
 public sealed class RuntimeWorkChecks:MonoBehaviour {
  public PrototypeGame Game;int count,fail;string directory;readonly StringBuilder report=new StringBuilder();
  void Awake(){Application.runInBackground=true;}
  void Check(bool ok,string name){count++;if(!ok)fail++;report.AppendLine((ok?"PASS: ":"FAIL: ")+name);Debug.Log((ok?"PASS: ":"FAIL: ")+name);}
  IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));yield return new WaitForSecondsRealtime(.25f);}
  void EnsureUsable(){if(Game.HasUsableTool)return;if(Game.PurchaseTool(Game.ToolIndex))Game.SelectOwned(Game.Tools.items[Game.Tools.items.Count-1].instanceId);else Game.NextTool();}
  IEnumerator Start(){directory=Path.Combine(Application.dataPath,"../TestResults-0.4");Directory.CreateDirectory(directory);Game.Controller.ExternalInput=true;yield return new WaitForSecondsRealtime(.7f);
   Check(Game.BoardEnabled&&Game.Session.Phase==WorkPhase.Board&&Game.InputBlocked,"Normal launch starts at protected job board");Check(Game.Jobs.Length>=5,"Five jobs available");yield return Capture("01-board");
   for(int index=0;index<5;index++){
    Game.ChooseJob(index);Check(Game.Session.Phase==WorkPhase.Detail&&Game.Session.Selected==index,"Board selects detail "+index);Game.SelectTool(index==2?0:1);
    // 0.6 tools are consumable: keep the original five-job assertions, buying
    // a replacement from earned money when the selected copy is below half.
    if(Game.ActiveState.Remaining(Game.Eraser,Game.Modifiers)<.5f&&Game.PurchaseTool(Game.ToolIndex))Game.SelectOwned(Game.Tools.items[Game.Tools.items.Count-1].instanceId);
    yield return Capture("02-detail-"+index);
    float untouched=Game.Paper.Drawing.Erased;Game.Rub(Vector2.zero,Vector2.one,4);Check(Game.Paper.Drawing.Erased==untouched,"Detail blocks work "+index);
    Game.BeginWork();Check(Game.Session.Phase==WorkPhase.Work&&!Game.InputBlocked&&Game.JobIndex==index,"Start loads chosen work "+index);yield return Capture("03-work-"+index);
    Game.SelectMode(Game.Definition.precision?ContactMode.Corner:ContactMode.Face);
    if(Game.Definition.precision){var paths=Game.Definition.customDrawing?Game.Definition.targetPaths:System.Array.ConvertAll(LetterLayout.Target,p=>new JobPath(p));
     for(int pass=0;pass<45&&!Game.CurrentJob.CanComplete(Game.Paper);pass++){foreach(var path in paths)for(int k=1;k<path.points.Length;k++){EnsureUsable();Game.Rub(path.points[k-1],path.points[k],3);}yield return null;}
     Check(Game.Paper.Protection.Loss==0,"Precision target erased while protected artwork survives "+index);
    }else{
     for(int pass=0;pass<12&&!Game.CurrentJob.CanComplete(Game.Paper);pass++){for(float y=-2.05f;y<=2.05f;y+=.13f){EnsureUsable();Game.Rub(new Vector2(-2.95f,y),new Vector2(2.95f,y),4);}yield return null;}
    }
    Check(Game.CurrentJob.CanComplete(Game.Paper),"Actual paper reaches 95 percent "+index);Game.CurrentJob.Tick(10000);long wallet=Game.Economy.Wallet.Balance;Game.Finish();
    Check(Game.Session.Phase==WorkPhase.Result&&Game.CurrentJob.Completed,"Work reaches result "+index);Check(Game.CurrentJob.Result.basic==Game.Definition.baseReward&&Game.CurrentJob.Result.speed==0,"Full base reward despite slow completion "+index);Check(Game.Economy.Wallet.Balance==wallet+Game.CurrentJob.Result.Total,"Job reward credited once "+index);
    Game.Finish();Check(Game.Economy.Wallet.Balance==wallet+Game.CurrentJob.Result.Total,"Repeat finish cannot duplicate payment "+index);yield return Capture("04-result-"+index);
    Game.OpenTrade();Check(Game.TradeOpen&&Game.InputBlocked,"Result opens buyer "+index);Game.RollCrumbs();long before=Game.Economy.Wallet.Balance;long price=Game.Economy.SellBall();Check(Game.Economy.Wallet.Balance==before+price&&Game.CurrentJob.Result.basic==Game.Definition.baseReward,"Crumb sale is separate income "+index);if(index==4)yield return Capture("05-trade");Game.CloseTrade();Game.ShowBoard();Check(Game.Session.Phase==WorkPhase.Board&&Game.InputBlocked,"Return to next job board "+index);
   }
   Check(Game.Session.CompletedCount==5,"Session tracks five completed jobs");long balance=Game.Economy.Wallet.Balance;Game.ChooseJob(1);Game.BeginWork();Check(!Game.CurrentJob.Completed&&Game.Paper.Drawing.Erased==0&&Game.Economy.Wallet.Balance==balance,"Same job can restart without losing wallet");
   Game.ShowBoard();yield return Capture("06-return-board");File.WriteAllText(Path.Combine(directory,"runtime.txt"),report+$"Checks={count}; Failures={fail}; Wallet={balance}\n");Application.Quit(fail==0?0:1);
  }
 }
}
