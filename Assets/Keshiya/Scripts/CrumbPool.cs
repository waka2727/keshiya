using System.Collections.Generic;
using UnityEngine;

namespace Keshiya
{
    public sealed class CrumbPool : MonoBehaviour
    {
        Transform[] pool;
        bool[] live, flying;
        Vector2Int[] cells;
        Vector3[] origins, velocities;
        float[] ages;
        readonly Dictionary<Vector2Int,int> occupancy=new Dictionary<Vector2Int,int>();
        int cursor;
        float carry;
        PrototypeConfig config;
        FeelConfig feel;
        Material material;
        MaterialPropertyBlock tint;
        Color toolColor=new Color(.73f,.72f,.66f);
        float toolSize=1, cohesion=.5f;
        public int Spawned {get;private set;}
        public int ActiveCount {get;private set;}
        public int MaxCellOccupancy {get {int maximum=0;foreach(var pair in occupancy)maximum=Mathf.Max(maximum,pair.Value);return maximum;}}

        public void Initialize(PrototypeConfig c,FeelConfig f,Shader shader)
        {
            config=c;feel=f;int count=Mathf.Max(1,c.crumbCapacity);
            pool=new Transform[count];live=new bool[count];flying=new bool[count];
            cells=new Vector2Int[count];origins=new Vector3[count];velocities=new Vector3[count];ages=new float[count];
            material=new Material(shader){color=new Color(.73f,.72f,.66f)};
            tint=new MaterialPropertyBlock();material.color=Color.white;
        }
        public void SetTool(EraserDefinition definition){toolColor=definition.crumbColor;toolSize=definition.crumbSizeMultiplier;cohesion=definition.crumbCohesion;}
        public void Emit(Vector2 a,Vector2 b,float amount)
        {
            if(amount<=0)return;
            float distance=Vector2.Distance(a,b);
            float spacing=Mathf.Max(.005f,config.crumbSpacing/(amount*Mathf.Max(.01f,feel.crumbDensity)));
            for(float d=spacing-carry;d<=distance;d+=spacing) Spawn(Vector2.Lerp(a,b,d/distance));
            carry=(carry+distance)%spacing;
        }
        Vector2Int Cell(Vector2 p) => new Vector2Int(Mathf.FloorToInt(p.x/Mathf.Max(.05f,feel.crumbCellSize)),Mathf.FloorToInt(p.y/Mathf.Max(.05f,feel.crumbCellSize)));
        void RemoveOccupancy(int slot)
        {
            if(!live[slot] || flying[slot])return;
            int count=occupancy[cells[slot]]-1;
            if(count<=0)occupancy.Remove(cells[slot]);else occupancy[cells[slot]]=count;
        }
        void Spawn(Vector2 p)
        {
            p+=new Vector2(Random.Range(-.22f,.22f),Random.Range(-.22f,.22f));
            var cell=Cell(p);int slot=cursor++%pool.Length;
            if(occupancy.TryGetValue(cell,out int count) && count>=Mathf.Max(1,feel.crumbsPerCell))
            {
                for(int i=0;i<pool.Length;i++)
                {
                    int candidate=(slot+i)%pool.Length;
                    if(live[candidate]&&!flying[candidate]&&cells[candidate]==cell){slot=candidate;break;}
                }
            }
            RemoveOccupancy(slot);
            if(pool[slot]==null)
            {
                var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);Destroy(obj.GetComponent<Collider>());
                obj.name="Reusable eraser crumb";obj.transform.SetParent(transform);
                obj.GetComponent<Renderer>().sharedMaterial=material;pool[slot]=obj.transform;
            }
            if(!live[slot])ActiveCount++;
            live[slot]=true;flying[slot]=false;ages[slot]=0;cells[slot]=cell;
            occupancy.TryGetValue(cell,out count);occupancy[cell]=count+1;
            var t=pool[slot];t.gameObject.SetActive(true);
            t.position=new Vector3(p.x,.072f,p.y);
            t.localScale=new Vector3(Random.Range(.025f,.06f)*Mathf.Lerp(.85f,1.15f,cohesion),.018f,Random.Range(.02f,.035f))*feel.crumbSize*toolSize;
            tint.SetColor("_Color",toolColor);t.GetComponent<Renderer>().SetPropertyBlock(tint);
            t.rotation=Quaternion.Euler(0,Random.Range(0,360),Random.Range(-12,12));Spawned++;
        }
        public bool Blow()
        {
            bool any=false;
            for(int i=0;i<pool.Length;i++)
            {
                if(!live[i]||flying[i])continue;
                RemoveOccupancy(i);flying[i]=true;ages[i]=0;origins[i]=pool[i].position;
                var dir=new Vector3(origins[i].x*.15f+.5f,0,1).normalized;
                velocities[i]=dir*feel.blowDistance;any=true;
            }
            return any;
        }
        void Update()
        {
            for(int i=0;i<pool.Length;i++)
            {
                if(!live[i]||!flying[i])continue;
                ages[i]+=Time.unscaledDeltaTime;
                float t=Mathf.Clamp01(ages[i]/Mathf.Max(.05f,feel.blowDuration));
                pool[i].position=origins[i]+velocities[i]*t+Vector3.up*(Mathf.Sin(t*Mathf.PI)*.24f);
                pool[i].Rotate(0,Time.unscaledDeltaTime*360,0,Space.World);
                if(t>=1){pool[i].gameObject.SetActive(false);live[i]=false;flying[i]=false;ActiveCount--;}
            }
        }
        public void Clear()
        {
            for(int i=0;i<pool.Length;i++){if(pool[i]!=null)pool[i].gameObject.SetActive(false);live[i]=false;flying[i]=false;}
            occupancy.Clear();cursor=0;carry=0;Spawned=0;ActiveCount=0;
        }
        void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
