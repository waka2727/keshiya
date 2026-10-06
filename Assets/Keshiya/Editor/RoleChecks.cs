using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
namespace Keshiya.Editor
{
    public static class RoleChecks
    {
        static int count;static StringBuilder report;
        static void Check(bool test,string name){if(!test)throw new Exception("0.2.1 FAIL: "+name);count++;report.AppendLine("PASS: "+name);Debug.Log("PASS: "+name);}
        static float Damage(PrototypeConfig c,EraserDefinition tool,JobDefinition job,float speed,int passes)
        {
            using(var paper=new Paper(c,.1f,job)){
                for(int i=0;i<passes;i++)paper.Stroke(new Vector2(-.7f,1),new Vector2(.7f,1),speed,i*.02f,tool,new PerformanceModifiers());
                return paper.AverageDamage;
            }
        }
        public static void Run()
        {
            count=0;report=new StringBuilder();
            var catalog=Resources.Load<ToolCatalog>("ToolCatalog");var n=catalog.tools[0];var s=catalog.tools[1];var abrasive=catalog.tools[2];
            var job=Resources.Load<JobDefinition>("PrecisionJob");var c=Resources.Load<PrototypeConfig>("PrototypeConfig");var m=new PerformanceModifiers();
            Check(n.erasePower==1.15f&&n.paperDamageMultiplier==1&&n.highSpeedRisk==1&&n.radius==.27f,"Ordinary base performance unchanged");
            Check(abrasive.erasePower==3.6f&&abrasive.paperDamageMultiplier==5&&abrasive.highSpeedRisk==1.6f,"Sand power and danger unchanged");
            Check(c.dangerousSpeed==16&&c.wearStartSpeed==12&&c.completionThreshold==.95f,"Existing ordinary-paper thresholds and 95 percent retained");
            Check(job.precision&&job.letterCorrection&&job.paperFragility==2,"Playable precision asset is a thin-paper letter correction");
            Check(s.Pickup(null)==1&&n.Pickup(job)==1,"Paper specialization does not alter ordinary-paper or ordinary-tool power");
            Check(s.Power(m)*s.Pickup(job)>n.Power(m)*.99f,"Soft picks up thin-paper graphite as efficiently as ordinary");
            Check(Damage(c,n,null,11,20)==0,"Ordinary paper remains safe at comfortable speed");
            float regularThin=Damage(c,n,job,11,20),softThin=Damage(c,s,job,11,20);
            Check(regularThin>0&&softThin==0,"At speed 11 thin paper requires care with ordinary but is safe with soft");
            Check(Damage(c,n,job,4,30)==0,"Patient ordinary-tool work on thin paper remains safe");
            Check(Damage(c,s,job,30,12)<Damage(c,n,job,30,12)*.2f,"Soft strongly reduces intense repeated thin-paper damage");
            Check(Damage(c,abrasive,null,25,5)>Damage(c,n,null,25,5)*3,"Sand remains powerful and dangerous on ordinary paper");
            Check(s.crumbCohesion>n.crumbCohesion&&n.crumbCohesion>abrasive.crumbCohesion,"Tool crumb cohesion differs");
            Check(s.longCrumbPotential>n.longCrumbPotential&&abrasive.longCrumbPotential<n.longCrumbPotential,"Future long-crumb metadata differs without a selling system");
            var f=n.Contact(m,ContactMode.Face,new EraserState(),0);var edge=n.Contact(m,ContactMode.Edge,new EraserState(),0);var corner=n.Contact(m,ContactMode.Corner,new EraserState(),0);
            Check(f.HalfSize.y>edge.HalfSize.y*4&&edge.HalfSize.x>corner.HalfSize.x*4,"Face, directional strip, and corner are distinct");
            var diagonal=n.Contact(m,ContactMode.Edge,new EraserState(),45);
            Check(edge.Weight(new Vector2(.17f,.17f))==0&&diagonal.Weight(new Vector2(.17f,.17f))>0,"45 degree edge reaches diagonal points the horizontal edge cannot");
            foreach(float angle in new[]{0f,45f,90f,135f}){
                var contact=n.Contact(m,ContactMode.Edge,new EraserState(),angle);var mesh=ContactMesh.Solid(contact,.014f);bool correct=true;
                for(int i=0;i<ContactMesh.Segments;i++){Vector3 v=mesh.vertices[i];var p=new Vector2(v.x,v.z);correct&=contact.Weight(p)<.0003f&&contact.Weight(p*.97f)>0;}
                Check(correct,"Edge mesh and pressure boundary agree at "+angle);UnityEngine.Object.DestroyImmediate(mesh);
            }
            foreach(var tool in new[]{catalog.tools[0],catalog.tools[1],catalog.tools[2]}){
                using(var paper=new Paper(c,.1f,job)){
                    var state=new EraserState();int passes=0;
                    while(paper.Drawing.Erased<.95f&&passes++<45)foreach(var path in LetterLayout.Target)for(int k=1;k<path.Length;k++){
                        paper.Stroke(path[k-1],path[k],3,passes,tool,m,tool.Contact(m,ContactMode.Corner,state,0));state.Use(Vector2.Distance(path[k-1],path[k]),ContactMode.Corner,tool,m);
                    }
                    Check(paper.Drawing.Erased>=.95f&&paper.Protection.Loss==0&&paper.PeakDamage==0,"Wearing corner erases only B safely: "+tool.name);
                    var work=new Job(c,job);work.StartWork();work.Tick(100000);Check(work.Complete(paper)&&work.Result.basic==500&&work.Result.speed==0,"Letter completion remains unlimited and full-base-pay: "+tool.name);
                }
            }
            using(var paper=new Paper(c,.1f,job)){
                paper.Stroke(new Vector2(.14f,-.4f),new Vector2(.14f,.4f),4,0,n,m,f);
                Check(paper.Protection.Loss>0&&paper.PeakDamage==0,"Face catches adjacent A independently of paper damage");
            }
            using(var normal=new Paper(c,.1f,job))using(var soft=new Paper(c,.1f,job)){
                normal.Stroke(new Vector2(.14f,-.4f),new Vector2(.14f,.4f),4,0,n,m,f);
                soft.Stroke(new Vector2(.14f,-.4f),new Vector2(.14f,.4f),4,0,s,m,s.Contact(m));
                Check(soft.Protection.Loss>0&&soft.Protection.Loss<normal.Protection.Loss*.5f,"Soft reduces accidental letter abrasion but is not immune");
            }
            using(var vertical=new Paper(c,.1f,job))using(var horizontal=new Paper(c,.1f,job)){
                var a=new Vector2(.14f,-.38f);var b=new Vector2(.14f,.38f);
                vertical.Stroke(a,b,3,0,n,m,n.Contact(m,ContactMode.Edge,new EraserState(),90));horizontal.Stroke(a,b,3,0,n,m,edge);
                Check(vertical.Drawing.Erased>0&&vertical.Protection.Loss==0&&horizontal.Protection.Loss>0,"Orienting the edge follows B's stem safely; sideways crosses A");
            }
            foreach(float fraction in new[]{.03f,.10f,.25f,1f}){
                using(var paper=new Paper(c,.1f,job)){
                    for(int i=0;i<paper.Protection.Ink.Length;i++)paper.Protection.ApplyContact(i,paper.Protection.Ink[i]*fraction/job.protectedSensitivity);
                    Check(paper.Protection.Major==(fraction>=.2f),"Protection major status based on letter mass at "+fraction);
                    if(fraction==.03f){for(int i=0;i<paper.Drawing.Ink.Length;i++)paper.Drawing.Erase(i,1);Check(new Job(c,job).Complete(paper)&&paper.Protection.Grade=="軽微","Minor letter mistakes still allow completing the job");}
                }
            }
            File.WriteAllText("TestResults/role-checks.txt",$"PASS {count} role checks\nThin-paper speed 11 average damage: ordinary={regularThin}, soft={softThin}\n"+report);
        }
    }
}

