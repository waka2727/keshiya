using System;
using System.Collections.Generic;
using UnityEngine;
namespace Keshiya {
 public static class DevelopmentMetrics {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public const bool Available=true;
  public static bool Enabled;
  public sealed class Sample {public string name;public double milliseconds;public long managedDelta,textureBytes;}
  public static readonly Queue<Sample> Samples=new Queue<Sample>();
  sealed class Scope:IDisposable {readonly string name;readonly long memory;readonly System.Diagnostics.Stopwatch timer=System.Diagnostics.Stopwatch.StartNew();public Scope(string n){name=n;memory=GC.GetTotalMemory(false);}public void Dispose(){Samples.Enqueue(new Sample{name=name,milliseconds=timer.Elapsed.TotalMilliseconds,managedDelta=GC.GetTotalMemory(false)-memory});while(Samples.Count>100)Samples.Dequeue();}}
  public static IDisposable Measure(string name)=>Enabled?new Scope(name):null;
  public static void Texture(Texture t){if(!Enabled||t==null)return;Samples.Enqueue(new Sample{name="texture: "+t.name,textureBytes=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t)});while(Samples.Count>100)Samples.Dequeue();}
#else
  public const bool Available=false;
  public static IDisposable Measure(string name)=>null;
  public static void Texture(Texture t){}
#endif
 }
}
