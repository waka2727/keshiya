using UnityEngine;
namespace Keshiya
{
    /// <summary>Separate receiver for erasable graphite or future protected ink.</summary>
    public interface IStrokeLayer { void ApplyContact(int pixelIndex,float strength); }
    public struct StrokeResult
    {
        public float erasedFraction, addedDamage, protectedLoss, graphiteArea, preciseArea, preciseFraction;
        public Vector2 damagePoint;
        public bool Damaged => addedDamage>0;
    }
}
