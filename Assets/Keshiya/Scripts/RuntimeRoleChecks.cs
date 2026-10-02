using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
namespace Keshiya
{
    public sealed class RuntimeRoleChecks:MonoBehaviour
    {
        public PrototypeGame Game;
        readonly StringBuilder report=new StringBuilder();string directory;int checks,failures;
        void Awake(){Application.runInBackground=true;}
        void Check(bool test,string name){checks++;if(!test)failures++;report.AppendLine((test?"PASS: ":"FAIL: ")+name);Debug.Log((test?"PASS: ":"FAIL: ")+name);}
        IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.15f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));yield return new WaitForSecondsRealtime(.25f);}
        IEnumerator Start()
        {
            directory=Path.Combine(Application.dataPath,"../TestResults-0.2.1");Directory.CreateDirectory(directory);
            Game.Controller.ExternalInput=true;yield return new WaitForSecondsRealtime(.8f);
            var hud=Game.GetComponent<PrototypeHUD>();Check(hud.ToolSlotCount==3,"Three actual HUD tool slots initialized");
            for(int i=0;i<3;i++){
                Game.Controller.HandleKeyDown((KeyCode)((int)KeyCode.Alpha1+i));
                Check(Game.ToolIndex==i&&hud.HighlightedTool==i,"Number-key routing switches and selects visual slot "+i);
                Check(hud.Icon(i)!=null&&hud.Icon(i).width==96,"Tool icon exists "+i);
                Game.Controller.ProcessInput(new Vector2(.8f,.6f),true,true,.016f);yield return Capture("01-tool-"+i);
            }
            hud.SelectToolSlot(1);Check(Game.ToolIndex==1&&Game.Presentation.RenderedBodyColor==Game.Eraser.bodyColor,"HUD slot action switches actual tool and renderer");
            Game.SelectJob(1);Check(Game.Definition.letterCorrection&&Game.Paper.Protection!=null,"Actual letter job loads protected and target layers");
            Game.Controller.ProcessInput(new Vector2(1.4f,.4f),false,true,.016f);yield return Capture("02-letter-start");
            Game.DebugEraseAreaVisible=true;Game.SyncToolVisual();Game.SelectMode(ContactMode.Edge);
            for(int i=0;i<4;i++){
                Game.Controller.HandleKeyDown(KeyCode.R);Game.Controller.ProcessInput(new Vector2(.3f,.1f),true,true,.016f);
                Check(Game.Yaw==(i+1)*45&&Game.Presentation.Footprint.Angle==Game.Yaw,"R advances exact edge mesh and contact by 45 degrees "+i);
                Check(Game.Presentation.ContactGuideVisible,"Edge contact outline visible while pressed "+i);yield return Capture("03-edge-"+Game.Yaw);
            }
            Game.Controller.HandleKeyDown(KeyCode.R,true);Check(Game.Yaw==135,"Shift R reverses by 45 degrees");
            Game.Controller.ProcessInput(Vector2.one,false,true,.016f);Check(!Game.Presentation.ContactGuideVisible,"Release hides contact outline");
            Game.SelectTool(0);Game.SelectMode(ContactMode.Face);
            Game.Rub(new Vector2(.14f,-.4f),new Vector2(.14f,.4f),4);
            Check(Game.Paper.Protection.Loss>0&&Game.Paper.PeakDamage==0&&!Game.CurrentJob.Completed,"Broad face touches A without timer or instant failure");yield return Capture("04-letter-mistake");
            Game.Restart();Game.SelectTool(1);Game.SelectMode(ContactMode.Corner);
            int passes=0;
            while(Game.Paper.Drawing.Erased<.95f&&passes++<45){
                foreach(var path in LetterLayout.Target)for(int k=1;k<path.Length;k++){
                    Game.Rub(path[k-1],path[k],3);Game.CurrentJob.Tick(Vector2.Distance(path[k-1],path[k])/3);
                }yield return null;
            }
            Check(Game.CurrentJob.CanComplete(Game.Paper)&&Game.Paper.Protection.Loss==0&&Game.Paper.PeakDamage==0,"Actual soft corner completes B while A and paper stay intact");
            Check(Game.ActiveState.UsedUnits>0&&Game.ActiveState.CornerSharpness<1,"Existing residual and corner wear still apply");
            Game.Controller.ProcessInput(new Vector2(1.6f,.5f),false,true,.016f);
            Game.Controller.HandleKeyDown(KeyCode.Space);yield return new WaitForSecondsRealtime(.8f);
            Check(Game.Crumbs.ActiveCount==0,"Space key routing clears crumbs");yield return Capture("05-letter-clean");
            bool muted=Game.Audio.Muted;Game.Controller.HandleKeyDown(KeyCode.M);Check(Game.Audio.Muted!=muted,"M key routing toggles mute");
            Game.CurrentJob.Tick(10000);Game.Finish();
            Check(Game.CurrentJob.Completed&&Game.CurrentJob.Result.basic==1000&&Game.CurrentJob.Result.speed==0,"Unlimited-time letter result retains full base pay");yield return Capture("06-letter-result");
            File.WriteAllText(Path.Combine(directory,"runtime.txt"),report+$"Checks={checks}; Failures={failures}; LetterPasses={passes}\n");Application.Quit(failures==0?0:1);
        }
    }
}
