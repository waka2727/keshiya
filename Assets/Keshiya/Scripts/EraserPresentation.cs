using System.Collections.Generic;
using UnityEngine;

namespace Keshiya
{
    public sealed class EraserPresentation : MonoBehaviour
    {
        const float PaperY=.058f;
        FeelConfig config;
        PrototypeConfig gameConfig;
        Transform sole, body, shadow, damageCue, label, namePlate;bool supplied;Material nameMaterial;public Vector2 PaperBounds=new Vector2(7,5);
        Material soleMaterial, damageMaterial;
        Material bodyMaterial,labelMaterial;
        LineRenderer contactGuide;
        readonly Vector3[] guidePoints=new Vector3[ContactMesh.Segments];
        public bool DebugEraseAreaVisible;
        public bool ContactGuideVisible=>contactGuide!=null&&contactGuide.gameObject.activeSelf;
        ContactFootprint bodyShape;
        Vector3[] bodyVertices;
        ContactMode mode;
        float yaw,remaining=1,reaction=1,speedRisk=1;
        public float BodyHeight {get;private set;}=.20f;
        public Color RenderedBodyColor=>body.GetComponent<Renderer>().sharedMaterial.color;
        public Vector2 BodyOffset=>new Vector2(body.localPosition.x,body.localPosition.z);
        Vector2 lastPosition;
        Vector2 lean;
        float flashUntil;
        readonly List<Mesh> meshes=new List<Mesh>();
        readonly List<Material> materials=new List<Material>();
        public float SoleHeight => sole.position.y;
        public Vector2 ContactPosition => new Vector2(sole.position.x,sole.position.z);
        public bool DamageVisible => damageCue!=null && damageCue.gameObject.activeSelf;
        public ContactFootprint Footprint { get; private set; }

