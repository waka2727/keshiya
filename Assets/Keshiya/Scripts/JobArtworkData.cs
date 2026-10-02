using UnityEngine;
namespace Keshiya {
 [CreateAssetMenu(menuName="Keshiya/Job Artwork Data")]
 public sealed class JobArtworkData:ScriptableObject {
  public Texture2D paper,protectedImage,erasable,eraseMask,protectMask,completePreview,initialPreview;
  public Vector2 worldSize=new Vector2(5,7.07258f);
  public string sourceFolder,manifestHash;
  public JobPath[] testStrokes;
  public bool Valid=>paper!=null&&protectedImage!=null&&erasable!=null&&eraseMask!=null&&protectMask!=null&&completePreview!=null&&eraseMask.width==protectMask.width&&eraseMask.height==protectMask.height;
  public static float[] ReadMask(Texture2D texture){var colors=texture.GetPixels32();var values=new float[colors.Length];for(int i=0;i<values.Length;i++)values[i]=colors[i].r/255f;return values;}
  public Material CreateMaterial(Shader shader,Texture2D state){var m=new Material(shader);m.SetTexture("_Paper",paper);m.SetTexture("_Protected",protectedImage);m.SetTexture("_Erasable",erasable);m.SetTexture("_State",state);return m;}
 }
}
