using UnityEngine;
namespace Keshiya {
 [System.Serializable] public sealed class JobPath { public ContactMode workMode=ContactMode.Corner;public float workAngle; public Vector2[] points; public JobPath(Vector2[] p){points=p;} }
 public static class JobArtwork {
  public static void Raster(float[] ink,int w,int h,Vector2 size,JobPath[] paths,bool pencil,float widthScale=1){
   if(paths==null)return;foreach(var path in paths)if(path!=null&&path.points!=null&&path.points.Length>=2)LetterLayout.Raster(ink,w,h,size,new[]{path.points},pencil,widthScale);
  }
 }
}
