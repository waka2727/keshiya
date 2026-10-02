using UnityEngine;
namespace Keshiya {
 [CreateAssetMenu(menuName="Keshiya/Prototype Config")]
 public class PrototypeConfig : ScriptableObject {
  [Range(128,1024)] public int resolution=512;
  public Vector2 paperSize=new Vector2(7,5);
  [Header("Precision assistance")] public float protectedGlanceExponent=2,senseMinimumInk=.002f;
  [Header("Speed (world units/sec)")] public float optimalSpeed=4, dangerousSpeed=16, maximumSpeed=35;
  public float wearStartSpeed=12;
  [HideInInspector] public int tuningVersion;
  [Header("Damage / distance")] public float highSpeedDamage=.16f, wearDamage=.035f, wearThreshold=7, wearRecovery=.8f;
  [Range(0,1)] public float completionThreshold=.95f, severeDamageThreshold=.85f;
  public float damageFreeTolerance=.00001f;
  [Header("Rewards")] public int baseReward=1000, finishBonus=300, pristineBonus=250, speedBonus=300;
  public float referenceSeconds=120;
  [Header("Crumbs")] public int crumbCapacity=350; public float crumbSpacing=.09f;
 }
 [System.Serializable] public class PerformanceModifiers {
  [Min(.01f)] public float erasePower=1, radius=1, paperDamage=1, crumbAmount=1, durability=1, crumbContinuity=1, salePrice=1;
  [Min(.01f)] public float cornerWear=1, precision=1;
  public float wear=1, protectedGlance=1, longGrowth=1, crumbTolerance=0, ballYield=1;
 }
}
