// Shader estilo PlayStation 1 para URP:
//  - Vertex snapping: los vértices se redondean a una rejilla de baja resolución (tiemblan al moverse)
//  - Affine texture mapping: las texturas se deforman como en el hardware de PS1
//  - Iluminación por vértice (Gouraud) con la luz principal, luces adicionales y ambiente
//  - Niebla de URP
Shader "Duende/PS1Lit"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _BaseMap ("Textura", 2D) = "white" {}
        [HDR] _EmissionColor ("Emisión", Color) = (0, 0, 0, 0)
        _Snap ("Resolución de vértices (líneas)", Float) = 120
        _Affine ("Deformación afín", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _BaseMap_ST;
            float4 _EmissionColor;
            float _Snap;
            float _Affine;
        CBUFFER_END

        // Redondea la posición en pantalla a una rejilla de _Snap líneas
        float4 SnapClip(float4 cs)
        {
            float2 grid = float2(_Snap * (_ScreenParams.x / _ScreenParams.y), _Snap) * 0.5;
            cs.xy = floor(cs.xy / cs.w * grid + 0.5) / grid * cs.w;
            return cs;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 uvw : TEXCOORD0;      // uv * w para el mapeo afín
                half3 luz : TEXCOORD1;
                half fog : TEXCOORD2;
            };

            Varyings vert(Attributes i)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(i);
                float3 posWS = TransformObjectToWorld(i.positionOS.xyz);
                float3 normalWS = normalize(TransformObjectToWorldNormal(i.normalOS));
                float4 cs = TransformWorldToHClip(posWS);
                o.fog = ComputeFogFactor(cs.z);
                cs = SnapClip(cs);
                o.positionCS = cs;

                float2 uv = TRANSFORM_TEX(i.uv, _BaseMap);
                float w = lerp(1.0, cs.w, _Affine);
                o.uvw = float3(uv * w, w);

                // Iluminación Gouraud
                Light principal = GetMainLight();
                half3 luz = SampleSH(normalWS) + principal.color * saturate(dot(normalWS, principal.direction));
                #if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX)
                uint total = GetAdditionalLightsCount();
                for (uint li = 0u; li < total; li++)
                {
                    Light extra = GetAdditionalLight(li, posWS);
                    luz += extra.color * extra.distanceAttenuation * saturate(dot(normalWS, extra.direction) * 0.7 + 0.3);
                }
                #endif
                o.luz = luz;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uvw.xy / i.uvw.z;
                half4 c = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv) * _BaseColor;
                half3 color = c.rgb * i.luz + _EmissionColor.rgb;
                color = MixFog(color, i.fog);
                return half4(color, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes i)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(i);
                o.positionCS = SnapClip(TransformObjectToHClip(i.positionOS.xyz));
                return o;
            }

            half frag(Varyings i) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
