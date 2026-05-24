Shader "Custom/SlimeFresnel"
{
    Properties
    {
        _Color ("Color", Color) = (0.2, 0.85, 0.2, 0.6)
        _FresnelPower ("Fresnel Power", Range(0.1, 5.0)) = 1.8
        _Opacity ("Opacity Base", Range(0, 1)) = 0.6
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
            };

            fixed4 _Color;
            float  _FresnelPower;
            float  _Opacity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos         = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos    = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal  = normalize(i.worldNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

                // Fresnel: borde = 0 (mas transparente), centro = 1 (mas opaco)
                float NdotV  = saturate(dot(normal, viewDir));
                float fresnel = 1.0 - NdotV;
                float alpha  = _Opacity * pow(fresnel, _FresnelPower);

                return fixed4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
