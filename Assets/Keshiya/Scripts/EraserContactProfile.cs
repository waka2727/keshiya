using UnityEngine;
namespace Keshiya
{
    [CreateAssetMenu(menuName="Keshiya/Contact Profile")]
    public sealed class EraserContactProfile : ScriptableObject
    {
        [Tooltip("Multipliers of the existing eraser radius, not absolute sizes.")]
        public Vector2 halfSizeMultiplier=new Vector2(1,.8472131f);
        [Range(2,8)] public float exponent=4;
        public Vector2 offsetInRadii;
        public float angleDegrees;
        // Future broad/edge/corner selection replaces this profile, not the Paper code.
        public ContactFootprint Create(float radius) => new ContactFootprint(
            halfSizeMultiplier*radius,exponent,offsetInRadii*radius,angleDegrees);
    }
}
