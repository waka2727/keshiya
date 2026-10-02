using UnityEngine;

namespace Keshiya
{
    public sealed class Paper : System.IDisposable
    {
        public readonly Drawing Drawing;
        public readonly Texture2D Texture;
        public readonly float[] Damage;
        // Null in 0.1.1: protected marks are a separate receiver, never Drawing.Ink.
        public IStrokeLayer ProtectedContent { get; set; }
        public ProtectedDrawing Protection {get;private set;}
        readonly float[] wear, last, grain;
        readonly bool[] nearProtected;
        readonly float initialPreciseArea;
        public float RevealStrength {get;private set;}
        public void Reveal(float strength){RevealStrength=Mathf.Clamp01(strength);dirtyMinX=dirtyMinY=0;dirtyMaxX=Width-1;dirtyMaxY=Height-1;Refresh();}
        readonly Color[] basePaper;
        readonly Color32[] pixels;
        readonly PrototypeConfig config;
        readonly float grainAmount;
        readonly JobDefinition definition;public readonly Vector2 Size;public bool Layered=>definition?.artwork!=null;int dirtyMinX,dirtyMinY,dirtyMaxX,dirtyMaxY;
        double damageSum;
        public float PeakDamage { get; private set; }
        public float AverageDamage => (float)(damageSum / Damage.Length);
        public float TearThreshold=>definition?.paper?.id=="old"?.28f:.65f;
        public bool Torn=>PeakDamage>=TearThreshold;
        public bool ShowFinished {get;private set;}
        public void SetFinished(bool value){ShowFinished=value;Refresh();}
        public Color TargetColor=>definition!=null&&definition.clearDraftStyle?definition.targetTint:definition?.writing!=null?definition.writing.traceColor:new Color(.22f,.23f,.23f);
        public bool Severe => PeakDamage >= config.severeDamageThreshold;
        public int Width => Drawing.Width;
        public int Height => Drawing.Height;

        public Paper(PrototypeConfig c, float erasureGrain = .10f,JobDefinition job=null)
        {
            config = c;definition=job;Size=job?.artwork!=null?job.artwork.worldSize:c.paperSize;
            grainAmount = Mathf.Clamp(erasureGrain, 0, .2f);
            int w=job?.artwork!=null?job.artwork.eraseMask.width:job!=null&&job.detailResolution>0?job.detailResolution:c.resolution, h=job?.artwork!=null?job.artwork.eraseMask.height:Mathf.RoundToInt(w*Size.y/Size.x);
            Drawing = new Drawing(w,h,job,Size);
            if(job!=null&&job.precision){Protection=new ProtectedDrawing(w,h,Size,job);ProtectedContent=Protection;}
            nearProtected=new bool[w*h];
            if(Protection!=null){int rx=Mathf.CeilToInt(.22f*w/Size.x),ry=Mathf.CeilToInt(.22f*h/Size.y);int stride=w+1;var sum=new int[(w+1)*(h+1)];for(int y=0;y<h;y++)for(int x=0;x<w;x++)sum[(y+1)*stride+x+1]=(Protection.Ink[y*w+x]>0?1:0)+sum[y*stride+x+1]+sum[(y+1)*stride+x]-sum[y*stride+x];for(int y=0;y<h;y++)for(int x=0;x<w;x++){int x0=Mathf.Max(0,x-rx),x1=Mathf.Min(w,x+rx+1),y0=Mathf.Max(0,y-ry),y1=Mathf.Min(h,y+ry+1);nearProtected[y*w+x]=sum[y1*stride+x1]-sum[y0*stride+x1]-sum[y1*stride+x0]+sum[y0*stride+x0]>0;}}

            // Normalize the finite precision EXP budget against only relevant graphite.
            // Broad strokes elsewhere cannot earn it; blank revisits have zero removal.
            double nearMass=0;for(int i=0;i<nearProtected.Length;i++)if(nearProtected[i])nearMass+=Drawing.Ink[i];
            initialPreciseArea=(float)(nearMass*Size.x*Size.y/(w*h));
            Damage = new float[w*h]; wear = new float[w*h]; last = new float[w*h];
            grain = new float[w*h]; basePaper = new Color[w*h]; pixels = new Color32[w*h];
            // Cache noise once; no Perlin calls or allocations during rubbing/refresh.
            for (int y=0; y<h; y++) for (int x=0; x<w; x++)
            {
                int i = y*w+x;
                float noise=Layered?.5f:Mathf.PerlinNoise(x*.7f,y*.7f);
                basePaper[i] = new Color(.94f,.91f,.83f) * (.97f+.03f*noise);
                if(job!=null&&job.letterCorrection){
                    float py=((y+.5f)/h-.5f)*Size.y,px=((x+.5f)/w-.5f)*Size.x;
                    basePaper[i]=new Color(.97f,.94f,.88f)*(.985f+.015f*noise);
                    if(Mathf.Abs(px)<2.8f && py<-.52f && py>-1.9f && Mathf.Repeat(py+.55f,.55f)<.012f)
                        basePaper[i]=Color.Lerp(basePaper[i],new Color(.55f,.64f,.69f),.22f);
                }
                uint hash = (uint)(x*73856093 ^ y*19349663);
                hash ^= hash >> 13;
                grain[i] = (hash % 1024) / 1023f * 2 - 1;
            }
            Texture = new Texture2D(w,h,TextureFormat.RGBA32,false,Layered) { name="Live graphite and paper" };
            dirtyMaxX=w-1;dirtyMaxY=h-1;Refresh();
        }

