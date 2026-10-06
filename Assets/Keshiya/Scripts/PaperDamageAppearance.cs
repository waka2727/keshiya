namespace Keshiya {
 // Keep synchronized with LayeredPaper.shader: presentation thresholds, not damage balance.
 public static class PaperDamageAppearance {
  public static int Stage(float damage,float tearThreshold){float d=damage/UnityEngine.Mathf.Max(.01f,tearThreshold);return d>1?3:d>.5f?2:d>.12f?1:0;}
 }
}