using UnityEngine;
namespace Keshiya
{
    public sealed class ProtectedDrawing : IStrokeLayer
    {
        public readonly float[] Ink;
        readonly float[] original;
        readonly JobDefinition job;
        double total,remaining;
        public float OriginalAt(int i)=>original[i];
        public float Loss => total<=0?0:Mathf.Clamp01((float)(1-remaining/total));
        public float PeakLoss {get;private set;}
        public bool Major => Loss>=job.majorLoss || (!job.letterCorrection && PeakLoss>=job.majorLocalLoss);
        public string Grade => Loss<=0?"無傷":Loss<=job.minorProtectionLoss?"軽微":Major?"重大ミス":"評価低下";
        public ProtectedDrawing(int width,int height,Vector2 paperSize,JobDefinition definition)
        {
            job=definition;Ink=new float[width*height];original=new float[Ink.Length];
            if(job.artwork!=null){Ink=JobArtworkData.ReadMask(job.artwork.protectMask);System.Array.Copy(Ink,original,Ink.Length);foreach(float value in Ink)total+=value;remaining=total;return;}
            if(job.customDrawing){JobArtwork.Raster(Ink,width,height,paperSize,job.protectedPaths,false,job.protectedLineScale);System.Array.Copy(Ink,original,Ink.Length);foreach(float value in Ink)total+=value;remaining=total;return;}
            if(job.letterCorrection){LetterLayout.Raster(Ink,width,height,paperSize,LetterLayout.Protected,false);System.Array.Copy(Ink,original,Ink.Length);foreach(float value in Ink)total+=value;remaining=total;return;}
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            {
                var point=new Vector2(((x+.5f)/width-.5f)*paperSize.x,((y+.5f)/height-.5f)*paperSize.y);
                if(Mathf.Abs(point.x)>job.protectedHalfWidth || Mathf.Abs(point.y)>job.protectedHalfLength)continue;
                int i=y*width+x;Ink[i]=original[i]=1;total++;
            }
            remaining=total;
        }
        public void ApplyContact(int pixelIndex,float strength)
        {
            float removed=Mathf.Min(Ink[pixelIndex],Mathf.Max(0,strength)*job.protectedSensitivity);
            Ink[pixelIndex]-=removed;remaining-=removed;
            if(original[pixelIndex]>0)PeakLoss=Mathf.Max(PeakLoss,1-Ink[pixelIndex]/original[pixelIndex]);
        }
    }
}
