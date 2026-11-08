Shader "Custom/MilkyWayFade"
{
    Properties
    {
        _Cube ("Milky Way Cubemap", Cube) = "" {}
        _CloudColor ("Fade Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent-10"
            "RenderType" = "Transparent"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            samplerCUBE _Cube;
            fixed4 _CloudColor;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 direction : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.direction = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 galaxy = texCUBE(
                    _Cube,
                    normalize(i.direction)
                );

                galaxy.rgb *= _CloudColor.rgb;
                galaxy.a = _CloudColor.a;

                return galaxy;
            }
            ENDCG
        }
    }
}