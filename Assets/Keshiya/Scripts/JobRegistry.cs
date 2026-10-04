using System.Collections.Generic;
using UnityEngine;
namespace Keshiya {
 [CreateAssetMenu(menuName="Keshiya/Job Registry")]
 public sealed class JobRegistry:ScriptableObject {
  public JobDefinition[] released=new JobDefinition[0];
  public JobDefinition[] Merge(JobDefinition[] original){var list=new List<JobDefinition>();var ids=new HashSet<string>();foreach(var j in original??new JobDefinition[0])if(j!=null&&!(j.content?.developmentOnly??false)&&ids.Add(j.id))list.Add(j);foreach(var j in released)if(j!=null&&!(j.content?.developmentOnly??false)&&ids.Add(j.id))list.Add(j);return list.ToArray();}
 }
}
