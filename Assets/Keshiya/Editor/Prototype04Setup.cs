using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Keshiya.Editor {
 public static class Prototype04Setup {
  static JobDefinition Get(string name){string path="Assets/Keshiya/Resources/"+name+".asset";var j=AssetDatabase.LoadAssetAtPath<JobDefinition>(path);if(j==null){j=ScriptableObject.CreateInstance<JobDefinition>();AssetDatabase.CreateAsset(j,path);}return j;}
  static JobPath Line(float x,float y,float xx,float yy)=>new JobPath(new[]{new Vector2(x,y),new Vector2(xx,yy)});
  public static void Apply(PrototypeGame game){
   var jobs=new[]{Get("NormalJob"),Get("PrecisionJob"),Get("DraftJob"),Get("CorrectionsJob"),Get("PatternJob")};
   string[] ids={"child","letter","draft","corrections","pattern"},names={"子どもの落書き","手紙の書き損じ","大量の鉛筆跡","丁寧な修正","包装紙の下書き"},clients={"子どもの親","会社員","漫画家","教師","文具店の店主"};
   string[] summaries={"お気に入りの紙に描いた、猫の鉛筆画を消してください。","宛名を一文字間違えました。隣の文字は残してください。","ペンを入れる前に、重なった構図の下書きを消したいです。","教材の数箇所を書き損じました。正しい文字と罫線はそのままに。","包装紙に引いた、長く続く鉛筆の模様を消してください。"};
   string[] quotes={"また描きたいそうなので、きれいにしてください。","急ぎません。左の文字には触れないようお願いします。","かなり広いですが、下書きを全部お願いします。","薄い紙なので、一つずつ丁寧にお願いします。","まっさらな紙に戻して、別の模様を考えたいんです。"};
   string[] features={"広い落書き","一文字の精密修正","重なった大量の線","複数箇所・保護対象あり","横に続く下書き"};
   string[] recommended={"普通消し / 面","やわらか消し / 角","普通消し、慎重な砂消し / 面","やわらか消し / 辺・角","普通消し、やわらか消し / 面"};
   int[] rewards={1000,1000,1800,2400,1400},difficulty={1,3,2,4,2};float[] times={120,150,210,240,180};
   for(int i=0;i<5;i++){var j=jobs[i];if(string.IsNullOrEmpty(j.id)){j.id=ids[i];j.displayName=names[i];j.client=clients[i];j.summary=summaries[i];j.clientQuote=quotes[i];j.feature=features[i];j.recommended=recommended[i];j.difficulty=difficulty[i];j.baseReward=rewards[i];j.referenceSeconds=i==1?120:times[i];j.requiredErasure=.95f;
    if(i==0)j.instruction="猫の鉛筆画を95%以上消してください。";
    if(i>=2){j.instruction=summaries[i]+"\n鉛筆を95%以上消すと完了です。";j.customDrawing=true;j.paperName=i==3?"薄い教材用紙":"丈夫な普通紙";j.paperFragility=i==3?2.2f:1;j.precision=i==3;j.letterCorrection=i==3;j.majorLoss=.2f;
     var target=new List<JobPath>();var protect=new List<JobPath>();
     if(i==2){for(int line=0;line<19;line++){float y=-1.7f+line*.18f;target.Add(Line(-2.6f,y,2.6f,y+.16f*Mathf.Sin(line)));}for(int line=0;line<16;line++){float x=-2.55f+line*.34f;target.Add(Line(x,-1.8f,x+.25f,1.8f));}}
     if(i==3){for(int row=0;row<2;row++)for(int col=0;col<2;col++){var offset=new Vector2(-1.3f+col*2.4f,-.9f+row*1.8f);foreach(var p in LetterLayout.Target){var q=new Vector2[p.Length];for(int k=0;k<q.Length;k++)q[k]=p[k]+offset;target.Add(new JobPath(q));}foreach(var p in LetterLayout.Protected){var q=new Vector2[p.Length];for(int k=0;k<q.Length;k++)q[k]=p[k]+offset;protect.Add(new JobPath(q));}protect.Add(Line(offset.x-.7f,offset.y-.62f,offset.x+.85f,offset.y-.62f));}}
     if(i==4){for(int band=0;band<3;band++)for(int line=0;line<7;line++){var pts=new Vector2[41];for(int k=0;k<pts.Length;k++){float x=-2.6f+k*.13f;pts[k]=new Vector2(x,-1.35f+band*1.35f+(line-3)*.055f+.045f*Mathf.Sin(x*2));}target.Add(new JobPath(pts));}}
     j.targetPaths=target.ToArray();j.protectedPaths=protect.ToArray();
    }EditorUtility.SetDirty(j);
   }}
   var soft=game.Catalog.tools[1];if(soft.specialtyVersion<2){soft.crumbCohesion=1;soft.longCrumbPotential=1.75f;soft.specialtyVersion=2;EditorUtility.SetDirty(soft);}game.Jobs=jobs;
  }
 }
}