        public StrokeResult Stroke(Vector2 from, Vector2 to, float speed, float time,
            EraserDefinition eraser, PerformanceModifiers modifiers) =>
            Stroke(from,to,speed,time,eraser,modifiers,eraser.Contact(modifiers));

        public StrokeResult Stroke(Vector2 from, Vector2 to, float speed, float time,
            EraserDefinition eraser, PerformanceModifiers modifiers, ContactFootprint footprint,EraserState state=null)
        {
            var result = new StrokeResult();
            float distance = Vector2.Distance(from,to);
            if (distance < .00001f) return result;
            float erasedBefore = Drawing.Erased;
            float protectedBefore=Protection?.Loss??0;
            int steps = Mathf.Max(1,Mathf.CeilToInt(distance / footprint.SampleSpacing));
            float ds = distance/steps;
            float efficiency = Mathf.Lerp(.45f,1,Mathf.Clamp01(speed/Mathf.Max(.01f,config.optimalSpeed)))
                + Mathf.Clamp01((speed-config.dangerousSpeed)/Mathf.Max(.01f,config.dangerousSpeed))*.65f;
            var stock=definition?.paper;
            float resistance=stock!=null?Mathf.Max(.1f,stock.frictionResistance):1;
            float localResistance=stock!=null?Mathf.Max(.1f,stock.localWearResistance):1;
            float removal=definition?.writing!=null?definition.writing.Removal(definition.pressure):1;
            float damageSpeed=speed*eraser.SpeedRisk(definition)*(stock!=null?stock.surfaceDrag:1);
            float fast = Mathf.Clamp01((damageSpeed-config.dangerousSpeed)/Mathf.Max(.01f,config.dangerousSpeed));
            float aggressive = Mathf.Clamp01((damageSpeed-config.wearStartSpeed)/Mathf.Max(.01f,config.dangerousSpeed-config.wearStartSpeed));
            Vector2 bounds = footprint.Bounds;
            float maxIncrease = 0;
            for (int s=1; s<=steps; s++)
            {
                Vector2 p = Vector2.Lerp(from,to,(float)s/steps);
                Vector2 center = p+footprint.Offset;
                int x0 = Mathf.Max(0,Mathf.FloorToInt(((center.x-bounds.x)/Size.x+.5f)*Width));
                int x1 = Mathf.Min(Width-1,Mathf.CeilToInt(((center.x+bounds.x)/Size.x+.5f)*Width));
                int y0 = Mathf.Max(0,Mathf.FloorToInt(((center.y-bounds.y)/Size.y+.5f)*Height));
                int y1 = Mathf.Min(Height-1,Mathf.CeilToInt(((center.y+bounds.y)/Size.y+.5f)*Height));
                for (int y=y0; y<=y1; y++) for (int x=x0; x<=x1; x++)
                {
                    Vector2 point = new Vector2(((x+.5f)/Width-.5f)*Size.x,((y+.5f)/Height-.5f)*Size.y);
                    float weight = footprint.Weight(point-p);
                    if (weight<=0) continue;
                    dirtyMinX=Mathf.Min(dirtyMinX,x);dirtyMaxX=Mathf.Max(dirtyMaxX,x);dirtyMinY=Mathf.Min(dirtyMinY,y);dirtyMaxY=Mathf.Max(dirtyMaxY,y);
                    int i = y*Width+x;
                    float strength = ds*eraser.Power(modifiers)*eraser.Pickup(definition)*efficiency*weight;
                    float before=Drawing.Ink[i];
                    float specialPower=state?.special.Power(eraser)??1;
                    float adsorption=eraser.specialKind==SpecialToolKind.Kneaded?Mathf.Lerp(1.8f,.7f,Mathf.Clamp01(before/.6f)):1;
                    Drawing.ApplyContact(i,strength*removal*specialPower*adsorption*(1+grain[i]*grainAmount));
                    if(nearProtected[i])result.preciseArea+=(before-Drawing.Ink[i])*Size.x*Size.y/(Width*Height);
                    ProtectedContent?.ApplyContact(i,strength*eraser.protectedInkAbrasion*Mathf.Lerp(modifiers.protectedGlance*eraser.equipment.protectedGlance,1,Mathf.Pow(weight,config.protectedGlanceExponent)));
                    // Local buildup comes from fast rubbing; gentle work accrues no hidden debt.
                    wear[i] = Mathf.Max(0,wear[i]-Mathf.Max(0,time-last[i])*config.wearRecovery)+ds*weight*aggressive/localResistance;
                    last[i] = time;
                    float increase = ds*weight*(fast*config.highSpeedDamage/resistance
                        + Mathf.Max(0,wear[i]-config.wearThreshold)*config.wearDamage*aggressive)
                        *eraser.DamageMultiplier(modifiers)*(state?.special.Damage(eraser)??1)*(stock!=null?stock.DamageScale:1);
                    float next = Mathf.Clamp01(Damage[i]+increase);
                    float added = next-Damage[i];
                    damageSum += added;
                    result.addedDamage += added;
                    if (added>maxIncrease) { maxIncrease=added; result.damagePoint=point; }
                    Damage[i]=next;
                    PeakDamage=Mathf.Max(PeakDamage,next);
                }
            }
            result.preciseFraction=initialPreciseArea>0?Mathf.Clamp01(result.preciseArea/initialPreciseArea):0;
            result.erasedFraction=Drawing.Erased-erasedBefore;
            result.graphiteArea=result.erasedFraction*Drawing.InitialMass*Size.x*Size.y/(Width*Height);
            result.protectedLoss=(Protection?.Loss??0)-protectedBefore;
            return result;
        }

