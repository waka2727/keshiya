using UnityEngine;
namespace Keshiya {
 // UI calls inventory transactions; it never edits the balance or an individual wear counter.
 public sealed class ShopHUD:MonoBehaviour {
  public PrototypeGame Game;public int Product {get;private set;}
  public string Message {get;private set;}="仕事に合う一本を。買った道具は在庫から選べます。";
  Font font;GUIStyle title,body,small,button;Texture2D[] icons;Vector2 productScroll,inventoryScroll;
  public bool DebugVisible=>debug;
  public string Category {get;private set;}="実用品";public bool CollectionOpen;
  bool debug,confirmReset;int trayPage=-1;string traySelected,inventorySelected;
  readonly Color bg=new Color(.065f,.10f,.11f),panel=new Color(.12f,.18f,.19f),accent=new Color(.70f,.84f,.66f);
  void Init(){if(title!=null)return;font=Font.CreateDynamicFontFromOSFont(new[]{"Yu Gothic","Meiryo","Arial"},18);body=new GUIStyle(GUI.skin.label){font=font,fontSize=17,wordWrap=true,normal={textColor=new Color(.90f,.91f,.86f)}};title=new GUIStyle(body){fontSize=27};small=new GUIStyle(body){fontSize=13};button=new GUIStyle(GUI.skin.button){font=font,fontSize=16,wordWrap=true};icons=new Texture2D[Game.Catalog.tools.Length];for(int i=0;i<icons.Length;i++)icons[i]=ToolIcon.Create(Game.Catalog.tools[i]);}
  void Fill(float x,float y,float w,float h,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture);GUI.color=old;}
  void Text(float x,float y,float w,float h,string s,GUIStyle style=null)=>GUI.Label(new Rect(x,y,w,h),s,style??body);
  bool Button(float x,float y,float w,float h,string s)=>GUI.Button(new Rect(x,y,w,h),s,button);
  void Frame(float x,float y,float w,float h,bool selected){Fill(x,y,w,h,selected?new Color(.25f,.35f,.28f):panel);if(selected){Fill(x,y,4,h,accent);Fill(x,y,w,3,accent);}}
  public void ChooseProduct(int index){if(index>=0&&index<Game.Catalog.tools.Length&&!Game.Catalog.tools[index].questOnly){Product=index;Category=Game.Catalog.tools[index].category;Game.Tools.Record(Game.Catalog.tools[index].id).discovered=true;}}
  public void SetCategory(string category){Category=category;for(int i=0;i<Game.Catalog.tools.Length;i++)if(!Game.Catalog.tools[i].questOnly&&(category=="全部"||Game.Catalog.tools[i].category==category)){Product=i;break;}}
  public bool BuySelected(){bool ok=Game.PurchaseTool(Product);Message=ok?Game.Catalog.tools[Product].displayName+(Game.Catalog.tools[Product].price==0?"を1個受け取りました。残量35%・丸い角の消しゴム片です。":"を購入しました。在庫に新品を追加しました。"):"所持金が足りません。依頼や消しカス買取で稼げます。";return ok;}
  public void ToggleDebug(){if(Game.DebugMode)debug=!debug;}
  public void Draw(){Init();GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/800f,1));Fill(0,0,1280,800,bg);
   if(Event.current.type==EventType.KeyDown&&Event.current.keyCode==KeyCode.F9){ToggleDebug();Event.current.Use();}
   Text(32,34,810,48,Game.InventoryOpen?"所持消しゴム / 一本ずつ選ぶ":"消しゴムショップ / 道具店",title);Text(934,39,310,40,$"所持金 {Game.Economy.Wallet.Balance:N0} 円");
   if(Button(32,94,245,37,Game.InventoryOpen?"ショップへ":"所持品・個体を選ぶ / T"))Game.OpenShop(!Game.InventoryOpen);
   if(Button(1000,94,246,37,"戻る / Esc"))Game.CloseShop();Text(300,101,690,29,"在庫と購入記録は、この起動中だけ保持されます。",small);
   if(Game.InventoryOpen)Inventory();else {string[] categories={"実用品","特殊","パン","野菜","動物","全部"};for(int i=0;i<categories.Length;i++)if(Button(32+i*151,139,143,27,(Category==categories[i]?"● ":"")+categories[i])){SetCategory(categories[i]);CollectionOpen=false;}if(Button(1000,139,246,27,CollectionOpen?"商品へ":"図鑑・シリーズ"))CollectionOpen=!CollectionOpen;if(CollectionOpen)Collection();else Products();}
   Text(32,650,1195,44,Message,body);if(debug)DebugPanel();else Text(32,751,1200,25,"持ち込みは所持品画面で選択。5本チャレンジは作業前にON/OFFできます（基本報酬+15%）。",small);
  }
  void Products(){var products=new System.Collections.Generic.List<int>();for(int i=0;i<Game.Catalog.tools.Length;i++){var d=Game.Catalog.tools[i];if(!d.questOnly&&(Category=="全部"||d.category==Category))products.Add(i);}
   productScroll=GUI.BeginScrollView(new Rect(32,174,340,460),productScroll,new Rect(0,0,316,products.Count*67));
   for(int row=0;row<products.Count;row++){int i=products[row];var d=Game.Catalog.tools[i];Game.Tools.Record(d.id).discovered=true;float y=row*67;Frame(0,y,311,61,Product==i);if(Button(0,y,311,61,""))Product=i;GUI.DrawTexture(new Rect(9,y+16,70,32),icons[i]);Text(89,y+5,216,29,d.shortName+(Product==i?" ●":""));Text(89,y+35,216,26,$"{d.price:N0}円 / "+Game.Tools.Discovery(d.id),small);}GUI.EndScrollView();
   var tool=Game.Catalog.tools[Product];Frame(390,174,300,460,false);GUI.DrawTexture(new Rect(416,226,248,114),icons[Product]);Text(411,366,261,88,tool.displayName,title);Text(411,471,261,68,tool.role);Text(411,571,261,31,$"所持 {Game.Tools.items.FindAll(x=>x.definitionId==tool.id).Count}本",small);
   Frame(710,174,536,460,false);Text(733,175,489,29,tool.brand+" / "+tool.series,small);Text(733,204,489,76,tool.description);
   var basis=Game.Catalog.tools[0];var m=new PerformanceModifiers();var state=tool.CreateInitialState();var face=tool.Contact(m);var normal=basis.Contact(m);var corner=tool.Contact(m,ContactMode.Corner,state,0);
   string[] rows={$"消去力 {tool.erasePower:0.00}  / 普通比 {tool.erasePower/basis.erasePower:0.00}倍",$"紙ダメージ倍率 {tool.paperDamageMultiplier:0.00} / 小さいほど安全",$"面の広さ 普通比 {face.HalfSize.x*face.HalfSize.y/(normal.HalfSize.x*normal.HalfSize.y):0.00}倍",$"角の幅 {corner.HalfSize.x*2:0.000} / 小さいほど精密",$"摩耗 {tool.wearRate:0.00}  / 角摩耗 {tool.cornerWearRate:0.000}",$"長尺成長 {tool.longCrumbPotential:0.00} / まとまり {tool.crumbCohesion:0.00}",$"カス量 {tool.crumbAmount:0.00} / 玉適性 {tool.ballAffinity:0.00}",$"保護文字への摩擦 {tool.protectedInkAbrasion:0.00}"};for(int i=0;i<rows.Length;i++)Text(733,289+i*29,489,28,rows[i],small);
   bool enabled=GUI.enabled;GUI.enabled=Game.Economy.Wallet.Balance>=tool.price;if(Button(733,552,489,56,tool.price==0?"消しゴム片を1個受け取る / 無料":$"新品を購入する  /  {tool.price:N0} 円"))BuySelected();GUI.enabled=enabled;if(Game.Economy.Wallet.Balance<tool.price)Text(733,611,489,22,"所持金が足りません",small);
  }
  void Collection(){string seriesInfo="";foreach(var series in new[]{"パンシリーズ","野菜シリーズ","どうぶつシリーズ"}){int total=0;foreach(var d in Game.Catalog.tools)if(d.series==series)total++;seriesInfo+=$"{series} {Game.Tools.CollectedSeries(Game.Catalog,series)}/{total} {(Game.Tools.CollectedSeries(Game.Catalog,series)==total?"COMPLETE":"")}   ";}Text(32,180,1200,37,seriesInfo);
   productScroll=GUI.BeginScrollView(new Rect(32,225,1214,410),productScroll,new Rect(0,0,1190,Mathf.CeilToInt(Game.Catalog.tools.Length/2f)*73));int row=0;foreach(var d in Game.Catalog.tools){if(d.questOnly)continue;var r=Game.Tools.Record(d.id);float x=(row%2)*592,y=(row/2)*73;Frame(x,y,580,66,false);if(r.discovered)GUI.DrawTexture(new Rect(x+8,y+17,75,34),icons[Game.Catalog.Index(d.id)]);Text(x+95,y+6,472,29,(r.discovered?d.displayName:"未発見の道具")+" / "+Game.Tools.Discovery(d.id));Text(x+95,y+36,472,25,$"{d.series}  使用 {r.uses}仕事 / 最長 {r.longestCm:0.0}cm",small);row++;}GUI.EndScrollView();
  }
  void Inventory(){int count=Game.Tools.items.Count;if(inventorySelected!=Game.Tools.selectedId){inventorySelected=Game.Tools.selectedId;int selected=Game.Tools.items.IndexOf(Game.ActiveTool);inventoryScroll.y=Mathf.Clamp(selected*96-192,0,Mathf.Max(0,count*96-479));}inventoryScroll=GUI.BeginScrollView(new Rect(32,155,742,479),inventoryScroll,new Rect(0,0,714,Mathf.Max(479,count*96)));
   for(int i=0;i<count;i++){var item=Game.Tools.items[i];var d=Game.Catalog.Find(item.definitionId);float y=i*96;bool selected=item==Game.ActiveTool;Frame(0,y,709,88,selected);GUI.DrawTexture(new Rect(15,y+24,90,41),icons[Game.Catalog.Index(d.id)]);Text(121,y+7,393,30,(selected?"● ":"")+d.displayName);Text(121,y+41,394,41,$"#{i+1} {item.Condition}"+(item.loan?" / 貸出":"")+$"  残量 {item.state.Remaining(d,Game.Modifiers)*100:0.0}% / 角 {item.state.CornerSharpness*100:0}%",small);
    bool enabled=GUI.enabled;GUI.enabled=!item.state.Exhausted&&Game.CanCarry(item)&&Game.Supplied==null;if(Button(532,y+7,158,33,selected?"選択中":"この一本を選ぶ"))Game.SelectOwned(item.instanceId);GUI.enabled=enabled&&!Game.Working&&Game.Tools.loadout.limited;if(Button(532,y+46,158,31,Game.Tools.loadout.Contains(item.instanceId)?"✓ 持ち込む":"□ 持ち込まない")){if(!Game.ToggleCarry(item.instanceId))Message="持ち込みは最大5本です。別の一本を外すか、制限をOFFにしてください。";}GUI.enabled=enabled;
   }GUI.EndScrollView();Frame(792,155,454,479,false);Text(813,176,412,40,"持ち込みトレイ",title);Text(813,228,412,87,Game.Working?"作業中は持ち込みを変更できません。\n預かり品の仕事では支給品を使用します。":"左のチェックで一本ずつ選びます。\n通常在庫と依頼専用の支給品は別管理です。");bool editable=GUI.enabled;GUI.enabled=!Game.Working;if(Button(813,313,412,36,Game.Tools.loadout.limited?$"5本チャレンジ ON / {Game.Tools.loadout.selected.Count}本選択":"通常：無制限 / 全品持ち込み"))Game.SetCarryLimit(!Game.Tools.loadout.limited);GUI.enabled=editable;Text(813,365,412,68,"1〜9：種類を選択\nTab：使用可能な個体を順送り\nT：この画面を開く・閉じる",small);
   var record=Game.Tools.Record(Game.Eraser.id);Text(813,447,412,111,$"選択中：{Game.Eraser.displayName}\nこの種類の使用：{record.uses}仕事\n最長：{record.longestCm:0.0}cm / 最高評価：{record.bestGrade}\n購入履歴：{Game.Tools.purchases.Count}本 / {Game.ActiveState.special.Text(Game.Eraser)}",small);
   if(!Game.Tools.items.Exists(x=>Game.CanCarry(x)&&!x.state.Exhausted)&&Game.Supplied==null&&Button(813,567,412,47,"普通消しを借りる / 無料の救済")){Game.BorrowTool();Message="使える道具がないため、普通消しをお貸しします。";}
  }
  void DebugPanel(){Fill(24,597,1230,94,panel);Text(34,602,1200,27,"選択中の一本だけを変更 / 残量と角は独立",small);for(int i=0;i<6;i++){float v=i%3==0?1:i%3==1?.5f:i<3?.2f:.1f;if(Button(34+i*201,638,192,40,(i<3?"残量 ":"角 ")+(v*100).ToString("0")+"%")){if(i<3)Game.ActiveState.DebugRemaining(v,Game.Eraser,Game.Modifiers);else Game.ActiveState.DebugSharpness(v);Game.SyncToolVisual();}}
   Fill(24,694,1230,87,panel);Text(34,698,1205,24,"比較テスト専用 / お金・在庫の操作はEXPやスキルを変更しません",small);
   if(confirmReset){Text(34,729,514,39,"在庫を初期3本に戻します。購入履歴は残ります。",small);if(Button(572,727,238,40,"在庫リセットを実行")){Game.Tools.DebugReset(Game.Catalog);Game.SelectOwned(Game.Tools.selectedId);confirmReset=false;}if(Button(823,727,185,40,"取り消す"))confirmReset=false;return;}
   if(Button(34,727,270,40,"所持金 +10,000円"))Game.Economy.Wallet.DebugCredit(10000);
   if(Button(320,727,270,40,"全種類をテスト用に追加"))Game.Tools.DebugAll(Game.Catalog);
   if(Button(606,727,270,40,"全個体を新品にする")){Game.Tools.DebugFresh();Game.SyncToolVisual();}
   if(Button(892,727,330,40,"在庫リセット…"))confirmReset=true;
  }
  public void DrawTray(float x,float y,float width,bool detail=false){Init();var tray=Game.TrayItems;int count=tray.Count;if(count==0){if(Button(x,y,width,48,"持ち込む道具を選ぶ / T"))Game.OpenShop(true);return;}
   if(traySelected!=Game.Tools.selectedId){traySelected=Game.Tools.selectedId;trayPage=tray.IndexOf(Game.ActiveTool)/3;}trayPage=Mathf.Clamp(trayPage,0,(count-1)/3);float slot=(width-12)/3;
   for(int n=0;n<3;n++){int index=trayPage*3+n;if(index>=count)break;var item=tray[index];var d=Game.Catalog.Find(item.definitionId);float left=x+n*(slot+6);bool selected=item==Game.ActiveTool;Frame(left,y,slot,detail?92:48,selected);if(Button(left,y,slot,detail?92:48,""))Game.SelectOwned(item.instanceId);
    float scale=detail?1.3f:.7f;GUI.DrawTexture(new Rect(left+5,y+3,48*scale,22*scale),icons[Game.Catalog.Index(d.id)]);Text(left+slot*.4f,y+2,slot*.6f,detail?37:21,(selected?"●":"")+d.shortName,new GUIStyle(small){fontSize=detail?13:10});Text(left+4,y+(detail?49:24),slot-8,detail?36:24,$"#{index+1} 残{item.state.Remaining(d,Game.Modifiers)*100:0}% 角{item.state.CornerSharpness*100:0}%",new GUIStyle(small){fontSize=detail?12:9});
   }
   float row=y+(detail?98:51);if(Button(x,row,38,detail?30:22,"‹"))trayPage=Mathf.Max(0,trayPage-1);if(Button(x+44,row,width-88,detail?30:22,$"持込 {trayPage+1}/{(count+2)/3} · T"))Game.OpenShop(true);if(Button(x+width-38,row,38,detail?30:22,"›"))trayPage=Mathf.Min((count-1)/3,trayPage+1);
  }
  void OnDestroy(){if(font!=null)Destroy(font);if(icons!=null)foreach(var icon in icons)Destroy(icon);}
 }
}
