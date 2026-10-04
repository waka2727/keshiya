using System;
namespace Keshiya {
 [Serializable] public sealed class JobContentMetadata {
  public string clientId,appearancePhase="unassigned",developmentNotes;
  public bool recurringClient,developmentOnly;
  public string[] recommendedTags=new string[0],specialRules=new string[0],storyFlags=new string[0],unlockConditions=new string[0],followUpJobIds=new string[0];
  public float precisionDifficulty,paperRisk,eraseVolume,protectDensity,maxOverlapRatio=.1f;
  public string expSettings="existing-default";
  public string titleTextId,letterTextId,instructionTextId,completionTextId;
 }
}
