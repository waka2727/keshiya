using UnityEngine;
namespace Keshiya
{
    // Drawn last by the existing HUD, so the trade modal cannot click through.
    public sealed class CrumbTradeHUD:MonoBehaviour
    {
        public PrototypeGame Game;
        Font font;GUIStyle body,small,title,button;Vector2 scroll;
        readonly Color accent=new Color(.72f,.83f,.63f);
        void Initialize()
        {
            font=Font.CreateDynamicFontFromOSFont(new[]{"Yu Gothic","Meiryo","Arial"},16);
            body=new GUIStyle(GUI.skin.label){font=font,fontSize=16,normal={textColor=new Color(.91f,.91f,.85f)}};
            small=new GUIStyle(body){fontSize=12};title=new GUIStyle(body){fontSize=24};button=new GUIStyle(GUI.skin.button){font=font,fontSize=14};
        }
        void Fill(Rect rect,Color color){Color old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;}
        void Label(float x,float y,float w,float h,string text,GUIStyle style)=>GUI.Label(new Rect(x,y,w,h),text,style);
        public void Draw()
        {
            if(font==null)Initialize();var e=Game.Economy;
            GUI.enabled=!Game.TradeOpen&&!Game.Assist.SampleOpen&&!Game.WorkHUD.AskRestart&&!Game.WorkHUD.AskLeave;
            if(!Game.CurrentJob.Completed){
                var growing=e.Growing;
                string text=e.RecentBreak?"ぷつり · "+e.LastBreakReason:growing!=null?$"{growing.LengthCm:0.0} cm  {e.Config.Rank(growing.LengthCm)}  /  見積 {growing.Price(e.Config)}円":e.Status;
                if(!Game.DebugMode&&!e.RecentBreak&&growing==null)text="消しカスは回収して、売却することもできます。";
                Label(390,91,825,27,text,body);
            }
            if(Game.CrumbDebug&&!Game.TradeOpen){
                var m=e.Motion;Fill(new Rect(390,139,490,112),new Color(.05f,.09f,.1f,.93f));
                Label(403,147,468,25,$"F3 デバッグ / 半往復 {m.Turns} / 幅 {m.Width:0.00} / 周期 {m.HalfPeriod:0.00}s",small);
                Label(403,176,468,25,$"中心 {m.Center} / 進行 {m.Direction} / 速度 {m.CenterSpeed:0.00}",small);
                Label(403,205,468,28,$"切断リスク {m.Risk:0.00} / 鉛筆由来の成長余力 {e.GraphiteCredit:0.00}cm",small);
            }
            Fill(new Rect(382,696,854,90),new Color(.08f,.13f,.14f,.97f));
            Label(398,701,800,24,$"回収済 {e.Inventory.Count+e.OverflowCount}本  /  細かいカス {e.LooseGrams:0.00} g  /  玉 {e.Ball.Grams:0.00} g（{e.Ball.DiameterCm:0.0} cm）  /  所持金 {e.Wallet.Balance:N0}円",small);
            if(GUI.Button(new Rect(398,732,135,34),"C  紙から回収",button))Game.CollectCrumbs();
            GUI.enabled=!Game.TradeOpen&&!Game.InputBlocked&&e.LooseGrams>0;
            if(GUI.Button(new Rect(543,732,130,34),"G  丸める",button))Game.RollCrumbs();
            GUI.enabled=!Game.TradeOpen&&!Game.Assist.SampleOpen&&!Game.WorkHUD.AskRestart&&!Game.WorkHUD.AskLeave;
            if(GUI.Button(new Rect(683,732,135,34),"V  買取窓口",button))Game.OpenTrade();
            string rescue=e.RescueRemaining>0?$"Z  吹いたカスを回収（{e.RescueRemaining:0.0}秒）":"Space：吹く / C：回収して残す";
            GUI.enabled=!Game.TradeOpen&&e.RescueRemaining>0;
            if(GUI.Button(new Rect(832,732,384,34),rescue,button))Game.RescueCrumbs();
            GUI.enabled=true;if(Game.TradeOpen)Trade();
        }
        void Trade()
        {
            var e=Game.Economy;
            Fill(new Rect(0,0,1280,800),new Color(.025f,.04f,.04f,.93f));Fill(new Rect(126,44,1028,715),new Color(.11f,.16f,.16f));
            Label(161,68,780,38,"路地の小さな買取窓口",title);
            Label(161,108,820,29,"謎の買取人  「長いものは高く買いますよ。細かいものも、玉なら。」",body);
            Label(161,140,820,28,"「お、いいですね。長いと、なんとなく嬉しいでしょう。」",small);
            if(GUI.Button(new Rect(1000,68,116,35),"戻る / V",button))Game.CloseTrade();
            Label(161,177,930,27,$"所持金 {e.Wallet.Balance:N0}円    今回の売却 {e.JobSales:N0}円    今回最長 {e.JobLongest:0.0} cm / 通算 {e.BestLength:0.0} cm",body);
            Label(161,212,920,24,"回収した一本もの（長さ / 生まれた道具 / 状態 / 価格）",small);
            scroll=GUI.BeginScrollView(new Rect(157,239,964,239),scroll,new Rect(0,0,940,Mathf.Max(230,e.Inventory.Count*35+35)));
            long sellId=-1;
            for(int i=0;i<e.Inventory.Count;i++){
                var item=e.Inventory[i];string state=item.WasBroken?"切れて離れた":"そのまま回収";
                Label(8,i*35,755,32,$"{item.LengthCm,5:0.0} cm  {e.Config.Rank(item.LengthCm),-10}  {item.Source}  /  {state}",small);
                if(GUI.Button(new Rect(767,i*35,155,29),$"{item.Price(e.Config):N0}円で売る",button))sellId=item.Id;
            }
            if(e.Inventory.Count==0)Label(8,12,800,30,"まだ一本ものがありません。紙の上のカスはCで回収できます。",small);
            GUI.EndScrollView();if(sellId>=0)e.SellPiece(sellId);
            Label(161,483,935,28,e.OverflowCount>0?$"追加保管箱 {e.OverflowCount}本 / 最長 {e.OverflowLongest:0.0} cm / {e.OverflowPrice:N0}円（全本売却に含む）":"細かなカスも丸めれば買い取ります。",small);
            Label(161,523,630,30,$"消しカス玉  {e.Ball.Grams:0.00} g / 直径 {e.Ball.DiameterCm:0.0} cm  →  {e.Config.BallPrice(e.Ball.Grams):N0}円",body);
            Label(161,553,630,28,$"未加工の細かいカス {e.LooseGrams:0.00} g / 玉への累計投入 {e.Ball.LifetimeAddedGrams:0.00} g",small);
            // A simple growing circular swatch echoes the physical ball on the desk.
            float d=Mathf.Clamp(e.Ball.DiameterCm*27,6,70);
            if(e.Ball.Grams>0){Color old=GUI.color;GUI.color=accent;GUI.DrawTexture(new Rect(1030-d*.5f,540-d*.5f,d,d),BallIcon,ScaleMode.StretchToFill);GUI.color=old;}
            GUI.enabled=e.LooseGrams>0;if(GUI.Button(new Rect(161,590,250,40),"細かなカスを丸める",button))Game.RollCrumbs();
            GUI.enabled=e.Inventory.Count+e.OverflowCount>0;if(GUI.Button(new Rect(426,590,335,40),$"一本ものを全部売る  {e.InventoryPrice:N0}円",button))e.SellLongs();
            GUI.enabled=e.Config.BallPrice(e.Ball.Grams)>0;if(GUI.Button(new Rect(776,590,335,40),$"玉を売る  {e.Config.BallPrice(e.Ball.Grams):N0}円",button))e.SellBall();GUI.enabled=true;
            Label(161,647,950,28,$"売却見積合計  {e.InventoryPrice+e.Config.BallPrice(e.Ball.Grams):N0}円   /   {e.Status}",body);
            Label(161,700,950,27,$"収入の内訳：依頼 {e.Wallet.JobIncome:N0}円 / 消しカス {e.Wallet.CrumbIncome:N0}円  ·  記録は起動中のみ",small);
        }
        Texture2D ballIcon;
        Texture2D BallIcon {get {
            if(ballIcon!=null)return ballIcon;ballIcon=new Texture2D(64,64,TextureFormat.RGBA32,false);
            var pixels=new Color[4096];for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f));pixels[y*64+x]=d<30?Color.Lerp(new Color(.38f,.43f,.32f),Color.white,(64-y-x)/128f+.4f):Color.clear;}ballIcon.SetPixels(pixels);ballIcon.Apply();return ballIcon;
        }}
        void OnDestroy(){if(font!=null)Destroy(font);if(ballIcon!=null)Destroy(ballIcon);}
    }
}
