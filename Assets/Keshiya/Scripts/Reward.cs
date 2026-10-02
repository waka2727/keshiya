using UnityEngine;
namespace Keshiya {
 public struct RewardResult { public float erased,damage,peak,seconds,protectedDamage,qualitySensitivity;public bool severe,protectedMajor;public int basic,finish,pristine,speed,protection,challenge;public int Total=>basic+finish+pristine+speed+protection+challenge; }
 public static class Reward {
  public static RewardResult Calculate(PrototypeConfig c,float erased,float damage,float peak,float seconds){bool severe=peak>=c.severeDamageThreshold;return new RewardResult {erased=erased,damage=damage,peak=peak,seconds=seconds,severe=severe,basic=c.baseReward,finish=Mathf.RoundToInt(c.finishBonus*Mathf.Clamp01(erased)*Mathf.Clamp01(1-damage*8)*(severe?.25f:1)),pristine=peak<=c.damageFreeTolerance?c.pristineBonus:0,speed=Mathf.RoundToInt(c.speedBonus*Mathf.Clamp01(1-seconds/Mathf.Max(.01f,c.referenceSeconds)))};}
  public static RewardResult ApplyProtection(RewardResult result,ProtectedDrawing layer,JobDefinition job){
   if(layer==null||job==null)return result;
   result.protectedDamage=layer.Loss;result.protectedMajor=layer.Major;
   float quality=Mathf.Clamp01(1-layer.Loss*3);
   result.finish=Mathf.RoundToInt(result.finish*quality*(layer.Major?.1f:1));
   result.protection=layer.Major?0:Mathf.RoundToInt(job.protectionBonus*quality);
   return result;
  }
 }
}
