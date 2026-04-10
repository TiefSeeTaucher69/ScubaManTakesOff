Shader "Custom/SpriteBark"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _BarkTex ("Bark Texture", 2D) = "white" {}
        _BarkTiling ("Bark Tiling", Float) = 3.0
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"          = "Transparent"
            "RenderType"     = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Cull Off
        ZWrite Off

        // ── Pass 1: URP 2D Lit Renderer ────────────────────────────────────
        // Wird aufgerufen wenn das Sprite in einem beleuchteten Sorting-Layer ist.
        Pass
        {
            Name "SpriteBark_2D"
            Tags { "LightMode" = "Universal2D" }

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_BarkTex); SAMPLER(sampler_BarkTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BarkTex_ST;
                float  _BarkTiling;
                float4 _Color;
            CBUFFER_END

            struct Attributes { float4 pos:POSITION; float2 uv:TEXCOORD0; float4 col:COLOR; };
            struct Varyings   { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float2 barkUV:TEXCOORD1; float4 col:COLOR; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.pos    = TransformObjectToHClip(IN.pos.xyz);
                OUT.uv     = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.barkUV = TRANSFORM_TEX(IN.uv, _BarkTex) * _BarkTiling;
                OUT.col    = IN.col;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 sprite = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 bark   = SAMPLE_TEXTURE2D(_BarkTex, sampler_BarkTex, IN.barkUV);
                return half4(bark.rgb * _Color.rgb * IN.col.rgb, sprite.a * _Color.a * IN.col.a);
            }
            ENDHLSL
        }


    }
}
