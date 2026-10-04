namespace Keshiya {
 // Only teaches controls. No job/tool recommendations or long-crumb recipe.
 public sealed class TutorialHints {
  static string[] steps;
  public static string[] Steps=>steps??(steps=new[]{
   TextCatalog.Require("TUTORIAL_BASIC_01"),
   TextCatalog.Require("TUTORIAL_BASIC_02"),
   TextCatalog.Require("TUTORIAL_BASIC_03"),
   TextCatalog.Require("TUTORIAL_BASIC_04"),
   TextCatalog.Require("TUTORIAL_BASIC_05"),
   TextCatalog.Require("TUTORIAL_BASIC_06"),
   TextCatalog.Require("TUTORIAL_BASIC_07")
  });
  public int Index {get;private set;} public bool Done {get;private set;} public bool Enabled=true;float elapsed;
  public string Text=>Done?"":Steps[Index];
  public void Next(){if(Done)return;elapsed=0;if(Index+1>=Steps.Length)Done=true;else Index++;}
  public void Tick(float delta,bool erased){if(!Enabled||Done)return;elapsed+=delta;if(elapsed>=18&&(Index!=0||erased||elapsed>=30))Next();}
  public void Skip(){Done=true;}
  public void Replay(){Index=0;elapsed=0;Done=false;Enabled=true;}
 }
}