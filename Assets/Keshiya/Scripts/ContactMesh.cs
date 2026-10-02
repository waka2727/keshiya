using UnityEngine;

namespace Keshiya
{
    public static class ContactMesh
    {
        public const int Segments=64;
        // Mesh boundary and collision sampling both derive from OutlinePoint/Weight.
        public static Mesh Solid(ContactFootprint contact,float height)
        {
            // Separate cap vertices prevent side normals from making the flat
            // plastic top look like a shiny dome.
            var vertices=new Vector3[Segments*4+2];
            var triangles=new int[Segments*12];
            for(int i=0;i<Segments;i++)
            {
                Vector2 p=contact.OutlinePoint(i*Mathf.PI*2/Segments);
                vertices[i]=new Vector3(p.x,0,p.y);
                vertices[i+Segments]=new Vector3(p.x,height,p.y);
                vertices[i+Segments*2]=vertices[i];
                vertices[i+Segments*3]=vertices[i+Segments];
            }
            vertices[Segments*4]=new Vector3(contact.Offset.x,0,contact.Offset.y);
            vertices[Segments*4+1]=new Vector3(contact.Offset.x,height,contact.Offset.y);
            for(int i=0;i<Segments;i++)
            {
                int j=(i+1)%Segments, t=i*12;
                triangles[t]=Segments*4; triangles[t+1]=i+Segments*2; triangles[t+2]=j+Segments*2;
                triangles[t+3]=Segments*4+1; triangles[t+4]=j+Segments*3; triangles[t+5]=i+Segments*3;
                triangles[t+6]=i; triangles[t+7]=i+Segments; triangles[t+8]=j;
                triangles[t+9]=j; triangles[t+10]=i+Segments; triangles[t+11]=j+Segments;
            }
            var mesh=new Mesh { name="Contact-profile eraser",vertices=vertices,triangles=triangles };
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        public static Mesh Shadow(ContactFootprint contact)
        {
            var v=new Vector3[Segments+1]; var c=new Color[v.Length]; var t=new int[Segments*3];
            c[0]=new Color(0,0,0,.3f);
            v[0]=new Vector3(contact.Offset.x,0,contact.Offset.y);
            for(int i=0;i<Segments;i++)
            {
                Vector2 p=contact.OutlinePoint(i*Mathf.PI*2/Segments);
                v[i+1]=new Vector3(p.x,0,p.y); c[i+1]=new Color(0,0,0,0);
                t[i*3]=0; t[i*3+1]=(i+1)%Segments+1; t[i*3+2]=i+1;
            }
            return new Mesh { name="Soft contact shadow",vertices=v,colors=c,triangles=t };
        }
    }
}
