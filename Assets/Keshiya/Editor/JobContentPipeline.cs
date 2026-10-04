using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Keshiya.Editor {
 [Serializable] public sealed class JobSource {
  public string id,title,clientId,clientName,clientLetter,workInstruction,completionMessage,paperAsset,writingAsset,runtimeAsset,artworkFolder,visibility,developmentNotes;
  public int baseReward,difficulty;public float requiredErasure,referenceSeconds,maxOverlapRatio=.1f;public bool precision,includeInExternalTest;
  public JobContentMetadata metadata;
 }
 public static class JobContentPipeline {
  public const string Source="Tools/JobPipeline/Source",Texts="Assets/Keshiya/Resources/Texts/ja.json";
  public static readonly string[] Layers={"Paper","Protected","Erasable","EraseMask","ProtectMask","CompletePreview","InitialPreview"};
  public static JobSource[] Definitions()=>Directory.GetFiles(Source,"*.json").OrderBy(x=>x).Select(p=>JsonUtility.FromJson<JobSource>(File.ReadAllText(p))).ToArray();
  public static T Ensure<T>(string path) where T:ScriptableObject {Directory.CreateDirectory(Path.GetDirectoryName(path));AssetDatabase.Refresh();var a=AssetDatabase.LoadAssetAtPath<T>(path);if(a==null){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,path);}return a;}
  [MenuItem("Keshiya/Content Pipeline/1 Import Definitions")]
  public static void Import(){
   var specs=Definitions();if(specs.GroupBy(s=>s.id).Any(g=>g.Count()>1))throw new InvalidDataException("Duplicate Job ID");
   // Japanese table is authoritative after initial export. Existing entries are never normalized.
   var entries=File.Exists(Texts)?TextCatalog.Parse(File.ReadAllText(Texts)):new Dictionary<string,string>();
   var released=new List<JobDefinition>();
   foreach(var s in specs){
    bool existing=s.visibility=="existing",dev=s.visibility=="development";
    if(dev&&!s.runtimeAsset.Contains("/Editor/"))throw new InvalidDataException("DEV must live under Editor");
    var j=existing?AssetDatabase.LoadAssetAtPath<JobDefinition>(s.runtimeAsset):Ensure<JobDefinition>(s.runtimeAsset);
    if(j==null)throw new InvalidDataException("Missing frozen job "+s.id);
    if(!existing){
     foreach(string name in Layers){string path=s.artworkFolder+"/"+name+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var imp=AssetImporter.GetAtPath(path) as TextureImporter;if(imp==null)throw new InvalidDataException("Missing "+name);bool mask=name.EndsWith("Mask");imp.textureType=TextureImporterType.Default;imp.sRGBTexture=!mask;imp.isReadable=mask;imp.mipmapEnabled=false;imp.npotScale=TextureImporterNPOTScale.None;imp.maxTextureSize=4096;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.filterMode=FilterMode.Bilinear;imp.wrapMode=TextureWrapMode.Clamp;imp.alphaIsTransparency=!mask;imp.SaveAndReimport();}
     var a=Ensure<JobArtworkData>(Path.ChangeExtension(s.runtimeAsset,null)+"-Artwork.asset");
     Func<string,Texture2D> tex=n=>AssetDatabase.LoadAssetAtPath<Texture2D>(s.artworkFolder+"/"+n+".png");a.paper=tex("Paper");a.protectedImage=tex("Protected");a.erasable=tex("Erasable");a.eraseMask=tex("EraseMask");a.protectMask=tex("ProtectMask");a.completePreview=tex("CompletePreview");a.initialPreview=tex("InitialPreview");a.worldSize=new Vector2(5,5f*a.paper.height/a.paper.width);a.sourceFolder="Tools/JobPipeline/Source/"+s.id+".json";EditorUtility.SetDirty(a);
     j.id=s.id;j.displayName=s.title;j.client=s.clientName;j.clientQuote=s.clientLetter;j.instruction=s.workInstruction;j.summary=s.workInstruction;j.thankYou=s.completionMessage;j.baseReward=s.baseReward;j.difficulty=s.difficulty;j.requiredErasure=s.requiredErasure;j.referenceSeconds=s.referenceSeconds;j.precision=s.precision;j.paper=AssetDatabase.LoadAssetAtPath<PaperDefinition>(s.paperAsset);j.writing=AssetDatabase.LoadAssetAtPath<WritingInstrumentDefinition>(s.writingAsset);j.artwork=a;
    }
    if(!existing){j.externalTest=s.includeInExternalTest;if(s.metadata!=null)j.content=s.metadata;}
    j.content=j.content??new JobContentMetadata();j.content.clientId=s.clientId;j.content.developmentOnly=dev;j.content.developmentNotes=s.developmentNotes;j.content.maxOverlapRatio=s.maxOverlapRatio;
    var fields=new[]{"TITLE","CLIENT_LETTER","INSTRUCTION","COMPLETION"};var values=new[]{j.displayName,j.clientQuote,j.instruction,j.thankYou};var ids=fields.Select(f=>"JOB_"+s.id+"_"+f).ToArray();
    for(int i=0;i<ids.Length;i++)if(!entries.ContainsKey(ids[i]))entries.Add(ids[i],values[i]??"");
    j.content.titleTextId=ids[0];j.content.letterTextId=ids[1];j.content.instructionTextId=ids[2];j.content.completionTextId=ids[3];
    // Materialize into the existing fields: UI and runtime need no second text path.
    j.displayName=entries[ids[0]];j.clientQuote=entries[ids[1]];j.instruction=entries[ids[2]];j.thankYou=entries[ids[3]];
    EditorUtility.SetDirty(j);if(!dev)released.Add(j);
   }
   foreach(var guid in AssetDatabase.FindAssets("t:EraserDefinition")){var a=AssetDatabase.LoadAssetAtPath<EraserDefinition>(AssetDatabase.GUIDToAssetPath(guid));ExportStringFields(a,"ITEM_"+a.name,entries);}
   foreach(var guid in AssetDatabase.FindAssets("t:SkillCatalog")){var a=AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));ExportStringFields(a,"SKILL_"+a.name,entries);}
   Directory.CreateDirectory(Path.GetDirectoryName(Texts));File.WriteAllText(Texts,JsonUtility.ToJson(new TextTable{entries=entries.OrderBy(x=>x.Key).Select(x=>new TextEntry{id=x.Key,value=x.Value}).ToArray()},true));
   var registry=Ensure<JobRegistry>("Assets/Keshiya/Resources/JobRegistry.asset");registry.released=released.ToArray();EditorUtility.SetDirty(registry);AssetDatabase.SaveAssets();AssetDatabase.Refresh();TextCatalog.Reset();
  }
  static void ExportStringFields(UnityEngine.Object a,string prefix,Dictionary<string,string> entries){if(a==null)return;var so=new SerializedObject(a);var p=so.GetIterator();while(p.NextVisible(true))if(p.propertyType==SerializedPropertyType.String&&!p.propertyPath.StartsWith("m_")){string id=prefix+"_"+p.propertyPath; if(!entries.ContainsKey(id))entries[id]=p.stringValue;else p.stringValue=entries[id];}so.ApplyModifiedPropertiesWithoutUndo();}
  [MenuItem("Keshiya/Content Pipeline/2 Validate All Jobs")]
  public static void ValidateAll(){var errors=Validate();Directory.CreateDirectory("TestResults-Foundation");File.WriteAllLines("TestResults-Foundation/editor-validation.txt",errors.Count==0?new[]{"PASS all job definitions"}:errors);if(errors.Count>0)throw new InvalidDataException(string.Join("\n",errors));Debug.Log("Job Content Validator: PASS");}
  public static List<string> Validate(){var errors=new List<string>();var seen=new HashSet<string>();
   var allJobs=AssetDatabase.FindAssets("t:JobDefinition").Select(g=>AssetDatabase.LoadAssetAtPath<JobDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();foreach(var group in allJobs.GroupBy(j=>j.id))if(string.IsNullOrWhiteSpace(group.Key)||group.Count()>1)errors.Add("Duplicate or empty registered Job ID "+group.Key);
   foreach(var s in Definitions()){
   if(!seen.Add(s.id))errors.Add("Duplicate Job ID "+s.id);
   if(string.IsNullOrWhiteSpace(s.clientId)||!s.clientId.StartsWith("CLIENT_"))errors.Add(s.id+" invalid Client ID");
   var j=AssetDatabase.LoadAssetAtPath<JobDefinition>(s.runtimeAsset);if(j==null){errors.Add(s.id+" runtime reference missing");continue;}
   if(j.paper==null||j.writing==null||j.artwork==null){errors.Add(s.id+" broken references");continue;}
   if(j.baseReward<0||j.difficulty<1||j.difficulty>5)errors.Add(s.id+" invalid reward/difficulty");
   foreach(var id in new[]{j.content.titleTextId,j.content.letterTextId,j.content.instructionTextId,j.content.completionTextId})try{if(string.IsNullOrWhiteSpace(TextCatalog.Require(id)))errors.Add(s.id+" empty text "+id);}catch(Exception){errors.Add(s.id+" missing text "+id);}
   var requiredTextures=new[]{j.artwork.paper,j.artwork.protectedImage,j.artwork.erasable,j.artwork.eraseMask,j.artwork.protectMask,j.artwork.completePreview,j.artwork.initialPreview};
   bool missingTexture=false;for(int n=0;n<requiredTextures.Length;n++)if(requiredTextures[n]==null){errors.Add(s.id+" missing runtime texture "+Layers[n]);missingTexture=true;}
   if(missingTexture)continue;
   int w=j.artwork.paper.width,h=j.artwork.paper.height;foreach(string layer in Layers){string p=s.artworkFolder+"/"+layer+".png";var t=AssetDatabase.LoadAssetAtPath<Texture2D>(p);if(t==null){errors.Add(s.id+" missing "+layer);continue;}if(t.width!=w||t.height!=h)errors.Add(s.id+" resolution mismatch "+layer);var imp=AssetImporter.GetAtPath(p) as TextureImporter;bool mask=layer.EndsWith("Mask");if(imp==null||imp.sRGBTexture==mask||imp.mipmapEnabled||imp.npotScale!=TextureImporterNPOTScale.None||imp.textureCompression!=TextureImporterCompression.Uncompressed||imp.wrapMode!=TextureWrapMode.Clamp||(mask&&!imp.isReadable))errors.Add(s.id+" incorrect import "+layer);}
   var refs=new[]{j.artwork.paper,j.artwork.protectedImage,j.artwork.erasable,j.artwork.eraseMask,j.artwork.protectMask,j.artwork.completePreview,j.artwork.initialPreview};
   for(int n=0;n<Layers.Length;n++)if(AssetDatabase.GetAssetPath(refs[n])!=s.artworkFolder+"/"+Layers[n]+".png")errors.Add(s.id+" mismatched layer reference "+Layers[n]);
   ValidatePixels(s,errors);
   if(j.content.developmentOnly&&!s.runtimeAsset.Contains("/Editor/"))errors.Add(s.id+" DEV asset location");
  }return errors;}
  static void ValidatePixels(JobSource s,List<string> errors){Color32[] erase=null,protect=null;int width=0,height=0;foreach(string name in Layers){string path=s.artworkFolder+"/"+name+".png";if(!File.Exists(path))continue;var raw=new Texture2D(2,2,TextureFormat.RGBA32,false);try{if(!raw.LoadImage(File.ReadAllBytes(path))){errors.Add(s.id+" unreadable PNG "+name);continue;}if(width==0){width=raw.width;height=raw.height;}else if(width!=raw.width||height!=raw.height)errors.Add(s.id+" source canvas mismatch "+name);var px=raw.GetPixels32();bool mask=name.EndsWith("Mask");if((mask||name=="Paper"||name=="CompletePreview")&&px.Any(c=>c.a!=255))errors.Add(s.id+" invalid alpha "+name);if((name=="Protected"||name=="Erasable")&&px.All(c=>c.a==255))errors.Add(s.id+" overlay must have transparent background "+name);if(mask&&px.Any(c=>c.r!=c.g||c.r!=c.b))errors.Add(s.id+" colored mask "+name);if(name=="EraseMask")erase=px;if(name=="ProtectMask")protect=px;}finally{UnityEngine.Object.DestroyImmediate(raw);}}
   if(erase==null||protect==null||erase.Length!=protect.Length)return;int e=0,p=0,o=0;for(int i=0;i<erase.Length;i++){if(erase[i].r>0)e++;if(protect[i].r>0)p++;if(erase[i].r>0&&protect[i].r>0)o++;}if(e==0)errors.Add(s.id+" empty EraseMask");if(s.precision&&p==0)errors.Add(s.id+" unexpected empty ProtectMask");if(o/(float)Math.Max(1,e)>s.maxOverlapRatio)errors.Add(s.id+" abnormal mask overlap");
  }
 }
}
