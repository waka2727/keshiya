using UnityEngine;
namespace Keshiya {
 [CreateAssetMenu(menuName="Keshiya/Playtest Config")] public sealed class PlaytestConfig:ScriptableObject {
  public float maximumZoom=4,zoomStep=1.2f,fiveToolRewardMultiplier=1.15f;
 }
}
