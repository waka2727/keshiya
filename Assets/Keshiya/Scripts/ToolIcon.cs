using UnityEngine;
namespace Keshiya
{
    // Tiny cached code-drawn icons: no external art or frame-by-frame texture work.
    public static class ToolIcon
    {
        public static Texture2D Create(EraserDefinition tool)
        {
            if(!string.IsNullOrEmpty(tool.iconMotif))return CollectionIcon.Create(tool);
            const int w=96,h=44;var texture=new Texture2D(w,h,TextureFormat.RGBA32,false);
            texture.name="Tool icon: "+tool.displayName;texture.filterMode=FilterMode.Bilinear;
            var pixels=new Color[w*h];float exponent=tool.crumbCohesion>.7f?3:6;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                float u=Mathf.Abs((x-47.5f)/(42*tool.iconScale.x)),v=Mathf.Abs((y-21.5f)/(15*tool.iconScale.y));
                if(Mathf.Pow(u,exponent)+Mathf.Pow(v,exponent)>1)continue;
                Color color=x>48?tool.labelColor:tool.bodyColor;
                color*=Mathf.Lerp(.76f,1.06f,y/(float)h);color.a=1;
                if(tool.crumbCohesion<.2f&&(x*17+y*31)%19<3)color=Color.Lerp(color,Color.black,.28f);
                pixels[y*w+x]=color;
            }
            texture.SetPixels(pixels);texture.Apply(false);return texture;
        }
    }
}
