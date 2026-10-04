using UnityEngine;
namespace Keshiya
{
    public enum CrumbState { Growing, OnPaper, Collected, Sold, BlownAway }
    [System.Serializable] public sealed class CrumbPiece
    {
        [field:SerializeField] public long Id {get;internal set;}
        [field:SerializeField] public string Source {get;internal set;}
        [field:SerializeField] public string ToolDefinitionId {get;internal set;}
        [field:SerializeField] public string ToolInstanceId {get;internal set;}
        [field:SerializeField] public float LengthCm {get;internal set;}
        [field:SerializeField] public float MassGrams {get;internal set;}
        [field:SerializeField] public float Thickness {get;internal set;}
        [field:SerializeField] public float Tension {get;internal set;}
        [field:SerializeField] public float ValueMultiplier {get;internal set;}
        [field:SerializeField] public Color Color {get;internal set;}
        [field:SerializeField] public Vector2 End {get;internal set;}
        [field:SerializeField] public Vector2 Direction {get;internal set;}
        [field:SerializeField] public CrumbState State {get;internal set;}
        [field:SerializeField] public bool WasBroken {get;internal set;}
        [field:SerializeField] public string EndReason {get;internal set;}
        public CrumbPiece Copy()=>(CrumbPiece)MemberwiseClone();
        public int Price(CrumbEconomyConfig config)=>config.LongPrice(LengthCm,ValueMultiplier);
    }
    public sealed class CrumbBall
    {
        [field:SerializeField] public float Grams {get;private set;}
        [field:SerializeField] public float LifetimeAddedGrams {get;private set;}
        public float DiameterCm=>Grams<=0?0:Mathf.Pow(6*Grams/Mathf.PI,1f/3f);
        internal System.Action CaptureRestart(){float g=Grams,l=LifetimeAddedGrams;return ()=>{Grams=g;LifetimeAddedGrams=l;};}
        internal void RestoreSave(float grams,float lifetime){Grams=grams;LifetimeAddedGrams=lifetime;}
        internal void Add(float grams){if(grams<=0)return;Grams+=grams;LifetimeAddedGrams+=grams;}
        internal float Empty(){float old=Grams;Grams=0;return old;}
    }
}
