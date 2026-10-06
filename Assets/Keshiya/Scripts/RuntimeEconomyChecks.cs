using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
namespace Keshiya
{
    public sealed class RuntimeEconomyChecks:MonoBehaviour
    {
        public PrototypeGame Game;string directory;int count,failures;readonly StringBuilder report=new StringBuilder();
        void Awake(){Application.runInBackground=true;}
        void Check(bool ok,string name){count++;if(!ok)failures++;report.AppendLine((ok?"PASS: ":"FAIL: ")+name);Debug.Log((ok?"PASS: ":"FAIL: ")+name);}
        IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.1f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));yield return new WaitForSecondsRealtime(.2f);}
        IEnumerator Stroke(float from,float to,float y)
        {
            var input=Game.Controller;Vector2 current=new Vector2(from,y-.18f);
            input.ProcessInput(current,false,true,.025f);input.ProcessInput(current,true,true,.025f);
            int halves=40;const int steps=6;
            for(int leg=0;leg<halves;leg++){
                Vector2 target=new Vector2(Mathf.Lerp(from,to,(leg+1)/(float)halves),y+(leg%2==0?.18f:-.18f));
                Vector2 start=current;
                for(int part=1;part<=steps;part++){
                    current=Vector2.Lerp(start,target,part/(float)steps);input.ProcessInput(current,true,true,.14f/steps);yield return null;
                }
            }
        }
        IEnumerator Start()
        {
            directory=Path.Combine(Application.dataPath,"../TestResults-0.3.1-economy");Directory.CreateDirectory(directory);Game.Controller.ExternalInput=true;
            yield return new WaitForSecondsRealtime(.8f);var e=Game.Economy;
            Check(e!=null&&Game.EconomyView.LineCapacity==e.Config.paperPieceCapacity*2,"Authoritative economy and bounded strand pool initialize");
            Game.Controller.HandleKeyDown(KeyCode.Alpha2);Game.SelectMode(ContactMode.Face);yield return Stroke(-2,2,-1.55f);
            Check(e.Growing!=null&&e.Growing.LengthCm>5,"Real oscillating input grows a sellable soft strand");
            Check(Game.EconomyView.VisibleLines>0&&e.Wallet.Balance==0,"Valuable strand is rendered but not paid before sale");yield return Capture("01-growing-long");
            int cuts=e.BreakCount;Game.Controller.ProcessInput(new Vector2(2,.0f),true,true,.025f);
            Check(e.BreakCount>cuts&&e.RecentBreak,"An abrupt large pull cuts and exposes explanatory feedback");yield return Capture("02-cut");
            Game.Controller.HandleKeyDown(KeyCode.C);yield return null;
            Check(e.Inventory.Count==1&&e.Paper.Count==0&&Game.Crumbs.ActiveCount==0,"C collects a long piece and clears physical clutter");
            Check(e.LooseGrams>0&&e.BestLength>5,"Collected fines and session longest length are retained");
            Game.Controller.HandleKeyDown(KeyCode.G);yield return null;
            Check(e.Ball.Grams>0&&Game.EconomyView.BallScale>0,"G rolls material and renders a ball");float ballSize=Game.EconomyView.BallScale;
            Game.SelectMode(ContactMode.Corner);yield return Stroke(-2,2,-1.6f);Game.CollectCrumbs();Game.RollCrumbs();yield return null;
            Check(Game.EconomyView.BallScale>ballSize,"Physical crumb ball grows after adding corner fragments");yield return Capture("03-collected-ball");
            Game.Restart();Game.SelectMode(ContactMode.Face);Game.SelectTool(0);yield return Stroke(-2,2,-1.55f);int held=e.Inventory.Count;
            Game.Controller.HandleKeyDown(KeyCode.Space);yield return new WaitForSecondsRealtime(.7f);
            Check(e.RescueRemaining>0&&Game.Crumbs.ActiveCount==0&&e.Inventory.Count==held,"Space blows paper material without touching inventory");
            Game.Controller.HandleKeyDown(KeyCode.Z);Check(e.Inventory.Count==held+1&&e.RescueRemaining==0,"Z rescues a blown long piece once");
            Game.Controller.HandleKeyDown(KeyCode.V);Check(Game.TradeOpen&&Game.InputBlocked,"V opens the buyer and blocks rubbing");
            float ink=Game.Paper.Drawing.Erased;Game.Controller.ProcessInput(Vector2.zero,true,true,.016f);Game.Controller.ProcessInput(Vector2.right,true,true,.016f);
            Check(Game.Paper.Drawing.Erased==ink&&!Game.Controller.Pressing,"Trade modal prevents accidental work");yield return Capture("04-buyer-before-sale");
            long quoted=e.InventoryPrice,prior=e.Wallet.Balance;long value=e.SellLongs();
            Check(value==quoted&&value>0&&e.Wallet.Balance==prior+value&&e.Wallet.JobIncome==0,"Selling actual long stock adds only crumb income");
            long ball=e.SellBall();Check(ball>0&&e.Ball.Grams==0,"The actual collected ball sells successfully");
            long paid=e.Wallet.Balance;Check(e.SellLongs()==0&&e.SellBall()==0&&e.Wallet.Balance==paid,"Trade buttons cannot pay twice for sold stock");yield return Capture("05-buyer-sold");
            Game.Controller.HandleKeyDown(KeyCode.V);Check(!Game.TradeOpen&&!Game.InputBlocked,"V returns to work with inputs restored");
            Game.SelectTool(2);Game.SelectMode(ContactMode.Face);yield return Stroke(-1,1,-1.9f);
            Check(e.Growing==null||e.Growing.LengthCm<=e.Config.sandZigLength+.001f,"Actual sand strokes produce short pieces");
            Game.Controller.ProcessInput(new Vector2(1,-1.9f),false,true,.016f);Game.CollectCrumbs();
            Game.SelectTool(0);int passes=0;
            while(!Game.CurrentJob.CanComplete(Game.Paper)&&passes++<15){for(float y=-2.4f;y<=2.4f;y+=.2f){Game.Rub(new Vector2(-3.4f,y),new Vector2(3.4f,y),3);yield return null;}}
            Check(Game.CurrentJob.CanComplete(Game.Paper)&&Game.Paper.PeakDamage==0,"Original broad job still completes safely with economy enabled");
            long priorMoney=e.Wallet.Balance;Game.CurrentJob.Tick(10000);Game.Finish();
            Check(Game.CurrentJob.Completed&&Game.CurrentJob.Result.basic==500&&Game.CurrentJob.Result.speed==0,"Economy adds no time limit or slow-work deduction");
            Check(e.Wallet.Balance==priorMoney+Game.CurrentJob.Result.Total&&e.Wallet.JobIncome==Game.CurrentJob.Result.Total,"Job reward is deposited separately on completion");
            long settled=e.Wallet.Balance;Game.Finish();Check(e.Wallet.Balance==settled,"Repeated completion cannot pay a job twice");
            Check(e.JobSales==paid&&e.Wallet.CrumbIncome==paid,"Result ledger keeps crumb sales distinct from job reward");yield return Capture("06-job-result");
            int stock=e.Inventory.Count;float best=e.BestLength;Game.Restart();
            Check(e.Inventory.Count==stock&&e.Wallet.Balance==settled&&e.BestLength==best,"Retry preserves collected inventory wallet and best length");
            Check(e.JobSales==0&&e.JobLongest==0,"Next job resets only its own statistics");
            Check(Mathf.Abs(e.ProducedGrams-e.AccountedMass)<.01f,"Integrated collection selling retries conserve generated material");
            File.WriteAllText(Path.Combine(directory,"runtime.txt"),report+$"Checks={count}; Failures={failures}; Sales={paid}; Wallet={e.Wallet.Balance}; BestCm={e.BestLength}\n");Application.Quit(failures==0?0:1);
        }
    }
}
