using UnityEditor;
using UnityEngine;
namespace Keshiya.Editor
{
    public static class Prototype021Setup
    {
        public static void Apply(PrototypeGame root)
        {
            var tools=root.Catalog.tools;
            for(int i=0;i<3;i++)
            {
                var e=tools[i];if(e.specialtyVersion>=1)continue;
                e.role=i==0?"万能・いつもの作業":i==1?"薄い便箋・文字のそばに":"速く消せる・紙削れに注意";
                if(i==1){e.thinPaperStress=.15f;e.thinPaperPickup=1.4f;e.protectedInkAbrasion=.35f;e.crumbCohesion=.9f;e.longCrumbPotential=1.6f;}
                if(i==2){e.crumbCohesion=.12f;e.longCrumbPotential=.35f;}
                e.specialtyVersion=1;EditorUtility.SetDirty(e);
            }
            var job=root.Jobs[1];if(job.contentVersion>=1)return;
            job.letterCorrection=true;job.paperFragility=2;job.paperName="薄い便箋";
            job.displayName="手紙の宛名を書き直す";
            job.instruction="宛名を「AB」と書き間違えました。\n右の B だけ消し、左の A を残してください。";
            job.majorLoss=.20f;job.minorProtectionLoss=.05f;job.contentVersion=1;
            EditorUtility.SetDirty(job);
        }
    }
}
