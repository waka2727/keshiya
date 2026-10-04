using UnityEngine;
namespace Keshiya {
 public sealed class PrototypeGame : MonoBehaviour {
  public PrototypeConfig Config; public EraserDefinition Eraser;public PerformanceModifiers Modifiers=new PerformanceModifiers();
  public Shader ObjectShader,PaperShader,OverlayShader,ArtworkShader;
  public FeelConfig Feel;
  public CrumbEconomyConfig EconomyConfig;
  public CrumbEconomy Economy {get;private set;}
  public CrumbEconomyView EconomyView {get;private set;}
  public CrumbTradeHUD TradeHUD {get;private set;}
  public bool TradeOpen {get;private set;}
  public ExternalTestSession External {get;private set;}
  public bool DebugMode=>ExternalTestPolicy.DeveloperTools;
  public WorkSession Session=>Progress.work;
  public ToolInventory Tools=>Progress.tools;
  public OwnedEraser Supplied {get;private set;}
  public OwnedEraser ActiveTool=>Supplied??Tools.Selected;
  public bool Working=>!BoardEnabled||Session.Phase==WorkPhase.Work;
  public bool CanCarry(OwnedEraser item)=>item!=null&&(!Working||Tools.loadout.Contains(item.instanceId));
  public System.Collections.Generic.List<OwnedEraser> TrayItems=>Supplied!=null?new System.Collections.Generic.List<OwnedEraser>{Supplied}:Tools.items.FindAll(x=>Tools.loadout.Contains(x.instanceId));
  public bool ToggleCarry(string id){if(Working)return false;var item=Tools.Find(id);return item!=null&&Tools.loadout.Toggle(item);}
  public void SetCarryLimit(bool limited){if(Working)return;Tools.loadout.limited=limited;while(limited&&Tools.loadout.selected.Count>Tools.loadout.capacity)Tools.loadout.selected.RemoveAt(Tools.loadout.selected.Count-1);}
  void ReleaseSupply(){if(Supplied==null)return;Supplied=null;ToolIndex=Catalog.Index(Tools.Selected.definitionId);Eraser=Catalog.tools[ToolIndex];SyncToolVisual();Crumbs.SetTool(Eraser);}

