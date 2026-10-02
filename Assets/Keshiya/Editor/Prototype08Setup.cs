using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Keshiya.Editor {
 public static class Prototype08Setup {
  static T Get<T>(string name,out bool fresh) where T:ScriptableObject {string path="Assets/Keshiya/Resources/"+name+".asset";var d=AssetDatabase.LoadAssetAtPath<T>(path);fresh=d==null;if(fresh){d=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(d,path);}return d;}
  static EraserContactProfile Shape(string name,Vector2 half,float exponent){var p=Get<EraserContactProfile>("Shape-"+name,out bool fresh);if(fresh){p.halfSizeMultiplier=half;p.exponent=exponent;EditorUtility.SetDirty(p);}return p;}
  public static void Apply(PrototypeGame game){
   string[] ids={"toast-collect","baguette","croissant","carrot","radish","cat","rabbit","kneaded","bread-special","named-supply"};
   string[] names={"食パン消し","フランスパン消し","クロワッサン消し","にんじん消し","だいこん消し","ねこ消し","うさぎ消し","練り消し","パン消し","名前入りの小さな消しゴム"};
   string[] descriptions={"耳まで使える食パン型。広い面はやさしいが、細かい所は苦手。これはゴム製です。","長い辺を、ゆっくりひと引き。細そうに見えて、角は大ざっぱ。","見た目はいい。持ちにくい。それでも買う人はいる。","畑から机へ。細身だが、製図道具ほど正確ではない。","白い部分を使ってください。広い仕事をのんびりと。","仕事中も机に一匹。速さより、この顔で選ぶ人へ。","耳に期待しすぎないでください。小さい範囲を少しずつ。","黒鉛を吸着する柔らかい塊。紙には優しい。黒ずむほど吸着は少し落ち、通常のカスは出ない。","こちらはパン型ゴムとは別の道具。使うほど焦げ色になり、少し硬く強くなる。","学生から預かった一本の最後のかけら。依頼の鉛筆跡を消した量に応じて使い切る。"};
   string[] categories={"パン","パン","パン","野菜","野菜","動物","動物","特殊","特殊","支給"};
   string[] series={"パンシリーズ","パンシリーズ","パンシリーズ","野菜シリーズ","野菜シリーズ","どうぶつシリーズ","どうぶつシリーズ","","",""};
   int[] prices={900,1100,1250,700,800,1300,1400,700,550,1};
   float[] power={.83f,1.05f,.72f,.9f,.82f,.86f,.78f,.45f,.75f,.92f},damage={.3f,1.3f,.8f,.55f,.4f,.65f,.5f,.025f,.2f,.4f},radius={.33f,.32f,.27f,.24f,.3f,.25f,.21f,.19f,.25f,.23f},wear={.28f,.24f,.27f,.23f,.3f,.25f,.27f,.12f,.3f,.24f},crumb={1.1f,.8f,.8f,.8f,1.0f,.9f,.9f,.001f,.85f,1};
   var colors=new[]{new Color(.94f,.85f,.64f),new Color(.73f,.49f,.22f),new Color(.85f,.58f,.24f),new Color(.97f,.43f,.14f),new Color(.92f,.94f,.80f),new Color(.76f,.72f,.65f),new Color(.91f,.82f,.84f),new Color(.83f,.84f,.81f),new Color(.97f,.90f,.73f),new Color(.83f,.83f,.72f)};
   var all=new List<EraserDefinition>(game.Catalog.tools);EraserDefinition supply=null;
   for(int i=0;i<ids.Length;i++){var d=Get<EraserDefinition>("WorldTool-"+ids[i],out bool fresh);if(fresh){d.id=ids[i];d.displayName=names[i];d.shortName=names[i].Replace("消しゴム","").Replace("消し","");d.description=descriptions[i];d.category=categories[i];d.series=series[i];d.iconMotif=ids[i];d.price=prices[i];d.erasePower=power[i];d.paperDamageMultiplier=damage[i];d.radius=radius[i];d.wearRate=wear[i];d.cornerWearRate=.026f;d.durability=100;d.crumbAmount=crumb[i];d.bodyColor=colors[i];d.labelColor=colors[i]*.72f;d.labelColor=new Color(d.labelColor.r,d.labelColor.g,d.labelColor.b,1);d.protectedInkAbrasion=i==7?.08f:.6f;d.thinPaperStress=i==7?.08f:i==8?.2f:.7f;d.thinPaperPickup=i==7?1.35f:1;d.crumbCohesion=i==7?.02f:.55f;d.longCrumbPotential=i==7?.01f:.85f;d.highSpeedRisk=i==7?.3f:1;d.role=i<7?"趣味の道具 / "+series[i]:i==7?"吸着 / 紙に優しい・通常カスなし":i==8?"軟質→硬化 / 使うほど少し強く":"この依頼だけの預かり品";
    Vector2 face=i==1?new Vector2(1.5f,.34f):i==2?new Vector2(.95f,.47f):i==3?new Vector2(.55f,1.1f):i==4?new Vector2(.65f,1.05f):i==6?new Vector2(.65f,1):new Vector2(1,.85f);
    d.contactProfile=Shape(ids[i]+"-face",face,i==2||i>=5?2:4);d.edgeProfile=Shape(ids[i]+"-edge",new Vector2(face.x,.16f),4);d.cornerProfile=Shape(ids[i]+"-corner",Vector2.one*(i==0||i==1?.24f:i==7?.13f:.18f),4);d.iconScale=i==1?new Vector2(1,.65f):i==3||i==4||i==6?new Vector2(.6f,1.1f):Vector2.one;
    if(i==7){d.specialKind=SpecialToolKind.Kneaded;d.usedColor=new Color(.22f,.23f,.24f);}if(i==8){d.specialKind=SpecialToolKind.Bread;d.usedColor=new Color(.36f,.18f,.07f);}if(i==9)d.questOnly=true;
    EditorUtility.SetDirty(d);
   }if(!all.Contains(d))all.Add(d);if(i==9)supply=d;}
   game.Catalog.tools=all.ToArray();EditorUtility.SetDirty(game.Catalog);
   string[] jobs={"named-remainder","late-manga","childs-gift","recipe-notes","unsent-letter","exam-corrections","giant-doodle","shop-banner"};
   string[] titles={"誰にも見せない名前","漫画家の修羅場","子どもの力作","古いレシピノート","渡さなかった手紙","答案の書き直し","集会所の巨大落書き","商店街の看板下書き"};
   string[] clients={"学生","漫画家","子どもの親","料理好きの人","会社員","教師","町内会の人","看板屋"};
   string[] desc={"名前を書いた消しゴム、最後のかけらです。預けた練習帳を消しながら使い切ってください。白紙を擦るだけでは減りません。","三つのコマに下描きが残っています。ペン線と枠を残して、鉛筆を消してください。","家族への絵が完成しました。濃い絵と文字は残し、下書きだけ消してください。","本文は昔のまま残したいです。横に足したXのメモだけ消してください。","この手紙は渡さないことにしました。書いてあるものを全部消してください。","名前・問題・正しい式は残して、横の誤答だけ消してください。授業でまた使います。","みんなで描いた練習の落書きです。紙いっぱいですが、全部お願いします。","看板の配置が決まったので、長く引いた鉛筆の帯を消してください。"};
   string[] quotes={"誰にも見られず使い切ると、って話があるんです。名前は包みの内側のままで。","急いでいるのは私だけなので、線は丁寧にお願いします。","濃いところが本人の完成形なんです。","この字を見ると、分量を思い出せるんです。","理由は、聞かなくて大丈夫です。","正解の方まで消さないようお願いします。","次の集まりでも、また描きたいそうです。","店名の位置は決まりました。もう目印はいりません。"};
   string[] papers={"ordinary","fine","drawing","old","thin","ordinary","drawing","drawing"},writing={"hb","2b","hb","hb","hb","mechanical","4b","2b"};
   int[] rewards={1600,2600,1900,2200,1300,2000,2300,1700};var jobList=new List<JobDefinition>(game.Jobs);
   for(int i=0;i<8;i++){var j=Get<JobDefinition>("WorldJob-"+jobs[i],out bool fresh);if(fresh){j.id=jobs[i];j.displayName=titles[i];j.client=clients[i];j.summary=desc[i];j.instruction=desc[i];j.clientQuote=quotes[i];j.paper=Resources.Load<PaperDefinition>("Paper-"+papers[i]);j.writing=Resources.Load<WritingInstrumentDefinition>("Writing-"+writing[i]);j.paperName=j.paper.displayName;j.paperFragility=j.paper.fragility;j.customDrawing=true;j.precision=i==1||i==2||i==3||i==5;j.category=i==0?"預かり品":j.precision?"精密":"広範囲";j.baseReward=rewards[i];j.difficulty=j.precision?4:i==0?2:1;j.requiredErasure=.95f;j.referenceSeconds=i==1||i==6?360:240;j.majorLoss=.2f;j.recommended=i==0?"預かった消しゴム（支給）":i==3?"やわらか、高級製図、練り消し":i==6?"大型、普通、砂消し":i==7?"普通、やわらか、格安やわらか":"普通、細型製図、好みの道具";j.feature=i==0?"実作業で支給品を使い切る":i==1?"広い領域・線沿い・交点":i==3?"古い本文を残す":i==4?"すべて消去 / 保護なし":i==5?"正解と名前を残す":i==6?"面で広く消す":i==7?"連続する鉛筆の帯":"下書きだけ消す";
    var target=new List<JobPath>();var keep=new List<JobPath>();
    if(i==0){j.suppliedTool=supply;j.suppliedRemaining=.12f;j.thankYou="使ってくれて、ありがとう。";for(int n=0;n<9;n++)WorldArtwork.Text(target,n%2==0?"ABC 123":"123 ABC",-2.1f,-1.8f+n*.42f,.045f);}
    if(i==1){for(int panel=0;panel<3;panel++){float x=-2.75f+panel*1.85f;WorldArtwork.Box(keep,x,-1.85f,1.7f,3.7f);keep.Add(new JobPath(new[]{new Vector2(x+.4f,.75f),new Vector2(x+.85f,1.2f),new Vector2(x+1.3f,.75f)}));WorldArtwork.Line(target,x+.15f,-1.68f,x+1.55f,-1.68f);WorldArtwork.Line(target,x+1.55f,-1.6f,x+1.55f,1.6f);target.Add(new JobPath(new[]{new Vector2(x+.4f,.60f),new Vector2(x+.85f,1.05f),new Vector2(x+1.3f,.60f)}));for(int n=0;n<10;n++)WorldArtwork.Line(target,x+.38f,-1.25f+n*.16f,x+1.3f,-1.19f+n*.16f);}}
    if(i==2){WorldArtwork.Text(keep,"THANKS",-1.7f,1.05f,.085f);WorldArtwork.Box(keep,-.75f,-1.35f,1.5f,1.1f);keep.Add(new JobPath(new[]{new Vector2(-1,-.25f),new Vector2(0,.45f),new Vector2(1,-.25f)}));WorldArtwork.Box(keep,-.2f,-1.35f,.4f,.65f);WorldArtwork.Line(target,-1.05f,-1.53f,1.05f,-1.53f);WorldArtwork.Line(target,-1.15f,-1.3f,-1.15f,-.5f);for(int n=0;n<6;n++)WorldArtwork.Line(target,-2.5f,-1.5f+n*.33f,-1.45f,-1.4f+n*.33f);for(int n=0;n<6;n++)WorldArtwork.Line(target,1.45f,-1.5f+n*.33f,2.5f,-1.4f+n*.33f);WorldArtwork.Line(target,-1.75f,.9f,1.45f,.9f);}
    if(i==3){for(int row=0;row<3;row++){float y=1.05f-row*1.05f;WorldArtwork.Text(keep,new[]{"SALT 2","SOUP 3","MILK 1"}[row],-2.1f,y,.07f);WorldArtwork.Text(target,"X",.41f,y+.06f,.052f);}}
    if(i==4){for(int row=0;row<6;row++)WorldArtwork.Text(target,new[]{"DEAR YOU","THANK YOU","I WAS HAPPY","WITH YOU","TAKE CARE","FROM ME"}[row],-2.15f,1.45f-row*.6f,.061f);}
    if(i==5){WorldArtwork.Text(keep,"NAME MIO",-2.4f,1.4f,.065f);for(int row=0;row<3;row++){float y=.55f-row*.85f;WorldArtwork.Text(keep,new[]{"2+2=4","3+3=6","4+4=8"}[row],-2.3f,y,.072f);WorldArtwork.Text(target,new[]{"5","7","9"}[row],-.16f,y+.035f,.058f);WorldArtwork.Line(keep,1.25f,y,1.6f,y+.4f);}}
    if(i==6){for(int n=0;n<21;n++){var pts=new Vector2[31];for(int k=0;k<31;k++){float x=-2.8f+k*.1866f;pts[k]=new Vector2(x,-1.8f+n*.18f+.055f*Mathf.Sin(x*3+n));}target.Add(new JobPath(pts));}}
    if(i==7){for(int band=0;band<4;band++)for(int n=0;n<6;n++){var pts=new Vector2[45];for(int k=0;k<45;k++){float x=-2.7f+k*.1227f;pts[k]=new Vector2(x,-1.5f+band*1.0f+(n-2.5f)*.06f+.06f*Mathf.Sin(x));}target.Add(new JobPath(pts));}}
    j.targetPaths=target.ToArray();j.protectedPaths=keep.ToArray();EditorUtility.SetDirty(j);
   }jobList.Add(j);}game.Jobs=jobList.ToArray();
  }
 }
}
