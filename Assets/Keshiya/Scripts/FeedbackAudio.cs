using System.Collections.Generic;
using UnityEngine;

namespace Keshiya
{
    public sealed class FeedbackAudio : MonoBehaviour
    {
        AudioSource friction, accents;
        AudioClip contactClip, damageClip, blowClip, completionClip;
        bool completedCue; public int CompletionEvents {get;private set;}
        public bool TryCompletion(float erased) {if(completedCue||erased<1f)return false;completedCue=true;CompletionEvents++;if(!Muted&&accents!=null)accents.PlayOneShot(completionClip,config.masterVolume*.32f);return true;}
        public void ResetCompletion(){completedCue=false;}
        FeelConfig config;
        readonly List<AudioClip> owned = new List<AudioClip>();
        float damageCooldown;
        public bool Muted { get; private set; }
        public bool Rubbing => friction != null && friction.isPlaying && friction.volume>0;
        public float RubVolume => friction == null ? 0 : friction.volume;
        public float RubPitch => friction == null ? 0 : friction.pitch;
        public int ContactEvents { get; private set; }
        public int DamageEvents { get; private set; }

        public void Initialize(FeelConfig feel)
        {
            config=feel;
            friction=gameObject.AddComponent<AudioSource>();
            accents=gameObject.AddComponent<AudioSource>();
            friction.playOnAwake=false; accents.playOnAwake=false;
            friction.loop=true; friction.spatialBlend=0; accents.spatialBlend=0;
            friction.clip=UseOrMake(feel.frictionClip,"Soft eraser friction",.6f,0);
            contactClip=UseOrMake(feel.contactClip,"Paper contact",.065f,1);
            damageClip=UseOrMake(feel.damageClip,"Paper fibres",.09f,2);
            completionClip=UseOrMake(null,"Complete paper",.32f,4);
            blowClip=UseOrMake(feel.blowClip,"Gentle puff",.28f,3);
        }

        AudioClip UseOrMake(AudioClip supplied,string name,float seconds,int kind)
        {
            if(supplied!=null) return supplied;
            const int rate=22050;
            float[] samples=new float[Mathf.RoundToInt(rate*seconds)];
            var random=new System.Random(813+kind);
            float smooth=0;
            for(int i=0;i<samples.Length;i++)
            {
                float t=(float)i/rate, u=(float)i/(samples.Length-1);
                float noise=(float)random.NextDouble()*2-1;
                smooth=Mathf.Lerp(smooth,noise,kind==2?.8f:.18f);
                float signal=kind==1 ? Mathf.Sin(t*2*Mathf.PI*160)*.42f+smooth*.25f : smooth;
                float envelope=kind==0? .72f+.28f*Mathf.Sin(t*2*Mathf.PI*35)
                    : Mathf.Sin(Mathf.PI*u)*Mathf.Exp(-u*(kind==3?1:3));
                // Fade loop endpoints to prevent seam clicks.
                if(kind==0) envelope*=Mathf.Min(1,Mathf.Min(u,1-u)*120);
                if(kind==4){signal=.4f*Mathf.Sin(t*2*Mathf.PI*660)+.2f*Mathf.Sin(t*2*Mathf.PI*990);envelope=Mathf.Sin(Mathf.PI*u)*Mathf.Exp(-u*4);}
                samples[i]=signal*envelope*.65f;
            }
            var clip=AudioClip.Create(name,samples.Length,1,rate,false);
            clip.SetData(samples,0); owned.Add(clip); return clip;
        }

        public void Contact()
        {
            ContactEvents++;
            if(!Muted) accents.PlayOneShot(contactClip,config.masterVolume*.5f);
        }
        public void SetRubbing(bool pressed,float speed,PrototypeConfig gameConfig,float toolRisk=1)
        {
            if(!pressed || speed<.03f || Muted) { StopRubbing(); return; }
            float pace=Mathf.Clamp01(speed/gameConfig.optimalSpeed);
            float risk=Mathf.Clamp01((speed*toolRisk-gameConfig.dangerousSpeed)/gameConfig.dangerousSpeed);
            friction.volume=config.masterVolume*(.055f+.20f*pace+.22f*risk);
            friction.pitch=.72f+.36f*pace+.32f*risk;
            if(!friction.isPlaying) friction.Play();
        }
        public void Damage()
        {
            if(Time.unscaledTime<damageCooldown) return;
            damageCooldown=Time.unscaledTime+.16f; DamageEvents++;
            if(!Muted) accents.PlayOneShot(damageClip,config.masterVolume*.55f);
        }
        public void Blow() { if(!Muted) accents.PlayOneShot(blowClip,config.masterVolume*.6f); }
        public void StopRubbing() { if(friction==null)return; friction.Stop(); friction.volume=0; }
        public void ToggleMute() { Muted=!Muted; if(Muted) { StopRubbing(); accents.Stop(); } }
        public void ResetFeedback() { StopRubbing(); accents.Stop(); damageCooldown=0; }
        void OnApplicationFocus(bool focus) { if(!focus) ResetFeedback(); }
        void OnDestroy() { foreach(var clip in owned) Destroy(clip); }
    }
}
