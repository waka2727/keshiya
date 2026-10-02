using System.Collections.Generic;
using UnityEngine;
namespace Keshiya.Editor {
 public static class PrecisionLayouts {
  static void L(List<JobPath> p,float x,float y,float xx,float yy,ContactMode mode=ContactMode.Corner,float angle=0){p.Add(new JobPath(new[]{new Vector2(x,y),new Vector2(xx,yy)}){workMode=mode,workAngle=angle});}
  static void Oval(List<JobPath> p,float x,float y,float rx,float ry){var a=new Vector2[33];for(int i=0;i<33;i++){float t=i*Mathf.PI/16;a[i]=new Vector2(x+Mathf.Cos(t)*rx,y+Mathf.Sin(t)*ry);}p.Add(new JobPath(a));}
  public static void Stroke(JobDefinition j){var t=new List<JobPath>();var k=new List<JobPath>();
   // A 0.24-unit F, with only 0.08 between its middle bar and the unwanted bar.
   L(k,0,0,0,.24f);L(k,0,.24f,.16f,.24f);L(k,0,.12f,.14f,.12f);L(t,.065f,.04f,.15f,.04f);WorldArtwork.Text(k,"INISH",.24f,0,.04f);
   j.targetPaths=t.ToArray();j.protectedPaths=k.ToArray();j.targetLineScale=.5f;j.protectedLineScale=.5f;j.detailResolution=1024;j.pressure=1;
  }
  public static void Exam(JobDefinition j){var t=new List<JobPath>();var k=new List<JobPath>();WorldArtwork.Text(k,"NAME MIO",-2.55f,1.6f,.035f);WorldArtwork.Text(k,"MATH  12",.7f,1.6f,.035f);
   for(int row=0;row<6;row++){float y=1.0f-row*.45f;L(k,-2.6f,y-.09f,2.5f,y-.09f);WorldArtwork.Text(k,(row+1)+" 12+8=",-2.5f,y,.032f);WorldArtwork.Text(t,row%2==0?"18":"19",-.67f,y,.027f);WorldArtwork.Text(k,"20",-.67f,y-.30f,.027f);WorldArtwork.Text(k,"OK",.2f,y,.028f);L(k,1.8f,y+.04f,1.86f,y);L(k,1.86f,y,1.97f,y+.16f);}
   j.targetPaths=t.ToArray();j.protectedPaths=k.ToArray();j.targetLineScale=.5f;j.protectedLineScale=.5f;j.detailResolution=1024;
  }
  public static void Manga(JobDefinition j,bool deadline){var t=new List<JobPath>();var k=new List<JobPath>();int cols=deadline?3:2;
   for(int row=0;row<2;row++)for(int col=0;col<cols;col++){float width=5.6f/cols-.1f,x=-2.8f+col*(width+.1f),y=-1.95f+row*2;
    WorldArtwork.Box(k,x,y,width,1.85f);float cx=x+width*.48f,cy=y+.94f;
    // Inked head, hair, eyes, neck, shoulders and hands.
    Oval(k,cx,cy,.25f,.31f);L(k,cx-.24f,cy+.10f,cx-.10f,cy+.38f);L(k,cx-.10f,cy+.38f,cx+.05f,cy+.23f);L(k,cx+.05f,cy+.23f,cx+.22f,cy+.34f);
    L(k,cx-.13f,cy+.04f,cx-.075f,cy+.04f);L(k,cx+.075f,cy+.04f,cx+.13f,cy+.04f);L(k,cx-.065f,cy-.13f,cx+.065f,cy-.13f);
    L(k,cx-.08f,cy-.32f,cx-.08f,cy-.42f);L(k,cx+.08f,cy-.32f,cx+.08f,cy-.42f);L(k,cx-.08f,cy-.42f,cx-.4f,cy-.50f);L(k,cx+.08f,cy-.42f,cx+.4f,cy-.50f);L(k,cx-.4f,cy-.50f,cx-.32f,y+.12f);L(k,cx+.4f,cy-.50f,cx+.32f,y+.12f);
    for(int f=0;f<3;f++)L(k,cx+.42f+f*.045f,cy-.42f,cx+.42f+f*.045f,cy-.29f);
    Oval(k,x+width*.73f,y+1.52f,width*.18f,.19f);WorldArtwork.Text(k,row==0?"HI":"OK",x+width*.64f,y+1.46f,.018f);
    // Construction lines: open patches, parallel contour strokes, narrow facial corrections.
    L(t,x+.17f,y+.28f,x+.17f,y+1.35f,ContactMode.Edge,90);
    L(t,cx-.40f,cy-.15f,cx-.40f,cy+.23f,ContactMode.Edge,90);
    L(t,cx+.39f,cy+.03f,cx+.39f,cy+.30f,ContactMode.Corner);
    L(t,cx-.04f,cy-.015f,cx+.04f,cy-.015f); // narrow pencil nose between the inked eyes and mouth
    L(t,cx-.02f,cy+.43f,cx+.10f,cy+.43f);
    if(width>2){for(int n=0;n<4;n++)L(t,x+.5f+n*.09f,y+1.40f,x+.68f+n*.09f,y+1.49f,ContactMode.Face);}
    else for(int n=0;n<3;n++)L(t,x+.3f+n*.045f,y+1.52f,x+.40f+n*.045f,y+1.62f,ContactMode.Face);
   }j.targetPaths=t.ToArray();j.protectedPaths=k.ToArray();j.targetLineScale=.5f;j.protectedLineScale=.5f;j.detailResolution=1024;
  }
 }
}
