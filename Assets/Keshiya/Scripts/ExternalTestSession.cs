using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Keshiya {
 public sealed class ExternalTestSession:MonoBehaviour {
  public PrototypeGame Game;public readonly TutorialHints Tutorial=new TutorialHints();public SessionJournal Journal {get;private set;}
  public bool Welcome {get;private set;}public bool Menu {get;private set;}public bool ResetConfirmation {get;private set;}public bool ModalOpen=>Welcome||Menu;
  public bool ShowFps;public int Restarts {get;private set;}public bool ShowTutorial=>!legacy&&Game.Working&&!Game.InputBlocked&&!Game.CurrentJob.Completed&&Game.Definition.id=="TEST_001"&&!Tutorial.Done;
  public bool PointerOverHint(Vector2 screen){var p=new Vector2(screen.x*1280/Screen.width,(Screen.height-screen.y)*800/Screen.height);return (ShowTutorial&&new Rect(395,88,850,112).Contains(p))||(!ShowTutorial&&Time.unscaledTime<hintUntil&&new Rect(395,145,770,95).Contains(p));}
  bool legacy,wasBlocked,ended,previewSeen,precisionSeen,shopSeen,skillSeen;float seconds,maxDelta,nextSample=15,hintUntil;int frames;string hint="";Font font;
  public float AverageFps=>seconds>0?frames/seconds:0;
  public void Initialize(PrototypeGame game){Game=game;legacy=ExternalTestPolicy.LegacyHarness(Environment.GetCommandLineArgs());Welcome=!legacy;Tutorial.Enabled=!legacy;Journal=new SessionJournal(Path.Combine(Application.persistentDataPath,"ExternalTest01","Logs"));Journal.Write(TestEvent.GameStarted,new TestLogEntry{code="ExternalTest01",width=Screen.width,height=Screen.height});}
  public void ContinueWelcome(bool first){Welcome=false;Game.InputBlocked=true;if(first){int i=Array.FindIndex(Game.Jobs,x=>x.id=="TEST_001");if(i>=0)Game.ChooseJob(i);}else Game.ShowBoard();}
  public void OpenMenu(){if(Welcome||Menu||Game.WorkHUD.AskRestart||Game.WorkHUD.AskLeave)return;wasBlocked=Game.InputBlocked;Game.Controller.ResetContact();Game.InputBlocked=true;Menu=true;}
  public void ReturnFromHint(){if(Menu)CloseMenu();else {Tutorial.Skip();hintUntil=0;}}
  public void CloseMenu(){Menu=false;ResetConfirmation=false;Game.InputBlocked=wasBlocked;}
  public void RequestDataReset(){if(Menu)ResetConfirmation=true;}
  public void CancelDataReset(){ResetConfirmation=false;}
  public void ConfirmDataReset(){if(!Menu||!ResetConfirmation)return;Journal.Sample(Game,TestEvent.DataReset,Restarts);End();SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);}
  public void ReplayTutorial(){Tutorial.Replay();CloseMenu();Hint("操作ヒントを再表示します。「机に置いてあった紙」の作業中に順番に表示します。");}
  readonly System.Collections.Generic.Queue<string> hints=new System.Collections.Generic.Queue<string>();
  void Hint(string text){hints.Enqueue(text);}
  public void Started(){Restarts=0;Journal.Sample(Game,TestEvent.JobStarted);if(!legacy&&Game.Definition.id=="TEST_002"&&!precisionSeen){precisionSeen=true;Hint("細かい場所では面・辺・角を使い分けられます。Wheelで上下移動、Ctrl＋Wheelで拡大できます。");}}
  public void Restarted(){Restarts++;Journal.Sample(Game,TestEvent.JobRestarted,Restarts);}
  public void Completed(){Journal.Sample(Game,TestEvent.JobCompleted,Restarts);if(!legacy&&!shopSeen){shopSeen=true;Hint("報酬で新しい消しゴムを購入できます。回収した消しカスの売却も任意です。次の仕事は自由に選べます。");}}
  void Update(){if(Game==null)return;float dt=Time.unscaledDeltaTime;if(dt>0){seconds+=dt;frames++;maxDelta=Mathf.Max(maxDelta,dt);}if(Input.GetKeyDown(KeyCode.F2))ShowFps=!ShowFps;
   if(Input.GetKeyDown(KeyCode.F1)){if(Menu)CloseMenu();else OpenMenu();}if(Menu&&Input.GetKeyDown(KeyCode.Escape)){if(ResetConfirmation)CancelDataReset();else ReturnFromHint();}else if(!ModalOpen&&ShowTutorial&&Input.GetKeyDown(KeyCode.Escape))ReturnFromHint();
   if(ShowTutorial)Tutorial.Tick(dt,Game.Paper.Drawing.Erased>.001f);
   if(!ModalOpen&&!Game.ShopOpen&&!Game.SkillsOpen&&!Game.Assist.SampleOpen&&!ShowTutorial&&Time.unscaledTime>=hintUntil&&hints.Count>0){hint=hints.Dequeue();hintUntil=Time.unscaledTime+12;}
   if(!legacy&&!ModalOpen&&!Game.InputBlocked&&Game.Definition.precision&&!previewSeen){previewSeen=true;Hint("Hまたは「完成見本」で納品状態を確認できます。見本もWheelで上下、Ctrl＋Wheelで拡大できます。");}
   if(!legacy&&!ModalOpen&&!skillSeen){foreach(var b in Game.Progress.banks)if(b.earnedSP>0){skillSeen=true;Hint("SPを獲得しました。Kまたは「スキル」から取得できます。選ぶスキルは自由です。");break;}}
   if(seconds>=nextSample){nextSample=seconds+15;if(Game.Working)Journal.Sample(Game,TestEvent.ProgressSample,Restarts);}
  }
  void InitFont(){if(font==null)font=Font.CreateDynamicFontFromOSFont(new[]{"Yu Gothic","Meiryo","Arial"},18);}
  void Fill(Rect r,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
  void OnGUI(){if(Game==null||legacy)return;InitFont();GUI.depth=-100;GUI.enabled=true;GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/800f,1));var label=new GUIStyle(GUI.skin.label){font=font,fontSize=17,wordWrap=true,normal={textColor=new Color(.92f,.94f,.88f)}};var button=new GUIStyle(GUI.skin.button){font=font,fontSize=16};
   Fill(new Rect(390,0,650,25),new Color(.05f,.09f,.1f,.94f));GUI.Label(new Rect(400,1,630,24),ExternalTestPolicy.Version+" / 開発中の試遊版"+(ShowFps?$" / FPS {1/Mathf.Max(.001f,Time.smoothDeltaTime):0}":""),new GUIStyle(label){fontSize=13});if(!ModalOpen&&GUI.Button(new Rect(1120,0,150,25),"ヘルプ・設定 / F1",button))OpenMenu();
   if(ModalOpen){Fill(new Rect(0,0,1280,800),new Color(.06f,.10f,.11f));GUI.Label(new Rect(180,110,920,65),Welcome?"消し屋 / Feedback 0.8.2":"ヘルプ・設定",new GUIStyle(label){fontSize=30});
    if(Welcome){GUI.Label(new Rect(180,205,900,110),"このゲームは開発中の試遊版です。\nUI・グラフィック・バランス・内容は今後変更されます。",label);GUI.Label(new Rect(180,340,900,130),"30～60分程度、気になるところまで自由に遊んでください。\n最初は「机に置いてあった紙」で操作を試せます。全8件を遊ぶ必要はありません。\n進行は今回の起動中のみ保持します。ゲーム内の行動ログはローカルに保存し、外部送信しません。",label);
     if(GUI.Button(new Rect(180,540,420,60),"最初の依頼を読む",button))ContinueWelcome(true);if(GUI.Button(new Rect(630,540,420,60),"自由に依頼を選ぶ",button))ContinueWelcome(false);
    }else if(ResetConfirmation){GUI.Label(new Rect(180,235,900,160),"External Testの全進行を初期化しますか？\n所持金・道具・摩耗・スキル・EXP/SP・依頼履歴・図鑑を初期状態へ戻します。\n作業中の「やり直す」と異なり、今回の全進行が対象です。調査用ログは残します。",label);if(GUI.Button(new Rect(180,490,420,60),"全進行を初期化する",button))ConfirmDataReset();if(GUI.Button(new Rect(630,490,420,60),"取り消す",button))CancelDataReset();
    }else{GUI.Label(new Rect(180,205,920,175),"マウス移動 / 左ボタンを押して擦る\nWheel：上下移動 / Ctrl＋Wheel：ズーム / 中ドラッグ：移動 / Home：全体\nQ・E：面辺角 / R：回転 / 1～9：種類 / Tab：個体 / T：持ち物\nC：回収 / Space：吹く / Z：直後の救済 / G：玉 / V：買取\nH：完成見本 / K：スキル / M：ミュート / F2：FPS表示",label);
     if(GUI.Button(new Rect(180,410,420,45),"操作ヒントを再表示",button))ReplayTutorial();if(GUI.Button(new Rect(630,410,420,45),"操作ヒントをスキップ",button))Tutorial.Skip();
     if(GUI.Button(new Rect(180,470,420,45),ShowFps?"FPS表示：ON":"FPS表示：OFF",button))ShowFps=!ShowFps;if(GUI.Button(new Rect(630,470,420,45),Game.ContactGuide.Enabled?"接触ガイド：ON":"接触ガイド：OFF",button))Game.ContactGuide.Enabled=!Game.ContactGuide.Enabled;
     if(GUI.Button(new Rect(180,555,420,50),"テストデータ初期化",button))RequestDataReset();if(GUI.Button(new Rect(630,555,420,50),"戻る / Esc・F1",button))CloseMenu();
    }GUI.Label(new Rect(180,705,920,35),"制限時間はありません。遊び方は自由です。",label);return;
   }
   if(Game.Assist.SampleOpen||Game.ShopOpen||Game.InventoryOpen||Game.SkillsOpen||Game.TradeOpen||Game.WorkHUD.AskRestart||Game.WorkHUD.AskLeave)return;
   if(ShowTutorial){Fill(new Rect(395,88,850,112),new Color(.07f,.14f,.15f,.96f));GUI.Label(new Rect(410,94,815,63),$"操作ヒント {Tutorial.Index+1}/{TutorialHints.Steps.Length}  "+Tutorial.Text,label);if(GUI.Button(new Rect(875,162,165,30),"次のヒント",button))Tutorial.Next();if(GUI.Button(new Rect(1058,162,170,30),"戻る",button))ReturnFromHint();}
   else if(Time.unscaledTime<hintUntil){Fill(new Rect(395,145,770,95),new Color(.07f,.14f,.15f,.96f));GUI.Label(new Rect(410,155,735,75),hint,label);}
  }
  void End(){if(ended||Journal==null)return;ended=true;Journal.Sample(Game,TestEvent.SessionEnded,Restarts);Journal.Write(TestEvent.SessionEnded,new TestLogEntry{averageFps=AverageFps,slowestFrameFps=maxDelta>0?1/maxDelta:0});Journal.Dispose();}
  void OnApplicationQuit(){End();}void OnDestroy(){End();if(font!=null)Destroy(font);}
 }
}