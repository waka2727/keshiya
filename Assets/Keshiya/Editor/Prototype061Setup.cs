using UnityEngine;
using UnityEditor;
namespace Keshiya.Editor {
 public static class Prototype061Setup {
  public static void Apply(PrototypeGame game){
   var skills=Resources.Load<SkillCatalog>("SkillCatalog");if(skills.balanceVersion>=61)return;
   skills.Find(SkillKind.Sense).durations=new float[11];
   for(int level=0;level<=10;level++){
    skills.Find(SkillKind.Efficiency).effects[level]=level<=5?Mathf.Lerp(.9f,1.4f,level/5f):Mathf.Lerp(1.4f,1.85f,(level-5)/5f);
    skills.Find(SkillKind.PaperCare).effects[level]=level<=5?Mathf.Lerp(1,.7f,level/5f):Mathf.Lerp(.7f,.45f,(level-5)/5f);
    skills.Find(SkillKind.InkCare).effects[level]=level<=5?Mathf.Lerp(1,.6f,level/5f):Mathf.Lerp(.6f,.3f,(level-5)/5f);
    skills.Find(SkillKind.CornerCare).effects[level]=level<=5?Mathf.Lerp(1,.65f,level/5f):Mathf.Lerp(.65f,.35f,(level-5)/5f);
    skills.Find(SkillKind.Sense).effects[level]=level==0?1.01f:level<=5?Mathf.Lerp(.9f,.78f,(level-1)/4f):Mathf.Lerp(.78f,.6f,(level-5)/5f);
    skills.Find(SkillKind.Sense).durations[level]=level==0?0:2.5f+level*.4f;
   }
   skills.Find(SkillKind.Efficiency).description="少ない往復で消せるようになる。砂消しの速さには届かない。";
   skills.Find(SkillKind.PaperCare).description="紙の傷みを抑える。砂消しには軽減が控えめに効く。";
   skills.Find(SkillKind.InkCare).description="接触面の端でかすった時を救済。中央で繰り返し擦れば傷む。";
   skills.Find(SkillKind.Sense).description="Fで残る鉛筆を一時表示。高Lvほど早い段階から長く感知。";
   float[] wear={.18f,.23f,.16f,.21f,.48f,.12f,.90f};float[] corners={.024f,.032f,.018f,.040f,.030f,.0015f,.065f};
   string[] ids={"ordinary","soft","sand","drafting","large","premium","budget-soft"};
   for(int i=0;i<ids.Length;i++){var d=game.Catalog.Find(ids[i]);d.wearRate=wear[i];d.cornerWearRate=corners[i];d.edgeCornerWear=.15f;d.minimumSharpness=.10f;EditorUtility.SetDirty(d);}
   var sand=game.Catalog.Find("sand");sand.paperCareResponse=8f/11f;EditorUtility.SetDirty(sand);
   var premium=game.Catalog.Find("premium");premium.price=7800;premium.erasePower=1.35f;premium.paperDamageMultiplier=.13f;premium.protectedInkAbrasion=.22f;premium.cornerSpread=.45f;premium.cornerProfile.halfSizeMultiplier=Vector2.one*.09f;EditorUtility.SetDirty(premium.cornerProfile);EditorUtility.SetDirty(premium);
   game.Config.protectedGlanceExponent=2;game.Config.senseMinimumInk=.002f;EditorUtility.SetDirty(game.Config);
   skills.balanceVersion=61;EditorUtility.SetDirty(skills);
  }
 }
}
