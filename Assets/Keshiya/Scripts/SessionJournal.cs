using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
namespace Keshiya {
 public enum TestEvent { GameStarted,JobSelected,JobStarted,JobCompleted,JobAbandoned,JobRestarted,ToolSelected,ToolUsage,ToolPurchased,SkillPurchased,ProgressSample,RuntimeError,SessionEnded,DataReset }
 [Serializable] public sealed class TestLogEntry {
  public string utc,eventName,job,tool,code,grade;public float seconds,erased,paperDamage,protectDamage,averageFps,slowestFrameFps;public long money;
  public int restartCount,width,height;public bool majorDamage;public float[] modeRatios;public float[] exp;public int[] sp;
 }
 // Whitelisted structured values only. Never writes raw exception messages, stacks, system paths or account names.
 public sealed class SessionJournal:IDisposable {
  readonly object gate=new object();readonly string file;bool closed;string currentJob="";public bool Available {get;private set;}
  public string FileForTests=>file;
  public static string Token(string s)=>!string.IsNullOrEmpty(s)&&Regex.IsMatch(s,"^[A-Za-z0-9_.-]{1,80}$")?s:"redacted";
  public SessionJournal(string folder){try{Directory.CreateDirectory(folder);file=Path.Combine(folder,DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff")+"_"+Guid.NewGuid().ToString("N").Substring(0,8)+".log");File.WriteAllText(file,"",new UTF8Encoding(false));Available=true;}catch{Available=false;}Application.logMessageReceived+=UnityError;AppDomain.CurrentDomain.UnhandledException+=Unhandled;}
  public void Write(TestEvent ev,TestLogEntry e=null){lock(gate){if(closed||!Available)return;e=e??new TestLogEntry();e.utc=DateTime.UtcNow.ToString("O");e.eventName=ev.ToString();if(string.IsNullOrEmpty(e.job))e.job=currentJob;e.job=Token(e.job);e.tool=Token(e.tool);e.code=Token(e.code);e.grade=Token(e.grade);try{File.AppendAllText(file,JsonUtility.ToJson(e)+"\n",new UTF8Encoding(false));}catch{Available=false;}}}
  public void Sample(PrototypeGame g,TestEvent ev,int restarts=0,string tool="",string code=""){
   currentJob=g.Definition.id;var e=new TestLogEntry{job=currentJob,tool=tool,code=code,restartCount=restarts,seconds=g.CurrentJob.Seconds,erased=g.Paper.Drawing.Erased,paperDamage=g.Paper.AverageDamage,protectDamage=g.Paper.Protection?.Loss??0,money=g.Progress.wallet.Balance,majorDamage=g.Paper.Severe||(g.Paper.Protection?.Major??false),exp=new float[3],sp=new int[3]};
   for(int i=0;i<3;i++){e.exp[i]=g.Progress.banks[i].total;e.sp[i]=g.Progress.banks[i].availableSP;}if(g.CurrentJob.Completed)e.grade=WorkSession.Grade(g.CurrentJob.Result);Write(ev,e);
   if(ev==TestEvent.JobCompleted||ev==TestEvent.JobAbandoned||ev==TestEvent.JobRestarted||ev==TestEvent.SessionEnded)foreach(var u in g.CurrentJob.Usage){float sum=u.modeDistance[0]+u.modeDistance[1]+u.modeDistance[2];var ratios=new float[3];for(int i=0;i<3;i++)ratios[i]=sum>0?u.modeDistance[i]/sum:0;Write(TestEvent.ToolUsage,new TestLogEntry{job=currentJob,tool=u.definitionId,seconds=u.contactSeconds,erased=u.erasedFraction,paperDamage=u.paperDamage,protectDamage=u.protectedDamage,modeRatios=ratios});}
  }
  void UnityError(string message,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert)Write(TestEvent.RuntimeError,new TestLogEntry{code=type.ToString()});}
  void Unhandled(object sender,UnhandledExceptionEventArgs args){Write(TestEvent.RuntimeError,new TestLogEntry{code="UnhandledException"});}
  public void Dispose(){lock(gate){if(closed)return;closed=true;Application.logMessageReceived-=UnityError;AppDomain.CurrentDomain.UnhandledException-=Unhandled;}}
 }
}