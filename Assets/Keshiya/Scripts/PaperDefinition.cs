using UnityEngine;
namespace Keshiya {
 [CreateAssetMenu(menuName="Keshiya/Paper Definition")]
 public sealed class PaperDefinition:ScriptableObject {
  public string id,displayName,description,toolTendency;
  [Min(.1f)] public float thickness=1,durability=1,frictionResistance=1,localWearResistance=1;
  [Min(.1f)] public float surfaceDrag=1;
  [Range(1,3)] public float fragility=1;
  [Min(1)] public float qualitySensitivity=1;
  public float DamageScale=>1/Mathf.Max(.1f,durability);
 }
}
