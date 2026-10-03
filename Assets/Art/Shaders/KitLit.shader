// Minimal lit shader for the Kenney kits: palette texture * tint, half-Lambert from the main
// directional light plus flat ambient. No reflections, shadows or fog, so no skybox cubemap
// or extra variants end up in the WebGL build.
Shader "Scrambly/KitLit"
{
    Properties
    {
        _MainTex ("Palette", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Flash ("Hit Flash", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed _Flash;
            fixed4 _LightColor0;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed3 light : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float3 n = UnityObjectToWorldNormal(v.normal);
                float halfLambert = dot(n, normalize(_WorldSpaceLightPos0.xyz)) * 0.5 + 0.5;
                o.light = UNITY_LIGHTMODEL_AMBIENT.rgb + _LightColor0.rgb * halfLambert * halfLambert;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * _Color;
                c.rgb *= i.light;
                c.rgb = lerp(c.rgb, 1, _Flash);
                return c;
            }
            ENDCG
        }
    }
}
