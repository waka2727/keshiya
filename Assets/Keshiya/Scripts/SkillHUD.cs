using UnityEngine;
namespace Keshiya {
 public sealed class SkillHUD:MonoBehaviour {
  public PrototypeGame Game;CraftBranch tab;bool debug;Font font;GUIStyle title,body,small,button;Vector2 scroll;int revision;float noticeUntil;
  public bool DebugVisible=>debug;
  public CraftBranch SelectedBranch=>tab;
  public void SelectBranch(CraftBranch branch){tab=branch;}
  public void ToggleDebug(){if(Game.DebugMode)debug=!debug;}
  void Init(){if(font!=null)return;font=Font.CreateDynamicFontFromOSFont(new[]{"Yu Gothic","Meiryo","Arial"},17);body=new GUIStyle(GUI.skin.label){font=font,fontSize=17,wordWrap=true,normal={textColor=new Color(.89f,.92f,.86f)}};small=new GUIStyle(body){fontSize=13};title=new GUIStyle(body){fontSize=29};button=new GUIStyle(GUI.skin.button){font=font,fontSize=15,wordWrap=true};}
  void Update(){if(Game==null)return;if(Game.Progress.notificationRevision!=revision){revision=Game.Progress.notificationRevision;noticeUntil=Time.unscaledTime+3;}if(Game.SkillsOpen&&Input.GetKeyDown(KeyCode.F8))ToggleDebug();}
  void Fill(Rect r,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
  void Label(float x,float y,float w,float h,string text,GUIStyle style=null)=>GUI.Label(new Rect(x,y,w,h),text,style??body);
  bool Button(float x,float y,float w,float h,string text)=>GUI.Button(new Rect(x,y,w,h),text,button);
  public void Notice(){Init();if(Time.unscaledTime<noticeUntil&&!string.IsNullOrEmpty(Game.Progress.notification)){Fill(new Rect(390,115,420,29),new Color(.13f,.22f,.17f,.95f));Label(400,118,405,25,Game.Progress.notification,small);}if(Time.unscaledTime<Game.SenseHintUntil){Fill(new Rect(390,650,840,30),new Color(.09f,.14f,.15f,.95f));Label(402,654,810,24,Game.SenseMessage,small);}}
  public void Draw(){Init();GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/800f,1));GUI.enabled=true;Fill(new Rect(0,0,1280,800),new Color(.065f,.10f,.11f));Label(32,40,960,48,"職人のスキル / 使うほど、少しずつ上達",title);Label(32,94,1170,30,"消去：鉛筆・依頼完了 / 精密：紙や文字を傷つけない精密作業 / 消しカス：長尺・回収・玉・売却",small);
   for(int i=0;i<3;i++){var branch=(CraftBranch)i;var bank=Game.Progress.banks[i];float x=32+i*410;Fill(new Rect(x,140,394,64),tab==branch?new Color(.27f,.38f,.30f):new Color(.12f,.18f,.19f));if(Button(x,140,394,64,$"{SkillCatalog.BranchName(branch)}  SP {bank.availableSP}\n次のSPまで {bank.remainder:0.0} / {Game.Progress.Next(branch):0} EXP"))SelectBranch(branch);}
   int row=0;foreach(var definition in Game.SkillDefinitions.skills){if(definition.branch!=tab)continue;int level=Game.Progress.Level(definition.kind);float y=224+row++*104;Fill(new Rect(32,y,810,96),new Color(.12f,.18f,.19f));Label(48,y+9,573,29,$"{definition.title}  Lv{level}/10");Label(48,y+39,595,27,$"現在 {definition.Display(level)}   →   "+(level>=10?"最大Lv":$"次 {definition.Display(level+1)}"),small);Label(48,y+66,620,26,definition.description,small);
    GUI.enabled=level<10&&Game.Progress.banks[(int)tab].availableSP>=definition.cost;if(Button(688,y+22,134,52,level>=10?"習得済み":$"取得 {definition.cost} SP"))Game.BuySkill(definition.kind);GUI.enabled=true;
   }
   Fill(new Rect(862,224,384,408),new Color(.10f,.15f,.16f));Label(880,235,346,35,"今回の経験の内訳",body);var lines=Game.Progress.ExperienceLines();
   scroll=GUI.BeginScrollView(new Rect(875,280,362,336),scroll,new Rect(0,0,338,Mathf.Max(330,lines.Count*42)));
   for(int i=0;i<lines.Count;i++)Label(2,i*42,330,41,lines[i],small);GUI.EndScrollView();
   if(debug){Label(32,642,300,25,"比較用デバッグ（成長を付与）",small);if(Button(320,646,200,38,"各系統 SP +10"))Game.Progress.DebugAddSP(10);if(Button(532,646,170,38,"全スキル Lv0")){Game.Progress.DebugPreset(0);Game.RefreshSkills();}if(Button(714,646,170,38,"全スキル Lv5")){Game.Progress.DebugPreset(5);Game.RefreshSkills();}if(Button(896,646,170,38,"全スキル Lv10")){Game.Progress.DebugPreset(10);Game.RefreshSkills();}}
   if(Button(32,714,400,49,"Prototype用：全スキル振り直し"))Game.ResetSkills();Label(449,715,540,48,"Lvを0に戻し、使用したSPを各系統へ返します。\nEXP・仕事履歴・所持金は残ります。",small);if(Button(1013,714,233,49,"戻る / K"))Game.CloseSkills();Label(32,773,1120,22,Game.DebugMode?"起動中のみ保存 / F8：比較用デバッグ":"起動中のみ保存 / 成長しても、乱暴な操作にはリスクが残ります。",small);
  }
  void OnDestroy(){if(font!=null)Destroy(font);}
 }
}
