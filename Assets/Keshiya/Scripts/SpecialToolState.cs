using UnityEngine;
namespace Keshiya {
 public enum SpecialToolKind { None, Kneaded, Bread }
 // Individual, serializable state. Adsorbed graphite is not bread hardening.
 [System.Serializable] public sealed class SpecialToolState {
  public float graphiteLoad,toastLevel;
  public float Power(EraserDefinition d)=>d.specialKind==SpecialToolKind.Kneaded?Mathf.Lerp(1,d.maturePowerMultiplier,Mathf.Clamp01(graphiteLoad)):d.specialKind==SpecialToolKind.Bread?1+toastLevel*d.toastPowerGain:1;
  public float Damage(EraserDefinition d)=>d.specialKind==SpecialToolKind.Bread?1+toastLevel*d.toastDamageGain:1;
  public void Advance(EraserDefinition d,float graphite,float distance){
   if(d.specialKind==SpecialToolKind.Kneaded)graphiteLoad=Mathf.Clamp01(graphiteLoad+Mathf.Max(0,graphite)/Mathf.Max(.01f,d.adsorptionCapacity));
   if(d.specialKind==SpecialToolKind.Bread)toastLevel=Mathf.Clamp01(toastLevel+Mathf.Max(0,distance)/Mathf.Max(1,d.toastDistance));
  }
  public Color Color(EraserDefinition d)=>d.specialKind==SpecialToolKind.None?d.bodyColor:UnityEngine.Color.Lerp(d.bodyColor,d.usedColor,d.specialKind==SpecialToolKind.Kneaded?graphiteLoad:toastLevel);
  public string Text(EraserDefinition d)=>d.specialKind==SpecialToolKind.Kneaded?$"馴染み・成長 {graphiteLoad*100:0}%":d.specialKind==SpecialToolKind.Bread?$"焦げ・硬化 {toastLevel*100:0}%":"";
 }
}
