#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Keshiya {
 public sealed class DevelopmentPerformanceRecorder:MonoBehaviour {
  [Serializable] sealed class Report {public string version="foundation-v2";public float seconds,averageFps,slowestFrameFps;public long managedBytes;public int gen0Collections;public Entry[] operations;}
  [Serializable] sealed class Entry {public string name;public double milliseconds;public long managedDelta,textureBytes;}
  int frames,collections;float seconds,maxFrame;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  static void StartRecording(){if(!Environment.GetCommandLineArgs().Contains("--measure-performance"))return;DevelopmentMetrics.Enabled=true;var go=new GameObject("Development performance recorder");DontDestroyOnLoad(go);go.AddComponent<DevelopmentPerformanceRecorder>();}
  void Awake(){collections=GC.CollectionCount(0);}
  void Update(){float delta=Time.unscaledDeltaTime;if(delta>0){frames++;seconds+=delta;maxFrame=Mathf.Max(maxFrame,delta);}}
  void OnApplicationQuit(){var report=new Report{seconds=seconds,averageFps=seconds>0?frames/seconds:0,slowestFrameFps=maxFrame>0?1/maxFrame:0,managedBytes=GC.GetTotalMemory(false),gen0Collections=GC.CollectionCount(0)-collections,operations=DevelopmentMetrics.Samples.Select(s=>new Entry{name=s.name,milliseconds=s.milliseconds,managedDelta=s.managedDelta,textureBytes=s.textureBytes}).ToArray()};string folder=Path.Combine(Application.persistentDataPath,"DevelopmentV2","Measurements");Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+".json"),JsonUtility.ToJson(report,true));}
 }
}
#endif
