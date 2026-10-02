using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Keshiya {
 public sealed class RuntimeExternalChecks:MonoBehaviour {
  public PrototypeGame Game;int count,fail;string dir;StringBuilder report=new StringBuilder();
  void Check(bool ok,string text){count++;if(!ok)fail++;report.AppendLine((ok?"PASS ":"FAIL ")+text);File.WriteAllText(Path.Combine(dir,"runtime.txt"),report+$"Checks={count}; Failures={fail}\n");Debug.Log((ok?"PASS ":"FAIL ")+text);}
  IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));yield return new WaitForSecondsRealtime(.2f);}
  Vector2 FirstTarget(){var ink=Game.Paper.Drawing.Ink;for(int i=ink.Length-1;i>=0;i--)if(ink[i]>.2f)return new Vector2(((i%Game.Paper.Width+.5f)/Game.Paper.Width-.5f)*Game.Paper.Size.x,((i/Game.Paper.Width+.5f)/Game.Paper.Height-.5f)*Game.Paper.Size.y);return Vector2.zero;}
  IEnumerator Start(){Application.runInBackground=true;dir=Path.Combine(Application.dataPath,"../TestResults-External");Directory.CreateDirectory(dir);Game.Controller.ExternalInput=true;yield return new WaitForSecondsRealtime(.5f);Game.Economy.Wallet.DebugCredit(10000);Game.PurchaseTool(3);Game.SetCarryLimit(false);Check(Game.Session.Phase==WorkPhase.Board,"External board at startup");yield return Capture("00-board");
   foreach(int index in Enumerable.Range(0,Game.Jobs.Length).Where(i=>Game.Jobs[i].externalTest)){
    var job=Game.Jobs[index];Game.ChooseJob(index);yield return Capture(job.id+"-detail");Game.SelectTool(job.precision?3:0);Game.BeginWork();Game.SelectMode(job.precision?ContactMode.Corner:ContactMode.Face);yield return null;Check(Game.Paper.Layered&&Game.Working&&!Game.InputBlocked,job.id+" layer job starts");Check(!Game.DebugEraseAreaVisible&&!Game.Presentation.ContactGuideVisible,job.id+" no leaked debug guide");yield return Capture(job.id+"-initial");if(job.suppliedTool!=null){var middle=(Vector2)Game.Viewport.View.WorldToScreenPoint(new Vector3(0,.058f,0));Game.Viewport.SetZoom(4,middle);yield return Capture(job.id+"-name-label");Game.Viewport.ResetView();}var target=FirstTarget();var camera=Game.Viewport.View;var screen=(Vector2)camera.WorldToScreenPoint(new Vector3(target.x,.058f,target.y));var size=Game.Contact.HalfSize;Game.Viewport.SetZoom(4,screen);Check(Game.Contact.HalfSize==size,job.id+" zoom retains world contact size");var projected=camera.WorldToScreenPoint(new Vector3(target.x,.058f,target.y));Check(Vector2.Distance(Game.Viewport.PaperPoint(projected),target)<.0001f,job.id+" zoom ray world roundtrip");Game.Viewport.Pan(new Vector2(800,400),new Vector2(850,420));projected=camera.WorldToScreenPoint(new Vector3(target.x,.058f,target.y));Check(Vector2.Distance(Game.Viewport.PaperPoint(projected),target)<.0001f,job.id+" pan ray world roundtrip");yield return Capture(job.id+"-zoom");Game.Viewport.ResetView();
    if(job.precision){Game.Assist.ToggleSample();Check(Game.Assist.SampleTexture==job.artwork.completePreview&&Game.InputBlocked,job.id+" completion preview blocks rubbing");yield return Capture(job.id+"-sample");Game.OpenShop(true);Game.CloseShop();Check(Game.InputBlocked&&Game.Assist.SampleOpen,job.id+" sample stays modal after inventory");Game.Assist.ToggleSample();
     foreach(var mode in new[]{ContactMode.Face,ContactMode.Edge,ContactMode.Corner}){Game.SelectMode(mode);var before=Game.Paper.Drawing.Erased;Game.Rub(target,target+new Vector2(.015f,0),2);Check(Game.Paper.Drawing.Erased>before,job.id+" real "+mode+" stroke erases");}Game.RetryWork();Game.BeginWork();
    }
    Game.SelectMode(job.precision?ContactMode.Corner:ContactMode.Face);int pass=0;
    for(;pass<120&&!Game.CurrentJob.CanComplete(Game.Paper);pass++){
     if(!Game.HasUsableTool&&Game.Supplied==null){Game.NextTool();Game.BorrowTool();Game.SelectMode(job.precision?ContactMode.Corner:ContactMode.Face);}
     if(job.precision){int n=0;foreach(var path in job.artwork.testStrokes){for(int k=1;k<path.points.Length;k++)Game.Rub(path.points[k-1],path.points[k],2);if(++n%35==0)yield return null;}}
     else for(float y=-Game.Paper.Size.y*.5f+.04f;y<Game.Paper.Size.y*.5f;y+=.24f){Game.Rub(new Vector2(-2.46f,y),new Vector2(2.46f,y),3);yield return null;}
     yield return null;
    }
    Game.Paper.Refresh();Check(Game.CurrentJob.CanComplete(Game.Paper),job.id+$" 95 percent with real strokes ({Game.Paper.Drawing.Erased:P2}, passes {pass})");Check(!Game.Paper.Severe,job.id+" safe-speed paper preserved");if(job.precision)Check(Game.Paper.Protection.Loss<.05f,job.id+$" precise completion protects text ({Game.Paper.Protection.Loss:P3})");if(job.suppliedTool!=null)Check(Game.CurrentJob.SupplyProgress==1,job.id+" named eraser consumed by graphite work");yield return Capture(job.id+"-erased");long beforeMoney=Game.Economy.Wallet.Balance;Game.CurrentJob.Tick(100000);Game.Finish();Check(Game.Session.Phase==WorkPhase.Result&&Game.Economy.Wallet.Balance>beforeMoney,job.id+" result and reward");if(Game.CurrentJob.Completed)Check(Game.CurrentJob.Result.basic==job.baseReward&&Game.CurrentJob.Result.speed==0,job.id+" no late penalty");yield return Capture(job.id+"-result");Game.RetryWork();Game.BeginWork();Check(Game.Paper.Drawing.Erased==0&&!Game.CurrentJob.Completed,job.id+" retry restores art");Game.ShowBoard();Game.Tools.DebugFresh();System.GC.Collect();yield return null;
   }
   Game.OpenShop();yield return Capture("90-shop");Game.CloseShop();Game.OpenSkills();yield return Capture("91-skills");Game.CloseSkills();Application.Quit(fail==0?0:1);
  }
 }
}
