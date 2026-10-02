using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Keshiya.Editor {
 public static class ExternalArtworkChecks {
  public static void Run(){int count=0;var log=new StringBuilder();Action<bool,string> check=(ok,name)=>{count++;log.AppendLine((ok?"PASS ":"FAIL ")+name);if(!ok)throw new Exception(name);};
   var game=UnityEngine.Object.FindFirstObjectByType<PrototypeGame>();var jobs=game.Jobs.Where(j=>j.externalTest).ToArray();check(jobs.Length==8,"Eight external documents");check(game.Jobs.Count(j=>!j.externalTest)==20,"Original twenty preserved");check(game.ArtworkShader!=null,"Layered shader bound in player");
   foreach(var job in jobs){var art=job.artwork;check(art.Valid,job.id+" valid layers");foreach(var tex in new[]{art.paper,art.protectedImage,art.erasable,art.eraseMask,art.protectMask,art.completePreview,art.initialPreview})check(tex.width==1240&&tex.height==1754,job.id+" aligned "+tex.name);var mask=JobArtworkData.ReadMask(art.eraseMask);check(mask.Any(v=>v>0),job.id+" real target");var protection=JobArtworkData.ReadMask(art.protectMask);check(protection.Any(v=>v>0)==job.precision,job.id+" independent protection");check(art.testStrokes.Length>0,job.id+" QA strokes");check(job.requiredErasure==.95f,job.id+" 95 percent");check(job.paper!=null&&job.writing!=null,job.id+" materials retained");check(File.Exists(art.sourceFolder+"/Paper.png"),job.id+" source preserved");}
   foreach(int index in new[]{1,5}){float faceLoss=0,cornerLoss=0;foreach(var mode in new[]{ContactMode.Face,ContactMode.Corner}){using(var paper=new Paper(game.Config,game.Feel.erasureGrain,jobs[index])){var tool=game.Catalog.tools[mode==ContactMode.Face?0:3];var skills=new PerformanceModifiers();var state=new EraserState();foreach(var path in jobs[index].artwork.testStrokes)paper.Stroke(path.points[0],path.points[1],2,0,tool,skills,tool.Contact(skills,mode,state,0),state);if(mode==ContactMode.Face)faceLoss=paper.Protection.Loss;else cornerLoss=paper.Protection.Loss;}}check(faceLoss>cornerLoss,jobs[index].id+" fine corner reduces collateral relative to broad face");}
   check(jobs[2].clientQuote.Contains("おnがい")&&jobs[2].thankYou.EndsWith("おやすみなさう。"),"Manga intentional typos preserved");check(jobs[4].suppliedTool!=null,"Named eraser supply");Directory.CreateDirectory("TestResults-External");File.WriteAllText("TestResults-External/editor.txt",log+"Checks="+count+"; Failures=0\n");Debug.Log("EXTERNAL_EDITOR_OK "+count);
  }
 }
}
