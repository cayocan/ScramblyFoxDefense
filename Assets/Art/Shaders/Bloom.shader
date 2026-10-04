// Light bloom for the built-in pipeline (OnRenderImage): bright-pass at low resolution, a small blur, then an
// additive combine. Three cheap passes so it stays light on phones. Used by Scrambly/Bloom (BloomEffect.cs).
Shader "Hidden/Scrambly/Bloom"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _BloomTex ("Bloom", 2D) = "black" {}
        _Threshold ("Threshold", Range(0, 1)) = 0.75
        _Intensity ("Intensity", Range(0, 2)) = 0.35
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;
    sampler2D _BloomTex;
    half _Threshold;
    half _Intensity;

    struct v2f
    {
        float4 pos : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    v2f vert (appdata_img v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord;
        return o;
    }

    // 4-tap box downsample that keeps only what is brighter than the threshold (soft knee).
    half4 fragPrefilter (v2f i) : SV_Target
    {
        float2 d = _MainTex_TexelSize.xy;
        half3 c = tex2D(_MainTex, i.uv + d * float2(-0.5, -0.5)).rgb
                + tex2D(_MainTex, i.uv + d * float2( 0.5, -0.5)).rgb
                + tex2D(_MainTex, i.uv + d * float2(-0.5,  0.5)).rgb
                + tex2D(_MainTex, i.uv + d * float2( 0.5,  0.5)).rgb;
        c *= 0.25;
        half brightness = max(c.r, max(c.g, c.b));
        half contribution = saturate((brightness - _Threshold) / max(1e-4, 1 - _Threshold));
        return half4(c * contribution, 1);
    }

    // 9-tap tent blur.
    half4 fragBlur (v2f i) : SV_Target
    {
        float2 d = _MainTex_TexelSize.xy * 1.5;
        half3 c = tex2D(_MainTex, i.uv).rgb * 4;
        c += (tex2D(_MainTex, i.uv + float2(d.x, 0)).rgb + tex2D(_MainTex, i.uv - float2(d.x, 0)).rgb
            + tex2D(_MainTex, i.uv + float2(0, d.y)).rgb + tex2D(_MainTex, i.uv - float2(0, d.y)).rgb) * 2;
        c += tex2D(_MainTex, i.uv + d).rgb + tex2D(_MainTex, i.uv - d).rgb
           + tex2D(_MainTex, i.uv + float2(d.x, -d.y)).rgb + tex2D(_MainTex, i.uv + float2(-d.x, d.y)).rgb;
        return half4(c / 16, 1);
    }

    half4 fragCombine (v2f i) : SV_Target
    {
        half4 src = tex2D(_MainTex, i.uv);
        src.rgb += tex2D(_BloomTex, i.uv).rgb * _Intensity;
        return src;
    }
    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass { CGPROGRAM
               #pragma vertex vert
               #pragma fragment fragPrefilter
               ENDCG }
        Pass { CGPROGRAM
               #pragma vertex vert
               #pragma fragment fragBlur
               ENDCG }
        Pass { CGPROGRAM
               #pragma vertex vert
               #pragma fragment fragCombine
               ENDCG }
    }
}
