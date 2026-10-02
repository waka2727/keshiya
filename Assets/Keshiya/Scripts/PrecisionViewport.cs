using UnityEngine;
namespace Keshiya {
 public sealed class PrecisionViewport:MonoBehaviour {
  public PrototypeGame Game;public Camera View;public float Zoom {get;private set;}=1;float fit=3.3f;Vector2 previous;bool dragging;
  public bool InWorkArea(Vector2 point)=>new Rect(358,105,922,560).Contains(new Vector2(point.x*1280/Screen.width,(Screen.height-point.y)*800/Screen.height));
  public Vector2 Center=>new Vector2(View.transform.position.x,View.transform.position.z);
  public Vector2 PaperPoint(Vector2 screen){var ray=View.ScreenPointToRay(screen);var plane=new Plane(Vector3.up,new Vector3(0,.058f,0));plane.Raycast(ray,out float enter);var p=ray.GetPoint(enter);return new Vector2(p.x,p.z);}
  public void Apply(float size){fit=size;View.orthographicSize=fit/Zoom;}
  void Move(Vector2 position){var half=Game.Paper.Size*.5f;View.transform.position=new Vector3(Mathf.Clamp(position.x,-half.x,half.x),10,Mathf.Clamp(position.y,-half.y,half.y));}
  public void SetZoom(float value,Vector2 screen){var anchor=PaperPoint(screen);Zoom=Mathf.Clamp(value,1,Game.Playtest!=null?Game.Playtest.maximumZoom:4);Apply(fit);Move(Center+anchor-PaperPoint(screen));Game.Controller.ResetContact();}
  public void Pan(Vector2 from,Vector2 to){Move(Center+PaperPoint(from)-PaperPoint(to));Game.Controller.ResetContact();}
  public void ResetView(){Zoom=1;Move(Vector2.zero);Apply(fit);dragging=false;Game.Controller.ResetContact();}
  public void HandleWheel(float wheel,bool control,Vector2 point){if(!control)Pan(point,point+Vector2.down*(wheel*80*Screen.height/800f));else SetZoom(Zoom*Mathf.Pow(Game.Playtest?.zoomStep??1.2f,wheel),point);}
  public bool HandleInput(){if(Game.InputBlocked||!Game.Working)return false;var point=(Vector2)Input.mousePosition;
   if(Input.GetKeyDown(KeyCode.Home)){ResetView();return true;}
   bool keyZoom=Input.GetKeyDown(KeyCode.Equals)||Input.GetKeyDown(KeyCode.KeypadPlus)||Input.GetKeyDown(KeyCode.Minus)||Input.GetKeyDown(KeyCode.KeypadMinus);float wheel=Input.mouseScrollDelta.y;if(Input.GetKeyDown(KeyCode.Equals)||Input.GetKeyDown(KeyCode.KeypadPlus))wheel=1;if(Input.GetKeyDown(KeyCode.Minus)||Input.GetKeyDown(KeyCode.KeypadMinus))wheel=-1;
   if(wheel!=0&&View.pixelRect.Contains(point)){HandleWheel(wheel,keyZoom||Input.GetKey(KeyCode.LeftControl)||Input.GetKey(KeyCode.RightControl),point);return true;}
   if(Input.GetMouseButtonDown(2)&&View.pixelRect.Contains(point)){dragging=true;previous=point;}
   if(dragging&&Input.GetMouseButton(2)){Pan(previous,point);previous=point;return true;}dragging=false;return false;
  }
 }
}
