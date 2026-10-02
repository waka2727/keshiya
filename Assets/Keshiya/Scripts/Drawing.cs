using UnityEngine;
namespace Keshiya {
 // Graphite mass is independent from paper colour and damage.
 public sealed class Drawing : IStrokeLayer {
  public readonly float[] Ink; public readonly int Width,Height;
  double initial, remaining;
  readonly float fallbackWidth;public float[] OriginalInk;
  public float InitialMass=>(float)initial;
  public float Erased => initial<=0 ? 1 : Mathf.Clamp01((float)(1-remaining/initial));
  public Drawing(int w,int h,JobDefinition job=null,Vector2 paperSize=default) { Width=w;Height=h;fallbackWidth=job?.writing!=null?job.writing.lineWidth:1;Ink=new float[w*h];if(job?.artwork!=null){Ink=JobArtworkData.ReadMask(job.artwork.eraseMask);if(job.writing!=null){float mass=job.writing.Mass(job.pressure);for(int k=0;k<Ink.Length;k++)Ink[k]*=mass;}OriginalInk=(float[])Ink.Clone();foreach(var v in Ink)initial+=v;remaining=initial;return;}if(job!=null&&job.customDrawing)JobArtwork.Raster(Ink,w,h,paperSize,job.targetPaths,true,(job.writing!=null?job.writing.lineWidth:1)*job.targetLineScale);else if(job!=null&&job.letterCorrection)LetterLayout.Raster(Ink,w,h,paperSize,LetterLayout.Target,true,fallbackWidth);else if(job!=null&&job.precision)GeneratePrecision(job,paperSize);else Generate(); if(job!=null&&job.writing!=null){float mass=job.writing.Mass(job.pressure);for(int i=0;i<Ink.Length;i++)Ink[i]*=mass;} foreach(var v in Ink) initial+=v;remaining=initial; }
  void GeneratePrecision(JobDefinition job,Vector2 size) {
   for(int side=-1;side<=1;side+=2)for(int i=0;i<100;i++){
    float ya=Mathf.Lerp(-job.targetHalfLength,job.targetHalfLength,i/100f),yb=Mathf.Lerp(-job.targetHalfLength,job.targetHalfLength,(i+1)/100f);
    float xa=side*(job.targetOffset+.009f*Mathf.Sin(ya*7)),xb=side*(job.targetOffset+.009f*Mathf.Sin(yb*7));
    Line(.5f+xa/size.x,.5f+ya/size.y,.5f+xb/size.x,.5f+yb/size.y,job.targetHalfWidth/size.x);
   }
  }
  public void Erase(int i,float amount) { float removed=Mathf.Min(Ink[i],Mathf.Max(0,amount));Ink[i]-=removed;remaining-=removed; }
  public void ApplyContact(int i,float strength) => Erase(i,strength);
  void Line(float ax,float ay,float bx,float by,float thickness=.004f) {
   thickness*=Mathf.Max(.5f,fallbackWidth);
   var a=new Vector2(ax,ay);var b=new Vector2(bx,by);var ab=b-a;
   int x0=Mathf.Max(0,Mathf.FloorToInt((Mathf.Min(ax,bx)-thickness)*Width));int x1=Mathf.Min(Width-1,Mathf.CeilToInt((Mathf.Max(ax,bx)+thickness)*Width));
   int y0=Mathf.Max(0,Mathf.FloorToInt((Mathf.Min(ay,by)-thickness)*Height));int y1=Mathf.Min(Height-1,Mathf.CeilToInt((Mathf.Max(ay,by)+thickness)*Height));
   for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++) {var p=new Vector2((x+.5f)/Width,(y+.5f)/Height);float t=Mathf.Clamp01(Vector2.Dot(p-a,ab)/Mathf.Max(.000001f,ab.sqrMagnitude));float d=Vector2.Distance(p,a+t*ab);if(d<thickness){float grain=.65f+.35f*Mathf.PerlinNoise(x*.73f,y*.81f);Ink[y*Width+x]=Mathf.Max(Ink[y*Width+x],.86f*grain*Mathf.Clamp01((thickness-d)/thickness*3));}}
  }
  void Ellipse(float cx,float cy,float rx,float ry) { for(int i=0;i<120;i++){float a=i*Mathf.PI/60,b=(i+1)*Mathf.PI/60;Line(cx+Mathf.Cos(a)*rx,cy+Mathf.Sin(a)*ry,cx+Mathf.Cos(b)*rx,cy+Mathf.Sin(b)*ry);} }
  void Generate() {
   Ellipse(.48f,.52f,.20f,.23f); // cat, hand-drawn pencil study
   Line(.30f,.65f,.31f,.87f);Line(.31f,.87f,.41f,.73f);Line(.55f,.73f,.65f,.87f);Line(.65f,.87f,.67f,.63f);
   Ellipse(.41f,.55f,.013f,.023f);Ellipse(.56f,.55f,.013f,.023f);
   Line(.46f,.47f,.5f,.47f);Line(.5f,.47f,.48f,.44f);Line(.48f,.44f,.46f,.47f);Line(.48f,.44f,.48f,.4f);Line(.48f,.4f,.43f,.38f);Line(.48f,.4f,.53f,.38f);
   for(int i=0;i<3;i++){Line(.36f,.46f-i*.025f,.20f,.50f-i*.065f);Line(.61f,.46f-i*.025f,.77f,.50f-i*.065f);}
   for(int i=0;i<140;i++){float x=.22f+i*.004f;Line(x,.19f+.025f*Mathf.Sin(i*.16f),x+.004f,.19f+.025f*Mathf.Sin((i+1)*.16f),.003f);}
   Ellipse(.81f,.77f,.05f,.065f);Line(.12f,.7f,.2f,.77f);Line(.12f,.77f,.2f,.7f);
  }
 }
}
