using UnityEngine;

namespace Keshiya
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        public PrototypeGame Game;
        Font font; GUIStyle title,body,small,big,button;
        Texture2D white;
        Texture2D[] toolIcons;
        public int ToolSlotCount=>toolIcons?.Length??0;
        public int HighlightedTool=>Game.ToolIndex;
        public Texture2D Icon(int index)=>toolIcons[index];
        public void SelectToolSlot(int index)=>Game.SelectTool(index);
        public static Rect ToolSlotRect(int i)=>new Rect(24+i*105,232,98,48);
        int pendingJob=-1;
        readonly Color ink=new Color(.88f,.9f,.86f),muted=new Color(.58f,.67f,.65f),accent=new Color(.61f,.82f,.67f);
        void Init()
        {
            font=Font.CreateDynamicFontFromOSFont(new[]{"Yu Gothic","Meiryo","Arial"},20);
            toolIcons=new Texture2D[Mathf.Min(3,Game.Catalog.tools.Length)];for(int i=0;i<toolIcons.Length;i++)toolIcons[i]=ToolIcon.Create(Game.Catalog.tools[i]);
            white=Texture2D.whiteTexture;title=Style(28,ink);body=Style(16,ink);small=Style(12,muted);big=Style(38,accent);
            button=new GUIStyle(GUI.skin.button){font=font,fontSize=14};
        }
        GUIStyle Style(int size,Color color)=>new GUIStyle(GUI.skin.label){font=font,fontSize=size,normal={textColor=color},wordWrap=true};
        void Label(float x,float y,float w,float h,string text,GUIStyle style)=>GUI.Label(new Rect(x,y,w,h),text,style);
        void Fill(Rect rect,Color color){Color old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,white);GUI.color=old;}
        bool Button(Rect rect,string text,bool selected=false)
        {
            Color old=GUI.backgroundColor;if(selected)GUI.backgroundColor=accent;
            bool clicked=GUI.Button(rect,text,button);GUI.backgroundColor=old;return clicked;
        }
        public static string TimeText(float t)=>$"{(int)t/60:00}:{(int)t%60:00}";
        public static string ModeName(ContactMode m)=>m==ContactMode.Face?"面":m==ContactMode.Edge?"辺":"角";
        void AskJob(int index)
        {
            if(index==Game.JobIndex)return;
            if(Game.CurrentJob.Started&&!Game.CurrentJob.Completed){pendingJob=index;Game.InputBlocked=true;Game.Controller.ResetContact();}
            else Game.SelectJob(index);
        }
        void OnGUI()
        {
            if(Game==null||Game.Paper==null||(Game.External?.ModalOpen??false))return;if(title==null)Init();
            if(Game.ShopOpen||Game.InventoryOpen){Game.ShopHUD.Draw();return;}
            if(Game.SkillsOpen){Game.SkillsHUD.Draw();return;}
            if(Game.BoardEnabled&&Game.Session.Phase!=WorkPhase.Work){Game.WorkHUD.Draw();return;}
            GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/800f,1));
            Fill(new Rect(0,0,358,800),new Color(.085f,.13f,.14f));Fill(new Rect(24,24,35,3),accent);
            Label(24,34,310,42,"消し屋",title);Label(25,78,310,22,"External Test 01 / 開発中",small);
            GUI.enabled=pendingJob<0&&!Game.TradeOpen&&!Game.WorkHUD.AskLeave&&!Game.WorkHUD.AskRestart&&!Game.Assist.SampleOpen;
            if(!Game.BoardEnabled&&Button(new Rect(24,108,148,31),"通常依頼",Game.JobIndex==0))AskJob(0);
            if(!Game.BoardEnabled&&Button(new Rect(182,108,150,31),"精密依頼",Game.JobIndex==1))AskJob(1);
            if(Game.BoardEnabled&&Button(new Rect(24,108,148,31),"依頼一覧へ")){Game.WorkHUD.AskLeave=true;Game.InputBlocked=true;Game.Controller.ResetContact();}
            if(Game.BoardEnabled&&Button(new Rect(182,108,150,31),"やり直す"))Game.RequestRestart();
            Label(24,149,308,44,Game.Definition.instruction,small);
            Label(24,204,314,31,Game.Eraser.displayName,body);
            Game.ShopHUD.DrawTray(24,232,308);
            float remaining=Game.ActiveState.Remaining(Game.Eraser,Game.Modifiers);
            Label(24,308,308,20,$"{(remaining<=0?"使い切り・Tで交換 / ":remaining<.1f?"残量わずか / ":"")}残量 {remaining*100:0.0}%     角の鋭さ {Game.ActiveState.CornerSharpness*100:0}%",small);
            Fill(new Rect(24,324,308,2),new Color(.2f,.27f,.27f));Fill(new Rect(24,324,308*remaining,2),Game.Eraser.labelColor);
            for(int i=0;i<3;i++)if(Button(new Rect(24+i*105,327,98,32),ModeName((ContactMode)i),(int)Game.Mode==i))Game.SelectMode((ContactMode)i);
            Label(24,365,230,23,$"{ModeName(Game.Mode)} / {Game.Yaw:0}°  ·  Rで45°回転",small);
            if(Button(new Rect(232,365,100,24),Game.ContactGuide.Enabled?"ガイド ON":"ガイド OFF"))Game.ContactGuide.Enabled=!Game.ContactGuide.Enabled;
            if(Game.DebugEraseAreaVisible)DrawContactIcon(new Vector2(307,376));
            Label(24,399,130,30,"消去率",body);Label(154,391,178,49,$"{Game.Paper.Drawing.Erased*100:0.0}%",big);
            Fill(new Rect(24,444,308,5),new Color(.2f,.27f,.27f));Fill(new Rect(24,444,308*Game.Paper.Drawing.Erased,5),accent);
            Label(24,464,310,27,$"紙 {Game.Paper.AverageDamage*100:0.00}% / 局所 {Game.Paper.PeakDamage*100:0.0}%"+(Game.Paper.Severe?" 重大損傷":""),small);
            var protection=Game.Paper.Protection;
            Label(24,494,310,26,protection==null?"保護対象：なし":$"保護文字 {protection.Loss*100:0.0}% · {protection.Grade}",body);
            string state=Game.Controller.Pressing?(Game.Controller.Speed>.03f?"擦っています":"押し付け中"):"浮かせています";
            Label(24,529,310,24,state,body);
            Label(24,554,310,22,Game.Supplied!=null?$"支給品の消耗 {Game.CurrentJob.SupplyProgress*100:0}%":Game.Eraser.specialKind==SpecialToolKind.None?Game.Eraser.role:Game.ActiveState.special.Text(Game.Eraser),small);
            Label(24,571,310,25,$"作業 {TimeText(Game.CurrentJob.Seconds)}  /  制限時間なし",small);
            GUI.enabled=pendingJob<0&&!Game.TradeOpen&&!Game.WorkHUD.AskLeave&&!Game.WorkHUD.AskRestart&&!Game.Assist.SampleOpen&&Game.CurrentJob.CanComplete(Game.Paper);
            if(Button(new Rect(24,614,308,44),"依頼完了"))Game.Finish();GUI.enabled=pendingJob<0&&!Game.TradeOpen&&!Game.WorkHUD.AskLeave&&!Game.WorkHUD.AskRestart&&!Game.Assist.SampleOpen;
            if(Button(new Rect(24,660,308,21),"K：スキル / F：消し残し感知"))Game.OpenSkills();
            Label(24,681,310,24,"マウス：移動 / 左ボタン：押して擦る",small);
            Label(24,706,310,24,"1〜9：種類 / Tab：個体 / T：持込品",small);
            Label(24,731,310,24,"Q/E：面辺角   R：45° / Shift+R：逆回転",small);
            Label(24,756,310,25,Game.Audio.Muted?"Space：吹く   M：音をオン":"Space：吹く   M：ミュート",small);
            if(Game.Definition.artwork!=null&&Game.Viewport.Zoom>1)Fill(new Rect(358,0,922,115),new Color(.06f,.10f,.11f,.85f));
            Label(390,28,820,30,"WORK DESK  /  "+Game.Definition.displayName+"  ·  "+Game.Definition.PaperName+" × "+Game.Definition.WritingName,body);
            string message=Time.unscaledTime-Game.LastProtectionTime<.7f?"残す文字が薄くなりました · 少し離して擦る":Time.unscaledTime-Game.LastDamageTime<.55f?"紙の繊維が傷みました · 少しゆっくり":"";
            Label(390,64,820,30,message,body);
            if(Game.Definition.letterCorrection&&!Game.Definition.customDrawing&&!Game.CurrentJob.Completed)LetterLabels();
            if(Game.CurrentJob.Completed)Result();
            GUI.enabled=true;if(pendingJob>=0)ConfirmJob();else if(!Game.WorkHUD.AskLeave)Game.TradeHUD.Draw();if(Game.BoardEnabled)Game.WorkHUD.DrawLeave();Game.SkillsHUD.Notice();Game.WorkHUD.DrawRestart();
        }
        void DrawContactIcon(Vector2 center)
        {
            var footprint=Game.Contact;var matrix=GUI.matrix;
            GUIUtility.RotateAroundPivot(-footprint.Angle,center);
            // Relative dimensions, not a second cursor: the long direction follows actual yaw.
            Vector2 size=footprint.HalfSize*70;
            Fill(new Rect(center-size*.5f,size),accent);GUI.matrix=matrix;
        }
        void PaperLabel(Vector2 p,float width,string text,int fontSize=13)
        {
            var screen=Game.Controller.View.WorldToScreenPoint(new Vector3(p.x,.058f,p.y));
            Vector2 origin=new Vector2(screen.x*1280/Screen.width,(Screen.height-screen.y)*800/Screen.height);
            var style=new GUIStyle(small){fontSize=fontSize,alignment=TextAnchor.MiddleCenter,normal={textColor=new Color(.29f,.33f,.31f)}};
            Label(origin.x-width*.5f,origin.y-12,width,24,text,style);
        }
        void LetterLabels()
        {
            PaperLabel(new Vector2(0,1.5f),360,"LETTER / DRAFT",19);
            PaperLabel(new Vector2(-.65f,.70f),92,"残す：A");
            PaperLabel(new Vector2(.65f,.70f),92,"消す：B");
            PaperLabel(new Vector2(-1.0f,0),70,"Dear",19);
            PaperLabel(new Vector2(0,-1.85f),480,"薄い便箋 — 右の書き損じだけを消してください",12);
        }
        void Result()
        {
            var r=Game.CurrentJob.Result;
            Fill(new Rect(358,0,922,800),new Color(.05f,.09f,.1f,.9f));Fill(new Rect(505,62,625,674),new Color(.1f,.16f,.17f));
            Label(543,91,548,44,"依頼完了",title);
            Label(543,142,548,30,r.protectedMajor?"重大ミス · 保護文字の仕上がり評価が低下":r.severe?"紙に重大損傷 · 仕上がり評価が低下":"おつかれさまでした。",body);
            var rows=new System.Collections.Generic.List<string>{
                $"消去率|{r.erased*100:0.0}%",$"紙ダメージ（平均 / 最大）|{r.damage*100:0.00}% / {r.peak*100:0.0}%",$"作業時間|{TimeText(r.seconds)}",
                $"基本報酬|{r.basic:N0} 円",$"仕上がりボーナス|+{r.finish:N0} 円",$"紙ダメージなしボーナス|+{r.pristine:N0} 円",$"スピードボーナス|+{r.speed:N0} 円"};
            if(Game.Definition.precision){rows.Insert(2,$"保護文字の損傷|{r.protectedDamage*100:0.0}%");rows.Add($"保護文字ボーナス|+{r.protection:N0} 円");}
            for(int i=0;i<rows.Count;i++){var parts=rows[i].Split('|');Label(543,196+i*35,312,30,parts[0],body);Label(876,196+i*35,230,30,parts[1],body);}
            Label(543,532,548,54,$"{r.Total:N0} 円",big);Label(543,584,560,27,$"消しカス売却 +{Game.Economy.JobSales:N0}円  /  所持金 {Game.Economy.Wallet.Balance:N0}円",body);Label(543,615,548,24,"依頼報酬は入金済み。回収品はVで売れます。時間の減額なし。",small);
            if(Button(new Rect(543,645,268,43),"同じ道具でもう一度"))Game.Restart();
            if(Button(new Rect(825,645,268,43),"新品の道具でやり直す"))Game.FreshTools();
        }
        void ConfirmJob()
        {
            Fill(new Rect(358,0,922,800),new Color(.05f,.09f,.1f,.85f));Fill(new Rect(510,270,620,250),new Color(.1f,.16f,.17f));
            Label(548,302,545,70,"依頼を切り替えますか？\n紙の進捗はリセットされ、道具の摩耗は残ります。",body);
            if(Button(new Rect(548,424,255,46),"切り替える")){int next=pendingJob;pendingJob=-1;Game.InputBlocked=false;Game.SelectJob(next);}
            if(Button(new Rect(827,424,255,46),"作業に戻る")){pendingJob=-1;Game.InputBlocked=false;}
        }
        void OnDestroy(){if(font!=null)Destroy(font);if(toolIcons!=null)foreach(var icon in toolIcons)Destroy(icon);}
    }
}
