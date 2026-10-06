using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace Keshiya
{
    public sealed class RuntimeToolChecks : MonoBehaviour
    {
        public PrototypeGame Game;
        readonly StringBuilder report=new StringBuilder();int checks,failures;string directory;
        void Awake(){Application.runInBackground=true;}
        void Check(bool value,string name){checks++;if(!value)failures++;report.AppendLine((value?"PASS: ":"FAIL: ")+name);Debug.Log((value?"PASS: ":"FAIL: ")+name);}
        IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.10f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));yield return new WaitForSecondsRealtime(.2f);}
        IEnumerator Start()
        {
            directory=Path.Combine(Application.dataPath,"../TestResults-0.2");Directory.CreateDirectory(directory);
            Game.Controller.ExternalInput=true;yield return new WaitForSecondsRealtime(.7f);
            Game.Controller.ProcessInput(new Vector2(-1,0),false,true,.016f);
            yield return Capture("01-normal-face");
            for(int index=0;index<3;index++)
            {
                Game.SelectTool(index);Game.Controller.ProcessInput(new Vector2(-1,0),true,true,.016f);
                Check(Game.Eraser==Game.Catalog.tools[index],"Switch selects correct data: "+index);
                Check(Game.Presentation.Footprint.HalfSize==Game.Contact.HalfSize,"Switch updates visible contact immediately: "+index);
                Check(Game.Presentation.RenderedBodyColor==Game.Eraser.bodyColor,"Switch updates the renderer's actual body colour: "+index);
                Game.Controller.ProcessInput(new Vector2(-.9f,0),true,true,.016f);
                Check(Game.Presentation.BodyOffset.sqrMagnitude<.0001f,"Broad-face friction does not displace the body from its sole: "+index);
                Check(Game.ActiveState.UsedUnits>0,"Input stroke consumes active tool: "+index);
                yield return Capture("02-tool-"+index);
            }
            Game.SelectTool(0);float oldRemaining=Game.ActiveState.Remaining(Game.Eraser,Game.Modifiers);
            Game.SelectTool(1);Game.SelectTool(0);Check(Game.ActiveState.Remaining(Game.Eraser,Game.Modifiers)==oldRemaining,"Switching away and back preserves wear");
            Game.Controller.ProcessInput(new Vector2(-1,0),true,true,.016f);float freshHeight=Game.Presentation.BodyHeight;
            Game.ActiveState.Use(400,ContactMode.Face,Game.Eraser,Game.Modifiers);Game.SyncToolVisual();Game.Controller.ProcessInput(new Vector2(-1,0),true,true,.016f);
            Check(Game.Presentation.BodyHeight<freshHeight*.85f,"Consumed tool is visibly shorter");yield return Capture("03-worn-tool");
            Game.SelectMode(ContactMode.Corner);float width=Game.Contact.HalfSize.x;
            Game.ActiveState.Use(80,ContactMode.Corner,Game.Eraser,Game.Modifiers);Game.SyncToolVisual();
            Check(Game.Contact.HalfSize.x>width,"Corner rounding widens actual contact");
            Check(Game.Presentation.Footprint.HalfSize==Game.Contact.HalfSize,"Corner wear stays synchronized with its visible pad");
            Game.FreshTools();var playableJob=Game.Jobs[1];var legacyJob=ScriptableObject.CreateInstance<JobDefinition>();legacyJob.precision=true;Game.Jobs[1]=legacyJob;Game.SelectJob(1);Game.SelectTool(0);
            Check(Game.Paper.Protection!=null&&Game.Definition.precision,"Precision job contains a distinct protected layer");
            foreach(ContactMode mode in new[]{ContactMode.Face,ContactMode.Edge,ContactMode.Corner})
            {
                Game.SelectMode(mode);Game.Controller.ProcessInput(new Vector2(.14f,.4f),true,true,.016f);
                Check(Game.Presentation.Footprint.HalfSize==Game.Contact.HalfSize,"Mode uses matching visible sole: "+mode);
                yield return Capture("04-precision-"+mode);
            }
            float angle=Game.Yaw;Game.RotateTool();Check(Game.Yaw!=angle&&Game.Presentation.Footprint.Angle==Game.Contact.Angle,"Rotate changes rendering and contact angle together");
            Game.DebugEraseAreaVisible=true;Game.SyncToolVisual();Game.Controller.ProcessInput(new Vector2(.14f,.4f),true,true,.016f);Check(Game.Presentation.ContactGuideVisible,"Pressed corner exposes its true contact outline even behind the body");
            Game.Controller.ProcessInput(new Vector2(.14f,.4f),false,true,.016f);Check(!Game.Presentation.ContactGuideVisible,"Precision outline disappears immediately on release");
            Game.SelectMode(ContactMode.Face);Game.Rub(new Vector2(.14f,-1.2f),new Vector2(.14f,1.2f),4);
            Check(Game.Paper.Protection.Loss>0&&Game.Paper.PeakDamage==0,"Careless face rubbing damages protection without paper damage");
            yield return Capture("05-protection-mistake");Game.Restart();Game.SelectMode(ContactMode.Corner);
            int passes=0;
            while(!Game.CurrentJob.CanComplete(Game.Paper)&&passes++<35)
            {
                for(int side=-1;side<=1;side+=2){float x=side*Game.Definition.targetOffset;Game.Rub(new Vector2(x,-1.43f),new Vector2(x,1.43f),3);Game.CurrentJob.Tick(2.86f/3);yield return null;}
            }
            Check(Game.CurrentJob.CanComplete(Game.Paper)&&Game.Paper.Protection.Loss==0,"Corner completes precision work with no protected damage");
            Check(Game.Paper.PeakDamage==0,"Safe precision work does not damage paper");
            Game.BlowCrumbs();yield return new WaitForSecondsRealtime(.7f);
            Check(Game.Crumbs.ActiveCount==0,"Space action still blows crumbs away");yield return Capture("06-precision-clean");
            Game.CurrentJob.Tick(10000);Game.Finish();
            Check(Game.CurrentJob.Completed&&Game.CurrentJob.Result.basic==Game.Definition.baseReward&&Game.CurrentJob.Result.speed==0,"Precision result preserves no-time-limit/full-base-pay rule");
            Check(Game.CurrentJob.Result.protection==Game.Definition.protectionBonus,"Clean precision result grants protected-line bonus");yield return Capture("07-precision-result");
            Game.SelectJob(0);Game.Jobs[1]=playableJob;Destroy(legacyJob);Game.SelectMode(ContactMode.Face);Game.SelectTool(0);
            Check(Game.Paper.Protection==null&&Game.Paper.Drawing.Erased==0,"Switch back restores original broad job");
            Game.InputBlocked=true;float before=Game.Paper.Drawing.Erased;Game.Controller.ProcessInput(Vector2.zero,true,true,.016f);Game.Controller.ProcessInput(Vector2.right,true,true,.016f);
            Check(Game.Paper.Drawing.Erased==before&&!Game.Controller.Pressing,"Job-switch confirmation blocks rubbing");Game.InputBlocked=false;
            Game.FreshTools();Check(Game.ActiveState.Remaining(Game.Eraser,Game.Modifiers)==1&&Game.ActiveState.CornerSharpness==1,"Prototype fresh-tool retry replenishes all tool state");
            File.WriteAllText(Path.Combine(directory,"runtime.txt"),report+$"Checks={checks}; Failures={failures}; PrecisionPasses={passes}\n");
            Application.Quit(failures==0?0:1);
        }
    }
}
