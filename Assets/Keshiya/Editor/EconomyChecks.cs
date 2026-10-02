using System;
using System.IO;
using System.Text;
using UnityEngine;
namespace Keshiya.Editor
{
    public static class EconomyChecks
    {
        static int count;static StringBuilder report;
        static void Check(bool result,string message){if(!result)throw new Exception("ECONOMY FAIL: "+message);count++;report.AppendLine("PASS: "+message);Debug.Log("PASS: "+message);}
        static void Line(CrumbEconomy e,EraserDefinition tool,float length=4,int steps=40,float speed=4,ContactMode mode=ContactMode.Face)
        {ZigzagTestPath.Trace(e,tool,length,40,steps==1?1:6,.36f,.56f/speed,.012f,mode);}
        static void Conserved(CrumbEconomy e,string name)=>Check(Mathf.Abs(e.ProducedGrams-e.AccountedMass)<.001f,"Material conserved: "+name);
        public static void Run()
        {
            count=0;report=new StringBuilder();var c=Resources.Load<CrumbEconomyConfig>("CrumbEconomyConfig");var tools=Resources.Load<ToolCatalog>("ToolCatalog").tools;var n=tools[0];var s=tools[1];var sand=tools[2];
            var a=new CrumbEconomy(c);Line(a,n);Check(a.Growing!=null&&a.Growing.LengthCm>20&&a.BreakCount==0,"Advancing rubbing grows one long piece");
            Check(a.Growing.Source==n.displayName&&a.Growing.Thickness>0&&a.Growing.MassGrams>0,"Piece retains source thickness length and mass");
            float length=a.Growing.LengthCm;a.EndStroke();Check(a.Paper[0].LengthCm==length&&!a.Paper[0].WasBroken,"Release preserves the earned length");
            var subdivision=new CrumbEconomy(c);Line(subdivision,n,4,1);Check(Mathf.Abs(subdivision.BestLength-a.BestLength)<.001f,"Rubbing length is independent of input subdivision");
            var soft=new CrumbEconomy(c);Line(soft,s);Check(soft.BestLength>a.BestLength*1.5f,"Soft produces longer strands for equal travel");
            var dust=new CrumbEconomy(c);Line(dust,sand);Check(dust.BestLength<=c.sandZigLength+.001f&&dust.BreakCount>0,"Sand repeatedly produces short fragments");
            Check(dust.PaperFineGrams/dust.ProducedGrams>soft.PaperFineGrams/soft.ProducedGrams,"Sand has a higher fine-powder fraction");
            var turn=new CrumbEconomy(c);Line(turn,n);turn.Stroke(new Vector2(4,-.18f),new Vector2(3.8f,-.18f),4,n,ContactMode.Face,1,.001f);
            Check(turn.BreakCount==0&&turn.Growing!=null,"A hand reversal alone is now accepted as rubbing, not a cut");
            var fast=new CrumbEconomy(c);Line(fast,n);fast.Stroke(new Vector2(4,-.18f),new Vector2(4.1f,-.18f),35,n,ContactMode.Face,1,.001f);
            Check(fast.BreakCount==1&&fast.Growing==null&&fast.PaperFineGrams>0,"Extreme speed interrupts the strand and produces fines");
            var jump=new CrumbEconomy(c);Line(jump,n);jump.Stroke(new Vector2(4,-.18f),new Vector2(4.1f,-.18f),1,n,ContactMode.Face,1,.001f);
            Check(jump.BreakCount==0,"Moderate speed variation no longer breaks a healthy strand");
            var ordinaryTurn=new CrumbEconomy(c);var softTurn=new CrumbEconomy(c);
            ZigzagTestPath.Trace(ordinaryTurn,n,width:.95f,halfSeconds:.35f);ZigzagTestPath.Trace(softTurn,s,width:.95f,halfSeconds:.35f);
            Check(softTurn.BestLength>ordinaryTurn.BestLength,"Soft tolerates wider rubbing than ordinary");
            var mode=new CrumbEconomy(c);Line(mode,n);mode.Stroke(new Vector2(4,-.18f),new Vector2(4.1f,-.18f),4,n,ContactMode.Corner,1,.001f);
            Check(mode.BreakCount==1&&mode.Growing==null,"Changing contact to corner cuts and makes fragments");
            var edge=new CrumbEconomy(c);Line(edge,n,4,40,4,ContactMode.Edge);Check(edge.Growing.LengthCm>5&&edge.Growing.Thickness<a.Paper[0].Thickness,"Edge creates a thinner long piece");
            var idle=new CrumbEconomy(c);Line(idle,n);idle.AdvanceTime(1);Check(idle.Growing==null&&idle.Paper.Count==1,"Pause lays down the current piece");
            Check(c.LongPrice(30,1)>c.LongPrice(20,1)&&c.LongPrice(20,1)>2*c.LongPrice(10,1),"Long value grows faster than proportionally");
            Check(c.Rank(12)=="LONG"&&c.Rank(24)=="VERY LONG"&&c.Rank(32)=="EXCELLENT","Length rank thresholds are predictable");
            a.Collect();Check(a.Paper.Count==0&&a.Inventory.Count==1&&a.LooseGrams>0,"Collect gathers both long pieces and loose fines");Conserved(a,"collection");
            float loose=a.LooseGrams;Check(a.Roll()&&a.LooseGrams==0&&Mathf.Abs(a.Ball.Grams-loose)<.0001f,"Collected fines turn into ball mass");
            float diameter=a.Ball.DiameterCm;Line(a,n,2);a.EndStroke();a.Collect();a.Roll();Check(a.Ball.DiameterCm>diameter&&a.Ball.LifetimeAddedGrams>=a.Ball.Grams,"Adding fines grows ball size and cumulative material");
            Conserved(a,"rolling");long id=a.Inventory[0].Id;long price=a.Inventory[0].Price(c);
            Check(price>c.BallPrice(a.Inventory[0].MassGrams),"An intact long piece is worth more than its ball material");
            Check(a.SellPiece(id)==price&&a.Wallet.Balance==price&&a.Wallet.CrumbIncome==price&&a.Wallet.JobIncome==0,"Selling a long piece credits only crumb revenue");
            Check(a.SellPiece(id)==0&&a.Wallet.Balance==price,"Selling the same piece twice cannot duplicate revenue");
            long before=a.Wallet.Balance;float mass=a.Ball.Grams;long ballPrice=c.BallPrice(mass);
            Check(ballPrice>0&&a.SellBall()==ballPrice&&a.Wallet.Balance==before+ballPrice&&a.Ball.Grams==0,"Ball sale credits wallet and consumes ball");Check(a.SellBall()==0,"An empty ball cannot be sold twice");Conserved(a,"sales");
            Check(a.Wallet.CreditJob(a.JobId,1000)&&a.Wallet.JobIncome==1000&&a.Wallet.CrumbIncome==price+ballPrice,"Job credit remains a distinct revenue ledger");
            Check(!a.Wallet.CreditJob(a.JobId,1000),"Duplicate job credit is rejected");
            long wallet=a.Wallet.Balance;a.BeginJob();Check(a.Wallet.Balance==wallet&&a.JobSales==0&&a.BestLength>0&&a.JobLongest==0,"New job preserves money best record and stock but resets job statistics");
            Check(!a.Wallet.TrySpend(-1)&&!a.Wallet.TrySpend(a.Wallet.Balance+1)&&a.Wallet.TrySpend(1),"Future-shop spend rejects invalid and insufficient amounts");
            var blow=new CrumbEconomy(c);Line(blow,n);blow.Collect();Line(blow,s);int stock=blow.Inventory.Count;
            Check(blow.Blow()&&blow.Paper.Count==0&&blow.Inventory.Count==stock&&blow.RescueRemaining>0,"Blow only removes uncollected material and starts rescue grace");
            Check(blow.Rescue()&&blow.Inventory.Count==stock+1&&!blow.Rescue(),"Rescue collects blown long pieces exactly once");Conserved(blow,"rescue");
            Line(blow,n);blow.Blow();blow.AdvanceTime(7);Check(!blow.Rescue()&&blow.DiscardedGrams>0&&blow.Blown.Count==0,"Expired blown material is unsellable and discarded");Conserved(blow,"expired blow");
            var fragment=new CrumbEconomy(c);Line(fragment,n,.3f,3);fragment.Collect();Check(fragment.Inventory.Count==0&&fragment.LooseGrams>0,"Short pieces are collected as ball material");
            fragment.Roll();Check(fragment.Ball.Grams>0,"Even short-stroke failures contribute to a ball");
            var limited=UnityEngine.Object.Instantiate(c);limited.paperPieceCapacity=2;limited.inventoryCapacity=2;var capped=new CrumbEconomy(limited);
            for(int i=0;i<12;i++){Line(capped,s,4,4);capped.EndStroke();}capped.Collect();
            Check(capped.Paper.Count<=2&&capped.Inventory.Count==2&&capped.OverflowCount==10,"Capacity overflow stores value in a bounded aggregate crate");Conserved(capped,"bounded overflow");
            long expected=capped.InventoryPrice;Check(capped.SellLongs()==expected&&capped.InventoryPrice==0&&capped.OverflowCount==0,"Overflow crate and inventory sell without losing value");Conserved(capped,"overflow sale");
            var zero=new CrumbEconomy(c);zero.Stroke(Vector2.zero,Vector2.zero,4,n,ContactMode.Face);Check(zero.ProducedGrams==0,"Stationary contact cannot generate material");
            var broken=new CrumbEconomy(c);Line(broken,n);broken.EndStroke("intentional change",true);Check(broken.BestLength>=24-.001f&&broken.Paper[0].Price(c)>0,"Breaking does not erase previously earned length or value");
            File.WriteAllText("TestResults/economy-checks.txt",$"PASS {count} economy checks\n"+report);UnityEngine.Object.DestroyImmediate(limited);
        }
    }
}
