using UnityEngine;
namespace Keshiya
{
    [CreateAssetMenu(menuName="Keshiya/Job Definition")]
    public sealed class JobDefinition : ScriptableObject
    {
        public string id, client="ご近所の方", summary, clientQuote, feature, recommended="普通消し", unlockRank;
        [Range(1,5)] public int difficulty=1;
        public int baseReward=1000;
        public float referenceSeconds=120, requiredErasure=.95f;
        public PaperDefinition paper;
        public WritingInstrumentDefinition writing;
        [Range(.5f,1.5f)] public float pressure=1;
        public string category="通常";
        public float Fragility=>paper!=null?paper.fragility:paperFragility;
        public string PaperName=>paper!=null?paper.displayName:paperName;
        public string WritingName=>writing!=null?writing.displayName:"HB鉛筆";
        public float CrumbModifier=>writing!=null?writing.crumbModifier:1;
        public EraserDefinition suppliedTool;
        [Range(.01f,1)] public float suppliedRemaining=.12f;
        [TextArea] public string thankYou;
        public JobArtworkData artwork;public bool externalTest;
        public int layoutVersion; public int detailResolution;
        public float targetLineScale=1,protectedLineScale=1,targetOpacity=1;
        public Color targetTint=new Color(.36f,.43f,.51f);
        public bool clearDraftStyle;
        public bool customDrawing;
        public JobPath[] targetPaths=new JobPath[0], protectedPaths=new JobPath[0];
        public string displayName="ねこの落書き";
        [TextArea] public string instruction="鉛筆の落書きを95%以上消す";
        public bool precision;
        public bool letterCorrection;
        [Range(1,3)] public float paperFragility=1;
        public int contentVersion;
        public float minorProtectionLoss=.05f;
        public string paperName="普通紙";
        public float targetOffset=.14f, targetHalfLength=1.35f, targetHalfWidth=.018f;
        public float protectedHalfWidth=.018f, protectedHalfLength=1.6f;
        public float protectedSensitivity=.45f, majorLoss=.35f, majorLocalLoss=.95f;
        public int protectionBonus=400;
    }
}
