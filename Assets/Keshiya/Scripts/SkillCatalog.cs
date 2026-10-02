using UnityEngine;
namespace Keshiya {
 public enum CraftBranch { Erasing, Precision, Crumbs }
 public enum SkillKind { Efficiency, Range, Wear, PaperCare, InkCare, CornerCare, Sense, LongGrowth, Continuity, CrumbAmount, BallYield }
 [System.Serializable] public sealed class SkillDefinition {
  public SkillKind kind;public CraftBranch branch;public string title,description;public int maxLevel=10;public int cost=1;public float[] effects=new float[11];public float[] durations=new float[11];
  public float Duration(int level)=>durations!=null&&durations.Length>level&&durations[level]>0?durations[level]:1.5f+level*.2f;
  public float Effect(int level)=>effects[Mathf.Clamp(level,0,effects.Length-1)];
  public string Display(int level){float e=Effect(level);if(kind==SkillKind.Sense)return level==0?"未習得":$"消去{e*100:0.#}%から / {Duration(level):0.0}秒";if(kind==SkillKind.Continuity)return $"許容補正 +{e*100:0.#}%";if(kind==SkillKind.Efficiency)return $"消去力 ×{e:0.00}";return e>=1?$"+{(e-1)*100:0.#}%":$"軽減 {(1-e)*100:0.#}%";}
 }
 [CreateAssetMenu(menuName="Keshiya/Skill Catalog")]
 public sealed class SkillCatalog:ScriptableObject {
  public SkillDefinition[] skills;public int balanceVersion;
  public SkillDefinition Find(SkillKind kind){foreach(var s in skills)if(s.kind==kind)return s;return null;}
  public static string BranchName(CraftBranch branch)=>branch==CraftBranch.Erasing?"消去技術":branch==CraftBranch.Precision?"精密技術":"消しカス技術";
 }
}
