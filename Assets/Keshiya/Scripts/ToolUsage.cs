using UnityEngine;
namespace Keshiya {
 [System.Serializable] public sealed class ToolUsage {
  public string instanceId,definitionId;public bool supplied;
  public float contactSeconds,erasedFraction,graphiteArea,paperDamage,protectedDamage;
  public float[] modeDistance=new float[3];
  public void Add(StrokeResult r,ContactMode mode,float distance,float speed,int pixels){contactSeconds+=distance/Mathf.Max(.01f,speed);erasedFraction+=r.erasedFraction;graphiteArea+=r.graphiteArea;paperDamage+=r.addedDamage/Mathf.Max(1,pixels);protectedDamage+=r.protectedLoss;modeDistance[(int)mode]+=distance;}
  public ToolUsage Copy()=>new ToolUsage{instanceId=instanceId,definitionId=definitionId,supplied=supplied,contactSeconds=contactSeconds,erasedFraction=erasedFraction,graphiteArea=graphiteArea,paperDamage=paperDamage,protectedDamage=protectedDamage,modeDistance=(float[])modeDistance.Clone()};
 }
}
