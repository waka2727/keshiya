namespace Keshiya {
 // Only teaches controls. No job/tool recommendations or long-crumb recipe.
 public sealed class TutorialHints {
  public static readonly string[] Steps={
   "マウスで移動。左ボタンを押しながら動かすと、紙を擦れます。触れたまま何度か往復してみましょう。",
   "速すぎる擦り方や同じ場所への集中は紙を傷めます。制限時間はありません。急がなくて大丈夫です。",
   "Q / E または左のボタンで面・辺・角を切り替え。面は広く、辺は線に沿って、角は小さな場所を狙えます。",
   "Rで向きを45度回転できます。Shift＋Rで逆回転。薄い輪郭が、実際に当たる範囲です。",
   "Wheelで上下移動、Ctrl＋Wheelで拡大・縮小。中ボタンドラッグで自由に移動、Homeで全体へ戻れます。",
   "Spaceで消しカスを吹く、Cで回収。Gで細かいカスを玉に、Vで買取へ。売るかどうかは自由です。",
   "消去率95%以上で左の「依頼完了」を押せます。「やり直す」は確認後、この仕事の開始時へ戻ります。"
  };
  public int Index {get;private set;} public bool Done {get;private set;} public bool Enabled=true;float elapsed;
  public string Text=>Done?"":Steps[Index];
  public void Next(){if(Done)return;elapsed=0;if(Index+1>=Steps.Length)Done=true;else Index++;}
  public void Tick(float delta,bool erased){if(!Enabled||Done)return;elapsed+=delta;if(elapsed>=18&&(Index!=0||erased||elapsed>=30))Next();}
  public void Skip(){Done=true;}
  public void Replay(){Index=0;elapsed=0;Done=false;Enabled=true;}
 }
}