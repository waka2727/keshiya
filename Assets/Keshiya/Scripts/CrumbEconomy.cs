using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Keshiya
{
    // Authoritative material ledger; never dependent on recycled visual particles.
    public sealed partial class CrumbEconomy
    {
        public readonly CrumbEconomyConfig Config;
        public PlayerProgress Progress;
        public PerformanceModifiers Skills=new PerformanceModifiers();
        public float ConversionBonusGrams {get;private set;}
        public readonly Wallet Wallet;
        public readonly CrumbBall Ball=new CrumbBall();
        public readonly ZigzagTracker Motion;
        float graphiteCredit,materialCredit;
        public float GraphiteCredit=>graphiteCredit;
        readonly List<CrumbPiece> paper=new List<CrumbPiece>(),inventory=new List<CrumbPiece>(),blown=new List<CrumbPiece>();
        public IReadOnlyList<CrumbPiece> Paper=>paper;
        public IReadOnlyList<CrumbPiece> Inventory=>inventory;
        public IReadOnlyList<CrumbPiece> Blown=>blown;
        public CrumbPiece Growing {get;private set;}
        public float PaperFineGrams {get;private set;}
        public float LooseGrams {get;private set;}
        public float ProducedGrams {get;private set;}
        public float SoldGrams {get;private set;}
        public float DiscardedGrams {get;private set;}
        public float BestLength {get;private set;}
        public float JobLongest {get;private set;}
        public long JobSales {get;private set;}
        public long JobId {get;private set;}=1;
        public string Status {get;private set;}="鉛筆の上で細かく往復し、少しずつ進む";
        public int BreakCount {get;private set;}
        public string LastBreakReason {get;private set;}
        float breakUntil;
        public bool RecentBreak=>now<breakUntil;
        public int OverflowCount {get;private set;}
        public float OverflowLongest {get;private set;}
        public long OverflowPrice {get;private set;}
        float overflowMass,blownFine,now,lastStroke=-100,rescueUntil;
        EraserDefinition priorTool;ContactMode priorMode;
        long nextId;
        public float RescueRemaining=>Mathf.Max(0,rescueUntil-now);
        public float LastBlowAge=>Config.rescueSeconds-RescueRemaining;
        public long InventoryPrice {get {long value=OverflowPrice;foreach(var item in inventory)value+=item.Price(Config);return value;}}
        public float AccountedMass {get {
            float value=PaperFineGrams+LooseGrams+Ball.Grams+SoldGrams+DiscardedGrams+blownFine+overflowMass;
            foreach(var item in paper)value+=item.MassGrams;foreach(var item in inventory)value+=item.MassGrams;foreach(var item in blown)value+=item.MassGrams;return value;
        }}
        public CrumbEconomy(CrumbEconomyConfig config,Wallet wallet=null){Wallet=wallet??new Wallet();Config=config;Motion=new ZigzagTracker(config);}
        public void AdvanceTime(float time)
        {
            now=Mathf.Max(now,time);
            if(now-lastStroke>Mathf.Max(.05f,Config.zigIdleReleaseSeconds))EndStroke("手を止めて置きました");
            if(rescueUntil>0&&now>=rescueUntil)DiscardBlown();
        }
        public void Stroke(Vector2 a,Vector2 b,float speed,EraserDefinition tool,ContactMode mode,float amount=1,float graphiteArea=0,float delta=0)
        {
            float distance=Vector2.Distance(a,b);if(distance<.00001f||amount<=0||speed<=0)return;
            if(priorTool!=null&&(priorTool!=tool||priorMode!=mode))EndStroke("接触を変えて切れました",true);
            priorTool=tool;priorMode=mode;lastStroke=now;
            // Only actually removed target graphite produces tradable material, including ball fines.
            float mass=Mathf.Max(0,graphiteArea)*Config.gramsPerGraphiteArea*amount;
            ProducedGrams+=mass;PaperFineGrams+=mass;
            float cohesion=Mathf.Clamp01(tool.crumbCohesion);
            graphiteCredit=Mathf.Min(Config.graphiteCarryCm,graphiteCredit+Mathf.Max(0,graphiteArea)*Config.graphiteCmPerArea*tool.longCrumbPotential*Skills.longGrowth*tool.equipment.longGrowth);
            materialCredit+=mass*Mathf.Lerp(.2f,.85f,cohesion);
            var step=Motion.Sample(a,b,delta>0?delta:distance/Mathf.Max(.01f,speed),speed,cohesion+Mathf.Clamp(Skills.crumbTolerance+tool.equipment.crumbTolerance,0,.3f));
            if(Growing!=null)Growing.Tension=step.Risk;
            if(step.Risk>=1){EndStroke(step.Reason??"大きく振って切れました",true);return;}
            float growth=Config.Growth(mode);
            if(growth<=0){EndStroke("角から細かなカスが出ています",true);return;}
            float candidate=step.Advance*Config.centimetersPerCenterUnit*tool.longCrumbPotential*growth*step.Quality*Skills.longGrowth*tool.equipment.longGrowth;
            float length=Mathf.Min(candidate,graphiteCredit,Mathf.Min(materialCredit,PaperFineGrams)/Mathf.Max(.0001f,Config.gramsPerCm));
            if(length<=.00001f){if(Growing==null)Status=graphiteCredit<=0?"鉛筆のある場所で細かく往復する":"細かく往復しながら、中心も少し進める";return;}
            float strandMass=length*Config.gramsPerCm;
            PaperFineGrams-=strandMass;materialCredit-=strandMass;graphiteCredit-=length;
            float cap=Mathf.Max(.1f,cohesion<.25f?Config.sandZigLength:Config.maximumLength);
            float left=length;
            while(left>.000001f){
                if(Growing==null){
                    if(paper.Count>=Mathf.Max(1,Config.paperPieceCapacity))Store(paper[0]);
                    Growing=new CrumbPiece{Id=++nextId,Source=tool.displayName,ToolDefinitionId=tool.id,ToolInstanceId=Progress?.tools.selectedId,Thickness=mode==ContactMode.Face?.026f:.015f,ValueMultiplier=tool.crumbValueMultiplier,Color=tool.crumbColor,State=CrumbState.Growing};paper.Add(Growing);
                }
                float used=Mathf.Min(left,cap-Growing.LengthCm);Growing.LengthCm+=used;Growing.MassGrams+=strandMass*used/length;
                Growing.Direction=step.Direction;Growing.End=step.Center+new Vector2(-step.Direction.y,step.Direction.x)*.2f;Growing.Tension=step.Risk;
                JobLongest=Mathf.Max(JobLongest,Growing.LengthCm);BestLength=Mathf.Max(BestLength,Growing.LengthCm);left-=used;
                if(Growing.LengthCm>=cap-.00001f)EndStroke(cohesion<.25f?"砂のカスが細かく離れました":"長くなって自然に離れました",true);
            }
            if(Growing!=null)Status="往復しながら育っています · "+Config.Rank(Growing.LengthCm);
        }
        public void EndStroke(string reason="紙に置きました",bool broken=false)
        {
            Motion.Reset();graphiteCredit=0;materialCredit=0;priorTool=null;
            if(Growing==null)return;
            Progress?.StrandFormed(Growing.Id,Growing.LengthCm);
            Progress?.tools.RecordLength(Growing.ToolDefinitionId,Growing.ToolInstanceId,Growing.LengthCm);
            Growing.State=CrumbState.OnPaper;Growing.WasBroken=broken;Growing.EndReason=reason;
            if(broken){BreakCount++;LastBreakReason=reason;breakUntil=now+1.2f;}Growing.Tension=0;Growing=null;Status=reason;
        }
        void Store(CrumbPiece item)
        {
            paper.Remove(item);item.State=CrumbState.Collected;
            if(item.LengthCm<Config.longMinimum){LooseGrams+=item.MassGrams;return;}
            Progress?.Collected(item.Id,item.LengthCm);
            if(inventory.Count<Mathf.Max(1,Config.inventoryCapacity))inventory.Add(item);
            else {OverflowCount++;OverflowPrice+=item.Price(Config);overflowMass+=item.MassGrams;OverflowLongest=Mathf.Max(OverflowLongest,item.LengthCm);}
        }
        public void Collect()
        {
            EndStroke();while(paper.Count>0)Store(paper[0]);LooseGrams+=PaperFineGrams;PaperFineGrams=0;
            Status="回収しました · 細かなカスはGで丸める";
        }
        public bool Roll()
        {
            if(LooseGrams<=0)return false;float raw=LooseGrams;float bonus=raw*(Mathf.Clamp(Skills.ballYield,1,1.2f)-1);ConversionBonusGrams+=bonus;ProducedGrams+=bonus;Ball.Add(raw+bonus);Progress?.Rolled(raw);LooseGrams=0;Status="玉が少し大きくなりました";return true;
        }
        public bool Blow()
        {
            EndStroke();if(paper.Count==0&&PaperFineGrams<=0)return false;
            DiscardBlown();blown.AddRange(paper);paper.Clear();foreach(var item in blown)item.State=CrumbState.BlownAway;
            blownFine=PaperFineGrams;PaperFineGrams=0;rescueUntil=now+Config.rescueSeconds;Status="吹き払いました · 6秒以内ならZで回収";return true;
        }
        void DiscardBlown(){foreach(var item in blown)DiscardedGrams+=item.MassGrams;DiscardedGrams+=blownFine;blown.Clear();blownFine=0;rescueUntil=0;}
        public bool Rescue()
        {
            if(RescueRemaining<=0)return false;
            foreach(var item in blown)Store(item);blown.Clear();LooseGrams+=blownFine;blownFine=0;rescueUntil=0;Status="吹き払ったカスを回収しました";return true;
        }
        void Credit(long price,float grams){Wallet.CreditCrumbs(price);Progress?.Sold(price);JobSales+=price;SoldGrams+=grams;Status=$"{price:N0}円で売れました";}
        public long SellPiece(long id)
        {
            int index=inventory.FindIndex(p=>p.Id==id);if(index<0)return 0;
            var item=inventory[index];long price=item.Price(Config);inventory.RemoveAt(index);item.State=CrumbState.Sold;Credit(price,item.MassGrams);return price;
        }
        public long SellLongs()
        {
            long value=0;while(inventory.Count>0)value+=SellPiece(inventory[0].Id);
            if(OverflowCount>0){value+=OverflowPrice;Credit(OverflowPrice,overflowMass);OverflowCount=0;OverflowPrice=0;overflowMass=0;OverflowLongest=0;}if(value>0)Status=$"一本ものを合計{value:N0}円で売りました";return value;
        }
        public long SellBall(){if(Ball.Grams<=0)return 0;long price=Config.BallPrice(Ball.Grams);if(price<=0)return 0;Credit(price,Ball.Empty());return price;}
        public System.Action CaptureRestart(){var saved=(CrumbEconomy)MemberwiseClone();var stock=inventory.ConvertAll(x=>x.Copy());var wind=blown.ConvertAll(x=>x.Copy());float rescue=RescueRemaining;var ball=Ball.CaptureRestart();
         return ()=>{paper.Clear();inventory.Clear();inventory.AddRange(stock.ConvertAll(x=>x.Copy()));blown.Clear();blown.AddRange(wind.ConvertAll(x=>x.Copy()));Growing=null;Motion.Reset();graphiteCredit=materialCredit=0;priorTool=null;lastStroke=-100;breakUntil=0;rescueUntil=rescue>0?now+rescue:0;ball();
          ConversionBonusGrams=saved.ConversionBonusGrams;
          PaperFineGrams=saved.PaperFineGrams;
          LooseGrams=saved.LooseGrams;
          ProducedGrams=saved.ProducedGrams;
          SoldGrams=saved.SoldGrams;
          DiscardedGrams=saved.DiscardedGrams;
          BestLength=saved.BestLength;
          JobLongest=saved.JobLongest;
          JobSales=saved.JobSales;
          JobId=saved.JobId;
          Status=saved.Status;
          BreakCount=saved.BreakCount;
          LastBreakReason=saved.LastBreakReason;
          OverflowCount=saved.OverflowCount;
          OverflowLongest=saved.OverflowLongest;
          OverflowPrice=saved.OverflowPrice;
          overflowMass=saved.overflowMass;
          blownFine=saved.blownFine;
          nextId=saved.nextId;
         };}
        public void BeginJob(){Collect();JobId++;JobLongest=0;JobSales=0;Status="鉛筆の上で細かく往復し、少しずつ進む";}
    }
}

