using UnityEngine;
namespace Keshiya {
 [CreateAssetMenu(menuName="Keshiya/Writing Instrument Definition")]
 public sealed class WritingInstrumentDefinition:ScriptableObject {
  public string id,displayName,description,recommendedTools;
  [Min(.1f)] public float density=1,erasability=1,lineWidth=1,crumbModifier=1;
  [Min(0)] public float penetration;
  public Color traceColor=new Color(.22f,.23f,.23f);
  public float Removal(float pressure)=>erasability/(1+penetration*Mathf.Max(.1f,pressure));
  public float Mass(float pressure)=>density*Mathf.Lerp(.85f,1.15f,Mathf.Clamp01(pressure-0.5f));
 }
}