        public void Refresh()
        {
            if(Layered){if(dirtyMaxX<dirtyMinX)return;int w=dirtyMaxX-dirtyMinX+1,h=dirtyMaxY-dirtyMinY+1;var update=new Color32[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++){int i=(y+dirtyMinY)*Width+x+dirtyMinX;float original=Drawing.OriginalInk[i];float protect=Protection?.OriginalAt(i)??0;update[y*w+x]=new Color32((byte)(255*(original>0?Mathf.Clamp01(Drawing.Ink[i]/original):1)),(byte)(255*(protect>0?Mathf.Clamp01(Protection.Ink[i]/protect):1)),(byte)(255*Damage[i]),(byte)(255*RevealStrength));}Texture.SetPixels32(dirtyMinX,dirtyMinY,w,h,update);Texture.Apply(false);dirtyMinX=Width;dirtyMinY=Height;dirtyMaxX=dirtyMaxY=-1;return;}

            for (int y=0;y<Height;y++) for (int x=0;x<Width;x++)
            {
                int i=y*Width+x;
                float damage=Damage[i];
                Color paper=Color.Lerp(basePaper[i],Color.white,damage*.85f);
                if(damage>.04f && (x+2*y)%11<2) paper=Color.Lerp(paper,new Color(.61f,.55f,.46f),damage*.6f);
                if(damage>.85f) paper=Color.Lerp(paper,new Color(.32f,.25f,.19f),(damage-.85f)/.15f);
                pixels[i]=Color.Lerp(paper,TargetColor,ShowFinished?0:Mathf.Clamp01(Drawing.Ink[i])*(definition?.targetOpacity??1));
                if(!ShowFinished&&RevealStrength>0&&Drawing.Ink[i]>config.senseMinimumInk)pixels[i]=Color.Lerp(pixels[i],new Color(.76f,.49f,.16f),RevealStrength);
                if(Protection!=null)pixels[i]=Color.Lerp(pixels[i],new Color(.035f,.04f,.045f),Protection.Ink[i]);
                if(damage>=TearThreshold){float fissure=Mathf.Abs(Mathf.Sin(x*.12f+y*.065f)*3+(x%23)-11);if(fissure<1.4f)pixels[i]=new Color(.20f,.15f,.10f);else if(fissure<2.5f)pixels[i]=new Color(.99f,.97f,.87f);}
            }
            Texture.SetPixels32(pixels);
            Texture.Apply(false);
        }
        public void Dispose()
        {
            if(Application.isPlaying) Object.Destroy(Texture); else Object.DestroyImmediate(Texture);
        }
    }
}
