using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Keshiya.Editor
{
    public static class ToolChecks
    {
        static int checks;
        static StringBuilder report;
        static void Check(bool value,string name){if(!value)throw new Exception("0.2 FAIL: "+name);checks++;report.AppendLine("PASS: "+name);Debug.Log("PASS: "+name);}
        static Vector2 DamageAfter(PrototypeConfig c,EraserDefinition e,float speed,int passes,float interval=.02f)
        {
            using(var p=new Paper(c)){
                for(int i=0;i<passes;i++)p.Stroke(new Vector2(-.8f,0),new Vector2(.8f,0),speed,i*interval,e,new PerformanceModifiers());
                return new Vector2(p.AverageDamage,p.PeakDamage);
            }
        }
        public static void Run()
        {
            checks=0;report=new StringBuilder();
            var catalog=AssetDatabase.LoadAssetAtPath<ToolCatalog>("Assets/Keshiya/Resources/ToolCatalog.asset");
            var normal=catalog.tools[0];var soft=catalog.tools[1];var sand=catalog.tools[2];
            var precise=ScriptableObject.CreateInstance<JobDefinition>();precise.precision=true;
            var c=ScriptableObject.CreateInstance<PrototypeConfig>();c.resolution=256;var m=new PerformanceModifiers();
            Check(catalog.tools.Length>=3&&normal!=soft&&soft!=sand,"Three distinct tool assets are available");
            Check(sand.erasePower>normal.erasePower*2&&soft.erasePower<normal.erasePower,"Power ordering supports distinct tool roles");
            Check(soft.paperDamageMultiplier<normal.paperDamageMultiplier&&sand.paperDamageMultiplier>normal.paperDamageMultiplier*3,"Damage multipliers differ substantially");
            Check(soft.crumbAmount>normal.crumbAmount&&sand.crumbSizeMultiplier<1,"Soft produces more crumbs and sand produces finer crumbs");
            var old=UnityEngine.Object.Instantiate(c);old.dangerousSpeed=10;old.wearStartSpeed=4;
            var previous=DamageAfter(old,normal,11,20);var comfortable=DamageAfter(c,normal,11,20);
            Check(previous.y>0&&comfortable.y==0,"Speed 11 rubbing is safe after threshold relaxation and damaged the old tuning");
            Check(DamageAfter(c,normal,35,10).y>0,"Extreme speed still damages ordinary paper");
            Check(DamageAfter(c,sand,25,5).x>DamageAfter(c,normal,25,5).x*3,"Sand at fast speed is clearly more dangerous than ordinary");
            Check(DamageAfter(c,soft,30,10).x<DamageAfter(c,normal,30,10).x*.2f,"Soft tool strongly reduces high-speed damage");
            Check(DamageAfter(c,sand,5,30).y==0,"Careful use of sand remains safe");
            Check(DamageAfter(c,normal,15,90,.01f).x>DamageAfter(c,normal,15,90,2).x,"Continuous local rubbing builds more wear than spaced passes");
            var state=new EraserState();state.Use(70,ContactMode.Face,normal,m);
            Check(state.Remaining(normal,m)<1&&state.UsedUnits>0,"Travel consumes durability");
            Check(state.CornerSharpness==1,"Face use preserves corner sharpness");
            var fresh=normal.Contact(m,ContactMode.Corner,state,0);state.Use(70,ContactMode.Corner,normal,m);
            Check(state.CornerSharpness<1,"Corner use rounds the corner");
            Check(normal.Contact(m,ContactMode.Corner,state,0).HalfSize.x>fresh.HalfSize.x,"Rounded corner has a slightly wider visible contact patch");
            state.Use(100000,ContactMode.Corner,normal,m);
            Check(Mathf.Abs(state.Remaining(normal,m)-normal.minimumRemaining)<.0001f&&state.CornerSharpness>=normal.minimumSharpness,"Prototype consumption floor keeps jobs playable");
            float units=state.UsedUnits;state.Use(0,ContactMode.Corner,normal,m);Check(state.UsedUnits==units,"Stationary contact cannot consume a tool");
            foreach(ContactMode mode in Enum.GetValues(typeof(ContactMode)))
            {
                var f=normal.Contact(m,mode,new EraserState(),90);var mesh=ContactMesh.Solid(f,.014f);bool matches=true;
                for(int i=0;i<ContactMesh.Segments;i++){var v=mesh.vertices[i];var p=new Vector2(v.x,v.z);matches&=f.Weight(p)<.0002f&&f.Weight(p*.98f)>0;}
                Check(matches,"Visible rotated sole matches contact geometry: "+mode);UnityEngine.Object.DestroyImmediate(mesh);
            }
            var face=normal.Contact(m,ContactMode.Face,new EraserState(),0);var corner=normal.Contact(m,ContactMode.Corner,new EraserState(),0);
            Check(face.HalfSize.x>corner.HalfSize.x*4,"Face and corner give meaningfully different contact areas");
            using(var a=new Paper(c))using(var b=new Paper(c))
            {
                a.Stroke(new Vector2(-1,0),new Vector2(1,0),4,0,normal,m);
                b.Stroke(new Vector2(-1,0),new Vector2(1,0),4,0,sand,m);
                Check(b.Drawing.Erased>a.Drawing.Erased,"Sand actually erases faster in the paper simulation");
            }
            using(var paper=new Paper(c,.1f,precise))
            {
                float graphite=paper.Drawing.Erased;
                paper.Stroke(Vector2.zero,Vector2.zero,3,0,normal,m);
                Check(paper.Protection.Loss==0&&paper.Drawing.Erased==graphite,"Touching protection without rubbing does no harm");
                paper.Stroke(new Vector2(precise.targetOffset,-1.2f),new Vector2(precise.targetOffset,1.2f),4,1,normal,m,face);
                Check(paper.Protection.Loss>0&&paper.PeakDamage==0,"Broad face harms the adjacent protected ink independently from paper");
                Check(!paper.Protection.Major,"A small protection mistake is not immediately a major mistake");
                for(int i=0;i<30;i++)paper.Stroke(new Vector2(0,-1.6f),new Vector2(0,1.6f),4,2+i,normal,m,face);
                Check(paper.Protection.Major,"Heavy protected-ink removal becomes a major mistake");
                var ordinary=Reward.Calculate(c,1,0,0,1000);var penalty=Reward.ApplyProtection(ordinary,paper.Protection,precise);
                Check(penalty.basic==ordinary.basic&&penalty.finish<ordinary.finish&&penalty.protection==0,"Protection mistakes affect quality bonuses, never time-based base pay");
                Check(new Job(c,precise).Complete(paper),"Even a major protection mistake allows submitting sufficiently erased work");
            }
            using(var vertical=new Paper(c,.1f,precise))using(var horizontal=new Paper(c,.1f,precise))
            {
                Vector2 a=new Vector2(precise.targetOffset,-1.2f),b=new Vector2(precise.targetOffset,1.2f);
                vertical.Stroke(a,b,3,0,normal,m,normal.Contact(m,ContactMode.Edge,new EraserState(),90));
                horizontal.Stroke(a,b,3,0,normal,m,normal.Contact(m,ContactMode.Edge,new EraserState(),0));
                Check(vertical.Drawing.Erased>0&&vertical.Protection.Loss==0,"A rotated edge can safely follow a narrow vertical pencil line");
                Check(horizontal.Protection.Loss>0,"Using the broad direction of the edge crosses the protected line");
            }
            foreach(var e in new[]{normal,soft,sand})
            {
                using(var paper=new Paper(c,.1f,precise))
                {
                    var inventory=new EraserState();var job=new Job(c,precise);job.StartWork();
                    for(int pass=0;pass<35&&!job.CanComplete(paper);pass++)for(int side=-1;side<=1;side+=2)
                    {
                        var a=new Vector2(side*precise.targetOffset,-precise.targetHalfLength-.08f);var b=new Vector2(a.x,precise.targetHalfLength+.08f);
                        paper.Stroke(a,b,3,pass*3,e,m,e.Contact(m,ContactMode.Corner,inventory,0));inventory.Use(Vector2.Distance(a,b),ContactMode.Corner,e,m);
                    }
                    Check(job.CanComplete(paper)&&paper.Protection.Loss==0&&paper.PeakDamage==0,"Precision job is safely completable with worn corner: "+e.name);
                    job.Tick(100000);Check(job.Complete(paper)&&job.Result.speed==0&&job.Result.protection==precise.protectionBonus,"No timer failure; pristine protected-line bonus: "+e.name);
                }
            }
            using(var paper=new Paper(c)){Check(paper.Protection==null,"Normal job has no protected target");}
            var modified=ScriptableObject.CreateInstance<EraserDefinition>();modified.equipment.erasePower=2;m.erasePower=3;
            Check(Mathf.Abs(modified.Power(m)-modified.erasePower*6)<.0001f,"Base, equipment and skill multipliers compose multiplicatively");
            Check(c.completionThreshold==.95f,"Completion threshold remains 95 percent");
            File.WriteAllText("TestResults/tool-checks.txt",$"PASS {checks} tool checks\n"+report);
            UnityEngine.Object.DestroyImmediate(precise);UnityEngine.Object.DestroyImmediate(modified);UnityEngine.Object.DestroyImmediate(c);UnityEngine.Object.DestroyImmediate(old);
        }
    }
}
