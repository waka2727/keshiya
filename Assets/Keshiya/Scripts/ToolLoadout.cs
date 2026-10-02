using System.Collections.Generic;
namespace Keshiya {
 [System.Serializable] public sealed class ToolLoadout {
  public bool limited=false;public int capacity=5;public List<string> selected=new List<string>();
  public bool Contains(string id)=>!limited||selected.Contains(id);
  public void Clean(ToolInventory inventory){selected.RemoveAll(id=>inventory.Find(id)==null);}
  public bool Add(OwnedEraser item){if(selected.Contains(item.instanceId))return true;if(limited&&selected.Count>=capacity)return false;selected.Add(item.instanceId);return true;}
  public bool Toggle(OwnedEraser item){if(selected.Remove(item.instanceId))return true;return Add(item);}
  public void Prepare(ToolInventory inventory){Clean(inventory);if(selected.Count==0)foreach(var item in inventory.items)if(!item.state.Exhausted){Add(item);if(limited&&selected.Count>=capacity)break;}}
  public void AdmitRescue(OwnedEraser item,ToolInventory inventory){Clean(inventory);if(selected.Count>=capacity)selected.RemoveAll(id=>inventory.Find(id).state.Exhausted);Add(item);}
 }
}
