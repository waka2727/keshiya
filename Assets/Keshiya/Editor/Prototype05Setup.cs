using UnityEngine;
using UnityEditor;
namespace Keshiya.Editor {
 public static class Prototype05Setup {
  public static void Apply(){const string path="Assets/Keshiya/Resources/SkillCatalog.asset";var catalog=AssetDatabase.LoadAssetAtPath<SkillCatalog>(path);
   if(catalog==null){catalog=ScriptableObject.CreateInstance<SkillCatalog>();string[] names={"消去効率","消去範囲","摩耗軽減","紙保護","保護対象ダメージ軽減","角耐久","消し残し感知","長尺成長","切断耐性","消しカス生成量","玉効率"};string[] descriptions={"同じ擦り方で、鉛筆を少し早く消す。","見えている接触面も一緒に少し広がる。","使うたびの消しゴムの減りを抑える。","紙の傷みを減らす。乱暴な砂消しは危険。","接触面の端で残す文字をかすった時の救済。中央で擦れば傷む。","角が丸くなる速度を抑える。","Fで短時間、消し残しを淡く表示。消去率が高い時に使える。","往復しながら進む時の長いカスの成長を助ける。","幅・周期・進行方向のブレへの許容を少し増す。","実際に消した量から生まれる材料を増やす。","回収した細かなカスを少し効率よく玉にする。"};float[] steps={.035f,.015f,-.035f,-.04f,-.05f,-.04f,0,.025f,.015f,.03f,.02f};catalog.skills=new SkillDefinition[11];for(int i=0;i<11;i++){var d=new SkillDefinition{kind=(SkillKind)i,branch=i<3?CraftBranch.Erasing:i<7?CraftBranch.Precision:CraftBranch.Crumbs,title=names[i],description=descriptions[i]};for(int level=0;level<=10;level++)d.effects[level]=i==6?(level==0?1.01f:.95f-.02f*level):i==8?steps[i]*level:1+steps[i]*level;catalog.skills[i]=d;}AssetDatabase.CreateAsset(catalog,path);}
   const string tuning="Assets/Keshiya/Resources/ProgressionConfig.asset";if(AssetDatabase.LoadAssetAtPath<ProgressionConfig>(tuning)==null)AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ProgressionConfig>(),tuning);
  }
 }
}
