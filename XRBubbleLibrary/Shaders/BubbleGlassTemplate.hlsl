// Bubble Glass Shader Template - HLSL Code
// This template shows the structure for the Shader Graph implementation
// Clone Unity glass samples and modify using these patterns

Shader "XRBubbleLibrary/BubbleGlass"
{
    Properties
    {
        [Header(Base Glass Properties)]
        _BaseColor("Base Color", Color) = (0.8, 0.4, 1.0, 0.7)
        _Transparency("Transparency", Range(0, 1)) = 0.7
        _Refraction("Refraction Strength", Range(0, 1)) = 0.3
        _FresnelPower("Fresnel Power", Range(0, 5)) = 2.0
        _NormalMap("Normal Map", 2D) = "bump" {}
        
        [Header(Neon Glow System)]
        [HDR] _GlowColor("Glow Color", Color) = (1.0, 0.4, 0.8, 2.0)
        _GlowIntensity("Glow Intensity", Range(0, 2)) = 0.6
        _GlowFalloff("Glow Falloff", Range(0.5, 2)) = 1.0
        _UnderlightStrength("Underlight Strength", Range(0, 1)) = 0.8
        
        [Header(Breathing Animation)]
        _BreathingSpeed("Breathing Speed", Range(0.2, 0.5)) = 0.3
        _BreathingIntensity("Breathing Intensity", Range(0, 0.5)) = 0.2
        _PhaseOffset("Phase Offset", Range(0, 6.28)) = 0
        
        [Header(Performance Optimization)]
        _LODLevel("LOD Level", Range(0, 2)) = 0
        _DistanceFade("Distance Fade", Range(1, 10)) = 5
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        
        LOD 200
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            // Properties
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Transparency;
                float _Refraction;
                float _FresnelPower;
                float4 _GlowColor;
                float _GlowIntensity;
                float _GlowFalloff;
                float _UnderlightStrength;
                float _BreathingSpeed;
                float _BreathingIntensity;
                float _PhaseOffset;
                float _LODLevel;
                float _DistanceFade;
            CBUFFER_END
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float3 tangentWS : TEXCOORD4;
                float3 bitangentWS : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.uv = input.uv;
                
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Sample normal map
                float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                float3x3 tangentToWorld = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                float3 normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
                normalWS = normalize(normalWS);
                
                // Calculate view direction
                float3 viewDirWS = normalize(input.viewDirWS);
                
                // Fresnel calculation for glass effect
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);
                
                // Breathing animation using time
                float breathingTime = _Time.y * _BreathingSpeed + _PhaseOffset;
                float breathingPulse = sin(breathingTime) * _BreathingIntensity;
                
                // Base glass color with neon-pastel tint
                float4 baseColor = _BaseColor;
                
                // Glow calculation with breathing animation
                float glowStrength = _GlowIntensity + breathingPulse;
                float4 glowColor = _GlowColor * glowStrength;
                
                // Combine fresnel with glow for edge lighting
                float edgeGlow = fresnel * _UnderlightStrength;
                
                // Final color combination
                float4 finalColor = baseColor;
                finalColor.rgb += glowColor.rgb * edgeGlow;
                finalColor.a = _Transparency * (1.0 + fresnel * 0.3);
                
                // Distance-based LOD (simple version)
                float distance = length(input.positionWS - _WorldSpaceCameraPos);
                float lodFactor = saturate(distance / _DistanceFade);
                finalColor.rgb = lerp(finalColor.rgb, baseColor.rgb, lodFactor * _LODLevel * 0.5);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    // Fallback for older hardware
    FallBack "Universal Render Pipeline/Lit"
}