Shader "Keshiya/LayeredPaper" {
 Properties { _Paper("Paper",2D)="white"{} _Protected("Protected",2D)="black"{} _Erasable("Erasable",2D)="black"{} _State("State",2D)="white"{} _TearThreshold("Tear",Float)=0.65 }
 SubShader { Tags {"RenderType"="Opaque"} Pass { ZWrite On Cull Off
 CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _Paper,_Protected,_Erasable,_State; float _TearThreshold;
 fixed4 frag(v2f_img i):SV_Target {
  float4 state=tex2D(_State,i.uv);float4 paper=tex2D(_Paper,i.uv);float4 ink=tex2D(_Protected,i.uv),pencil=tex2D(_Erasable,i.uv);
  float3 color=lerp(paper.rgb,1,state.b*.6);
  color=lerp(color,pencil.rgb,pencil.a*state.r);color=lerp(color,ink.rgb,ink.a*state.g);
  float ripple=abs(sin(i.uv.y*350)*.3+frac(i.uv.x*170)-.5);
  if(state.b>_TearThreshold&&ripple<.08)color=float3(.22,.16,.10);
  else if(state.b>_TearThreshold&&ripple<.12)color=float3(.99,.97,.89);
  if(state.a>.01&&state.r>.01)color=lerp(color,float3(.85,.48,.1),state.a*pencil.a*.45);
  return float4(color,1);
 }
 ENDCG
 } }
}
