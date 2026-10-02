using UnityEngine;
namespace Keshiya
{
    [CreateAssetMenu(menuName="Keshiya/Crumb Economy Config")]
    public sealed class CrumbEconomyConfig:ScriptableObject
    {
        [HideInInspector] public float centimetersPerUnit=6, gramsPerUnit=.06f; // 0.3 migration only
        public float longMinimum=5;
        [HideInInspector] public float turnDegrees=42, speedJumpRatio=1.7f, minimumSpeed=.4f, maximumSpeed=8;
        [HideInInspector] public float idleReleaseSeconds=.45f, brittleLength=2;
        public float rescueSeconds=6, maximumLength=80;
        public float priceCurve=.08f, ballPricePerGram=20, worldUnitsPerCm=.10f;
        public int paperPieceCapacity=24, inventoryCapacity=128;
        public float faceGrowth=1, edgeGrowth=.85f, cornerGrowth=0;
        [Header("0.3.1 rubbing motion")]
        public float zigMinWidth=.07f, zigMaxWidth=.38f, zigWidthTolerance=.4f, idealZigWidth=.28f;
        public float reversalHysteresis=.022f, zigMinHalfPeriod=.025f, zigMaxHalfPeriod=.8f, zigPeriodTolerance=.5f;
        public float centerDeadZone=.04f, minCenterAdvance=.008f, centerTurnDegrees=40, centerTurnTolerance=40, maxCenterSpeed=.9f;
        public float zigExtremeSpeed=22, zigSpeedTolerance=10, contactGap=.08f, teleportDistance=1.2f, historySeconds=3;
        public int turnsToGrow=3;
        public float centimetersPerCenterUnit=10, graphiteCmPerArea=600, graphiteCarryCm=3, gramsPerGraphiteArea=12, gramsPerCm=.003f;
        public float sandZigLength=8, zigIdleReleaseSeconds=.9f;
        public float Growth(ContactMode mode)=>mode==ContactMode.Face?faceGrowth:mode==ContactMode.Edge?edgeGrowth:cornerGrowth;
        public int LongPrice(float length,float valueMultiplier)=>length<longMinimum?0:Mathf.Max(1,Mathf.FloorToInt(Mathf.Pow(Mathf.Max(0,length-3),2)*priceCurve*valueMultiplier));
        public int BallPrice(float grams)=>Mathf.Max(0,Mathf.FloorToInt(grams*ballPricePerGram));
        public string Rank(float length)=>length>=30?"EXCELLENT":length>=20?"VERY LONG":length>=10?"LONG":"短め";
    }
}

