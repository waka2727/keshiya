using UnityEditor;
using UnityEngine;
namespace Keshiya.Editor {
 public sealed class DevelopmentToolsWindow:EditorWindow {
  Vector2 scroll;
  [MenuItem("Keshiya/Development/Save and Performance")]
  public static void Open()=>GetWindow<DevelopmentToolsWindow>("Development Tools");
  void OnGUI(){var game=Object.FindFirstObjectByType<PrototypeGame>();EditorGUILayout.HelpBox("Save v2 is opt-in during development. The release keeps ET01 session-only behavior. Save/load only on the job board; no active-paper resume.",MessageType.Info);
   using(new EditorGUI.DisabledScope(!Application.isPlaying||game==null||game.Session.Phase!=WorkPhase.Board)){
    if(GUILayout.Button("Save progress (development slot)")){SaveStore.Save(SaveStore.DefaultDirectory,SaveStore.Capture(game.Progress,game.Economy));Debug.Log("Save v2: saved");}
    if(GUILayout.Button("Load progress (development slot)")){var result=SaveStore.Load(SaveStore.DefaultDirectory);if(result.data==null)EditorUtility.DisplayDialog("Save v2",result.status,"OK");else if(EditorUtility.DisplayDialog("Save v2","Replace the current session with the saved progress?","Load","Cancel")){if(result.fromBackup)SaveStore.RecoverBackup(SaveStore.DefaultDirectory);game.Progress.ImportSave(result.data.progress);game.Economy.ImportSave(result.data.economy);game.Progress.Apply(game.Modifiers);game.SelectOwned(game.Tools.selectedId);}}
   }
   using(new EditorGUI.DisabledScope(!Application.isPlaying||game==null)){
    if(GUILayout.Button("Save settings (separate development file)"))SaveStore.SaveSettings(SaveStore.DefaultDirectory,new UserSettings{volume=AudioListener.volume,width=Screen.width,height=Screen.height,fullscreen=Screen.fullScreen,contactGuide=game.ContactGuide.Enabled});
    if(GUILayout.Button("Load settings")){var settings=SaveStore.LoadSettings(SaveStore.DefaultDirectory);AudioListener.volume=settings.volume;game.ContactGuide.Enabled=settings.contactGuide;Screen.SetResolution(settings.width,settings.height,settings.fullscreen);}
   }
   DevelopmentMetrics.Enabled=EditorGUILayout.Toggle("Capture performance",DevelopmentMetrics.Enabled);
   EditorGUILayout.HelpBox("Managed delta is live managed-heap change, not precise GC allocation. Texture bytes use Unity runtime memory. Samples are capped at 100 and never serialized into progress.",MessageType.None);
   if(GUILayout.Button("Clear measurements"))DevelopmentMetrics.Samples.Clear();scroll=EditorGUILayout.BeginScrollView(scroll);foreach(var s in DevelopmentMetrics.Samples)EditorGUILayout.LabelField(s.name,$"{s.milliseconds:F2} ms | managed Δ {s.managedDelta:N0} B | texture {s.textureBytes:N0} B");EditorGUILayout.EndScrollView();if(Application.isPlaying)Repaint();
  }
 }
}
