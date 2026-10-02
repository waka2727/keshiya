using System;
using System.IO;
using System.Text;
using UnityEngine;
namespace Keshiya.Editor
{
    public static class ZigzagChecks
    {
        static int count;static StringBuilder report;
        static void Check(bool ok,string name){if(!ok)throw new Exception("ZIGZAG FAIL: "+name);count++;report.AppendLine("PASS: "+name);Debug.Log("PASS: "+name);}
        public static void Run()
        {
            count=0;report=new StringBuilder();var c=Resources.Load<CrumbEconomyConfig>("CrumbEconomyConfig");var tools=Resources.Load<ToolCatalog>("ToolCatalog").tools;var n=tools[0];var soft=tools[1];var sand=tools[2];
            var regular=new CrumbEconomy(c);ZigzagTestPath.Trace(regular,n);
            Check(regular.BestLength>20&&regular.BreakCount==0,"Small alternating strokes plus center advance produce an intact long piece");
            Check(regular.Motion.Turns>=30&&regular.Motion.Width>.25f&&regular.Motion.Width<.5f,"Tracker detects amplitude and many alternating half strokes");
            Check(Vector2.Dot(regular.Motion.Direction,Vector2.right)>.98f,"Growth direction comes from the center, not hand oscillation");
            var straight=new CrumbEconomy(c);for(int i=0;i<100;i++)straight.Stroke(new Vector2(i*.04f,0),new Vector2((i+1)*.04f,0),4,n,ContactMode.Face,1,.002f);
            Check(straight.BestLength==0&&straight.PaperFineGrams>0,"Straight sliding makes fines but not valuable long pieces");
            var stationary=new CrumbEconomy(c);ZigzagTestPath.Trace(stationary,n,advance:0,halves:200);
            Check(stationary.BestLength==0,"Two hundred in-place reversals cannot grow an infinite strand");
            var slow=new CrumbEconomy(c);ZigzagTestPath.Trace(slow,n,advance:.6f);
            Check(slow.BestLength>0&&slow.BestLength<regular.BestLength*.3f,"Slower center advance grows less despite equal reversal count");
            var tiny=new CrumbEconomy(c);ZigzagTestPath.Trace(tiny,n,advance:.3f,width:.01f);
            Check(tiny.BestLength<1,"Very tiny oscillations do not become profitable rubbing");
            var widthWarning=new CrumbEconomy(c);ZigzagTestPath.Trace(widthWarning,n,width:.72f,halfSeconds:.25f);
            Check(widthWarning.Motion.Risk>regular.Motion.Risk,"Large but not extreme amplitude visibly increases tension");
            var wild=new CrumbEconomy(c);ZigzagTestPath.Trace(wild,n);ZigzagTestPath.Trace(wild,n,advance:1,halves:10,width:1.6f,origin:new Vector2(4,.62f));
            Check(wild.BreakCount>0,"Oversized rubbing ends an existing strand");
            var turn=new CrumbEconomy(c);ZigzagTestPath.Trace(turn,n);ZigzagTestPath.Trace(turn,n,origin:new Vector2(4,0),forward:Vector2.left);
            Check(turn.BreakCount>0&&turn.Paper[0].EndReason.Contains("進む向き"),"Reversing center progression, not hand direction, produces an explained cut");
            var fast=new CrumbEconomy(c);Vector2 end=ZigzagTestPath.Trace(fast,n);fast.Stroke(end,end+Vector2.right*.1f,35,n,ContactMode.Face,1,.001f);
            Check(fast.BreakCount==1&&fast.Growing==null,"Extreme speed cuts deterministically");
            var teleport=new CrumbEconomy(c);end=ZigzagTestPath.Trace(teleport,n);teleport.Stroke(end+Vector2.right,end+Vector2.right*1.1f,4,n,ContactMode.Face,1,.001f);
            Check(teleport.BreakCount==1&&teleport.LastBreakReason.Contains("接触位置"),"A disconnected contact position cannot bridge a long strand");
            var lift=new CrumbEconomy(c);end=ZigzagTestPath.Trace(lift,n);float before=lift.BestLength;lift.EndStroke();
            Check(lift.Growing==null&&lift.Motion.Turns==0&&lift.GraphiteCredit==0,"Contact release ends growth and clears motion and graphite carry");
            lift.Stroke(end,end+Vector2.right*.1f,4,n,ContactMode.Face,1,.001f);Check(lift.BestLength==before&&lift.Growing==null,"A new press must establish fresh rubbing history");
            var white=new CrumbEconomy(c);ZigzagTestPath.Trace(white,soft,halves:400,graphitePerUnit:0);
            white.Collect();white.Roll();Check(white.BestLength==0&&white.ProducedGrams==0&&white.InventoryPrice==0&&white.Ball.Grams==0,"Blank rubbing cannot farm long pieces or ball income");
            var sparse=new CrumbEconomy(c);ZigzagTestPath.Trace(sparse,n,graphitePerUnit:.00005f);
            Check(sparse.BestLength<regular.BestLength*.3f,"Actual graphite quantity bounds growth");
            var carry=new CrumbEconomy(c);end=ZigzagTestPath.Trace(carry,n);before=carry.BestLength;
            ZigzagTestPath.Trace(carry,n,origin:new Vector2(4,0),graphitePerUnit:0);
            Check(carry.BestLength-before<=c.graphiteCarryCm+.001f,"Crossing a blank gap consumes at most the finite carried graphite credit");
            var gentle=new CrumbEconomy(c);ZigzagTestPath.Trace(gentle,soft,irregular:true);
            Check(gentle.BestLength>regular.BestLength&&gentle.BreakCount==0,"Soft accepts natural amplitude and period variation and grows longer");
            var ordinaryIrregular=new CrumbEconomy(c);ZigzagTestPath.Trace(ordinaryIrregular,n,irregular:true);
            Check(ordinaryIrregular.BestLength>10,"Ordinary accepts non-metronomic human-like rubbing too");
            var sandTrace=new CrumbEconomy(c);ZigzagTestPath.Trace(sandTrace,sand,advance:6,halves:60);
            Check(sandTrace.BestLength>=5&&sandTrace.BestLength<=c.sandZigLength+.001f&&sandTrace.BreakCount>0,"Skilled sand use can reach sellable length but breaks sooner");
            var edge=new CrumbEconomy(c);ZigzagTestPath.Trace(edge,n,mode:ContactMode.Edge);
            Check(edge.BestLength>5&&edge.BestLength<regular.BestLength&&edge.Growing.Thickness<regular.Growing.Thickness,"Edge grows a thinner shorter strand than face");
            var corner=new CrumbEconomy(c);ZigzagTestPath.Trace(corner,n,mode:ContactMode.Corner);corner.Collect();corner.Roll();
            Check(corner.BestLength==0&&corner.Ball.Grams>0,"Corner creates ball material so precision work is not wasted");
            var moreRubs=new CrumbEconomy(c);ZigzagTestPath.Trace(moreRubs,n,halves:80);
            Check(moreRubs.BestLength<regular.BestLength*1.15f,"Doubling traveled zigzag distance cannot double length at equal center progress");
            var vertical=new CrumbEconomy(c);ZigzagTestPath.Trace(vertical,n,forward:Vector2.up,oscillation:Vector2.right);
            Check(vertical.BestLength>20&&Vector2.Dot(vertical.Motion.Direction,Vector2.up)>.98f,"Horizontal hand reversals with vertical center progression work");
            var diagonal=new CrumbEconomy(c);ZigzagTestPath.Trace(diagonal,n,forward:new Vector2(1,1).normalized,oscillation:new Vector2(-1,1).normalized);
            Check(diagonal.BestLength>20,"Diagonal rubbing does not require exact axis alignment");
            var dense=new CrumbEconomy(c);ZigzagTestPath.Trace(dense,n,subdivisions:12);
            Check(Mathf.Abs(dense.BestLength-regular.BestLength)<.02f,"Sampling rate variation preserves growth");
            var failed=new CrumbEconomy(c);ZigzagTestPath.Trace(failed,n);failed.EndStroke("接触変更",true);failed.Collect();failed.Roll();long quote=failed.InventoryPrice+c.BallPrice(failed.Ball.Grams);
            Check(failed.SellLongs()+failed.SellBall()==quote&&failed.Wallet.CrumbIncome==quote,"New generation still feeds unchanged collection ball and price rules");
            Check(Mathf.Abs(failed.ProducedGrams-failed.AccountedMass)<.001f,"Graphite-based generation and all transfers conserve material");
            var config=Resources.Load<PrototypeConfig>("PrototypeConfig");using(var paper=new Paper(config)){
                var old=paper.Drawing.Erased;var result=paper.Stroke(new Vector2(-2,-1.55f),new Vector2(2,-1.55f),4,0,n,new PerformanceModifiers());
                Check(result.graphiteArea>0&&paper.Drawing.Erased>old,"Paper exposes actual removed target graphite area");
                var blank=paper.Stroke(new Vector2(-3,2.4f),new Vector2(3,2.4f),4,0,n,new PerformanceModifiers());
                Check(blank.graphiteArea==0,"Paper does not report graphite for truly blank space");
            }
            Check(c.LongPrice(30,1)==58&&c.BallPrice(1)==20,"Existing price formula remains unchanged");
            File.WriteAllText("TestResults/zigzag-checks.txt",$"PASS {count} zigzag checks\nOrdinary={regular.BestLength:0.00}cm Soft irregular={gentle.BestLength:0.00}cm Sand={sandTrace.BestLength:0.00}cm\n"+report);
        }
    }
}