        Material Material(Shader shader,Color color)
        {
            var m=new Material(shader) { color=color };
            if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",.08f);
            if(m.HasProperty("_SpecularHighlights")){m.SetFloat("_SpecularHighlights",0);m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");}
            materials.Add(m); return m;
        }
        Transform MeshObject(string name,Mesh mesh,Material material,Transform parent)
        {
            meshes.Add(mesh);
            var obj=new GameObject(name); obj.transform.SetParent(parent,false);
            obj.AddComponent<MeshFilter>().sharedMesh=mesh;
            obj.AddComponent<MeshRenderer>().sharedMaterial=material;
            return obj.transform;
        }
        public void Initialize(PrototypeConfig game,FeelConfig feel,ContactFootprint footprint,Shader objectShader,Shader overlayShader)
        {
            gameConfig=game; config=feel; Footprint=footprint;
            soleMaterial=Material(objectShader,new Color(.86f,.85f,.78f));
            sole=MeshObject("Actual contact sole",ContactMesh.Solid(footprint,.014f),soleMaterial,transform);
            bodyMaterial=Material(objectShader,new Color(.95f,.92f,.84f));bodyShape=footprint;
            var bodyMesh=ContactMesh.Solid(footprint,.20f);bodyVertices=bodyMesh.vertices;
            body=MeshObject("Soft eraser body",bodyMesh,bodyMaterial,sole);
            body.localPosition=new Vector3(0,.014f,0);
            var sleeve=GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(sleeve.GetComponent<Collider>());
            sleeve.name="Blue top label"; sleeve.transform.SetParent(body,false);
            label=sleeve.transform;
            sleeve.transform.localPosition=new Vector3(footprint.HalfSize.x*.38f,.203f,0);
            sleeve.transform.localScale=new Vector3(footprint.HalfSize.x*.80f,.012f,footprint.HalfSize.y*1.48f);
            labelMaterial=Material(objectShader,new Color(.16f,.34f,.41f));sleeve.GetComponent<Renderer>().sharedMaterial=labelMaterial;
            shadow=MeshObject("Contact shadow",ContactMesh.Shadow(footprint),Material(overlayShader,Color.white),transform);
            contactGuide=new GameObject("Visible contacting edge").AddComponent<LineRenderer>();contactGuide.transform.SetParent(transform,false);
            var guideMaterial=Material(overlayShader,new Color(.18f,.60f,.38f,.95f));guideMaterial.SetFloat("_ZTest",8);
            contactGuide.sharedMaterial=guideMaterial;contactGuide.useWorldSpace=false;contactGuide.loop=true;contactGuide.positionCount=ContactMesh.Segments;contactGuide.widthMultiplier=.009f;
            contactGuide.startColor=Color.white;contactGuide.endColor=Color.white;UpdateGuide(footprint);
            damageMaterial=Material(overlayShader,new Color(.91f,.46f,.19f));
            damageCue=new GameObject("New paper damage cue").transform; damageCue.SetParent(transform,false);
            for(int i=0;i<3;i++)
            {
                var line=new GameObject("Fibre flash").AddComponent<LineRenderer>();
                line.transform.SetParent(damageCue,false); line.sharedMaterial=damageMaterial;
                line.useWorldSpace=false; line.positionCount=3; line.widthMultiplier=.013f;
                line.SetPositions(new[]{new Vector3(-.18f+i*.14f,0,-.12f),new Vector3(-.10f+i*.14f,0,0),new Vector3(-.15f+i*.14f,0,.13f)});
                line.startColor=Color.white;line.endColor=Color.white;
            }
            damageCue.gameObject.SetActive(false);
            SetPose(Vector2.zero,false,Vector2.zero,0,0);
        }

        public void SetPose(Vector2 position,bool pressed,Vector2 velocity,float speed,float dt)
        {
            lastPosition=position;
            float risk=Mathf.Clamp01((speed*speedRisk-gameConfig.dangerousSpeed)/gameConfig.dangerousSpeed);
            Vector2 target=pressed?Vector2.ClampMagnitude(velocity/gameConfig.optimalSpeed,1)*config.leanDegrees:Vector2.zero;
            lean=pressed?Vector2.Lerp(lean,target,1-Mathf.Exp(-config.leanResponse*dt)):Vector2.zero;
            sole.position=new Vector3(position.x,PaperY+(pressed?0:config.hoverHeight),position.y);
            if(contactGuide!=null){contactGuide.gameObject.SetActive(DebugEraseAreaVisible&&pressed&&mode!=ContactMode.Face);contactGuide.transform.position=new Vector3(position.x,PaperY+.015f,position.y);}
            sole.localRotation=pressed?Quaternion.identity:Quaternion.Euler(config.hoverTiltDegrees.x,0,config.hoverTiltDegrees.y);
            // Only the upper body reacts; the exact sole stays under the mouse with no lag.
            float motion=reaction*(mode==ContactMode.Face?1:.15f);
            Quaternion tilt=mode==ContactMode.Face?Quaternion.identity:mode==ContactMode.Edge?Quaternion.Euler(65,0,0):Quaternion.Euler(55,0,45);
            body.localRotation=Quaternion.Euler(0,-yaw,0)*tilt*Quaternion.Euler(lean.y*motion,0,-lean.x*motion);
            float height=(supplied?Mathf.Lerp(.12f,1,Mathf.Clamp01(remaining/.12f)):Mathf.Lerp(.55f,1,remaining))*(pressed?config.pressedHeightScale:1);
            BodyHeight=.20f*height;
            body.localScale=new Vector3(pressed?.94f:.90f,height,pressed?.94f:.90f);
            float buzz=pressed&&speed>.03f?Mathf.Sin(Time.unscaledTime*110)*config.frictionVibration*(.25f+risk):0;
            // Pivot the full-size tool around its lowest edge/corner, not its centre.
            // The small visible contact pad is fixed at the exact input location.
            float low=float.MaxValue;
            for(int i=0;i<ContactMesh.Segments*2;i++)low=Mathf.Min(low,(body.localRotation*Vector3.Scale(bodyVertices[i],body.localScale)).y);
            Vector3 support=Vector3.zero;int samples=0;
            for(int i=0;i<ContactMesh.Segments*2;i++){var v=body.localRotation*Vector3.Scale(bodyVertices[i],body.localScale);if(v.y<=low+.003f){support+=v;samples++;}}
            support/=Mathf.Max(1,samples);
            // Broad-face friction leans must never pivot about a distant corner:
            // doing so would visibly shift the whole body by half its width.
            body.localPosition=new Vector3(mode==ContactMode.Face?0:-support.x,.014f-low+Mathf.Abs(buzz),mode==ContactMode.Face?0:-support.z);
            soleMaterial.color=pressed?Color.Lerp(new Color(.57f,.72f,.62f),new Color(.9f,.57f,.31f),risk):new Color(.86f,.85f,.78f);
            shadow.position=new Vector3(position.x+(pressed?0:.13f),PaperY-.001f,position.y-(pressed?0:.13f));
            shadow.localScale=Vector3.one*(pressed?1.16f:1.65f);
            shadow.gameObject.SetActive(Mathf.Abs(position.x)<PaperBounds.x*.5f && Mathf.Abs(position.y)<PaperBounds.y*.5f);
        }
        public void Release() => SetPose(lastPosition,false,Vector2.zero,0,0);
        public void SetTool(EraserDefinition definition,EraserState state,PerformanceModifiers skills,ContactMode contactMode,float angle,float effectiveRisk=-1)
        {
            supplied=definition.questOnly;mode=contactMode;yaw=angle;remaining=state.Remaining(definition,skills);reaction=definition.reactionMultiplier;speedRisk=effectiveRisk<0?definition.highSpeedRisk:effectiveRisk;
            bodyMaterial.color=state.special.Color(definition);labelMaterial.color=definition.labelColor;
            var full=definition.Contact(skills);
            if(supplied&&namePlate==null){var obj=GameObject.CreatePrimitive(PrimitiveType.Quad);Destroy(obj.GetComponent<Collider>());namePlate=obj.transform;namePlate.SetParent(body,false);namePlate.localRotation=Quaternion.Euler(90,0,0);nameMaterial=new Material(Resources.Load<Material>("SuppliedNameLabel"));materials.Add(nameMaterial);obj.GetComponent<Renderer>().sharedMaterial=nameMaterial;}
            if(namePlate!=null){namePlate.gameObject.SetActive(supplied);float fraction=Mathf.Clamp01(remaining/.12f);namePlate.localScale=new Vector3(full.HalfSize.x*1.8f*fraction,full.HalfSize.y,1);namePlate.localPosition=new Vector3(full.HalfSize.x*(1-fraction)*.9f,.211f,0);nameMaterial.mainTextureScale=new Vector2(fraction,1);nameMaterial.mainTextureOffset=new Vector2(1-fraction,0);}
            label.gameObject.SetActive(!supplied);
            if(full.HalfSize!=bodyShape.HalfSize||full.Exponent!=bodyShape.Exponent){
                bodyShape=full;var mesh=ContactMesh.Solid(full,.20f);bodyVertices=mesh.vertices;ReplaceMesh(body,mesh);
                label.localPosition=new Vector3(full.HalfSize.x*.38f,.203f,0);
                label.localScale=new Vector3(full.HalfSize.x*.80f,.012f,full.HalfSize.y*1.48f);
            }
            SyncFootprint(definition.Contact(skills,mode,state,yaw));
        }
        public void SyncFootprint(ContactFootprint footprint)
        {
            if(footprint.HalfSize==Footprint.HalfSize && footprint.Offset==Footprint.Offset && footprint.Exponent==Footprint.Exponent && footprint.Angle==Footprint.Angle)return;
            Footprint=footprint;
            UpdateGuide(footprint);
            ReplaceMesh(sole,ContactMesh.Solid(footprint,.014f));
            ReplaceMesh(shadow,ContactMesh.Shadow(footprint));
        }
        void ReplaceMesh(Transform target,Mesh mesh)
        {
            var filter=target.GetComponent<MeshFilter>();var old=filter.sharedMesh;
            meshes.Remove(old);Destroy(old);filter.sharedMesh=mesh;meshes.Add(mesh);
        }
        void UpdateGuide(ContactFootprint footprint)
        {
            if(contactGuide==null)return;
            for(int i=0;i<guidePoints.Length;i++){var p=footprint.OutlinePoint(i*Mathf.PI*2/guidePoints.Length);guidePoints[i]=new Vector3(p.x,0,p.y);}
            contactGuide.SetPositions(guidePoints);
        }
        public void Damage(Vector2 point)
        {
            flashUntil=Time.unscaledTime+config.damageFlashSeconds;
            damageCue.position=new Vector3(point.x,.080f,point.y);
            damageCue.gameObject.SetActive(true);
        }
        public void ClearFeedback() { flashUntil=0;damageCue.gameObject.SetActive(false);Release(); }
        void Update()
        {
            if(damageCue==null || !damageCue.gameObject.activeSelf)return;
            float alpha=Mathf.Clamp01((flashUntil-Time.unscaledTime)/Mathf.Max(.01f,config.damageFlashSeconds));
            damageMaterial.color=new Color(.91f,.46f,.19f,alpha);
            if(alpha<=0)damageCue.gameObject.SetActive(false);
        }
        void OnDestroy() { foreach(var mesh in meshes)Destroy(mesh);foreach(var m in materials)Destroy(m); }
    }
}
