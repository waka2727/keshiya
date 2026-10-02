using UnityEditor;
using UnityEngine;

namespace Keshiya.Editor
{
    public static class Prototype02Setup
    {
        static T Asset<T>(string name,out bool created) where T:ScriptableObject
        {
            string path="Assets/Keshiya/Resources/"+name+".asset";
            var asset=AssetDatabase.LoadAssetAtPath<T>(path);created=asset==null;
            if(created){asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);}
            return asset;
        }
        public static void Apply(PrototypeGame root)
        {
            var c=root.Config;
            if(c.tuningVersion<2){c.dangerousSpeed=16;c.wearStartSpeed=12;c.tuningVersion=2;EditorUtility.SetDirty(c);}
            var edge=Asset<EraserContactProfile>("Edge",out bool newEdge);
            if(newEdge){edge.halfSizeMultiplier=new Vector2(1,.17f);EditorUtility.SetDirty(edge);}
            var corner=Asset<EraserContactProfile>("Corner",out bool newCorner);
            if(newCorner){corner.halfSizeMultiplier=Vector2.one*.17f;EditorUtility.SetDirty(corner);}
            var soft=Asset<EraserDefinition>("SoftEraser",out bool newSoft);
            if(newSoft){soft.displayName="やわらかい消しゴム";soft.erasePower=.82f;soft.paperDamageMultiplier=.18f;soft.crumbAmount=1.35f;soft.durability=85;soft.wearRate=.15f;soft.cornerWearRate=.012f;soft.highSpeedRisk=.55f;soft.reactionMultiplier=.65f;soft.bodyColor=new Color(.86f,.94f,.81f);soft.labelColor=new Color(.27f,.52f,.35f);soft.crumbColor=new Color(.67f,.73f,.62f);}
            var sand=Asset<EraserDefinition>("SandEraser",out bool newSand);
            if(newSand){sand.displayName="砂消しゴム";sand.erasePower=3.6f;sand.paperDamageMultiplier=5;sand.radius=.235f;sand.crumbAmount=.65f;sand.crumbSizeMultiplier=.6f;sand.durability=110;sand.wearRate=.08f;sand.cornerWearRate=.005f;sand.highSpeedRisk=1.6f;sand.reactionMultiplier=1.25f;sand.bodyColor=new Color(.78f,.57f,.47f);sand.labelColor=new Color(.47f,.20f,.13f);sand.crumbColor=new Color(.48f,.30f,.21f);}
            foreach(var e in new[]{root.Eraser,soft,sand})
            {
                if(e.contactProfile==null)e.contactProfile=root.Eraser.contactProfile;
                if(e.edgeProfile==null)e.edgeProfile=edge;
                if(e.cornerProfile==null)e.cornerProfile=corner;
                EditorUtility.SetDirty(e);
            }
            var catalog=Asset<ToolCatalog>("ToolCatalog",out bool newCatalog);
            if(newCatalog){catalog.tools=new[]{root.Eraser,soft,sand};EditorUtility.SetDirty(catalog);}root.Catalog=catalog;
            var normal=Asset<JobDefinition>("NormalJob",out bool newNormal);
            if(newNormal){normal.displayName="ねこの落書き";normal.instruction="広い面で、気持ちよくゴシゴシ。\n鉛筆の落書きを95%以上消しましょう。";EditorUtility.SetDirty(normal);}
            var precision=Asset<JobDefinition>("PrecisionJob",out bool newPrecision);
            if(newPrecision){precision.precision=true;precision.displayName="中央の黒い線を残す";precision.instruction="黒い中央線を残し、両脇の鉛筆線を消す。\n角がおすすめ。Rで本体を線の外へ。";EditorUtility.SetDirty(precision);}
            root.Jobs=new[]{normal,precision};
        }
    }
}
