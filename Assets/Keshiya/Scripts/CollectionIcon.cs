using UnityEngine;
namespace Keshiya {
 public static class CollectionIcon {
  public static Texture2D Create(EraserDefinition d){const int w=96,h=44;var texture=new Texture2D(w,h,TextureFormat.RGBA32,false);texture.name=d.displayName;var pixels=new Color[w*h];
   for(int y=0;y<h;y++)for(int x=0;x<w;x++){float u=(x-47.5f)/43,v=(y-21.5f)/19;bool inside=false;Color color=d.bodyColor;string motif=d.iconMotif;
    if(motif=="toast-collect"){inside=Mathf.Abs(u)<.65f&&v>-.8f&&v<.65f||u*u/.49f+(v-.5f)*(v-.5f)/.16f<1;color=Mathf.Abs(u)>.53f||v<-.64f||v>.67f?new Color(.65f,.37f,.12f):d.bodyColor;}
    else if(motif=="baguette"){inside=u*u+v*v/.36f<1;if((x+y)%20<3)color*=.65f;}
    else if(motif=="croissant"){inside=u*u+v*v<1&&(u*u+(v+.55f)*(v+.55f)>.7f);if((x-y)%20<2)color*=.72f;}
    else if(motif=="carrot"){inside=v<.55f&&v>-.95f&&Mathf.Abs(u)<(v+.98f)*.35f;if(v>.55f&&v<.98f&&Mathf.Abs(u)<.28f){inside=true;color=new Color(.25f,.5f,.18f);}}
    else if(motif=="radish"){inside=u*u/.26f+(v+.1f)*(v+.1f)/.7f<1;if(v>.55f&&v<.98f&&Mathf.Abs(u)<.35f){inside=true;color=new Color(.3f,.55f,.22f);}}
    else if(motif=="cat"||motif=="rabbit"){inside=u*u/.55f+(v+.2f)*(v+.2f)/.45f<1;if(motif=="cat")inside|=v>.2f&&v<.9f&&Mathf.Abs(u)>.2f&&Mathf.Abs(u)<.7f-(v-.2f)*.4f;else inside|=v>.1f&&v<.95f&&Mathf.Abs(Mathf.Abs(u)-.31f)<.12f;if(Mathf.Abs(v)<.06f&&Mathf.Abs(Mathf.Abs(u)-.3f)<.05f)color=Color.black;if(v<-.2f&&v>-.29f&&Mathf.Abs(u)<.07f)color=new Color(.35f,.22f,.2f);}
    else {inside=u*u/.7f+v*v/.7f<1;if(motif=="bread-special"&&v<-.4f)color=new Color(.66f,.46f,.23f);}
    if(inside){color*=Mathf.Lerp(.85f,1.05f,y/(float)h);color.a=1;pixels[y*w+x]=color;}
   }texture.SetPixels(pixels);texture.Apply(false);return texture;
  }
 }
}
