using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Keshiya.Editor {
 public static class Prototype07Setup {
  static T Get<T>(string name,out bool fresh) where T:ScriptableObject {string path="Assets/Keshiya/Resources/"+name+".asset";var value=AssetDatabase.LoadAssetAtPath<T>(path);fresh=value==null;if(fresh){value=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(value,path);}return value;}
  static JobPath L(float x,float y,float xx,float yy)=>new JobPath(new[]{new Vector2(x,y),new Vector2(xx,yy)});
  static void Add(List<JobPath> into,Vector2[][] paths,Vector2 offset,float scale=1){foreach(var path in paths){var points=new Vector2[path.Length];for(int i=0;i<points.Length;i++)points[i]=path[i]*scale+offset;into.Add(new JobPath(points));}}
  static void Box(List<JobPath> into,float x,float y,float w,float h){into.Add(new JobPath(new[]{new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h),new Vector2(x,y)}));}
  public static void Apply(PrototypeGame game){
   string[] paperIds={"ordinary","thin","drawing","fine","old"},paperNames={"普通紙","薄い便箋","画用紙","上質紙","古い紙"};
   string[] notes={"標準的な丈夫さ。普段使いの紙。","薄く、局所的な擦れに弱い紙。","厚く丈夫。少し粗い表面。","滑らかで丈夫。傷は仕上がり評価に強く響きます。","傷みやすく、同じ場所の強い往復に弱い紙。"};
   float[] thickness={1,.6f,1.6f,1.1f,.65f},durability={1,.8f,1.5f,1.15f,.6f},friction={1,.9f,1.2f,1.1f,.8f},local={1,.75f,1.5f,1.1f,.5f},fragility={1,2,1,1,2.6f};
   var papers=new PaperDefinition[5];for(int i=0;i<5;i++){var p=Get<PaperDefinition>("Paper-"+paperIds[i],out bool fresh);if(fresh){p.id=paperIds[i];p.displayName=paperNames[i];p.description=notes[i];p.thickness=thickness[i];p.durability=durability[i];p.frictionResistance=friction[i];p.localWearResistance=local[i];p.fragility=fragility[i];p.surfaceDrag=i==2?1.08f:1;p.qualitySensitivity=i==3?2:1;p.toolTendency=i==0?"普通・万能":i==2?"普通・大型":i==3?"普通・高級製図":"やわらか・高級製図";EditorUtility.SetDirty(p);}papers[i]=p;}
   string[] ids={"hb","2b","4b","mechanical","colored"},names={"HB鉛筆","2B鉛筆","4B鉛筆","シャープペン","色鉛筆"};
   float[] density={1,1.35f,1.7f,.95f,1.15f},ease={1,1.12f,1.16f,1,.72f},penetration={0,.02f,.05f,.22f,.3f},width={1,1.1f,1.3f,.7f,1.1f},crumb={1,1.08f,1.12f,.85f,.6f};
   string[] descriptions={"標準的な鉛筆跡。","濃い黒鉛。取れやすい粉が多め。","かなり濃く、消す量の多い鉛筆跡。","細い線。強い筆圧では紙へ食い込みます。","色が染み込み、鉛筆より時間がかかります。"};
   var writing=new WritingInstrumentDefinition[5];for(int i=0;i<5;i++){var w=Get<WritingInstrumentDefinition>("Writing-"+ids[i],out bool fresh);if(fresh){w.id=ids[i];w.displayName=names[i];w.description=descriptions[i];w.density=density[i];w.erasability=ease[i];w.penetration=penetration[i];w.lineWidth=width[i];w.crumbModifier=crumb[i];w.recommendedTools=i==3?"細型製図・普通・高級製図":i==4?"普通・砂消し":"普通・大型・やわらか";if(i==4)w.traceColor=new Color(.48f,.22f,.20f);EditorUtility.SetDirty(w);}writing[i]=w;}
   var jobs=new List<JobDefinition>(game.Jobs);int[] pi={0,1,2,1,2},wi={0,0,1,0,1};string[] candidates={"普通、やわらか","普通、やわらか、細型製図","普通、大型、砂消し","やわらか、細型製図、高級製図","普通、やわらか、格安やわらか"};
   for(int i=0;i<5;i++){var j=jobs[i];if(j.paper==null){j.paper=papers[pi[i]];j.writing=writing[wi[i]];j.recommended=candidates[i];j.category=j.precision?"精密":i>=2?"広範囲":"通常";EditorUtility.SetDirty(j);}}
   string[] jobIds={"close-letter","extra-stroke","ruled-margin","manga","large-study","old-letter","color-doodle"};
   string[] jobNames={"詰まった宛名の一文字","文字の余分な一画","斜め罫線のはみ出し","漫画原稿の下描き","画用紙いっぱいの下描き","古い手紙の追記","色鉛筆の落書き"};
   string[] clients={"会社員","文具店の店主","学生","漫画家","美術部の学生","ご近所の方","子どもの親"};
   string[] summary={"宛名の中央のBだけ消してください。両隣のAは正しい文字です。","Fに余分な下線を書いてしまいました。下の短い一画だけ消してください。","斜めの罫線の間に鉛筆が残りました。濃い二本の線は残してください。","ペン入れ済みの原稿です。薄い下描きだけ消して、枠とペン線を残してください。","濃く重なった構図の下描きを全部消してください。","後から足した小さなBのメモだけ消して、元の文章を残してください。","色鉛筆で描いた絵を消して、もう一度使いたいそうです。"};
   string[] quotes={"文字の間隔が狭くて、自分では隣まで消しそうなんです。","清書用の上質紙です。小さい一画だけ、お願いします。","罫線はこの向きのまま残したいんです。","ペン線が気に入っているので、大切に残してください。","紙は丈夫です。また同じ紙に描き直したくて。","昔の手紙なので、本文も紙も大切にしてもらえると。","少し落ちにくいみたいです。急がなくて大丈夫です。"};
   string[] features={"隣接文字 / 小さな修正","一画修正 / 上質紙","方向のある細長い跡","広い部分とペン線の近く","広範囲 / 濃い線","薄い古紙 / 本文を保護","広範囲 / 消えにくい色"};
   string[] recommend={"普通、細型製図、高級製図","細型製図、普通、高級製図","普通、細型製図","普通、細型製図、高級製図","大型、普通、砂消し","やわらか、高級製図、普通","普通、大型、砂消し"};
   int[] paperIndex={1,3,0,3,2,4,0},writeIndex={3,3,0,0,2,0,4},reward={1700,2100,1700,2300,2200,2100,1600},difficulty={4,4,3,4,2,5,3};
   for(int i=0;i<7;i++){var j=Get<JobDefinition>("Specialist-"+jobIds[i],out bool fresh);if(fresh){j.id=jobIds[i];j.displayName=jobNames[i];j.client=clients[i];j.summary=summary[i];j.clientQuote=quotes[i];j.instruction=summary[i];j.feature=features[i];j.recommended=recommend[i];j.paper=papers[paperIndex[i]];j.writing=writing[writeIndex[i]];j.paperName=j.paper.displayName;j.paperFragility=j.paper.fragility;j.precision=i!=4&&i!=6;j.category=j.precision?"精密":"広範囲";j.difficulty=difficulty[i];j.baseReward=reward[i];j.referenceSeconds=i==3||i==4?300:180;j.requiredErasure=.95f;j.customDrawing=true;j.majorLoss=.2f;j.protectionBonus=j.precision?400:0;
    var target=new List<JobPath>();var protect=new List<JobPath>();
    if(i==0){Add(target,LetterLayout.Target,new Vector2(.044f,-.076f),.4f);Add(protect,LetterLayout.Protected,Vector2.zero,.6f);Add(protect,LetterLayout.Protected,new Vector2(.65f,0),.6f);j.pressure=.5f;}
    if(i==1){protect.Add(new JobPath(new[]{new Vector2(0,-.3f),new Vector2(0,.3f),new Vector2(.4f,.3f)}));protect.Add(L(0,.04f,.32f,.04f));target.Add(L(.08f,-.25f,.38f,-.25f));j.pressure=1.3f;}
    if(i==2){Vector2 a=new Vector2(-1.5f,-1.5f),b=new Vector2(1.5f,1.5f),offset=new Vector2(-.105f,.105f);target.Add(new JobPath(new[]{a,b}));protect.Add(new JobPath(new[]{a+offset-Vector2.one*.2f,b+offset+Vector2.one*.2f}));protect.Add(new JobPath(new[]{a-offset-Vector2.one*.2f,b-offset+Vector2.one*.2f}));}
    if(i==3){Box(protect,-2.75f,-1.8f,2.6f,3.6f);Box(protect,.15f,-1.8f,2.6f,3.6f);for(int side=-1;side<=1;side+=2){float x=side*1.45f;protect.Add(new JobPath(new[]{new Vector2(x-.45f,.5f),new Vector2(x,.95f),new Vector2(x+.45f,.5f)}));target.Add(L(x-1.14f,-1.64f,x+1.14f,-1.64f));target.Add(L(x+1.13f,-1.6f,x+1.13f,1.6f));target.Add(L(x-.46f,.35f,x,.8f));target.Add(L(x,.8f,x+.46f,.35f));for(int n=0;n<6;n++)target.Add(L(x-.82f,-1.1f+n*.18f,x+.82f,-1.04f+n*.18f));}}
    if(i==4){for(int n=0;n<22;n++){float y=-1.8f+n*.17f;target.Add(L(-2.7f,y,2.7f,y+.06f));}for(int n=0;n<13;n++){float x=-2.5f+n*.4f;target.Add(L(x,-1.85f,x+.25f,1.85f));}}
    if(i==5){for(int row=0;row<3;row++){float y=-1.05f+row*1.05f;for(int col=0;col<5;col++)Add(protect,LetterLayout.Protected,new Vector2(-1.7f+col*.8f,y),.65f);Add(target,LetterLayout.Target,new Vector2(-.82f+row*.8f,y),.55f);}}
    if(i==6){target.Add(new JobPath(new[]{new Vector2(-1.8f,-1.25f),new Vector2(-1.8f,.5f),new Vector2(0,1.65f),new Vector2(1.8f,.5f),new Vector2(1.8f,-1.25f),new Vector2(-1.8f,-1.25f)}));for(int n=0;n<12;n++)target.Add(L(-1.65f,-1.12f+n*.13f,1.65f,-1.12f+n*.13f));}
    j.targetPaths=target.ToArray();j.protectedPaths=protect.ToArray();EditorUtility.SetDirty(j);
   }jobs.Add(j);}game.Jobs=jobs.ToArray();
  }
 }
}
