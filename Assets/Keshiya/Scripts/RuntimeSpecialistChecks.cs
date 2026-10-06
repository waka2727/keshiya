using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
namespace Keshiya {
 public sealed class RuntimeSpecialistChecks:MonoBehaviour {
  public PrototypeGame Game;int count,fail;string directory;readonly StringBuilder report=new StringBuilder();
  void Awake(){Application.runInBackground=true;}
  void Check(bool ok,string name){count++;if(!ok)fail++;report.AppendLine((ok?"PASS: ":"FAIL: ")+name);Debug.Log((ok?"PASS: ":"FAIL: ")+name);}
  IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));yield return new WaitForSecondsRealtime(.25f);}
  IEnumerator Start(){directory=Path.Combine(Application.dataPath,"../TestResults-0.7");Directory.CreateDirectory(directory);Game.Controller.ExternalInput=true;yield return new WaitForSecondsRealtime(.7f);
   Check(Game.BoardEnabled&&Game.Session.Phase==WorkPhase.Board&&Game.Jobs.Length>=12,"Twelve contracts available on startup board");yield return Capture("01-board-top");Game.WorkHUD.BoardScroll=new Vector2(0,600);yield return Capture("02-board-bottom");Check(Game.WorkHUD.BoardScroll.y>500,"Board scroll reaches lower specialist cards");
   for(int index=5;index<12;index++){Game.ChooseJob(index);Check(Game.Session.Phase==WorkPhase.Detail&&Game.Session.Selected==index,"Open specialist detail / "+index);yield return Capture("03-detail-"+index);Game.SelectTool(0);Game.BeginWork();Check(Game.JobIndex==index&&Game.Definition.paper!=null&&Game.Definition.writing!=null,"Material pair loads / "+index);yield return Capture("04-work-"+index);Game.ShowBoard();}
   Game.ChooseJob(5);Game.BeginWork();Game.SelectTool(0);Game.SelectMode(ContactMode.Corner);var initial=Game.ActiveState.CornerSharpness;for(int pass=0;pass<100&&!Game.CurrentJob.CanComplete(Game.Paper);pass++){foreach(var path in Game.Definition.targetPaths)for(int n=1;n<path.points.Length;n++){var a=path.points[n-1];var b=path.points[n];Game.Rub(pass%2==0?a:b,pass%2==0?b:a,3);}yield return null;}
   Check(Game.CurrentJob.CanComplete(Game.Paper)&&Game.Paper.Protection.Loss<.005f,"Lv0 ordinary corner completes close lettering with protected neighbors");Check(Game.ActiveState.CornerSharpness<initial&&Game.HasUsableTool,"Precision work consumes a usable individual corner");Check(Game.Progress.jobExp[1]>0,"Specialist removal grants precision EXP");long before=Game.Economy.Wallet.Balance;Game.CurrentJob.Tick(100000);Game.Finish();Check(Game.CurrentJob.Completed&&Game.Session.Phase==WorkPhase.Result&&Game.CurrentJob.Result.basic==850&&Game.CurrentJob.Result.speed==0,"Slow specialist work reaches result and keeps base pay");Check(Game.Economy.Wallet.Balance==before+Game.CurrentJob.Result.Total,"Specialist reward credits wallet once");yield return Capture("05-letter-result");Game.OpenSkills();yield return Capture("06-precision-exp");Game.CloseSkills();Game.OpenTrade();Check(Game.TradeOpen,"Specialist reward connects to existing buyer");Game.CloseTrade();Game.ShowBoard();
   Game.ChooseJob(7);Game.BeginWork();Game.SelectMode(ContactMode.Edge);Game.RotateTool(45);Check(Mathf.Approximately(Game.Contact.Angle,Game.Yaw)&&Game.Presentation.Footprint.HalfSize==Game.Contact.HalfSize,"Rotated visible edge and actual footprint agree");yield return Capture("07-rotated-edge");
   Game.ShowBoard();Game.OpenShop();Check(Game.Catalog.tools.Length>=7&&Game.Catalog.tools[5].price==7800,"Shop retains seven tools and premium price");Game.CloseShop();Game.OpenSkills();Game.SkillsHUD.ToggleDebug();Check(Game.Progress.Level(SkillKind.Efficiency)==0,"Skills remain Lv0 until player changes them");yield return Capture("08-skills-debug");Game.CloseSkills();Game.ShowBoard();
   File.WriteAllText(Path.Combine(directory,"runtime.txt"),report+$"Checks={count}; Failures={fail}\n");Application.Quit(fail==0?0:1);
  }
 }
}

