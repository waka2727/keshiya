using UnityEngine;
namespace Keshiya {
 [CreateAssetMenu(menuName="Keshiya/Progression Config")]
 public sealed class ProgressionConfig:ScriptableObject {
  public float firstThreshold=80,thresholdStep=15;
  public float erasedAreaExp=12,erasedFractionExp=60,nearProtectionExp=40,cornerFractionExp=50,edgeFractionExp=30,modePracticeExp=3;
  public float completionExp=20,milestoneExp=10,efficiencyExp=10,safePaperExp=10,precisionSafeExp=20,protectedSafeExp=30;
  public float longExpScale=.30f,longExpPower=1.2f,collectionExpPerCm=.15f,bestExpPerCm=.30f,ballExpPerGram=2,ballExpCap=25,saleExpPerYen=.015f,saleExpCap=30;
  public float Required(int earned)=>firstThreshold+thresholdStep*earned;
 }
}
