using UnityEngine;

namespace Keshiya
{
    public sealed class EraserController : MonoBehaviour
    {
        public PrototypeGame Game;
        public Camera View;
        // Explicit test harness injection uses the same transition path as real mouse input.
        public bool ExternalInput {get;set;}
        Vector2 previous;
        bool contact;
        public bool PointerOnPaper {get;private set;}
        public float Speed {get;private set;}
        public bool Pressing {get;private set;}

        public void ResetContact()
        {
            contact=false;Speed=0;Pressing=false;PointerOnPaper=false;
            if(Game==null)return;
            Game.Economy?.EndStroke();Game.Presentation?.Release();Game.Audio?.StopRubbing();
        }
        void OnApplicationFocus(bool focus){if(!focus)ResetContact();}
        void OnApplicationPause(bool paused){if(paused)ResetContact();}
        void OnDisable(){ResetContact();}
        void Update()
        {
            if(Game==null)return;
            if(Game.External?.ModalOpen??false){ResetContact();return;}
            if(Input.GetKeyDown(KeyCode.K)){if(Game.SkillsOpen)Game.CloseSkills();else Game.OpenSkills();return;}
            if(Game.TradeOpen&&Input.GetKeyDown(KeyCode.V)){Game.CloseTrade();return;}
            if(Game.TradeOpen&&Input.GetKeyDown(KeyCode.Z))Game.RescueCrumbs();
            if(Input.GetKeyDown(KeyCode.T)){if(Game.InventoryOpen)Game.CloseShop();else Game.OpenShop(true);return;}
            if((Game.ShopOpen||Game.InventoryOpen)&&Input.GetKeyDown(KeyCode.Escape)){Game.CloseShop();return;}
            if(Game.InputBlocked){ResetContact();return;}
            if(Input.GetKeyDown(KeyCode.M))HandleKeyDown(KeyCode.M);
            if(Input.GetKeyDown(KeyCode.Space))HandleKeyDown(KeyCode.Space);
            if(ExternalInput)return;
            foreach(var key in WorkKeys)if(Input.GetKeyDown(key))HandleKeyDown(key,Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift));
            if(Game.CurrentJob.Completed){ResetContact();return;}
            if(Game.Viewport!=null&&Game.Viewport.HandleInput()){ResetContact();return;}
            bool viewport=View.pixelRect.Contains(Input.mousePosition)&&Game.Viewport.InWorkArea(Input.mousePosition)&&!(Game.External?.PointerOverHint(Input.mousePosition)??false);
            var ray=View.ScreenPointToRay(Input.mousePosition);
            var plane=new Plane(Vector3.up,new Vector3(0,.058f,0));
            if(!viewport||!plane.Raycast(ray,out float enter)){ResetContact();return;}
            Vector3 hit=ray.GetPoint(enter);
            ProcessInput(new Vector2(hit.x,hit.z),Input.GetMouseButton(0),Application.isFocused,Time.unscaledDeltaTime);
        }
        static readonly KeyCode[] WorkKeys={KeyCode.Alpha1,KeyCode.Alpha2,KeyCode.Alpha3,KeyCode.Alpha4,KeyCode.Alpha5,KeyCode.Alpha6,KeyCode.Alpha7,KeyCode.Alpha8,KeyCode.Alpha9,KeyCode.Tab,KeyCode.Q,KeyCode.E,KeyCode.R,KeyCode.C,KeyCode.G,KeyCode.V,KeyCode.Z,KeyCode.F,KeyCode.F3,KeyCode.F4};
        // Shared by physical keys and the player-build regression harness.
        public void HandleKeyDown(KeyCode key,bool reverse=false)
        {
            if(Game==null)return;
            if(Game.External?.ModalOpen??false)return;
            if(!Game.DebugMode&&(key==KeyCode.F3||key==KeyCode.F4))return;
            if(key==KeyCode.T){if(Game.InventoryOpen)Game.CloseShop();else Game.OpenShop(true);return;}
            if(key==KeyCode.K){if(Game.SkillsOpen)Game.CloseSkills();else Game.OpenSkills();return;}
            if(key==KeyCode.V&&Game.TradeOpen){Game.CloseTrade();return;}
            if(Game.InputBlocked)return;
            switch(key){
                case KeyCode.Alpha1:Game.SelectTool(0);break;
                case KeyCode.Alpha2:Game.SelectTool(1);break;
                case KeyCode.Alpha3:Game.SelectTool(2);break;
                case KeyCode.Alpha4:Game.SelectTool(3);break;
                case KeyCode.Alpha5:Game.SelectTool(4);break;
                case KeyCode.Alpha6:Game.SelectTool(5);break;
                case KeyCode.Alpha7:Game.SelectTool(6);break;
                case KeyCode.Alpha8:Game.SelectTool(7);break;
                case KeyCode.Alpha9:Game.SelectTool(8);break;
                case KeyCode.Tab:Game.NextTool();break;
                case KeyCode.Q:Game.SelectMode((ContactMode)(((int)Game.Mode+2)%3));break;
                case KeyCode.E:Game.SelectMode((ContactMode)(((int)Game.Mode+1)%3));break;
                case KeyCode.R:Game.RotateTool(reverse?-45:45);break;
                case KeyCode.F:Game.Sense();break;
                case KeyCode.F4:Game.DebugEraseAreaVisible=!Game.DebugEraseAreaVisible;Game.SyncToolVisual();break;
                case KeyCode.F3:Game.CrumbDebug=!Game.CrumbDebug;break;
                case KeyCode.C:Game.CollectCrumbs();break;
                case KeyCode.G:Game.RollCrumbs();break;
                case KeyCode.V:Game.OpenTrade();break;
                case KeyCode.Z:Game.RescueCrumbs();break;
                case KeyCode.Space:Game.BlowCrumbs();break;
                case KeyCode.M:Game.Audio.ToggleMute();break;
            }
        }
        public void ProcessInput(Vector2 position,bool held,bool focused,float delta)
        {
            Game.SyncToolVisual();
            if(!focused||Game.CurrentJob.Completed||Game.InputBlocked){ResetContact();return;}
            Vector2 half=Game.Paper.Size*.5f;
            bool inside=Mathf.Abs(position.x)<=half.x&&Mathf.Abs(position.y)<=half.y;
            PointerOnPaper=inside;Pressing=inside&&held&&Game.HasUsableTool;if(!Pressing)Game.Economy.EndStroke();
            Vector2 velocity=Vector2.zero;
            if(Pressing&&!contact)Game.Audio.Contact();
            if(Pressing&&contact&&delta>0&&delta<.2f)
            {
                velocity=(position-previous)/Mathf.Max(.001f,delta);Speed=velocity.magnitude;
                Game.Rub(previous,position,Mathf.Min(Speed,Game.Config.maximumSpeed));
            }
            else Speed=0;
            Game.Presentation.SetPose(position,Pressing,velocity,Speed,delta);
            Game.Audio.SetRubbing(Pressing,Speed,Game.Config,Game.EffectiveSpeedRisk);
            previous=position;contact=Pressing;
        }
    }
}
