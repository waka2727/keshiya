using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
namespace Keshiya {
 public sealed class RuntimeBalanceChecks:MonoBehaviour {
  public PrototypeGame Game;int count,fail;string directory;readonly StringBuilder report=new StringBuilder();
  void Awake(){Application.runInBackground=true;}
  void Check(bool ok,string name){count++;if(!ok)fail++;report.AppendLine((ok?"PASS: ":"FAIL: ")+name);Debug.Log((ok?"PASS: ":"FAIL: ")+name);}
  IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));yield return new WaitForSecondsRealtime(.25f);}
  IEnumerator Start(){directory=Path.Combine(Application.dataPath,"../TestResults-0.6.1");Directory.CreateDirectory(directory);Game.Controller.ExternalInput=true;yield return new WaitForSecondsRealtime(.7f);
   Check(Game.BoardEnabled&&Game.Modifiers.erasePower==.9f,"Normal startup applies actual Lv0 multiplier");float[] erased=new float[3];int[] levels={0,5,10};
   for(int i=0;i<3;i++){Game.Progress.DebugPreset(levels[i]);Game.RefreshSkills();Game.ChooseJob(1);Game.BeginWork();Game.SelectTool(0);Game.SelectMode(ContactMode.Face);Game.Rub(new Vector2(.14f,-.38f),new Vector2(.14f,.38f),3);erased[i]=Game.Paper.Drawing.Erased;Check(Game.Presentation.Footprint.HalfSize==Game.Contact.HalfSize,"Existing visual contact kept Lv"+levels[i]);yield return Capture("01-efficiency-"+levels[i]);}
   Check(erased[1]>erased[0]*1.4f&&erased[2]>erased[1]*1.2f,"Actual game has clear Lv0/5/10 gains");Game.OpenSkills();Game.SkillsHUD.ToggleDebug();yield return Capture("02-exp-and-presets");Game.CloseSkills();
   Game.Progress.DebugPreset(0);Game.RefreshSkills();Game.ChooseJob(0);Game.BeginWork();Game.SelectTool(0);Game.SelectMode(ContactMode.Face);Game.ActiveState.DebugRemaining(1,Game.Eraser,Game.Modifiers);for(int pass=0;pass<8&&!Game.CurrentJob.CanComplete(Game.Paper);pass++){for(float y=-2.05f;y<=2.05f;y+=.13f)Game.Rub(new Vector2(-2.95f,y),new Vector2(2.95f,y),4);yield return null;}
   Check(Game.CurrentJob.CanComplete(Game.Paper),"Lv0 ordinary can complete broad job with finite body");Check(Game.ActiveState.Remaining(Game.Eraser,Game.Modifiers)<.85f&&Game.HasUsableTool,"Work leaves clearly used but usable ordinary");long wallet=Game.Economy.Wallet.Balance;Game.CurrentJob.Tick(100000);Game.Finish();Check(Game.CurrentJob.Completed&&Game.Economy.Wallet.Balance==wallet+Game.CurrentJob.Result.Total&&Game.CurrentJob.Result.speed==0,"Slow Lv0 work earns full job reward");yield return Capture("03-earned-exp");Game.OpenSkills();yield return Capture("04-grouped-exp");Game.CloseSkills();
   Game.Economy.Wallet.DebugCredit(10000);Game.OpenShop();Game.ShopHUD.ChooseProduct(5);Check(Game.ShopHUD.BuySelected(),"Can buy retuned premium from real shop");yield return Capture("05-premium");Game.OpenShop(true);Game.SelectOwned(Game.Tools.items[Game.Tools.items.Count-1].instanceId);float corner=Game.ActiveState.CornerSharpness;Game.ActiveState.DebugRemaining(.2f,Game.Eraser,Game.Modifiers);Check(Mathf.Abs(Game.ActiveState.Remaining(Game.Eraser,Game.Modifiers)-.2f)<.001f&&Game.ActiveState.CornerSharpness==corner,"Independent remaining comparison preset");Game.ActiveState.DebugSharpness(.1f);Check(Game.ActiveState.CornerSharpness==.1f,"Ten percent corner comparison preset");Game.ShopHUD.ToggleDebug();yield return Capture("06-worn-state");Game.CloseShop();
   Game.ChooseJob(1);Game.BeginWork();Game.SelectTool(0);Game.Progress.DebugPreset(10);Game.RefreshSkills();Check(!Game.Sense(),"High sensing does not activate on fresh paper");for(int i=0;i<Game.Paper.Drawing.Ink.Length;i++)Game.Paper.Drawing.Erase(i,Game.Paper.Drawing.Ink[i]*.65f);Check(Game.Sense()&&Game.SenseUntil-Time.unscaledTime>6,"Lv10 sensing works from 65 percent for over six seconds");yield return Capture("07-sense");yield return new WaitForSecondsRealtime(6.6f);Check(Game.Paper.RevealStrength==0,"Sensing expires and never becomes permanent");
   File.WriteAllText(Path.Combine(directory,"runtime.txt"),report+$"Checks={count}; Failures={fail}; Erase0={erased[0]}; Erase5={erased[1]}; Erase10={erased[2]}\n");Application.Quit(fail==0?0:1);
  }
 }
}
