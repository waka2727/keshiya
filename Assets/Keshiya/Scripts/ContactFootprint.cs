using UnityEngine;

namespace Keshiya
{
    /// <summary>Shared contact geometry for the sole mesh and stroke sampling.</summary>
    public readonly struct ContactFootprint
    {
        public readonly Vector2 HalfSize, Offset;
        public readonly float Exponent, Angle;
        readonly float cosine, sine;
        public ContactFootprint(Vector2 halfSize, float exponent = 4, Vector2 offset = default, float angle = 0)
        {
            HalfSize = new Vector2(Mathf.Max(.005f, halfSize.x), Mathf.Max(.005f, halfSize.y));
            Exponent = Mathf.Clamp(exponent, 2, 8); Offset = offset; Angle = angle;
            cosine = Mathf.Cos(angle * Mathf.Deg2Rad); sine = Mathf.Sin(angle * Mathf.Deg2Rad);
        }
        // n=4 area=3.70814935*a*b. Retain pi*r*r and pressure integral=area/2.
        public static ContactFootprint BroadFace(float radius) =>
            new ContactFootprint(new Vector2(radius, radius * .8472131f));
        public Vector2 Bounds => new Vector2(
            Mathf.Abs(cosine)*HalfSize.x + Mathf.Abs(sine)*HalfSize.y,
            Mathf.Abs(sine)*HalfSize.x + Mathf.Abs(cosine)*HalfSize.y);
        public static Vector2 PixelCenter(int x,int y,int width,int height,Vector2 size)=>new Vector2(((x+.5f)/width-.5f)*size.x,((y+.5f)/height-.5f)*size.y);
        public float PixelWeight(int x,int y,int width,int height,Vector2 size,Vector2 position)=>Weight(PixelCenter(x,y,width,height,size)-position);
        public float SampleSpacing => Mathf.Min(HalfSize.x, HalfSize.y) * .22f;
        public float Weight(Vector2 relative)
        {
            relative -= Offset;
            float x = Mathf.Abs((cosine*relative.x + sine*relative.y)/HalfSize.x);
            float y = Mathf.Abs((-sine*relative.x + cosine*relative.y)/HalfSize.y);
            if (x>=1 || y>=1) return 0;
            float radialSquared;
            if (Exponent==4) radialSquared = Mathf.Sqrt(x*x*x*x + y*y*y*y);
            else if (Exponent==2) radialSquared = x*x+y*y;
            else radialSquared = Mathf.Pow(Mathf.Pow(x,Exponent)+Mathf.Pow(y,Exponent),2/Exponent);
            return Mathf.Max(0,1-radialSquared);
        }
        public Vector2 OutlinePoint(float radians)
        {
            float x=Mathf.Cos(radians), y=Mathf.Sin(radians);
            x=Mathf.Sign(x)*Mathf.Pow(Mathf.Abs(x),2/Exponent)*HalfSize.x;
            y=Mathf.Sign(y)*Mathf.Pow(Mathf.Abs(y),2/Exponent)*HalfSize.y;
            return Offset+new Vector2(cosine*x-sine*y,sine*x+cosine*y);
        }
    }
}
