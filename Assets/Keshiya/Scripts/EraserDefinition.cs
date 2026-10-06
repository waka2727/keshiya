using UnityEngine;
namespace Keshiya {
 [CreateAssetMenu(menuName="Keshiya/Eraser Definition")]
 public class EraserDefinition : ScriptableObject {
  [Header("Collection / special use")]
  public string category="実用品",series="",iconMotif="";
  public bool questOnly;
  public SpecialToolKind specialKind;
  public float maturePowerMultiplier=4;
  public float adsorptionCapacity=.35f,dirtyPowerFloor=.65f,toastDistance=90,toastPowerGain=.45f,toastDamageGain=1.6f;
  public Color usedColor=new Color(.25f,.23f,.20f);
  [Header("Shop / identity")]
  public string id, shortName, brand="消し屋道具店";
  [TextArea] public string description;
  [Min(0)] public int price=160;
  [Range(0,1)] public float initialRemaining=1,initialSharpness=1;
  public EraserState CreateInitialState(){var state=new EraserState{allowExhaustion=true};state.SetInitialCondition(initialRemaining,initialSharpness,this);return state;}
  public Vector2 iconScale=Vector2.one;
  // Material yield is currently standard; amount/cohesion already distinguish ball production.
  public float ballAffinity=1;
  public string displayName="普通のプラスチック消しゴム";
  [Min(.01f)] public float erasePower=1.15f, paperDamageMultiplier=1, radius=.27f, crumbAmount=1;
  public EraserContactProfile contactProfile;
  public EraserContactProfile edgeProfile,cornerProfile;
  [Range(0,1)] public float paperCareResponse=1,edgeCornerWear=.15f,minimumSharpness=.1f;
  public float PaperCare(float multiplier)=>Mathf.LerpUnclamped(1,multiplier,paperCareResponse);
  public float durability=100, wearRate=.12f, cornerWearRate=.008f, highSpeedRisk=1;
  [Range(.01f,.5f)] public float minimumRemaining=.15f;
  public float cornerSpread=.65f, crumbSizeMultiplier=1, reactionMultiplier=1;
  public Color bodyColor=new Color(.95f,.92f,.84f), labelColor=new Color(.16f,.34f,.41f), crumbColor=new Color(.73f,.72f,.66f);
  [Header("Paper and crumb specialties")]
  [Range(0,1)] public float thinPaperStress=1;
  public float thinPaperPickup=1, protectedInkAbrasion=1;
  [Range(0,1)] public float crumbCohesion=.5f;
  public float longCrumbPotential=1;
  public float crumbValueMultiplier=1;
  [TextArea] public string role="万能 / 標準";
  public int specialtyVersion;
  public float SpeedRisk(JobDefinition job)=>highSpeedRisk*(1+Mathf.Max(0,(job?.Fragility??1)-1)*thinPaperStress);
  public float Pickup(JobDefinition job)=>Mathf.Lerp(1,thinPaperPickup,Mathf.Clamp01((job?.Fragility??1)-1));
  public PerformanceModifiers equipment=new PerformanceModifiers();
  public float Power(PerformanceModifiers skills)=>erasePower*equipment.erasePower*skills.erasePower;
  public float DamageMultiplier(PerformanceModifiers skills)=>paperDamageMultiplier*equipment.paperDamage*PaperCare(skills.paperDamage);
  public float Crumbs(PerformanceModifiers skills)=>crumbAmount*equipment.crumbAmount*skills.crumbAmount;
  public ContactFootprint Contact(PerformanceModifiers modifiers) {
   float size=Mathf.Max(.015f,radius*equipment.radius*modifiers.radius);
   return contactProfile!=null?contactProfile.Create(size):ContactFootprint.BroadFace(size);
  }
  public ContactFootprint Contact(PerformanceModifiers skills,ContactMode mode,EraserState state,float yaw) {
   if(mode==ContactMode.Face){var face=Contact(skills);return new ContactFootprint(face.HalfSize,face.Exponent,face.Offset,face.Angle+yaw);}
   float size=Mathf.Max(.015f,radius*equipment.radius*skills.radius);
   var profile=mode==ContactMode.Edge?edgeProfile:cornerProfile;
   var footprint=profile!=null?profile.Create(size):new ContactFootprint(mode==ContactMode.Edge?new Vector2(size,size*.17f):Vector2.one*size*.17f);
   float spread=mode==ContactMode.Corner?(1+cornerSpread*(1-(state?.CornerSharpness??1)))/Mathf.Max(.2f,equipment.precision*skills.precision):1;
   // Quantize sub-pixel wear changes so a long stroke doesn't rebuild meshes every frame.
   spread=Mathf.Round(spread*100)/100;
   float exponent=mode==ContactMode.Corner?Mathf.Lerp(2,footprint.Exponent,state?.CornerSharpness??1):footprint.Exponent;
   exponent=Mathf.Round(exponent*20)/20;
   return new ContactFootprint(footprint.HalfSize*spread,exponent,footprint.Offset,footprint.Angle+yaw);
  }
 }
}
