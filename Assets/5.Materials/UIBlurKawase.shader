// UIBlurFeature 전용. Dual Kawase(ARM, SIGGRAPH 2015) 다운/업샘플 커널.
// 바이리니어 필터에 기대어 탭 수를 줄이므로 가우시안 대비 대역폭이 훨씬 적다.
Shader "Hidden/GRstory/UIBlurKawase"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float _Offset; // 샘플 간격(소스 텍셀 단위)

        #define SAMPLE(uv) SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv)

        // 절반 해상도로 내리면서 5탭
        half4 FragDown(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 uv = input.texcoord;
            float2 o = _BlitTexture_TexelSize.xy * _Offset;

            half4 sum = SAMPLE(uv) * 4.0;
            sum += SAMPLE(uv - o);
            sum += SAMPLE(uv + o);
            sum += SAMPLE(uv + float2(o.x, -o.y));
            sum += SAMPLE(uv - float2(o.x, -o.y));
            return sum * 0.125;
        }

        // 두 배 해상도로 올리면서 8탭
        half4 FragUp(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 uv = input.texcoord;
            float2 o = _BlitTexture_TexelSize.xy * _Offset;

            half4 sum = SAMPLE(uv + float2(-o.x * 2.0, 0.0));
            sum += SAMPLE(uv + float2(-o.x, o.y)) * 2.0;
            sum += SAMPLE(uv + float2(0.0, o.y * 2.0));
            sum += SAMPLE(uv + float2(o.x, o.y)) * 2.0;
            sum += SAMPLE(uv + float2(o.x * 2.0, 0.0));
            sum += SAMPLE(uv + float2(o.x, -o.y)) * 2.0;
            sum += SAMPLE(uv + float2(0.0, -o.y * 2.0));
            sum += SAMPLE(uv + float2(-o.x, -o.y)) * 2.0;
            return sum / 12.0;
        }
        ENDHLSL

        Pass
        {
            Name "Downsample"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragDown
            ENDHLSL
        }

        Pass
        {
            Name "Upsample"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragUp
            ENDHLSL
        }
    }
}
