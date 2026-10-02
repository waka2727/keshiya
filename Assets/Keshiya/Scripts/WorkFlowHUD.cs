using UnityEngine;
namespace Keshiya {
 public sealed class WorkFlowHUD:MonoBehaviour {
  public PrototypeGame Game;public bool AskLeave,AskRestart;public bool ShowToolRecommendations=>Game.DebugMode;public float LetterContentHeight {get;private set;}
  public Vector2 BoardScroll;Vector2 quoteScroll,thanksScroll;public bool ExternalOnly=true;
  Font font;GUIStyle title,body,small,button,big;Texture2D[] icons;Paper preview;int previewIndex=-1;
  readonly Color bg=new Color(.065f,.10f,.11f),panel=new Color(.12f,.18f,.19f),accent=new Color(.70f,.84f,.66f);
  void Init(){font=Font.CreateDynamicFontFromOSFont(new[]{"Yu Gothic","Meiryo","Arial"},18);body=new GUIStyle(GUI.skin.label){font=font,fontSize=17,wordWrap=true,normal={textColor=new Color(.90f,.91f,.86f)}};title=new GUIStyle(body){fontSize=29};small=new GUIStyle(body){fontSize=14};big=new GUIStyle(title){fontSize=44,normal={textColor=accent}};button=new GUIStyle(GUI.skin.button){font=font,fontSize=16,wordWrap=true};icons=new Texture2D[Game.Catalog.tools.Length];for(int i=0;i<icons.Length;i++)icons[i]=ToolIcon.Create(Game.Catalog.tools[i]);}
  void Fill(float x,float y,float w,float h,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture);GUI.color=old;}
  void Text(float x,float y,float w,float h,string s,GUIStyle style=null)=>GUI.Label(new Rect(x,y,w,h),s,style??body);
  bool Button(float x,float y,float w,float h,string s)=>GUI.Button(new Rect(x,y,w,h),s,button);
  public void Draw(){if(title==null)Init();GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/800f,1));Fill(0,0,1280,800,bg);GUI.enabled=!Game.TradeOpen;
   if(Game.Session.Phase==WorkPhase.Board)Board();else if(Game.Session.Phase==WorkPhase.Detail)Detail();else Results();GUI.enabled=true;Game.SkillsHUD.Notice();
   if(Game.Session.Phase==WorkPhase.Result)Game.TradeHUD.Draw();
  }
  void Header(string heading,string sub){Fill(32,32,42,3,accent);Text(32,46,850,48,heading,title);Text(32,100,670,35,sub,small);Text(975,51,275,40,$"所持金 {Game.Economy.Wallet.Balance:N0} 円",body);if(Button(738,99,245,34,"消しゴムショップ"))Game.OpenShop();if(Button(997,99,249,34,"スキル / K"))Game.OpenSkills();}
  void Board(){Header("消し屋 / 依頼一覧","今日も、残すものを大切に。気になる仕事を選んでください。制限時間はありません。");
   if(Game.DebugMode&&Button(32,135,300,26,ExternalOnly?"外部試遊 8件 / 全依頼に切替":"全依頼 / 外部試遊に切替")){ExternalOnly=!ExternalOnly;BoardScroll=Vector2.zero;}
   int shown=0;foreach(var job in Game.Jobs)if(!ExternalOnly||job.externalTest)shown++;
   BoardScroll=GUI.BeginScrollView(new Rect(20,171,1240,535),BoardScroll,new Rect(0,0,1218,Mathf.CeilToInt(shown/3f)*274));
   int slot=0;for(int i=0;i<Game.Jobs.Length;i++){var j=Game.Jobs[i];if(ExternalOnly&&!j.externalTest)continue;int col=slot%3,row=slot/3;slot++;float x=12+col*403,y=5+row*274;Fill(x,y,394,255,panel);Text(x+20,y+16,353,35,j.displayName,title);Text(x+20,y+59,350,25,$"依頼人：{j.client}   {new string('★',j.difficulty)}",small);Text(x+20,y+94,352,51,j.summary,body);Text(x+20,y+151,352,27,$"{j.PaperName} × {j.WritingName} / {j.category}",small);Text(x+20,y+187,175,34,$"基本 {j.baseReward:N0} 円",body);if(Button(x+214,y+185,160,47,"依頼を読む"))Game.ChooseJob(i);}
   GUI.EndScrollView();
   Text(32,725,1150,30,$"完了した仕事 {Game.Session.CompletedCount}件  /  回収した一本もの {Game.Economy.Inventory.Count+Game.Economy.OverflowCount}本は保管中",small);Text(32,758,1160,25,"すべての依頼を受注できます。所持金・在庫・仕事の記録は、この起動中だけ保持されます。",small);
  }
  void Detail(){var j=Game.Jobs[Game.Session.Selected];Header("依頼詳細 / "+j.displayName,$"依頼人：{j.client}  /  難易度 {new string('★',j.difficulty)}  /  基本報酬 {j.baseReward:N0}円");
   Fill(32,150,700,510,panel);Text(56,172,652,35,"依頼者からの手紙",title);
   var letter=new GUIStyle(body){fontSize=19};float height=Mathf.Max(385,letter.CalcHeight(new GUIContent(j.clientQuote),625)+12);
   LetterContentHeight=height;quoteScroll=GUI.BeginScrollView(LetterViewport,quoteScroll,new Rect(0,0,630,height));Text(0,0,625,height,j.clientQuote,letter);GUI.EndScrollView();
   Fill(752,150,494,510,panel);Text(776,172,446,35,"作業情報",title);
   Text(776,222,444,57,$"紙：{j.PaperName}\n筆記具：{j.WritingName}",body);
   Text(776,288,444,110,j.instruction,body);
   Text(776,401,444,102,$"特徴：{j.category} / 保護対象：{(j.precision?"あり":"なし")}{(ShowToolRecommendations?"\n候補："+j.recommended:"")}\n完了：{j.requiredErasure*100:0}%以上 / 目安 {j.referenceSeconds/60:0.#}分",small);
   if(previewIndex!=Game.Session.Selected){preview?.Dispose();preview=j.artwork==null?new Paper(Game.Config,Game.Feel.erasureGrain,j):null;previewIndex=Game.Session.Selected;quoteScroll=Vector2.zero;}
   GUI.DrawTexture(new Rect(776,510,106,136),j.artwork!=null?j.artwork.initialPreview:preview.Texture,ScaleMode.ScaleToFit);
   var record=Game.Session.Record(j.id);Text(899,516,320,128,(j.suppliedTool!=null?"支給品："+j.suppliedTool.displayName:"持込 "+Game.Tools.loadout.selected.Count+"本 / Tで変更")+"\n遅くても基本報酬は同じです。\n"+(record==null?"この仕事は初めてです。":$"履歴 {record.completions}回 / 最高{record.bestGrade}"),small);
   if(Button(32,704,240,55,"依頼一覧へ戻る"))Game.ShowBoard();if(Button(300,704,395,55,"持ち物を選ぶ / T"))Game.OpenShop(true);if(Button(914,704,332,55,"仕事を始める"))Game.BeginWork();
  }
  public static readonly Rect LetterViewport=new Rect(56,224,652,414);

  void Results(){var r=Game.CurrentJob.Result;Header("仕事の結果 / "+Game.Definition.displayName,"依頼人："+Game.Definition.client+"  /  依頼報酬は入金済みです。");Fill(32,153,585,480,panel);Fill(640,153,606,480,panel);
   Text(56,177,235,61,"評価 "+WorkSession.Grade(r),big);string thanks=r.severe||r.protectedMajor?"大きな損傷が残りました。":string.IsNullOrEmpty(Game.Definition.thankYou)?"おつかれさまでした。":Game.Definition.thankYou;thanksScroll=GUI.BeginScrollView(new Rect(270,174,317,90),thanksScroll,new Rect(0,0,289,Mathf.Max(85,body.CalcHeight(new GUIContent(thanks),285))));Text(0,0,285,900,thanks,body);GUI.EndScrollView();
   string[] quality={$"消去率  {r.erased*100:0.0}%",$"紙ダメージ  {r.damage*100:0.00}% / 最大 {r.peak*100:0.0}%",$"保護対象ダメージ  {r.protectedDamage*100:0.0}%",$"作業時間  {PrototypeHUD.TimeText(r.seconds)}"};for(int i=0;i<quality.Length;i++)Text(56,271+i*52,533,42,quality[i]);Text(56,490,531,89,$"今回の経験  消去 +{Game.Progress.jobExp[0]:0.0} / 精密 +{Game.Progress.jobExp[1]:0.0} / カス +{Game.Progress.jobExp[2]:0.0}\n獲得SP  消去 +{Game.Progress.jobSP[0]} / 精密 +{Game.Progress.jobSP[1]} / カス +{Game.Progress.jobSP[2]}\n時間だけで品質評価や基本報酬は下がりません。",small);if(Button(56,588,531,31,"EXP内訳・獲得条件を見る / K"))Game.OpenSkills();
   string[] rows={$"基本報酬|{r.basic:N0} 円（縛り +{r.challenge:N0}）",$"仕上がりボーナス|+{r.finish:N0} 円",$"紙保護ボーナス|+{r.pristine:N0} 円",$"保護対象ボーナス|+{r.protection:N0} 円",$"スピードボーナス|+{r.speed:N0} 円",$"依頼報酬 合計|{r.Total:N0} 円",$"消しカス売却|{Game.Economy.JobSales:N0} 円",$"今回の収入|{r.Total+Game.Economy.JobSales:N0} 円"};for(int i=0;i<rows.Length;i++){var parts=rows[i].Split('|');Text(666,180+i*45,345,34,parts[0]);Text(1022,180+i*45,204,34,parts[1]);}
   Text(666,562,545,52,"未売却の回収品は保管できます。\n今回の売却額には、この仕事中に売った在庫も含みます。",small);
   if(Button(32,644,290,40,"同じ依頼をもう一度"))Game.RetryWork();if(Button(340,644,290,40,"消しカス買取へ"))Game.OpenTrade();if(Button(946,644,300,40,"次の仕事を選ぶ"))Game.ShowBoard();
  }
  public void DrawLeave(){if(!AskLeave)return;if(title==null)Init();Fill(0,0,1280,800,new Color(.03f,.06f,.07f,.92f));Text(380,285,560,100,"作業を中断して依頼一覧へ戻りますか？\n報酬は未獲得です。回収品と道具の摩耗は残ります。",body);if(Button(380,420,250,50,"依頼一覧へ")){AskLeave=false;Game.ShowBoard();}if(Button(658,420,250,50,"作業を続ける")){AskLeave=false;Game.InputBlocked=false;}}
  public void DrawRestart(){if(!AskRestart)return;if(title==null)Init();Fill(0,0,1280,800,new Color(.03f,.06f,.07f,.96f));Text(360,250,580,95,"現在の作業内容を破棄して最初からやり直しますか？",title);Text(360,350,580,72,"道具・所持金・在庫・EXPも仕事開始時に戻ります。\nこの作業中の回収・売却・購入・スキル変更は取り消されます。",body);if(Button(360,465,270,55,"やり直す"))Game.ConfirmRestart();if(Button(658,465,270,55,"作業へ戻る"))Game.CancelRestart();}
  void Update(){if(AskRestart&&Input.GetKeyDown(KeyCode.Escape))Game.CancelRestart();}
  void OnDestroy(){preview?.Dispose();if(font!=null)Destroy(font);if(icons!=null)foreach(var i in icons)Destroy(i);}
 }
}

