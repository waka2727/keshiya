using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Keshiya {
 public sealed class RuntimeUXChecks:MonoBehaviour {
  public PrototypeGame Game;int count,fail;string dir;StringBuilder report=new StringBuilder();
  void Check(bool ok,string name){count++;if(!ok)fail++;report.AppendLine((ok?"PASS ":"FAIL ")+name);File.WriteAllText(Path.Combine(dir,"runtime.txt"),report+$"Checks={count}; Failures={fail}\n");}
  IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));yield return new WaitForSecondsRealtime(.15f);}
  Vector2 Mark(float[] ink){int i=System.Array.FindIndex(ink,x=>x>.25f);return new Vector2(((i%Game.Paper.Width+.5f)/Game.Paper.Width-.5f)*Game.Paper.Size.x,((i/Game.Paper.Width+.5f)/Game.Paper.Height-.5f)*Game.Paper.Size.y);}
  static readonly string[] ExpectedQuotes={@"机の上に置いていた紙です。いつの間にか、町がひとつできていました。全部消して、また使えるようにお願いします。",@"書き終えてから、一文字多いことに気づきました。そこだけお願いできますか。ほかの言葉は、このまま届けたいんです。",@"下書きけしをおnがいします。

黒いペンのせnは完成線です。
薄い鉛筆線だけ消してくだしあ。

締切につては聞かないでｋださい。

……昨日でしあt。",@"母の料理ノートが出てきました。星と変な顔は、昔の私が描いたものです。レシピの文字は残してもらえますか。",@"この消しゴムを、仕事で使い切ってほしいんです。名前は見なくて大丈夫です。ちゃんと紙を消して使ってくださいね。",@"点数のところだけ、お願いします。百の「1」と、隣の最初の「0」だけです。答えは消さないでください。",@"この手紙は、渡さないことにしました。本文を全部消してください。紙だけは、また使おうと思います。",@"ずいぶん描き直したんですが、構図を変えることにしました。全部消して構いません。同じ紙で、もう一回やってみます。"};
  IEnumerator Start(){Application.runInBackground=true;Game.Controller.ExternalInput=true;dir=Path.Combine(Application.dataPath,"../TestResults-UX");Directory.CreateDirectory(dir);yield return new WaitForSecondsRealtime(.5f);
   Check(Game.Session.Phase==WorkPhase.Board,"Board startup");Game.Economy.Wallet.DebugCredit(100000);Game.Tools.DebugAll(Game.Catalog);Game.SetCarryLimit(false);
   foreach(int index in Enumerable.Range(0,Game.Jobs.Length).Where(i=>Game.Jobs[i].externalTest)){
    var job=Game.Jobs[index];Check(job.clientQuote==ExpectedQuotes[int.Parse(job.id.Substring(5))-1].Replace("\r\n","\n"),job.id+" exact original client letter retained");Game.ChooseJob(index);yield return Capture(job.id+"-detail");
    Check(WorkFlowHUD.LetterViewport.width>535&&WorkFlowHUD.LetterViewport.height>94,job.id+" enlarged letter viewport");Check(Game.WorkHUD.LetterContentHeight<=WorkFlowHUD.LetterViewport.height,job.id+" full client letter fits without clipping");Check(!string.IsNullOrEmpty(job.clientQuote)&&!string.IsNullOrEmpty(job.instruction)&&job.clientQuote!=job.instruction,job.id+" separate client and work text");
    Game.SelectTool(0);Game.SelectMode(ContactMode.Face);Game.BeginWork();yield return null;var target=Mark(Game.Paper.Drawing.Ink);var screen=(Vector2)Game.Viewport.View.WorldToScreenPoint(new Vector3(target.x,.058f,target.y));var footprint=Game.Contact;
    Game.Viewport.HandleWheel(3,true,screen);Check(Game.Viewport.Zoom>1,job.id+" Ctrl Wheel zoom");float zoom=Game.Viewport.Zoom;var center=Game.Viewport.Center;Game.Viewport.HandleWheel(1,false,screen);Check(Game.Viewport.Zoom==zoom&&Vector2.Distance(center,Game.Viewport.Center)>.01f,job.id+" Wheel pans only");
    Check(Game.Contact.HalfSize==footprint.HalfSize,job.id+" world footprint unchanged");var point=Game.Viewport.PaperPoint(Game.Viewport.View.WorldToScreenPoint(new Vector3(target.x,.058f,target.y)));Check(Vector2.Distance(point,target)<.0001f,job.id+" camera coordinate round trip");
    foreach(ContactMode mode in System.Enum.GetValues(typeof(ContactMode))){Game.SelectMode(mode);Game.Controller.ProcessInput(target,false,true,.016f);Game.ContactGuide.Refresh();Check(Game.ContactGuide.Visible,job.id+" "+mode+" hover guide");bool boundary=true;for(int i=0;i<96;i++){Vector2 p=Game.ContactGuide.Point(i);boundary&=Game.Contact.Weight(p)<.0001f;boundary&=Game.Contact.Weight((p-Game.Contact.Offset)*.98f+Game.Contact.Offset)>0;}Check(boundary,job.id+" "+mode+" guide matches actual hit boundary");}
    if(job.id=="TEST_003"){Game.Viewport.ResetView();Game.Viewport.SetZoom(4,(Vector2)Game.Viewport.View.WorldToScreenPoint(new Vector3(0,.058f,0)));foreach(ContactMode m in System.Enum.GetValues(typeof(ContactMode))){Game.SelectMode(m);Game.Controller.ProcessInput(Vector2.zero,false,true,.016f);yield return Capture("guide-close-"+m);}}Game.SelectMode(ContactMode.Edge);Game.RotateTool(45);Game.ContactGuide.Refresh();Check(Vector2.Distance(Game.ContactGuide.Point(0),Game.Contact.OutlinePoint(0))<.00001f,job.id+" rotated guide");var unchanged=Game.Contact;Game.ContactGuide.Enabled=false;Game.ContactGuide.Refresh();Check(!Game.ContactGuide.Visible&&Game.Contact.HalfSize==unchanged.HalfSize&&Game.Contact.Angle==unchanged.Angle,job.id+" guide off preserves hit area");Game.ContactGuide.Enabled=true;
    if(job.precision){Game.Assist.ToggleSample();yield return null;Check(Game.Assist.SampleOpen&&Game.InputBlocked,job.id+" preview opens and blocks erasure");var nav=Game.Assist.Preview;nav.Wheel(50,true,nav.Frame.center);Check(nav.Zoom==Game.Playtest.maximumZoom,job.id+" preview maximum matches work");var offset=nav.Offset;nav.Wheel(1,false,nav.Frame.center);Check(nav.Zoom==Game.Playtest.maximumZoom&&nav.Offset!=offset,job.id+" preview Wheel pans only");nav.Pan(new Vector2(100000,-100000));Check(nav.ImageRect.xMin<=.01f&&nav.ImageRect.xMax>=nav.Frame.width-.01f&&nav.ImageRect.yMin<=0&&nav.ImageRect.yMax>=nav.Frame.height-.01f,job.id+" preview pan bounded");yield return Capture(job.id+"-preview-zoom");Game.Assist.ToggleSample();}
    Game.Viewport.ResetView();Game.SelectMode(ContactMode.Corner);Game.Controller.ProcessInput(target,false,true,.016f);yield return Capture(job.id+"-guide");
    var before=JsonUtility.ToJson(Game.Tools);long cash=Game.Economy.Wallet.Balance;float exp=Game.Progress.banks.Sum(x=>x.total);long id=Game.Economy.JobId;
    // Restore snapshot includes the initial face mode, before test-only changes above.
    for(int k=0;k<6;k++)Game.Rub(target-Vector2.right*.08f,target+Vector2.right*.08f,30);
    Check(Game.Paper.Drawing.Erased>0,job.id+" real stroke changed ink");Game.RequestRestart();Check(Game.WorkHUD.AskRestart&&Game.InputBlocked,job.id+" restart confirmation");Game.OpenShop();Game.OpenSkills();Game.ContactGuide.Refresh();Check(!Game.ShopOpen&&!Game.SkillsOpen&&!Game.ContactGuide.Visible,job.id+" restart modal prevents panel and guide leakage");float erased=Game.Paper.Drawing.Erased;Game.CancelRestart();Check(!Game.WorkHUD.AskRestart&&!Game.InputBlocked&&Game.Paper.Drawing.Erased==erased,job.id+" cancel preserves work");Game.RequestRestart();yield return Capture(job.id+"-restart-dialog");Game.ConfirmRestart();
    Check(Game.Paper.Drawing.Erased==0&&Game.Paper.PeakDamage==0&&(Game.Paper.Protection==null||Game.Paper.Protection.Loss==0),job.id+" restore ink paper protection");Check(!Game.CurrentJob.Started&&Game.CurrentJob.Seconds==0&&Game.CurrentJob.Usage.Count==0,job.id+" restore work time and tool usage");Check(Game.Economy.Paper.Count==0&&Game.Economy.PaperFineGrams==0&&Game.Economy.JobId==id,job.id+" restore crumbs and transaction ID");Check(Game.Economy.Wallet.Balance==cash&&Game.Progress.banks.Sum(x=>x.total)==exp,job.id+" restore wallet EXP");Check(Game.Mode==ContactMode.Face&&!Game.WorkHUD.AskRestart&&!Game.InputBlocked,job.id+" restore initial mode and resume");
    Game.ShowBoard();
   }
   // Transaction rollback: even selling prior stock and buying during work cannot retain income while rewinding wear.
   Game.Economy.Stroke(Vector2.zero,Vector2.right*.2f,2,Game.Catalog.tools[0],ContactMode.Face,1,1,.1f);Game.Economy.Collect();Game.Economy.Roll();Game.ChooseJob(System.Array.FindIndex(Game.Jobs,x=>x.id=="TEST_006"));Game.SelectMode(ContactMode.Face);Game.BeginWork();
   string tools=JsonUtility.ToJson(Game.Tools),wallet=JsonUtility.ToJson(Game.Progress.wallet);float mass=Game.Economy.AccountedMass,ball=Game.Economy.Ball.Grams;float earned=Game.Progress.banks.Sum(x=>x.total);
   for(int repeat=0;repeat<2;repeat++){
    Check(Game.Economy.SellBall()>0,"sell prior ball before rollback "+repeat);Game.PurchaseTool(0);Game.Progress.Gain(CraftBranch.Erasing,500,"test");foreach(var tool in Game.Tools.items){tool.state.DebugRemaining(.2f,Game.Catalog.tools.First(x=>x.id==tool.definitionId),Game.Modifiers);tool.state.DebugSharpness(.1f);tool.state.special.graphiteLoad=.8f;tool.state.special.toastLevel=.9f;}
    var protection=Mark(Game.Paper.Protection.Ink);for(int k=0;k<20;k++)Game.Rub(protection-Vector2.right*.1f,protection+Vector2.right*.1f,80);
    Check(Game.Paper.Protection.Loss>0&&Game.Paper.PeakDamage>0,"damage exists before restart "+repeat);Game.RequestRestart();Game.ConfirmRestart();
    Check(JsonUtility.ToJson(Game.Tools)==tools,"individual wear sharpness special states purchases restored "+repeat);Check(JsonUtility.ToJson(Game.Progress.wallet)==wallet,"money and revenue ledgers restored "+repeat);Check(Game.Progress.banks.Sum(x=>x.total)==earned,"EXP SP rollback "+repeat);Check(Mathf.Abs(Game.Economy.AccountedMass-mass)<.0001f&&Game.Economy.Ball.Grams==ball,"old inventory mass restored repeatedly "+repeat);Check(Game.Paper.Protection.Loss==0&&Game.Paper.PeakDamage==0,"actual paper/protection damage cleared "+repeat);
   }
   Game.ShowBoard();Game.ContactGuide.Refresh();Check(!Game.ContactGuide.Visible,"guide hidden on board");Game.OpenShop();yield return Capture("shop");Game.ContactGuide.Refresh();Check(!Game.ContactGuide.Visible,"guide hidden in shop");Game.CloseShop();Game.OpenSkills();yield return Capture("skills");Game.ContactGuide.Refresh();Check(!Game.ContactGuide.Visible,"guide hidden in skills");Application.Quit(fail==0?0:1);
  }
 }
}



