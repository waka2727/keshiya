using System;
namespace Keshiya {
 public enum JobReactionKind { Success,LowQuality,HeavyPaperDamage,HeavyProtectedDamage,VeryLowErase,UnexpectedDestruction }
 [Serializable] public sealed class JobReaction { public JobReactionKind kind; public string textId; }
 public static class JobReactions {
  public static JobReactionKind Classify(RewardResult r) {
   if(r.severe&&r.protectedMajor)return JobReactionKind.UnexpectedDestruction;
   if(r.protectedMajor)return JobReactionKind.HeavyProtectedDamage;
   if(r.severe)return JobReactionKind.HeavyPaperDamage;
   if(r.erased<.5f)return JobReactionKind.VeryLowErase;
   if(WorkSession.Grade(r)=="C"||WorkSession.Grade(r)=="B")return JobReactionKind.LowQuality;
   return JobReactionKind.Success;
  }
  public static string Resolve(JobDefinition job,RewardResult r){
   var kind=Classify(r);var entry=Array.Find(job.reactions??Array.Empty<JobReaction>(),x=>x!=null&&x.kind==kind);
   if(entry==null&&kind!=JobReactionKind.Success)entry=Array.Find(job.reactions??Array.Empty<JobReaction>(),x=>x!=null&&x.kind==JobReactionKind.LowQuality);
   if(entry!=null&&!string.IsNullOrEmpty(entry.textId)){try{return TextCatalog.Require(entry.textId);}catch(System.Collections.Generic.KeyNotFoundException){}}
   return kind==JobReactionKind.Success?(string.IsNullOrEmpty(job.thankYou)?"おつかれさまでした。":job.thankYou):"大きな損傷や消し残しが残りました。";
  }
 }
}
