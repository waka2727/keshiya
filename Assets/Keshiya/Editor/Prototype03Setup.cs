using UnityEngine;
using UnityEditor;
namespace Keshiya.Editor
{
    public static class Prototype03Setup
    {
        public static void Apply(PrototypeGame game)
        {
            const string path="Assets/Keshiya/Resources/CrumbEconomyConfig.asset";
            var c=AssetDatabase.LoadAssetAtPath<CrumbEconomyConfig>(path);
            if(c==null){c=ScriptableObject.CreateInstance<CrumbEconomyConfig>();AssetDatabase.CreateAsset(c,path);}game.EconomyConfig=c;
        }
    }
}
