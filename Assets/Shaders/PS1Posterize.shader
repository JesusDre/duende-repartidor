// Pase de pantalla completa: reduce los colores por canal con dithering ordenado (Bayer 4x4),
// como la salida de 15 bits con dithering de la PlayStation 1.
Shader "Duende/PS1Posterize"
{
    Properties
    {
        _Niveles ("Niveles por canal", Float) = 24
        _Dither ("Intensidad del dithering", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off Cull Off ZTest Always

        Pass
        {
            Name "PS1Posterize"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Niveles;
            float _Dither;

            static const float Bayer[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };

            half4 frag(Varyings i) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, i.texcoord);
                uint2 p = uint2(i.positionCS.xy) % 4u;
                float d = (Bayer[p.y * 4u + p.x] / 16.0 - 0.5) * _Dither;
                float n = max(_Niveles - 1.0, 1.0);
                c.rgb = saturate(floor(c.rgb * n + 0.5 + d) / n);
                return c;
            }
            ENDHLSL
        }
    }
}
