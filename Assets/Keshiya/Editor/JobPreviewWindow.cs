using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Keshiya.Editor {
 public sealed class JobPreviewWindow:EditorWindow {
  string search="";Vector2 listScroll,artScroll;JobDefinition selected;Texture2D preview;int layer,state;bool eraseOverlay,protectOverlay;float zoom=.4f;string statistics="";
  static readonly string[] Names={"Composite","Paper","Protected","Erasable","EraseMask","ProtectMask","CompletePreview"};
  [MenuItem("Keshiya/Content Pipeline/3 Job Preview")]
  public static void Open(){var window=GetWindow<JobPreviewWindow>("Job Preview");if(window.position.width<900)window.position=new Rect(80,80,1100,760);window.minSize=new Vector2(900,600);}
  void OnDisable(){if(preview!=null)DestroyImmediate(preview);}
  void OnGUI(){EditorGUILayout.LabelField("Development only — art inspection (not gameplay simulation)",EditorStyles.boldLabel);search=EditorGUILayout.TextField("Job / Client / Paper",search);
   EditorGUILayout.BeginHorizontal();listScroll=EditorGUILayout.BeginScrollView(listScroll,GUILayout.Width(250));
   foreach(var g in AssetDatabase.FindAssets("t:JobDefinition")){var j=AssetDatabase.LoadAssetAtPath<JobDefinition>(AssetDatabase.GUIDToAssetPath(g));string label=j.id+" "+j.displayName+" "+j.client+" "+j.PaperName;if(label.IndexOf(search,StringComparison.OrdinalIgnoreCase)<0)continue;if(GUILayout.Button(label,GUILayout.Height(45))){selected=j;Rebuild();}}
   EditorGUILayout.EndScrollView();EditorGUILayout.BeginVertical();if(selected!=null&&selected.artwork!=null){EditorGUI.BeginChangeCheck();layer=EditorGUILayout.Popup("Layer",layer,Names);state=EditorGUILayout.Popup("Erasure coverage",state,new[]{"0%","50%","95%","100%"});eraseOverlay=EditorGUILayout.Toggle("Erase overlay",eraseOverlay);protectOverlay=EditorGUILayout.Toggle("Protect overlay",protectOverlay);if(EditorGUI.EndChangeCheck())Rebuild();zoom=EditorGUILayout.Slider("Preview zoom",zoom,.1f,2);EditorGUILayout.HelpBox(statistics,MessageType.None);artScroll=EditorGUILayout.BeginScrollView(artScroll);if(preview!=null)GUILayout.Label(preview,GUILayout.Width(preview.width*zoom),GUILayout.Height(preview.height*zoom));EditorGUILayout.EndScrollView();}else EditorGUILayout.HelpBox("Select a layered job. Legacy geometric jobs have no artwork layers.",MessageType.Info);EditorGUILayout.EndVertical();EditorGUILayout.EndHorizontal();
  }
  public static Texture2D Read(Texture2D src){var t=new Texture2D(2,2,TextureFormat.RGBA32,false);t.LoadImage(System.IO.File.ReadAllBytes(AssetDatabase.GetAssetPath(src)));return t;}
  public static Texture2D Compose(JobArtworkData a,int layer,float erased,bool eOverlay,bool pOverlay,out string stats){
   var inputs=new[]{a.paper,a.protectedImage,a.erasable,a.eraseMask,a.protectMask,a.completePreview};var copies=inputs.Select(Read).ToArray();try{var pixels=copies.Select(t=>t.GetPixels()).ToArray();var result=new Color[pixels[0].Length];int e=0,p=0,overlap=0;double total=pixels[3].Sum(c=>(double)c.r),removed=0;
    for(int i=0;i<result.Length;i++){bool et=pixels[3][i].r>0,pt=pixels[4][i].r>0;if(et)e++;if(pt)p++;if(et&&pt)overlap++;float coverage=0;if(et&&removed<total*erased){coverage=Mathf.Clamp01((float)((total*erased-removed)/pixels[3][i].r));removed+=pixels[3][i].r*coverage;}
     Color c=pixels[0][i];if(layer==0){c=Color.Lerp(c,pixels[1][i],pixels[1][i].a);c=Color.Lerp(c,pixels[2][i],pixels[2][i].a*(1-coverage));}else c=pixels[layer-1][i];if(eOverlay&&et)c=Color.Lerp(c,Color.cyan,.45f);if(pOverlay&&pt)c=Color.Lerp(c,Color.magenta,.45f);result[i]=c;
    }stats=$"Erase {e:N0} px ({100f*e/result.Length:F2}%) / Protect {p:N0} px ({100f*p/result.Length:F2}%) / Overlap {overlap:N0} px\nCoverage scan uses mask weight, not a uniform fade.";var output=new Texture2D(a.paper.width,a.paper.height,TextureFormat.RGBA32,false);output.SetPixels(result);output.Apply();return output;
   }finally{foreach(var t in copies)DestroyImmediate(t);}
  }
  void Rebuild(){if(preview!=null)DestroyImmediate(preview);preview=null;if(selected?.artwork==null)return;preview=Compose(selected.artwork,layer,new[]{0f,.5f,.95f,1f}[state],eraseOverlay,protectOverlay,out statistics);}
 }
}
