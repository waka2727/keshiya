using UnityEngine;

namespace Keshiya
{
    public enum ContactMode { Face, Edge, Corner }

    // One instance per owned tool. Switching tools never resets its wear.
    [System.Serializable] public sealed class EraserState
    {
        [field:SerializeField] public float UsedUnits { get; private set; }
        public SpecialToolState special=new SpecialToolState();
        public bool allowExhaustion;
        [SerializeField] bool exhausted;
        public bool Exhausted=>allowExhaustion&&exhausted;
        [field:SerializeField] public float CornerSharpness { get; private set; } = 1;
        [field:SerializeField] public float Travel { get; private set; }
        public float Remaining(EraserDefinition definition, PerformanceModifiers skills)
        {
            float capacity=definition.durability*definition.equipment.durability*skills.durability;
            return Exhausted?0:Mathf.Clamp(1-UsedUnits/Mathf.Max(.01f,capacity),allowExhaustion?0:definition.minimumRemaining,1);
        }
        public void DebugRemaining(float fraction,EraserDefinition definition,PerformanceModifiers skills){if(float.IsNaN(fraction))return;fraction=Mathf.Clamp01(fraction);float capacity=definition.durability*definition.equipment.durability*skills.durability;UsedUnits=capacity*(1-fraction);exhausted=fraction==0;}
        public void DebugSharpness(float fraction){if(!float.IsNaN(fraction))CornerSharpness=Mathf.Clamp01(fraction);}
        public void Use(float distance,ContactMode mode,EraserDefinition definition,PerformanceModifiers skills)
        {
            if(distance<=0||Exhausted||float.IsNaN(distance)||float.IsInfinity(distance))return;
            Travel+=distance;
            float capacity=definition.durability*definition.equipment.durability*skills.durability;
            UsedUnits=Mathf.Min(capacity*(1-(allowExhaustion?0:definition.minimumRemaining)),UsedUnits+distance*definition.wearRate*definition.equipment.wear*skills.wear);
            if(allowExhaustion&&UsedUnits>=capacity)exhausted=true;
            float cornerFactor=mode==ContactMode.Corner?1:mode==ContactMode.Edge?definition.edgeCornerWear:0;
            CornerSharpness=Mathf.Max(Mathf.Min(CornerSharpness,definition.minimumSharpness),CornerSharpness-distance*definition.cornerWearRate*definition.equipment.cornerWear*skills.cornerWear*cornerFactor);
        }
    }
}
