Shader "Keshiya/FeelOverlay"
{
    Properties { _Color ("Tint", Color) = (1,1,1,1) _ZTest ("Depth comparison", Float) = 4 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest [_ZTest]
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; fixed4 color:COLOR; };
            struct v2f { float4 position:SV_POSITION; fixed4 color:COLOR; };
            fixed4 _Color;
            v2f vert(appdata v) { v2f o; o.position=UnityObjectToClipPos(v.vertex);o.color=v.color*_Color;return o; }
            fixed4 frag(v2f i):SV_Target { return i.color; }
            ENDCG
        }
    }
}
