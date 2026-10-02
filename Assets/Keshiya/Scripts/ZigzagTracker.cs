using UnityEngine;
namespace Keshiya
{
    public struct ZigzagStep
    {
        public float Advance,Quality,Risk;
        public Vector2 Center,Direction;
        public string Reason;
    }
    // Turning-point history, bounded to eight half-strokes. No per-frame allocations.
    public sealed class ZigzagTracker
    {
        readonly CrumbEconomyConfig c;
        readonly Vector2[] centers=new Vector2[8];readonly float[] times=new float[8];
        int history;bool started,headingSet;float clock,extremeTime,pivotTime,sign=1,frontier,previousWidth,previousPeriod;
        Vector2 axis,pivot,extreme,origin,heading,lastEnd;
        public int Turns {get;private set;}
        public float Width {get;private set;}
        public float HalfPeriod {get;private set;}
        public float CenterSpeed {get;private set;}
        public float Risk {get;private set;}
        public Vector2 Center {get;private set;}
        public Vector2 Direction=>headingSet?heading:Vector2.zero;
        public ZigzagTracker(CrumbEconomyConfig config){c=config;}
        public void Reset(){started=false;headingSet=false;history=0;Turns=0;clock=0;sign=1;frontier=0;previousWidth=0;previousPeriod=0;Width=0;HalfPeriod=0;CenterSpeed=0;Risk=0;}
        public ZigzagStep Sample(Vector2 a,Vector2 b,float dt,float speed,float cohesion)
        {
            var result=new ZigzagStep();float distance=Vector2.Distance(a,b);if(distance<.000001f)return result;
            float maxWidth=c.zigMaxWidth+cohesion*c.zigWidthTolerance;
            if(started&&Vector2.Distance(a,lastEnd)>c.contactGap){result.Risk=1;result.Reason="接触位置が飛んで切れました";return result;}
            if(distance>c.teleportDistance||speed>c.zigExtremeSpeed+cohesion*c.zigSpeedTolerance){result.Risk=1;result.Reason="急に速く引いて切れました";return result;}
            if(!started){started=true;axis=(b-a).normalized;pivot=a;extreme=a;pivotTime=0;extremeTime=0;}
            clock+=Mathf.Max(.0001f,dt);lastEnd=b;
            float projection=Vector2.Dot(b-extreme,axis)*sign;
            if(projection>=0){extreme=b;extremeTime=clock;}
            float currentWidth=Vector2.Distance(pivot,extreme);
            Risk=Mathf.Max(0,Risk-dt*.5f);
            if(currentWidth>maxWidth*.8f)Risk=Mathf.Max(Risk,Mathf.InverseLerp(maxWidth*.8f,maxWidth*1.5f,currentWidth));
            result.Risk=Risk;result.Center=Center;result.Direction=Direction;
            if(Risk>=1){result.Reason="往復を大きく振って切れました";return result;}
            if(projection>-c.reversalHysteresis)return result;
            Vector2 leg=extreme-pivot;
            Width=leg.magnitude;HalfPeriod=Mathf.Max(.001f,extremeTime-pivotTime);
            Center=(pivot+extreme)*.5f;
            // Smoothed consecutive midpoints suppress slight differences between left/right reach.
            Vector2 center=Center;
            if(history>0)center=(center+centers[(history-1)%8])*.5f;
            float periodLimit=c.zigMaxHalfPeriod+cohesion*c.zigPeriodTolerance;
            if(history>0&&clock-times[(history-1)%8]>c.historySeconds){history=0;Turns=0;headingSet=false;frontier=0;}
            centers[history%8]=center;times[history%8]=clock;history++;
            float minWidth=c.zigMinWidth*(1-cohesion*.25f);
            bool valid=Width>=minWidth&&HalfPeriod>=c.zigMinHalfPeriod&&HalfPeriod<=periodLimit;
            Turns=valid?Turns+1:0;
            float periodRisk=Mathf.InverseLerp(periodLimit,periodLimit*2,HalfPeriod);
            float widthJump=previousWidth>0?Mathf.Max(Width/previousWidth,previousWidth/Mathf.Max(.001f,Width)):1;
            float periodJump=previousPeriod>0?Mathf.Max(HalfPeriod/previousPeriod,previousPeriod/HalfPeriod):1;
            Risk=Mathf.Max(Risk,periodRisk,Mathf.InverseLerp(2+cohesion,3.5f+cohesion,widthJump),Mathf.InverseLerp(3+cohesion*3,6+cohesion*3,periodJump));
            if(history==1)origin=center;
            if(!headingSet&&Turns>=c.turnsToGrow&&(center-origin).magnitude>=c.centerDeadZone){heading=(center-origin).normalized;headingSet=true;frontier=Vector2.Dot(center-origin,heading);}
            else if(headingSet&&Turns>=c.turnsToGrow){
                int back=Mathf.Min(3,history-1);Vector2 recent=center-centers[(history-1-back)%8];
                float elapsed=Mathf.Max(.001f,clock-times[(history-1-back)%8]);CenterSpeed=recent.magnitude/elapsed;
                float allowed=c.centerTurnDegrees+cohesion*c.centerTurnTolerance;
                float turn=recent.magnitude>=c.centerDeadZone?Vector2.Angle(heading,recent):0;
                float turnRisk=Mathf.InverseLerp(allowed*.75f,allowed,turn);
                float speedRisk=Mathf.InverseLerp(c.maxCenterSpeed+cohesion,c.maxCenterSpeed+cohesion+1,CenterSpeed);
                Risk=Mathf.Max(Risk,turnRisk,speedRisk);
                float projectionForward=Vector2.Dot(center-origin,heading);
                float advance=Mathf.Max(0,projectionForward-frontier);
                if(advance>=c.minCenterAdvance){result.Advance=advance;frontier=projectionForward;}
                if(turnRisk>=1)result.Reason="往復の進む向きを急に変えて切れました";
                else if(speedRisk>=1)result.Reason="往復の中心を速く進めすぎて切れました";
            }
            result.Quality=Mathf.Clamp01(Width/c.idealZigWidth)*Mathf.Lerp(1,.45f,Mathf.Clamp01(Risk));
            result.Risk=Risk;result.Center=center;result.Direction=Direction;
            if(Risk>=1&&result.Reason==null)result.Reason="往復の幅・間隔が急に変わって切れました";
            previousWidth=Width;previousPeriod=HalfPeriod;
            axis=Vector2.Lerp(axis,leg.normalized*sign,.15f).normalized;pivot=extreme;pivotTime=extremeTime;sign=-sign;extreme=b;extremeTime=clock;
            return result;
        }
    }
}

