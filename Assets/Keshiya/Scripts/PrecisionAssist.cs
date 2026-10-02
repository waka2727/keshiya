using UnityEngine;
namespace Keshiya {
 public sealed class PrecisionAssist:MonoBehaviour {
  public PrototypeGame Game;public bool SampleOpen {get;private set;}public Texture2D SampleTexture=>Game.Definition.artwork!=null?Game.Definition.artwork.completePreview:sample?.Texture;Paper sample;float tearUntil;Font font;public readonly PreviewViewport Preview=new PreviewViewport();Vector2 previous;bool dragging;
  public void ResetSample(){SampleOpen=false;tearUntil=0;Preview.Reset();dragging=false;sample?.Dispose();sample=null;}
  public void ToggleSample(){if(Game.WorkHUD.AskRestart||!Game.Working||!Game.Definition.precision||Game.ShopOpen||Game.InventoryOpen||Game.SkillsOpen||Game.TradeOpen)return;SampleOpen=!SampleOpen;Preview.Reset();dragging=false;if(SampleOpen&&sample==null&&Game.Definition.artwork==null){sample=new Paper(Game.Config,0,Game.Definition);sample.SetFinished(true);}Game.InputBlocked=SampleOpen;Game.Controller.ResetContact();}
  public void Tear(){tearUntil=Time.unscaledTime+1.2f;}
  void Update(){if(Game.External?.ModalOpen??false)return;if(Input.GetKeyDown(KeyCode.H)||(SampleOpen&&Input.GetKeyDown(KeyCode.Escape)))ToggleSample();if(Game.InputBlocked)Game.Presentation.DebugEraseAreaVisible=false;
   if(SampleOpen){var pos=new Vector2(Input.mousePosition.x*1280/Screen.width,(Screen.height-Input.mousePosition.y)*800/Screen.height);Preview.ImageSize=new Vector2(SampleTexture.width,SampleTexture.height);
    if(Input.GetKeyDown(KeyCode.Home))Preview.Reset();bool keyZoom=Input.GetKeyDown(KeyCode.Equals)||Input.GetKeyDown(KeyCode.KeypadPlus)||Input.GetKeyDown(KeyCode.Minus)||Input.GetKeyDown(KeyCode.KeypadMinus);float wheel=Input.mouseScrollDelta.y;if(Input.GetKeyDown(KeyCode.Equals)||Input.GetKeyDown(KeyCode.KeypadPlus))wheel=1;if(Input.GetKeyDown(KeyCode.Minus)||Input.GetKeyDown(KeyCode.KeypadMinus))wheel=-1;
    if(Preview.Frame.Contains(pos)&&wheel!=0)Preview.Wheel(wheel,keyZoom||Input.GetKey(KeyCode.LeftControl)||Input.GetKey(KeyCode.RightControl),pos,Game.Playtest?.maximumZoom??4,Game.Playtest?.zoomStep??1.2f);
    if(Input.GetMouseButtonDown(2)&&Preview.Frame.Contains(pos)){dragging=true;previous=pos;}if(dragging&&Input.GetMouseButton(2)){Preview.Pan(pos-previous);previous=pos;}else dragging=false;
   }}
  void OnGUI(){if(Game==null||(Game.External?.ModalOpen??false)||!Game.Working||Game.ShopOpen||Game.InventoryOpen||Game.SkillsOpen||Game.TradeOpen||Game.WorkHUD.AskLeave||Game.WorkHUD.AskRestart)return;
   var old=GUI.matrix;GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/800f,1));
   if(font==null)font=Font.CreateDynamicFontFromOSFont(new[]{"Yu Gothic","Meiryo"},16);
   var style=new GUIStyle(GUI.skin.label){font=font,fontSize=14,normal={textColor=Color.black}};var b=new GUIStyle(GUI.skin.button){font=font,fontSize=16};
   var tint=GUI.color;GUI.color=new Color(.06f,.10f,.11f,.85f);GUI.DrawTexture(new Rect(382,668,864,28),Texture2D.whiteTexture);GUI.color=tint;
   GUI.Label(new Rect(390,671,810,24),$"Zoom {Game.Viewport.Zoom:0.0}x · Wheel 上下 / Ctrl+Wheel 拡大 / 中ドラッグ移動 / Home 全体 / H 見本",new GUIStyle(style){normal={textColor=Color.white}});
   if(Game.Definition.precision&&GUI.Button(new Rect(1090,62,155,32),"完成見本 / H",b))ToggleSample();
   if(Time.unscaledTime<tearUntil)GUI.Label(new Rect(630,64,430,40),"紙が裂けました。力を抜いてください。",new GUIStyle(style){fontSize=21,normal={textColor=new Color(1,.65f,.35f)}});
   if(SampleOpen){GUI.color=new Color(.10f,.16f,.17f,1);GUI.DrawTexture(new Rect(385,135,860,525),Texture2D.whiteTexture);GUI.color=tint;GUI.Box(new Rect(385,135,860,525),"納品見本：濃い線だけを残します");GUI.BeginGroup(Preview.Frame);GUI.DrawTexture(Preview.ImageRect,SampleTexture,ScaleMode.StretchToFill);GUI.EndGroup();GUI.Label(new Rect(415,590,480,55),$"見本 {Preview.Zoom:0.0}x / Wheel 上下・Ctrl+Wheel 拡大\n中ドラッグ移動・Home 全体",new GUIStyle(style){normal={textColor=Color.white}});if(GUI.Button(new Rect(905,603,299,38),"作業へ戻る / H・Esc",b))ToggleSample();}
   GUI.matrix=old;
  }
  void OnDestroy(){sample?.Dispose();if(font!=null)Destroy(font);}
 }
}
