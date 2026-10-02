using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Keshiya.Editor
{
    public static class FeelChecks
    {
        static int count;
        static StringBuilder report;
        static void Check(bool ok,string name)
        {
            if(!ok)throw new Exception("Feel regression FAIL: "+name);
            count++;report.AppendLine("PASS: "+name);Debug.Log("PASS: "+name);
        }
        sealed class ProtectedProbe : IStrokeLayer
        {
            public int hits;
            public readonly float protectedInk=1;
            public void ApplyContact(int index,float strength){if(strength>0)hits++;}
        }
        static float Integrate(ContactFootprint f,bool area)
        {
            double sum=0;const int n=300;
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float w=f.Weight(new Vector2(((x+.5f)/n*2-1)*f.HalfSize.x,((y+.5f)/n*2-1)*f.HalfSize.y));
                sum+=area?(w>0?1:0):w;
            }
            return (float)(sum*4*f.HalfSize.x*f.HalfSize.y/(n*n));
        }
        public static void Run()
        {
            count=0;report=new StringBuilder();
            var saved=AssetDatabase.LoadAssetAtPath<PrototypeConfig>("Assets/Keshiya/Resources/PrototypeConfig.asset");
            var savedE=AssetDatabase.LoadAssetAtPath<EraserDefinition>("Assets/Keshiya/Resources/OrdinaryEraser.asset");
            Check(saved.completionThreshold==.95f && saved.baseReward==1000 && saved.referenceSeconds==120 && saved.finishBonus==300 && saved.pristineBonus==250 && saved.speedBonus==300,"Existing completion and reward tuning retained");
            Check(saved.highSpeedDamage==.16f && saved.wearDamage==.035f && saved.wearThreshold==7 && saved.wearRecovery==.8f && saved.optimalSpeed==4 && saved.dangerousSpeed==16 && saved.wearStartSpeed==12 && saved.maximumSpeed==35 && saved.severeDamageThreshold==.85f,"Damage coefficients retained; authorized 0.2 speed thresholds applied");
            Check(savedE.radius==.27f && savedE.erasePower==1.15f && savedE.paperDamageMultiplier==1 && savedE.crumbAmount==1,"Original single eraser tuning retained");
            var face=ContactFootprint.BroadFace(.27f);
            float circleArea=Mathf.PI*.27f*.27f;
            Check(Mathf.Abs(Integrate(face,true)/circleArea-1)<.003f,"Contact area equals legacy circle within 0.3 percent");
            Check(Mathf.Abs(Integrate(face,false)/(circleArea*.5f)-1)<.003f,"Integrated contact pressure equals legacy circle within 0.3 percent");
            Check(face.Weight(Vector2.zero)==1 && face.Weight(new Vector2(.28f,0))==0 && face.Weight(new Vector2(.26f,.22f))==0,"Sole boundary clips outer corners and outside pixels");
            var mesh=ContactMesh.Solid(face,.2f);bool matches=true;
            for(int i=0;i<ContactMesh.Segments;i++)
            {
                Vector3 vertex=mesh.vertices[i];var p=new Vector2(vertex.x,vertex.z);
                matches &= face.Weight(p)<.0001f && face.Weight(p*.99f)>0;
            }
            Check(matches,"Every sole mesh boundary vertex matches the sampled footprint");UnityEngine.Object.DestroyImmediate(mesh);
            var edge=new ContactFootprint(new Vector2(.05f,.22f),4,new Vector2(.1f,0),90);
            Check(edge.Weight(new Vector2(.1f,0))==1 && edge.Weight(new Vector2(.1f,.1f))==0 && edge.Weight(new Vector2(.25f,0))>0,"Offset rotated edge footprint works without changing Paper");

            var c=ScriptableObject.CreateInstance<PrototypeConfig>();c.resolution=256;
            var e=ScriptableObject.CreateInstance<EraserDefinition>();var modifiers=new PerformanceModifiers();
            using(var a=new Paper(c,0))using(var b=new Paper(c,.1f))
            {
                var from=new Vector2(-2,0);var to=new Vector2(2,0);
                var quiet=a.Stroke(from,to,3,0,e,modifiers);
                b.Stroke(from,to,3,0,e,modifiers);
                Check(!quiet.Damaged && quiet.erasedFraction>0,"Safe stroke reports erasure but no damage event");
                float difference=0;for(int i=0;i<a.Drawing.Ink.Length;i++)difference+=Mathf.Abs(a.Drawing.Ink[i]-b.Drawing.Ink[i]);
                Check(difference>0 && Mathf.Abs(a.Drawing.Erased-b.Drawing.Erased)<.005f,"Cached grain introduces variation without a large erase-rate change");
                var probe=new ProtectedProbe();a.ProtectedContent=probe;
                a.Stroke(from,to,3,1,e,modifiers);
                Check(probe.hits>0 && probe.protectedInk==1 && a.PeakDamage==0,"Protected layer receives contact independently without erasing its ink");
            }
            using(var a=new Paper(c))using(var b=new Paper(c))
            {
                var from=new Vector2(-2,0);var to=new Vector2(2,0);
                var hit=a.Stroke(from,to,30,0,e,modifiers);
                b.Stroke(from,to,30,0,e,modifiers,new ContactFootprint(Vector2.one*.27f,2));
                Check(hit.Damaged && a.PeakDamage>0,"Actual new damage produces a damage result");
                Check(Mathf.Abs(a.AverageDamage/b.AverageDamage-1)<.03f,"Initial high-speed damage mass remains within 3 percent of circular baseline");
                var stationary=a.Stroke(to,to,30,1,e,modifiers);
                Check(!stationary.Damaged && stationary.erasedFraction==0,"Stationary contact produces no damage cue or erase event");
            }
            using(var paper=new Paper(c))
            {
                var result=paper.Stroke(new Vector2(-1,0),new Vector2(1,0),30,0,e,modifiers);
                bool bounded=true;
                for(int y=0;y<paper.Height;y++)for(int x=0;x<paper.Width;x++)
                {
                    if(paper.Damage[y*paper.Width+x]<=0)continue;
                    float px=((x+.5f)/paper.Width-.5f)*c.paperSize.x;
                    float py=((y+.5f)/paper.Height-.5f)*c.paperSize.y;
                    bounded &= Mathf.Abs(py)<face.HalfSize.y && px>-1-face.HalfSize.x && px<1+face.HalfSize.x;
                }
                Check(bounded,"Paper mutations stay inside the swept sole bounds");
            }
            Directory.CreateDirectory("TestResults");File.WriteAllText("TestResults/feel-checks.txt",$"PASS {count} feel checks\n"+report);
            UnityEngine.Object.DestroyImmediate(c);UnityEngine.Object.DestroyImmediate(e);
        }
    }
}
