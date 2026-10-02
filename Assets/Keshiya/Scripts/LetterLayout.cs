using UnityEngine;
namespace Keshiya
{
    // The same world-space paths seed texture ink and provide deterministic test strokes.
    // The protected A ends at x=0; the incorrect B begins at x=.14.
    public static class LetterLayout
    {
        public static readonly Vector2[][] Protected={
            new[]{new Vector2(-.50f,-.38f),new Vector2(-.25f,.38f),new Vector2(0,-.38f)},
            new[]{new Vector2(-.38f,-.05f),new Vector2(-.12f,-.05f)}};
        public static readonly Vector2[][] Target={
            new[]{new Vector2(.14f,-.38f),new Vector2(.14f,.38f),new Vector2(.39f,.38f),new Vector2(.53f,.28f),new Vector2(.53f,.12f),new Vector2(.40f,0),new Vector2(.14f,0)},
            new[]{new Vector2(.40f,0),new Vector2(.55f,-.12f),new Vector2(.55f,-.26f),new Vector2(.40f,-.38f),new Vector2(.14f,-.38f)}};
        public static void Raster(float[] ink,int width,int height,Vector2 size,Vector2[][] paths,bool pencil,float widthScale=1)
        {
            float radius=.022f*Mathf.Max(.5f,widthScale);
            foreach(var path in paths)for(int segment=1;segment<path.Length;segment++)
            {
                Vector2 a=path[segment-1],b=path[segment],ab=b-a;
                int x0=Mathf.Max(0,Mathf.FloorToInt(((Mathf.Min(a.x,b.x)-radius)/size.x+.5f)*width));
                int x1=Mathf.Min(width-1,Mathf.CeilToInt(((Mathf.Max(a.x,b.x)+radius)/size.x+.5f)*width));
                int y0=Mathf.Max(0,Mathf.FloorToInt(((Mathf.Min(a.y,b.y)-radius)/size.y+.5f)*height));
                int y1=Mathf.Min(height-1,Mathf.CeilToInt(((Mathf.Max(a.y,b.y)+radius)/size.y+.5f)*height));
                for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)
                {
                    var p=new Vector2(((x+.5f)/width-.5f)*size.x,((y+.5f)/height-.5f)*size.y);
                    float d=Vector2.Distance(p,a+ab*Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude));
                    float value=Mathf.Clamp01((radius-d)/radius*3);
                    if(pencil)value*=.75f+.15f*Mathf.PerlinNoise(x*.73f,y*.81f);
                    ink[y*width+x]=Mathf.Max(ink[y*width+x],value);
                }
            }
        }
    }
}
