using UnityEngine;
namespace Keshiya
{
    public enum CrumbState { Growing, OnPaper, Collected, Sold, BlownAway }
    public sealed class CrumbPiece
    {
        public long Id {get;internal set;}
        public string Source {get;internal set;}
        public string ToolDefinitionId {get;internal set;}
        public string ToolInstanceId {get;internal set;}
        public float LengthCm {get;internal set;}
        public float MassGrams {get;internal set;}
        public float Thickness {get;internal set;}
        public float Tension {get;internal set;}
        public float ValueMultiplier {get;internal set;}
        public Color Color {get;internal set;}
        public Vector2 End {get;internal set;}
        public Vector2 Direction {get;internal set;}
        public CrumbState State {get;internal set;}
        public bool WasBroken {get;internal set;}
        public string EndReason {get;internal set;}
        public CrumbPiece Copy()=>(CrumbPiece)MemberwiseClone();
        public int Price(CrumbEconomyConfig config)=>config.LongPrice(LengthCm,ValueMultiplier);
    }
    public sealed class CrumbBall
    {
        public float Grams {get;private set;}
        public float LifetimeAddedGrams {get;private set;}
        public float DiameterCm=>Grams<=0?0:Mathf.Pow(6*Grams/Mathf.PI,1f/3f);
        internal System.Action CaptureRestart(){float g=Grams,l=LifetimeAddedGrams;return ()=>{Grams=g;LifetimeAddedGrams=l;};}
        internal void Add(float grams){if(grams<=0)return;Grams+=grams;LifetimeAddedGrams+=grams;}
        internal float Empty(){float old=Grams;Grams=0;return old;}
    }
}
