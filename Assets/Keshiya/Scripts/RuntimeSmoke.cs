using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace Keshiya
{
    // Opt-in standalone integration test. Real input and these tests share ProcessInput.
    public sealed class RuntimeSmoke : MonoBehaviour
    {
        public PrototypeGame Game;
        string directory;
        int checks, failures;
        readonly StringBuilder report=new StringBuilder();
        void Awake(){Application.runInBackground=true;}
        void Check(bool ok,string name)
        {
            checks++;if(!ok)failures++;
            report.AppendLine((ok?"PASS: ":"FAIL: ")+name);
            Debug.Log((ok?"PASS: ":"FAIL: ")+name);
        }
        IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));
            yield return new WaitForSecondsRealtime(.2f);
        }
        IEnumerator Start()
        {
            directory=Path.Combine(Application.dataPath,"../TestResults-legacy");Directory.CreateDirectory(directory);
            var input=Game.Controller;input.ExternalInput=true;
            yield return new WaitForSecondsRealtime(.7f);
            Vector2 start=new Vector2(-.6f,-.1f);
            input.ProcessInput(start,false,true,.016f);
            float hover=Game.Presentation.SoleHeight;
            Check(!input.Pressing&&!Game.Audio.Rubbing,"Hover has no contact or friction sound");
            yield return Capture("01-hover");
            int contactEvents=Game.Audio.ContactEvents;
            input.ProcessInput(start,true,true,.016f);
            Check(input.Pressing&&Game.Presentation.SoleHeight<hover-.2f,"Press lowers the sole immediately");
            Check(Game.Presentation.ContactPosition==start,"Contact location equals mouse location with no lag");
            Check(Game.Audio.ContactEvents==contactEvents+1,"Press triggers a single contact sound event");
            input.ProcessInput(start,true,true,.016f);
            Check(Game.Audio.ContactEvents==contactEvents+1 && !Game.Audio.Rubbing && !Game.CurrentJob.Started,"Stationary hold does not retrigger sound or start erasing");
            yield return Capture("02-pressed");
            input.ProcessInput(start+Vector2.right*.015f,true,true,.03f);
            float quiet=Game.Audio.RubVolume, quietPitch=Game.Audio.RubPitch;
            input.ProcessInput(start+Vector2.right*.105f,true,true,.03f);
            Check(Game.Audio.Rubbing && Game.Audio.RubVolume>quiet && Game.Audio.RubPitch>quietPitch,"Moderate rubbing is fuller than slow rubbing");
            Check(Game.Paper.Drawing.Erased>0&&Game.Paper.PeakDamage==0,"Input pipeline erases safely while rubbing");
            yield return Capture("03-rubbing");
            input.ProcessInput(start,false,true,.016f);
            Check(!input.Pressing&&!Game.Audio.Rubbing&&Mathf.Abs(Game.Presentation.SoleHeight-hover)<.0001f,"Release restores hover and silences friction in the same update");
            float before=Game.Paper.Drawing.Erased;
            input.ProcessInput(start+Vector2.right,true,true,.016f);
            Check(Game.Paper.Drawing.Erased==before,"New press does not erase across the hover gap");
            input.ProcessInput(new Vector2(5,0),true,true,.016f);
            input.ProcessInput(new Vector2(-2,0),true,true,.016f);
            Check(Game.Paper.Drawing.Erased==before,"Leaving and re-entering paper never bridges an erase stroke");
            input.ProcessInput(start,true,false,.016f);
            Check(!input.Pressing&&!Game.Audio.Rubbing&&Game.Presentation.SoleHeight==hover,"Focus loss releases visuals and sound");
            input.ProcessInput(start,true,true,.016f);
            Game.Audio.ToggleMute();input.ProcessInput(start+Vector2.right*.1f,true,true,.03f);
            Check(Game.Audio.Muted&&!Game.Audio.Rubbing,"Mute prevents friction audio");Game.Audio.ToggleMute();
            Game.Restart();
            Check(Game.Paper.Drawing.Erased==0&&Game.Paper.PeakDamage==0&&!Game.CurrentJob.Started&&Game.Crumbs.ActiveCount==0,"Restart resets paper, timing, input and crumbs");

            for(int i=0;i<200;i++)Game.Crumbs.Emit(new Vector2(-.8f,0),new Vector2(.8f,0),1);
            int generated=Game.Crumbs.Spawned;
            Check(generated>1000&&Game.Crumbs.ActiveCount<=Game.Config.crumbCapacity,"Thousands of crumbs stay within the pool cap");
            Check(Game.Crumbs.MaxCellOccupancy<=Game.Feel.crumbsPerCell,"Repeated rubbing respects the local cell density cap");
            yield return Capture("04-crumb-density");
            before=Game.Paper.Drawing.Erased;
            float damageBefore=Game.Paper.AverageDamage;
            Game.BlowCrumbs();
            yield return new WaitForSecondsRealtime(Game.Feel.blowDuration*.45f);
            yield return Capture("05-blowing");
            yield return new WaitForSecondsRealtime(Game.Feel.blowDuration);
            Check(Game.Crumbs.ActiveCount==0&&Game.Crumbs.MaxCellOccupancy==0,"Blow removes visible crumbs and clears density counts");
            Check(Game.Paper.Drawing.Erased==before&&Game.Paper.AverageDamage==damageBefore&&!Game.CurrentJob.Started,"Blow never changes graphite, paper damage or job state");
            yield return Capture("06-cleared");
            Game.Crumbs.Emit(Vector2.zero,Vector2.right,1);
            Check(Game.Crumbs.ActiveCount>0,"Crumb pool can emit again after blowing");Game.Crumbs.Clear();

            input.ProcessInput(new Vector2(-.6f,0),true,true,.016f);
            input.ProcessInput(new Vector2(.6f,0),true,true,.03f);
            Check(Game.Paper.PeakDamage>0&&Game.Audio.DamageEvents>0&&Game.Presentation.DamageVisible,"Actual paper damage triggers sound and local flash");
            Check(Game.Audio.RubVolume>quiet,"Fast rubbing intensifies friction feedback");
            yield return Capture("07-damage-cue");
            input.ProcessInput(new Vector2(.6f,0),false,true,.016f);
            yield return new WaitForSecondsRealtime(.6f);
            Check(!Game.Presentation.DamageVisible,"Damage flash expires rather than remaining on screen");
            Game.Restart();int passes=0;
            var timer=System.Diagnostics.Stopwatch.StartNew();
            while(!Game.CurrentJob.CanComplete(Game.Paper)&&passes++<14)
            {
                for(float y=-2.35f;y<=2.35f;y+=.22f)
                {
                    Game.Rub(new Vector2(-3.4f,y),new Vector2(3.4f,y),3);
                    Game.CurrentJob.Tick(6.8f/3);yield return null;
                }
            }
            timer.Stop();
            Check(Game.CurrentJob.CanComplete(Game.Paper)&&Game.Paper.PeakDamage==0,"Full safe erasure still reaches the 95 percent goal without damage");
            Game.CurrentJob.Tick(10000);Game.Finish();
            Check(Game.CurrentJob.Completed&&Game.CurrentJob.Result.basic==500&&Game.CurrentJob.Result.speed==0,"Slow job still completes with full base reward and no penalty");
            Check(!Game.Audio.Rubbing&&!input.Pressing,"Completing a job releases the eraser and stops rubbing audio");
            yield return Capture("08-result");
            Game.Restart();
            Check(!Game.CurrentJob.Completed&&Game.Crumbs.ActiveCount==0&&Game.Paper.Drawing.Erased==0,"Result retry returns to a clean playable job");
            Game.Modifiers.radius=1.2f;input.ProcessInput(Vector2.zero,true,true,.016f);
            Check(Game.Presentation.Footprint.HalfSize==Game.Eraser.Contact(Game.Modifiers).HalfSize,"Modifier changes keep rendered sole and brush size synchronized");Game.Modifiers.radius=1;
            report.AppendLine($"Checks={checks}; Failures={failures}; CrumbsGenerated={generated}; SafePasses={passes}; SweepWallMs={timer.ElapsedMilliseconds}");
            File.WriteAllText(Path.Combine(directory,"runtime.txt"),report.ToString());
            Application.Quit(failures==0?0:1);
        }
    }
}
