using UnityEngine;
namespace Keshiya
{
    public sealed class CrumbEconomyView:MonoBehaviour
    {
        public PrototypeGame Game;
        LineRenderer[] lines;float[] shownLengths;Vector2[] shownEnds;long[] shownIds;Material strandMaterial,ballMaterial;Transform ball;
        readonly Vector3[] points=new Vector3[32];
        public int LineCapacity=>lines?.Length??0;
        public float BallScale=>ball==null?0:ball.localScale.x;
        public int VisibleLines {get;private set;}
        public void Initialize(PrototypeGame game)
        {
            Game=game;int capacity=Mathf.Max(1,game.Economy.Config.paperPieceCapacity)*2;lines=new LineRenderer[capacity];shownLengths=new float[capacity];shownEnds=new Vector2[capacity];shownIds=new long[capacity];
            strandMaterial=new Material(game.OverlayShader);strandMaterial.color=Color.white;
            for(int i=0;i<capacity;i++){
                var line=new GameObject("Pooled crumb strand").AddComponent<LineRenderer>();line.transform.SetParent(transform);line.sharedMaterial=strandMaterial;
                line.positionCount=points.Length;line.numCapVertices=3;line.useWorldSpace=true;line.enabled=false;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;lines[i]=line;
            }
            var obj=GameObject.CreatePrimitive(PrimitiveType.Sphere);Destroy(obj.GetComponent<Collider>());ball=obj.transform;ball.SetParent(transform);obj.name="Collected crumb ball";
            ballMaterial=new Material(game.ObjectShader);ballMaterial.color=new Color(.56f,.59f,.49f);obj.GetComponent<Renderer>().sharedMaterial=ballMaterial;
        }
        void LateUpdate()
        {
            if(Game==null)return;var economy=Game.Economy;int slot=0;
            foreach(var item in economy.Paper){if(slot>=lines.Length)break;Draw(lines[slot],item,0,slot);slot++;}
            float blowAge=economy.LastBlowAge;
            if(economy.RescueRemaining>0&&blowAge<.55f)foreach(var item in economy.Blown){if(slot>=lines.Length)break;Draw(lines[slot],item,blowAge/.55f,slot);slot++;}
            VisibleLines=slot;for(int i=slot;i<lines.Length;i++)lines[i].enabled=false;
            float scale=Mathf.Clamp(economy.Ball.DiameterCm*.19f,.08f,.65f);ball.gameObject.SetActive(economy.Ball.Grams>0);ball.localScale=Vector3.one*scale;ball.position=new Vector3(3.82f,scale*.5f-.04f,-1.9f);
        }
        void Draw(LineRenderer line,CrumbPiece item,float flying,int slot)
        {
            float target=item.LengthCm*Game.Economy.Config.worldUnitsPerCm;
            if(shownIds[slot]!=item.Id||item.State!=CrumbState.Growing){shownIds[slot]=item.Id;shownLengths[slot]=target;shownEnds[slot]=item.End;}
            else {float blend=1-Mathf.Exp(-20*Time.unscaledDeltaTime);shownLengths[slot]=Mathf.Lerp(shownLengths[slot],target,blend);shownEnds[slot]=Vector2.Lerp(shownEnds[slot],item.End,blend);}
            float length=shownLengths[slot];
            Vector2 normal=new Vector2(-item.Direction.y,item.Direction.x);
            for(int i=0;i<points.Length;i++){
                float t=i/(float)(points.Length-1);Vector2 p=shownEnds[slot]-item.Direction*(length*(1-t))+normal*(Mathf.Sin(t*Mathf.PI*2)*Mathf.Min(.035f,length*.06f));
                points[i]=new Vector3(p.x+flying*.4f,.085f+Mathf.Sin(flying*Mathf.PI)*.25f,p.y+flying*2.5f);
            }
            Color color=Color.Lerp(item.Color,new Color(.32f,.37f,.28f),item.LengthCm>=20?.2f:0);color=Color.Lerp(color,new Color(.75f,.57f,.39f),item.Tension*.45f);color.a=(.88f-item.Tension*.2f)*(1-flying);
            line.enabled=true;line.startColor=color;line.endColor=color;line.widthMultiplier=item.Thickness*(1-item.Tension*.38f);line.SetPositions(points);
        }
        void OnDestroy(){if(strandMaterial!=null)Destroy(strandMaterial);if(ballMaterial!=null)Destroy(ballMaterial);}
    }
}
