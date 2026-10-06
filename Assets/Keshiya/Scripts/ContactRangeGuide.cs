using UnityEngine;
namespace Keshiya {
 // Product guide: same superellipse boundary used by ContactFootprint.Weight, no second hit area.
 public sealed class ContactRangeGuide:MonoBehaviour {
  public PrototypeGame Game;public bool Enabled=true;LineRenderer line;Material material;ContactMode previousMode;float emphasisUntil;bool initialized;
  public bool Visible=>line!=null&&line.enabled;
  public ContactFootprint Geometry=>Game.Controller.Pressing&&Game.LastStrokeFrame==Time.frameCount?Game.LastStrokeContact:Game.Contact;
  public Vector2 Point(int i)=>Geometry.OutlinePoint(i*Mathf.PI*2/96);
  void LateUpdate(){Refresh();}
  public void Refresh(){if(Game==null||Game.Paper==null)return;
   if(line==null){line=gameObject.AddComponent<LineRenderer>();material=new Material(Game.OverlayShader);material.SetInt("_ZTest",8);line.sharedMaterial=material;line.loop=true;line.useWorldSpace=true;line.positionCount=96;line.numCornerVertices=2;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
   var p=Game.Presentation.ContactPosition;var half=Game.Paper.Size*.5f;
   line.enabled=Enabled&&Game.Working&&!Game.InputBlocked&&!Game.CurrentJob.Completed&&Game.HasUsableTool&&Game.Controller.PointerOnPaper&&Mathf.Abs(p.x)<=half.x&&Mathf.Abs(p.y)<=half.y;
   if(!initialized||previousMode!=Game.Mode){initialized=true;previousMode=Game.Mode;emphasisUntil=Time.unscaledTime+.7f;}
   if(!line.enabled)return;bool emphasize=Time.unscaledTime<emphasisUntil;float pixel=Game.Viewport.View.orthographicSize*2/Mathf.Max(1,Game.Viewport.View.pixelHeight);line.widthMultiplier=Mathf.Max(emphasize?.009f:.006f,pixel*(emphasize?1.4f:1));material.color=new Color(.08f,.48f,.40f,emphasize?.85f:Game.Controller.Pressing?.74f:.45f);
   for(int i=0;i<96;i++){var v=p+Point(i);line.SetPosition(i,new Vector3(v.x,.075f,v.y));}
  }
  void OnDestroy(){if(material!=null)Destroy(material);}
 }
}