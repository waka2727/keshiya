using UnityEngine;
namespace Keshiya {
 // GUI pixel coordinates; independent of the work camera and all world hit geometry.
 public sealed class PreviewViewport {
  public float Zoom {get;private set;}=1; public Vector2 Offset {get;private set;}
  public Rect Frame=new Rect(415,179,800,405); public Vector2 ImageSize=new Vector2(1240,1754);
  Vector2 Fit=>ImageSize*Mathf.Min(Frame.width/ImageSize.x,Frame.height/ImageSize.y);
  public Rect ImageRect=>new Rect(Frame.size*.5f+Offset-Fit*Zoom*.5f,Fit*Zoom);
  public void Reset(){Zoom=1;Offset=Vector2.zero;}
  void Clamp(){var limit=Vector2.Max(Vector2.zero,(Fit*Zoom-Frame.size)*.5f);Offset=new Vector2(Mathf.Clamp(Offset.x,-limit.x,limit.x),Mathf.Clamp(Offset.y,-limit.y,limit.y));}
  public void Pan(Vector2 delta){Offset+=delta;Clamp();}
  public void Wheel(float wheel,bool control,Vector2 point,float maximum=4,float step=1.2f){if(!control){Pan(Vector2.up*wheel*80);return;}float next=Mathf.Clamp(Zoom*Mathf.Pow(step,wheel),1,maximum);Vector2 anchor=point-Frame.center;Offset=anchor-(anchor-Offset)*(next/Zoom);Zoom=next;Clamp();}
 }
}