using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
namespace Keshiya
{
    public sealed class RuntimeZigzagChecks:MonoBehaviour
    {
        public PrototypeGame Game;int count,failures;string directory;readonly StringBuilder report=new StringBuilder();
        void Awake(){Application.runInBackground=true;}
        void Check(bool ok,string name){count++;if(!ok)failures++;report.AppendLine((ok?"PASS: ":"FAIL: ")+name);Debug.Log((ok?"PASS: ":"FAIL: ")+name);}
        IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.08f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));yield return new WaitForSecondsRealtime(.15f);}
        IEnumerator Rub(float y,float progress,int halves,float width=.36f)
        {
            Vector2 current=new Vector2(-2,y-width*.5f);var input=Game.Controller;
            input.ProcessInput(current,false,true,.02f);input.ProcessInput(current,true,true,.02f);
            for(int leg=0;leg<halves;leg++){
                Vector2 a=current,b=new Vector2(-2+progress*(leg+1)/halves,y+(leg%2==0?1:-1)*width*.5f);
                for(int part=1;part<=6;part++){current=Vector2.Lerp(a,b,part/6f);input.ProcessInput(current,true,true,.024f);yield return null;}
            }
        }
        IEnumerator Start()
        {
            directory=Path.Combine(Application.dataPath,"../TestResults-0.3.1");Directory.CreateDirectory(directory);Game.Controller.ExternalInput=true;
            yield return new WaitForSecondsRealtime(.7f);var e=Game.Economy;Game.SelectTool(1);Game.SelectMode(ContactMode.Face);
            Check(!Game.CrumbDebug,"Debug display is hidden by default");
            yield return Rub(-1.55f,4,40);
            Check(e.Growing!=null&&e.Growing.LengthCm>=5,"Actual cat graphite plus advancing reciprocation creates a sellable strand");
            Check(Game.Paper.Drawing.Erased>0&&e.ProducedGrams>0,"Generation is connected to actual target erasure");
            Check(e.Motion.Turns>20&&e.Motion.Direction.x>.9f&&e.BreakCount==0,"Hand reversals are accepted while the center direction stays right");
            float length=e.BestLength;yield return Capture("01-natural-rubbing");
            Game.Controller.HandleKeyDown(KeyCode.F3);Check(Game.CrumbDebug,"F3 toggles diagnostic display on");yield return Capture("02-diagnostics");
            Game.Controller.HandleKeyDown(KeyCode.F3);Check(!Game.CrumbDebug,"F3 restores uncluttered normal play");
            Vector2 tip=new Vector2(2,-1.73f);
            foreach(var target in new[]{new Vector2(2.1f,-1.05f),new Vector2(2.2f,-2.05f)}){
                Vector2 start=tip;for(int part=1;part<=8;part++){tip=Vector2.Lerp(start,target,part/8f);Game.Controller.ProcessInput(tip,true,true,.025f);yield return null;}
            }
            Check(e.Growing!=null&&e.Growing.Tension>.3f&&e.Growing.Tension<1,"A wider stroke warns through strand tension before it cuts");yield return Capture("02b-tension-warning");
            Game.Controller.ProcessInput(tip,false,true,.024f);
            Check(e.Growing==null&&e.Motion.Turns==0,"Mouse release ends the strand and clears history");
            Game.CollectCrumbs();Check(e.Inventory.Count>0&&Game.Crumbs.ActiveCount==0,"Natural rubbing strand is collectible through the unchanged C action");
            Game.Restart();float produced=e.ProducedGrams;yield return Rub(2.2f,4,40,.20f);
            Check(e.JobLongest==0&&e.ProducedGrams==produced,"Blank paper rubbing creates no saleable length or ball material");yield return Capture("03-blank-no-farming");
            Game.Restart();yield return Rub(-1.55f,0,80);
            Check(e.JobLongest<.5f,"Actual in-place rubbing cannot grow an infinite strand");
            Game.Restart();Game.Controller.ProcessInput(new Vector2(-2,-1.55f),true,true,.02f);
            for(int i=1;i<=80;i++){Game.Controller.ProcessInput(new Vector2(-2+i*.05f,-1.55f),true,true,.02f);yield return null;}
            Check(e.JobLongest==0,"Actual straight erasing is not the profitable long-strand technique anymore");
            Game.Restart();yield return Rub(-1.55f,4,40);int cuts=e.BreakCount;Game.Controller.ProcessInput(new Vector2(2,.2f),true,true,.016f);
            Check(e.BreakCount>cuts&&e.RecentBreak,"A sudden large pull creates explicit cut feedback");yield return Capture("04-explained-cut");
            Game.CollectCrumbs();Game.RollCrumbs();long quote=e.InventoryPrice+e.Config.BallPrice(e.Ball.Grams);long money=e.Wallet.Balance;
            Game.OpenTrade();yield return Capture("05-unchanged-trade");e.SellLongs();e.SellBall();Game.CloseTrade();
            Check(quote>0&&e.Wallet.Balance==money+quote,"New-generation material sells through the existing price and wallet pipeline");
            Check(Mathf.Abs(e.ProducedGrams-e.AccountedMass)<.01f,"Integrated graphite-based material is conserved");
            File.WriteAllText(Path.Combine(directory,"runtime.txt"),report+$"Checks={count}; Failures={failures}; NaturalCm={length:0.00}; Sale={quote}\n");Application.Quit(failures==0?0:1);
        }
    }
}
