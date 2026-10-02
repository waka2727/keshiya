using UnityEngine;
namespace Keshiya
{
    [CreateAssetMenu(menuName="Keshiya/Tool Catalog")]
    public sealed class ToolCatalog : ScriptableObject { public EraserDefinition[] tools;
  public EraserDefinition Find(string id)=>System.Array.Find(tools,x=>x.id==id);
  public int Index(string id)=>System.Array.FindIndex(tools,x=>x.id==id); }
}
