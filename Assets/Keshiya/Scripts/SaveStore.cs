using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
namespace Keshiya {
 [Serializable] public sealed class GameSave {
  public int SaveVersion=2;public ProgressSnapshot progress;public EconomySnapshot economy;
  public string[] storyFlags=new string[0];public string[] unlockedJobIds=new string[0];
 }
 // ET01 had session state, not disk saves. This explicit interchange DTO is the v1 migration input.
 [Serializable] public sealed class LegacySessionExport {public int SaveVersion=1;public ProgressSnapshot progress;public EconomySnapshot economy;}
 [Serializable] public sealed class UserSettings {public int SettingsVersion=1;public float volume=1;public int width=1280,height=720;public bool contactGuide=true,fullscreen;}
 [Serializable] sealed class SaveEnvelope {public string format="KeshiyaSave",payload,sha256;}
 [Serializable] sealed class VersionProbe {public int SaveVersion;}
 public sealed class SaveLoadResult {public GameSave data;public string status;public bool fromBackup;public bool RequiresReset=>data==null;}
 public static class SaveStore {
  public const int CurrentVersion=2;
  public static string DefaultDirectory=>Path.Combine(Application.persistentDataPath,"DevelopmentV2");
  public static GameSave Capture(PlayerProgress p,CrumbEconomy e)=>new GameSave{progress=p.ExportSave(),economy=e.ExportSave()};
  public static GameSave Decode(string json){var v=JsonUtility.FromJson<VersionProbe>(json);if(v==null||v.SaveVersion<1||v.SaveVersion>CurrentVersion)throw new InvalidDataException("Unsupported save version");GameSave s;
   if(v.SaveVersion==1){var old=JsonUtility.FromJson<LegacySessionExport>(json);s=new GameSave{progress=old.progress,economy=old.economy};}else s=JsonUtility.FromJson<GameSave>(json);
   Validate(s);return s;
  }
  public static void Validate(GameSave s){
   if(s==null||s.SaveVersion!=2||s.progress==null||s.economy==null||string.IsNullOrWhiteSpace(s.progress.playerJson))throw new InvalidDataException("Missing progress");
   var p=JsonUtility.FromJson<PlayerProgress>(s.progress.playerJson);var w=s.progress.work;var e=s.economy;
   if(p==null||p.wallet==null||p.tools==null||p.banks==null||p.banks.Length!=3||p.levels==null||p.levels.Length!=11||p.statistics==null||w==null||w.history==null||w.recorded==null||s.progress.formed==null||s.progress.collected==null||s.progress.completed==null||e.inventory==null)throw new InvalidDataException("Incomplete save");
   if(p.wallet.Balance<0||p.wallet.JobIncome<0||p.wallet.CrumbIncome<0||p.levels.Any(n=>n<0||n>10)||p.banks.Any(b=>b==null||!FinitePositive(b.total)||!FinitePositive(b.remainder)||b.availableSP<0||b.earnedSP<0))throw new InvalidDataException("Invalid progression");
   if(p.tools.items==null||p.tools.records==null||p.tools.purchases==null||p.tools.loadout==null||p.tools.items.Any(t=>t==null||string.IsNullOrEmpty(t.instanceId)||string.IsNullOrEmpty(t.definitionId)||t.state==null||!FinitePositive(t.state.UsedUnits)||!FinitePositive(t.state.CornerSharpness)||t.state.CornerSharpness>1)||p.tools.items.Select(t=>t.instanceId).Distinct().Count()!=p.tools.items.Count)throw new InvalidDataException("Invalid inventory");
   if(w.completedCount<0||w.qualityPoints<0||w.history.Any(h=>h==null||string.IsNullOrEmpty(h.jobId)||h.record==null)||w.history.Select(h=>h.jobId).Distinct().Count()!=w.history.Length)throw new InvalidDataException("Invalid history");
   if(e.jobId<1||e.nextId<0||e.jobSales<0||e.overflowPrice<0||e.overflowCount<0||new[]{e.loose,e.produced,e.sold,e.discarded,e.best,e.longest,e.bonus,e.ball,e.ballLifetime,e.overflowMass,e.overflowLongest}.Any(f=>!FinitePositive(f))||e.inventory.Any(c=>c==null||c.Id<0||!FinitePositive(c.LengthCm)||!FinitePositive(c.MassGrams)))throw new InvalidDataException("Invalid crumb ledger");
  }
  static bool FinitePositive(float f)=>!float.IsNaN(f)&&!float.IsInfinity(f)&&f>=0;
  static string Hash(string s){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-","").ToLowerInvariant();}
  static string Pack(string payload)=>JsonUtility.ToJson(new SaveEnvelope{payload=payload,sha256=Hash(payload)},true);
  static string Unpack(string raw){var e=JsonUtility.FromJson<SaveEnvelope>(raw);if(e==null||e.format!="KeshiyaSave"||e.payload==null||e.sha256!=Hash(e.payload))throw new InvalidDataException("Save checksum mismatch");return e.payload;}
  static void WriteAtomic(string path,string payload,Action<string> validate){
   Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp",backup=path+".bak";byte[] bytes=Encoding.UTF8.GetBytes(Pack(payload));
   using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)){stream.Write(bytes,0,bytes.Length);stream.Flush(true);}validate(Unpack(File.ReadAllText(temp)));
   if(File.Exists(path)){
    // Do not replace a valid backup with a corrupt primary. Caller must first explicitly recover/reset it.
    validate(Unpack(File.ReadAllText(path)));File.Replace(temp,path,backup);
   }else File.Move(temp,path);
  }
  public static void Save(string folder,GameSave s){Validate(s);WriteAtomic(Path.Combine(folder,"progress.json"),JsonUtility.ToJson(s),j=>Decode(j));}
  public static SaveLoadResult Load(string folder){string path=Path.Combine(folder,"progress.json");foreach(var candidate in new[]{path,path+".bak"})if(File.Exists(candidate))try{return new SaveLoadResult{data=Decode(Unpack(File.ReadAllText(candidate))),fromBackup=candidate!=path,status=candidate==path?"loaded":"backup-recovered"};}catch(Exception ex)when(ex is IOException||ex is ArgumentException||ex is InvalidOperationException){Debug.LogWarning("Save v2: invalid progress candidate (details withheld)");}return new SaveLoadResult{status=File.Exists(path)||File.Exists(path+".bak")?"reset-confirmation-required":"new-game"};}
  public static void RecoverBackup(string folder){var r=Load(folder);if(!r.fromBackup||r.data==null)throw new InvalidOperationException("No valid backup");string path=Path.Combine(folder,"progress.json"),temp=path+".recovery";File.Copy(path+".bak",temp,true);if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);}
  public static void SaveSettings(string folder,UserSettings s){ValidateSettings(s);WriteAtomic(Path.Combine(folder,"settings.json"),JsonUtility.ToJson(s),j=>ValidateSettings(JsonUtility.FromJson<UserSettings>(j)));}
  static void ValidateSettings(UserSettings s){if(s==null||s.SettingsVersion!=1||!FinitePositive(s.volume)||s.volume>1||s.width<320||s.height<200)throw new InvalidDataException("Invalid settings");}
  public static UserSettings LoadSettings(string folder){foreach(var suffix in new[]{"",".bak"})try{var s=JsonUtility.FromJson<UserSettings>(Unpack(File.ReadAllText(Path.Combine(folder,"settings.json"+suffix))));ValidateSettings(s);return s;}catch(Exception ex)when(ex is IOException||ex is ArgumentException||ex is InvalidOperationException){}return new UserSettings();}
 }
}
