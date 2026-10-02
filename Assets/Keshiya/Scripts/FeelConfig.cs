using UnityEngine;
namespace Keshiya
{
    [CreateAssetMenu(menuName="Keshiya/Feel Config")]
    public sealed class FeelConfig : ScriptableObject
    {
        [Header("Pressure and movement (visual only)")]
        public float hoverHeight=.30f;
        public Vector2 hoverTiltDegrees=new Vector2(8,-7);
        [Range(.6f,1)] public float pressedHeightScale=.78f;
        [Range(0,5)] public float leanDegrees=2.2f;
        public float leanResponse=24, frictionVibration=.0018f, damageFlashSeconds=.30f;
        [Header("Crumb readability")]
        [Range(.1f,1)] public float crumbDensity=.70f;
        [Range(.2f,1)] public float crumbSize=.65f;
        public float crumbCellSize=.32f;
        public int crumbsPerCell=5;
        public float blowDuration=.55f, blowDistance=3.2f;
        [Header("Pencil grain")]
        [Range(0,.2f)] public float erasureGrain=.10f;
        [Header("Audio: optional clips override synthesized defaults")]
        [Range(0,1)] public float masterVolume=.45f;
        public AudioClip contactClip, frictionClip, damageClip, blowClip;
    }
}
