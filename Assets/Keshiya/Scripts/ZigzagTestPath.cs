using UnityEngine;
namespace Keshiya
{
    // Deterministic test input only; never attached to the normal game.
    public static class ZigzagTestPath
    {
        public static Vector2 Trace(CrumbEconomy e,EraserDefinition tool,float advance=4,int halves=40,int subdivisions=6,
            float width=.36f,float halfSeconds=.14f,float graphitePerUnit=.012f,ContactMode mode=ContactMode.Face,
            Vector2 origin=default,Vector2 forward=default,Vector2 oscillation=default,bool irregular=false)
        {
            if(forward==Vector2.zero)forward=Vector2.right;if(oscillation==Vector2.zero)oscillation=Vector2.up;
            Vector2 a=origin-oscillation*width*.5f;
            for(int leg=0;leg<halves;leg++){
                float wobble=irregular?1+.15f*Mathf.Sin(leg*1.7f):1;
                Vector2 b=origin+forward*(advance*(leg+1)/halves)+oscillation*((leg%2==0?1:-1)*width*.5f*wobble);
                float time=halfSeconds*(irregular?1+.35f*Mathf.Sin(leg*.9f):1);
                Vector2 from=a;
                for(int part=1;part<=subdivisions;part++){
                    Vector2 to=Vector2.Lerp(a,b,part/(float)subdivisions);float distance=Vector2.Distance(from,to),dt=time/subdivisions;
                    e.Stroke(from,to,distance/dt,tool,mode,tool.crumbAmount,distance*graphitePerUnit,dt);from=to;
                }
                a=b;
            }
            return a;
        }
    }
}
