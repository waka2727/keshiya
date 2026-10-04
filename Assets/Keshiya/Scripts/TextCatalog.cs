using System;
using System.Collections.Generic;
using UnityEngine;
namespace Keshiya {
 [Serializable] public sealed class TextEntry {public string id,value;}
 [Serializable] public sealed class TextTable {public string language="ja";public TextEntry[] entries=new TextEntry[0];}
 public static class TextCatalog {
  static Dictionary<string,string> cached;
  public static Dictionary<string,string> Parse(string json){var t=JsonUtility.FromJson<TextTable>(json);if(t==null||t.entries==null)throw new FormatException("Text table missing");var d=new Dictionary<string,string>();foreach(var e in t.entries){if(e==null||string.IsNullOrWhiteSpace(e.id)||e.value==null||d.ContainsKey(e.id))throw new FormatException("Duplicate or invalid text ID");d.Add(e.id,e.value);}return d;}
  public static string Require(string id){if(cached==null){var file=Resources.Load<TextAsset>("Texts/ja");if(file==null)throw new InvalidOperationException("Missing ja text catalog");cached=Parse(file.text);}if(!cached.TryGetValue(id,out var value))throw new KeyNotFoundException("Missing Text ID: "+id);return value;}
  public static void Reset(){cached=null;}
 }
}