  public ShopHUD ShopHUD {get;private set;}
  public bool ShopOpen {get;private set;}
  public bool InventoryOpen {get;private set;}
  public bool HasUsableTool=>ActiveTool!=null&&!ActiveState.Exhausted;
  public bool BoardEnabled {get;private set;}
  public WorkFlowHUD WorkHUD {get;private set;}
  public PlayerProgress Progress {get;private set;}
  public SkillCatalog SkillDefinitions {get;private set;}
  public ProgressionConfig ProgressionTuning {get;private set;}
  public SkillHUD SkillsHUD {get;private set;}
  public bool SkillsOpen {get;private set;}
  public float SenseUntil {get;private set;}
  public float SenseHintUntil {get;private set;}
  public string SenseMessage {get;private set;}="F：消し残し感知 / K：スキル";
  public PrecisionViewport Viewport {get;private set;}
  public PrecisionAssist Assist {get;private set;}
  public PlaytestConfig Playtest {get;private set;}
  public ContactRangeGuide ContactGuide {get;private set;}
  System.Action restoreProgress,restoreEconomy;ContactMode startMode;float startYaw;
  public bool DebugEraseAreaVisible {get;set;}
  public bool CrumbDebug {get;set;}
  public ToolCatalog Catalog;
  public JobDefinition[] Jobs;
  public int ToolIndex {get;private set;}
  public int JobIndex {get;private set;}
  public ContactMode Mode {get;private set;}
  public float Yaw {get;private set;}
  public EraserState ActiveState=>ActiveTool.state;
  public JobDefinition Definition=>Jobs[JobIndex];
  public float EffectiveSpeedRisk=>Eraser.SpeedRisk(Definition)*(Definition.paper!=null?Definition.paper.surfaceDrag:1);
  public ContactFootprint Contact=>Eraser.Contact(Modifiers,Mode,ActiveState,Yaw);
  public float LastProtectionTime {get;private set;}=-10;
  public bool InputBlocked {get;set;}
  public EraserPresentation Presentation {get;private set;}
  public FeedbackAudio Audio {get;private set;}
  public float LastDamageTime {get;private set;} = -10;
  public Vector2 LastDamagePoint {get;private set;}
  public Paper Paper {get;private set;} public Job CurrentJob {get;private set;}public CrumbPool Crumbs {get;private set;}public EraserController Controller {get;private set;}
  Material paperMaterial;Camera view;float refreshAt;bool dirty;readonly System.Collections.Generic.List<Material> materials=new System.Collections.Generic.List<Material>();
  Paper CreatePaperMeasured(){using(var timing=DevelopmentMetrics.Measure("job-paper-load")){var p=new Paper(Config,Feel.erasureGrain,Definition);if(Definition.artwork!=null){var a=Definition.artwork;foreach(var t in new[]{a.paper,a.protectedImage,a.erasable,a.eraseMask,a.protectMask,a.completePreview})DevelopmentMetrics.Texture(t);}DevelopmentMetrics.Texture(p.Texture);return p;}}
  void Awake(){var registry=Resources.Load<JobRegistry>("JobRegistry");if(registry!=null)Jobs=registry.Merge(Jobs);SkillDefinitions=Resources.Load<SkillCatalog>("SkillCatalog");ProgressionTuning=Resources.Load<ProgressionConfig>("ProgressionConfig");Progress=new PlayerProgress(SkillDefinitions,ProgressionTuning);Progress.Apply(Modifiers);if(EconomyConfig==null)EconomyConfig=Resources.Load<CrumbEconomyConfig>("CrumbEconomyConfig");Economy=new CrumbEconomy(EconomyConfig,Progress.wallet);Economy.Progress=Progress;Economy.Skills=Modifiers;if(Config==null)Config=Resources.Load<PrototypeConfig>("PrototypeConfig");if(Feel==null)Feel=Resources.Load<FeelConfig>("FeelConfig");if(Catalog==null)Catalog=Resources.Load<ToolCatalog>("ToolCatalog");if(Jobs==null||Jobs.Length==0)Jobs=new[]{Resources.Load<JobDefinition>("NormalJob"),Resources.Load<JobDefinition>("PrecisionJob")};Tools.Initialize(Catalog);Eraser=Catalog.tools[0];Paper=CreatePaperMeasured();CurrentJob=new Job(Config,Definition);BuildRoom();SyncToolVisual();Crumbs.SetTool(Eraser);gameObject.AddComponent<PrototypeHUD>().Game=this;WorkHUD=gameObject.AddComponent<WorkFlowHUD>();WorkHUD.Game=this;SkillsHUD=gameObject.AddComponent<SkillHUD>();SkillsHUD.Game=this;ShopHUD=gameObject.AddComponent<ShopHUD>();ShopHUD.Game=this;
   Playtest=Resources.Load<PlaytestConfig>("PlaytestConfig");Viewport=gameObject.AddComponent<PrecisionViewport>();Viewport.Game=this;Viewport.View=view;Assist=gameObject.AddComponent<PrecisionAssist>();Assist.Game=this;ContactGuide=new GameObject("Contact range guide").AddComponent<ContactRangeGuide>();ContactGuide.transform.SetParent(transform);ContactGuide.Game=this;
   var args=System.Environment.GetCommandLineArgs();BoardEnabled=true;foreach(var arg in args)if(arg.EndsWith("-test")&&arg!="--external-test"&&arg!="--external01-test")WorkHUD.ExternalOnly=false;
   // Older regression scenarios exercise the explicitly supported unlimited carry setting.
   foreach(var arg in args)if(arg.EndsWith("-test")&&arg!="--world-test")Tools.loadout.limited=false;foreach(var arg in args)if(arg.EndsWith("-test")&&arg!="--work-test"&&arg!="--skills-test"&&arg!="--shop-test"&&arg!="--balance-test"&&arg!="--specialist-test"&&arg!="--world-test"&&arg!="--precision-test"&&arg!="--external-test"&&arg!="--ux-test"&&arg!="--external01-test")BoardEnabled=false;
   if(BoardEnabled)InputBlocked=true;
   if(System.Array.IndexOf(args,"--ux-test")>=0)gameObject.AddComponent<RuntimeUXChecks>().Game=this;
   if(System.Array.IndexOf(args,"--external-test")>=0)gameObject.AddComponent<RuntimeExternalChecks>().Game=this;
   if(System.Array.IndexOf(args,"--world-test")>=0)gameObject.AddComponent<RuntimeWorldChecks>().Game=this;
   if(System.Array.IndexOf(args,"--specialist-test")>=0)gameObject.AddComponent<RuntimeSpecialistChecks>().Game=this;
   if(System.Array.IndexOf(args,"--balance-test")>=0)gameObject.AddComponent<RuntimeBalanceChecks>().Game=this;
   if(System.Array.IndexOf(args,"--shop-test")>=0)gameObject.AddComponent<RuntimeShopChecks>().Game=this;
   if(System.Array.IndexOf(args,"--skills-test")>=0)gameObject.AddComponent<RuntimeSkillChecks>().Game=this;
   if(System.Array.IndexOf(args,"--work-test")>=0)gameObject.AddComponent<RuntimeWorkChecks>().Game=this;
   TradeHUD=gameObject.AddComponent<CrumbTradeHUD>();TradeHUD.Game=this;if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--zigzag-test")>=0)gameObject.AddComponent<RuntimeZigzagChecks>().Game=this;if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--economy-test")>=0)gameObject.AddComponent<RuntimeEconomyChecks>().Game=this;if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--smoke-test")>=0)gameObject.AddComponent<RuntimeSmoke>().Game=this;if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--role-test")>=0)gameObject.AddComponent<RuntimeRoleChecks>().Game=this;if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--tool-test")>=0)gameObject.AddComponent<RuntimeToolChecks>().Game=this;External=gameObject.AddComponent<ExternalTestSession>();External.Initialize(this);if(System.Array.IndexOf(args,"--external01-test")>=0)new GameObject("External01 checks").AddComponent<RuntimeExternal01Checks>().Game=this;}
  Material Mat(Color color){var m=new Material(ObjectShader);m.color=color;materials.Add(m);return m;}
  GameObject Box(string name,Vector3 position,Vector3 scale,Color color){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(transform);g.transform.position=position;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=Mat(color);return g;}
  void BuildRoom(){
   RenderSettings.ambientLight=new Color(.75f,.76f,.8f);
   Box("Desk",new Vector3(0,-.25f,0),new Vector3(20,.4f,14),new Color(.27f,.20f,.15f));
   for(int i=-6;i<=6;i++)Box("Wood seam",new Vector3(0,-.047f,i*1.1f),new Vector3(20,.004f,.014f),new Color(.21f,.15f,.11f));
   Box("Paper backing",new Vector3(0,.01f,0),new Vector3(Paper.Size.x,.08f,Paper.Size.y),new Color(.88f,.85f,.76f));
   var surface=GameObject.CreatePrimitive(PrimitiveType.Quad);surface.name="Paper live surface";surface.transform.SetParent(transform);surface.transform.position=new Vector3(0,.056f,0);surface.transform.rotation=Quaternion.Euler(90,0,0);surface.transform.localScale=new Vector3(Paper.Size.x,Paper.Size.y,1);
   paperMaterial=new Material(PaperShader);materials.Add(paperMaterial);paperMaterial.mainTexture=Paper.Texture;surface.GetComponent<Renderer>().sharedMaterial=paperMaterial;
   Presentation=new GameObject("Eraser feel presentation").AddComponent<EraserPresentation>();Presentation.transform.SetParent(transform);Presentation.Initialize(Config,Feel,Eraser.Contact(Modifiers),ObjectShader,OverlayShader);
   Audio=gameObject.AddComponent<FeedbackAudio>();Audio.Initialize(Feel);
   var cam=new GameObject("Overhead camera");cam.transform.SetParent(transform);view=cam.AddComponent<Camera>();view.orthographic=true;view.transform.position=new Vector3(0,10,0);view.transform.rotation=Quaternion.Euler(90,0,0);view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.17f,.13f,.1f);view.nearClipPlane=.1f;view.farClipPlane=50;cam.AddComponent<AudioListener>();
   var lightObject=new GameObject("Soft work light");lightObject.transform.SetParent(transform);var light=lightObject.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1;light.transform.rotation=Quaternion.Euler(55,-30,0);light.shadows=LightShadows.Soft;
   Crumbs=new GameObject("Crumb pool").AddComponent<CrumbPool>();Crumbs.transform.SetParent(transform);Crumbs.Initialize(Config,Feel,ObjectShader);
   EconomyView=new GameObject("Bounded crumb value visuals").AddComponent<CrumbEconomyView>();EconomyView.transform.SetParent(transform);EconomyView.Initialize(this);
   Controller=gameObject.AddComponent<EraserController>();Controller.Game=this;Controller.View=view;ResizeCamera();
  }
  void ResizeCamera(){view.rect=new Rect(.28f,0,.72f,1);float size=Mathf.Max(Paper.Size.y*(Definition.artwork!=null?.78f:.66f),Paper.Size.x*.60f/Mathf.Max(.1f,view.aspect));if(Viewport!=null)Viewport.Apply(size);else view.orthographicSize=size;}
  void Update(){if(SenseUntil>0&&Time.unscaledTime>=SenseUntil){SenseUntil=0;Paper.Reveal(0);}Economy.AdvanceTime(Time.unscaledTime);ResizeCamera();if(Application.isFocused&&!InputBlocked)CurrentJob.Tick(Time.unscaledDeltaTime);if(dirty&&Time.unscaledTime>=refreshAt){Paper.Refresh();dirty=false;refreshAt=Time.unscaledTime+1f/30;} }
  public void Rub(Vector2 a,Vector2 b,float speed){
   if(!HasUsableTool||CurrentJob.Completed||InputBlocked||Vector2.Distance(a,b)<.00001f)return;
   bool tornBefore=Paper.Torn;CurrentJob.StartWork();float distance=Vector2.Distance(a,b);
   var result=Paper.Stroke(a,b,speed,CurrentJob.Seconds,Eraser,Modifiers,Contact,ActiveState);
   Progress.Stroke(result,Mode,Definition.precision,distance);
   if(Supplied!=null){
    // The tiny supplied remainder is calibrated to this job's graphite, not blank travel.
    // A successful 95% cleanup consumes it exactly, so neither blank farming nor a dead end is possible.
    CurrentJob.RecordSupply(result.erasedFraction);
    bool cleaned=Paper.Drawing.Erased>=Definition.requiredErasure;
    if(cleaned)CurrentJob.RecordSupply(Definition.requiredErasure);
    if(result.graphiteArea>0)ActiveState.Use(distance,Mode,Eraser,Modifiers);
    ActiveState.DebugRemaining(cleaned?0:Mathf.Max(.00001f,Definition.suppliedRemaining*(1-CurrentJob.SupplyProgress)),Eraser,Modifiers);
   }else ActiveState.Use(distance,Mode,Eraser,Modifiers);
   ActiveState.special.Advance(Eraser,result.graphiteArea,distance);
   CurrentJob.RecordUse(ActiveTool,Supplied!=null,result,Mode,distance,speed,Paper.Width*Paper.Height);
   SyncToolVisual();float crumbSource=Eraser.specialKind==SpecialToolKind.Kneaded?0:result.graphiteArea*Definition.CrumbModifier;
   if(crumbSource>0)Crumbs.Emit(a,b,Eraser.crumbAmount*Eraser.equipment.crumbAmount*Definition.CrumbModifier);
   Economy.Stroke(a,b,speed,Eraser,Mode,Eraser.Crumbs(Modifiers),crumbSource);
   if(Supplied==null)Tools.RecordUse(ActiveTool,Economy.JobId,Economy.Growing?.LengthCm??0);
   if(!tornBefore&&Paper.Torn){Assist?.Tear();Audio.Damage();}
   if(result.Damaged){LastDamageTime=Time.unscaledTime;LastDamagePoint=result.damagePoint;Presentation.Damage(result.damagePoint);Audio.Damage();}
   if(result.protectedLoss>0){LastProtectionTime=Time.unscaledTime;Audio.Damage();}dirty=true;
  }
  public void SyncToolVisual(){Presentation.PaperBounds=Paper.Size;Presentation.DebugEraseAreaVisible=DebugEraseAreaVisible&&!InputBlocked&&Working;Presentation.SetTool(Eraser,ActiveState,Modifiers,Mode,Yaw,EffectiveSpeedRisk);}
  public void SelectTool(int index){if(index<0||index>=Catalog.tools.Length)return;var item=Tools.items.Find(x=>x.definitionId==Catalog.tools[index].id&&!x.state.Exhausted&&CanCarry(x));if(item!=null)SelectOwned(item.instanceId);}
  public bool SelectOwned(string id){if(Supplied!=null||!CanCarry(Tools.Find(id))||!Tools.Select(id))return false;Economy.EndStroke("持ち替えて切れました",true);Controller.ResetContact();ToolIndex=Catalog.Index(ActiveTool.definitionId);Eraser=Catalog.tools[ToolIndex];SyncToolVisual();Crumbs.SetTool(Eraser);Presentation.Release();External?.Journal.Sample(this,TestEvent.ToolSelected,0,Eraser.id);return true;}
  public void NextTool(){int start=Tools.items.IndexOf(ActiveTool);for(int n=1;n<=Tools.items.Count;n++){var item=Tools.items[(start+n)%Tools.items.Count];if(!item.state.Exhausted&&CanCarry(item)){SelectOwned(item.instanceId);return;}}}
  public void OpenShop(bool inventory=false){if((External?.ModalOpen??false)||TradeOpen||SkillsOpen||WorkHUD.AskLeave||WorkHUD.AskRestart)return;Controller.ResetContact();ShopOpen=!inventory;InventoryOpen=inventory;InputBlocked=true;}
  public void CloseShop(){ShopOpen=InventoryOpen=false;InputBlocked=(BoardEnabled&&(Session.Phase==WorkPhase.Board||Session.Phase==WorkPhase.Detail))||(Assist!=null&&Assist.SampleOpen);Controller.ResetContact();}
  public bool PurchaseTool(int index){if(index<0||index>=Catalog.tools.Length)return false;bool bought=Tools.Purchase(Catalog.tools[index],Economy.Wallet,out _);if(bought)External?.Journal.Sample(this,TestEvent.ToolPurchased,0,Catalog.tools[index].id);return bought;}
  public void BorrowTool(){if(Supplied!=null||Tools.items.Exists(x=>CanCarry(x)&&!x.state.Exhausted))return;var item=Tools.items.Find(x=>x.loan);if(item==null)item=Tools.Add(Catalog.tools[0],true);else item.state=new EraserState{allowExhaustion=true};Tools.loadout.AdmitRescue(item,Tools);SelectOwned(item.instanceId);}

  public void SelectMode(ContactMode mode){if(mode!=Mode)Economy.EndStroke("面・辺・角を変えて切れました",true);Controller.ResetContact();Mode=mode;SyncToolVisual();Presentation.Release();}
  public void RotateTool(float degrees=45){Economy.EndStroke("向きを変えて切れました",true);Controller.ResetContact();Yaw=Mathf.Repeat(Yaw+degrees,360);SyncToolVisual();Presentation.Release();}
  public void OpenSkills(){if((External?.ModalOpen??false)||TradeOpen||SkillsOpen||ShopOpen||InventoryOpen||WorkHUD.AskLeave||WorkHUD.AskRestart)return;Controller.ResetContact();SkillsOpen=true;InputBlocked=true;}
  public void CloseSkills(){if(!SkillsOpen)return;SkillsOpen=false;InputBlocked=(BoardEnabled&&(Session.Phase==WorkPhase.Board||Session.Phase==WorkPhase.Detail))||(Assist!=null&&Assist.SampleOpen);Controller.ResetContact();}
  public void RefreshSkills(){Controller.ResetContact();Progress.Apply(Modifiers);SenseUntil=0;Paper.Reveal(0);SyncToolVisual();}
  public bool BuySkill(SkillKind kind){if(!Progress.Buy(kind))return false;RefreshSkills();External?.Journal.Sample(this,TestEvent.SkillPurchased,0,"",kind.ToString());return true;}
  public void ResetSkills(){Progress.ResetSkills();RefreshSkills();}
  public bool Sense(){SenseHintUntil=Time.unscaledTime+3;int level=Progress.Level(SkillKind.Sense);if(level==0){SenseMessage="消し残し感知はスキル画面で習得できます";return false;}if(Paper.Drawing.Erased<Progress.Effect(SkillKind.Sense)){SenseMessage=$"感知は消去率{Progress.Effect(SkillKind.Sense)*100:0}%から使えます";return false;}SenseUntil=Time.unscaledTime+SkillDefinitions.Find(SkillKind.Sense).Duration(level);Paper.Reveal(.18f+level*.022f);SenseMessage="残っている鉛筆を淡く表示しています";return true;}
  public void ShowBoard(){if(Working&&CurrentJob.Started&&!CurrentJob.Completed)External?.Journal.Sample(this,TestEvent.JobAbandoned,External.Restarts);Assist?.ResetSample();CloseTrade();CollectCrumbs();ReleaseSupply();Session.ShowBoard();InputBlocked=true;}
  public void ChooseJob(int index){if(Session.Choose(index,Jobs.Length)){External?.Journal.Write(TestEvent.JobSelected,new TestLogEntry{job=Jobs[index].id});ReleaseSupply();Controller.ResetContact();InputBlocked=true;}}
  public void RequestRestart(){if(!Working||CurrentJob.Completed||InputBlocked||restoreProgress==null)return;WorkHUD.AskRestart=true;InputBlocked=true;Controller.ResetContact();}
  public void CancelRestart(){if(!WorkHUD.AskRestart)return;WorkHUD.AskRestart=false;InputBlocked=false;Controller.ResetContact();}
  public void ConfirmRestart(){if(!WorkHUD.AskRestart||restoreProgress==null||CurrentJob.Completed)return;
   External?.Restarted();Controller.ResetContact();ReleaseSupply();restoreProgress();restoreEconomy();Progress.Apply(Modifiers);ToolIndex=Catalog.Index(Tools.Selected.definitionId);Eraser=Catalog.tools[ToolIndex];Mode=startMode;Yaw=startYaw;
   // Restart clears paper and particles; restoring again avoids a second economy transaction/JobId.
   Restart();restoreProgress();restoreEconomy();Progress.Apply(Modifiers);if(Supplied!=null)Supplied.instanceId="supply-"+Economy.JobId;SyncToolVisual();Crumbs.SetTool(Eraser);WorkHUD.AskRestart=false;InputBlocked=false;
  }
  public void BeginWork(){if(!Session.Begin())return;Tools.loadout.Prepare(Tools);if(!HasUsableTool||!CanCarry(ActiveTool)){NextTool();if(!HasUsableTool||!CanCarry(ActiveTool))BorrowTool();}JobIndex=Session.Selected;Restart();InputBlocked=false;restoreProgress=Progress.CaptureRestart();restoreEconomy=Economy.CaptureRestart();startMode=Mode;startYaw=Yaw;External?.Started();}
  public void RetryWork(){ChooseJob(JobIndex);}
  public void SelectJob(int index){if(index<0||index>=Jobs.Length)return;JobIndex=index;Restart();}
  public void FreshTools(){Tools.DebugFresh();Restart();}
  public void BlowCrumbs(){Controller.ResetContact();bool valued=Economy.Blow();if(Crumbs.Blow()||valued)Audio.Blow();}
  public void CollectCrumbs(){Controller.ResetContact();Economy.Collect();Crumbs.Clear();}
  public void RollCrumbs(){Economy.Roll();}
  public void RescueCrumbs(){Economy.Rescue();}
  public void OpenTrade(){if(InputBlocked||TradeOpen||SkillsOpen)return;CollectCrumbs();TradeOpen=true;InputBlocked=true;}
  public void CloseTrade(){if(!TradeOpen)return;TradeOpen=false;InputBlocked=(BoardEnabled&&(Session.Phase==WorkPhase.Board||Session.Phase==WorkPhase.Detail))||(Assist!=null&&Assist.SampleOpen);Controller.ResetContact();}
  public void Finish(){if(CurrentJob.Complete(Paper)){CollectCrumbs();Economy.Wallet.CreditJob(Economy.JobId,CurrentJob.Result.Total);Progress.Complete(Economy.JobId,CurrentJob.Result,Definition.precision);Tools.Complete(Economy.JobId,WorkSession.Grade(CurrentJob.Result));if(BoardEnabled)Session.Finish(Economy.JobId,Definition,CurrentJob.Result,CurrentJob.Usage);External?.Completed();ReleaseSupply();}}
  public void Restart(){using(var timing=DevelopmentMetrics.Measure("job-switch")){Viewport?.ResetView();Assist?.ResetSample();CloseTrade();ReleaseSupply();Economy.BeginJob();Progress.BeginJob();SenseUntil=0;SenseHintUntil=0;SenseMessage="F：消し残し感知 / K：スキル";var old=Paper;Paper=CreatePaperMeasured();SetPaperArtwork();old.Dispose();CurrentJob=new Job(Config,Definition);CurrentJob.ChallengeMultiplier=Tools.loadout.limited?(Playtest?.fiveToolRewardMultiplier??1.15f):1;
   if(Definition.suppliedTool!=null){Supplied=new OwnedEraser{instanceId="supply-"+Economy.JobId,definitionId=Definition.suppliedTool.id,loan=true};Eraser=Definition.suppliedTool;ToolIndex=Catalog.Index(Eraser.id);ActiveState.DebugRemaining(Definition.suppliedRemaining,Eraser,Modifiers);Crumbs.SetTool(Eraser);}
   Crumbs.Clear();Controller.ResetContact();Presentation.ClearFeedback();Audio.ResetFeedback();LastDamageTime=-10;LastProtectionTime=-10;SyncToolVisual();Presentation.Release();dirty=false;}}
  void SetPaperArtwork(){var art=Definition.artwork;if(art!=null){paperMaterial.shader=ArtworkShader;paperMaterial.SetTexture("_Paper",art.paper);paperMaterial.SetTexture("_Protected",art.protectedImage);paperMaterial.SetTexture("_Erasable",art.erasable);paperMaterial.SetTexture("_State",Paper.Texture);paperMaterial.SetFloat("_TearThreshold",Paper.TearThreshold);}else{paperMaterial.shader=PaperShader;paperMaterial.mainTexture=Paper.Texture;}transform.Find("Paper live surface").localScale=new Vector3(Paper.Size.x,Paper.Size.y,1);transform.Find("Paper backing").localScale=new Vector3(Paper.Size.x,.08f,Paper.Size.y);}
  void OnDestroy(){Paper?.Dispose();foreach(var m in materials)if(m!=null)Destroy(m);}
 }
}



