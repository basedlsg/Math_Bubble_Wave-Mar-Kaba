Shader "XRBubbleLibrary/BubbleGlass"
{
    Properties
    {
        // Cloned from Unity glass shader samples
        _BubbleColor ("Bubble Color", Color) = (1, 0.5, 1, 0.8)
        _FresnelPower ("Fresnel Power", Range(0.1, 5.0)) = 2.0
        _GlowIntensity ("Glow Intensity", Range(0.0, 3.0)) = 1.5
        _Transparency ("Transparency", Range(0.0, 1.0)) = 0.8
        _Refraction ("Refraction", Range(0.0, 1.0)) = 0.1
        
        // Breathing animation properties
        _BreathScale ("Breath Scale", Float) = 1.0
        _BreathOpacity ("Breath Opacity", Float) = 1.0
        
        // Neon-pastel color variations (adapted from Unity color samples)
        _NeonPink ("Neon Pink", Color) = (1, 0.4, 0.8, 0.8)
        _NeonBlue ("Neon Blue", Color) = (0.4, 0.8, 1, 0.8)
        _NeonPurple ("Neon Purple", Color) = (0.8, 0.4, 1, 0.8)
        _NeonTeal ("Neon Teal", Color) = (0.4, 1, 0.8, 0.8)
        
        // Performance optimization for Quest 3 (from Unity mobile samples)
        _LODFade ("LOD Fade", Range(0.0, 1.0)) = 1.0

        // Cymatics visualization
        _CymaticsTex ("Cymatics Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        
        LOD 300
        
        // Cloned from Unity URP transparent shader samples
        Pass
        {
            Name "BubbleGlassForward"
            Tags { "LightMode"="UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Unity URP includes (cloned from Unity samples)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            // Shader properties (cloned structure from Unity glass samples)
            CBUFFER_START(UnityPerMaterial)
                float4 _BubbleColor;
                float _FresnelPower;
                float _GlowIntensity;
                float _Transparency;
                float _Refraction;
                float4 _NeonPink;
                float4 _NeonBlue;
                float4 _NeonPurple;
                float4 _NeonTeal;
                float _LODFade;
                float _BreathScale;
                float _BreathOpacity;
            CBUFFER_END

            TEXTURE2D(_CymaticsTex);
            SAMPLER(sampler_CymaticsTex);
            
            // Vertex input/output structures (cloned from Unity URP samples)
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };
            
            // Vertex shader (cloned from Unity URP vertex shader samples)
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Apply breathing scale to vertex positions
                float3 scaledPosition = input.positionOS.xyz * _BreathScale;
                
                // Transform positions (Unity URP standard transformations)
                VertexPositionInputs positionInputs = GetVertexPositionInputs(scaledPosition);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                
                // Transform normals (Unity URP standard normal transformation)
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInputs.normalWS;
                
                // Calculate view direction (cloned from Unity view calculation samples)
                output.viewDirWS = GetWorldSpaceViewDir(output.positionWS);
                
                output.uv = input.uv;
                
                return output;
            }
            
            // Fresnel calculation (cloned from Unity fresnel shader samples)
            float CalculateFresnel(float3 normal, float3 viewDir, float power)
            {
                float fresnel = 1.0 - saturate(dot(normalize(normal), normalize(viewDir)));
                return pow(fresnel, power);
            }
            
            // Neon color blending (adapted from Unity color blending samples)
            float4 BlendNeonColors(float2 uv, float time)
            {
                // Create color variation based on UV and time (Unity animation samples)
                float colorMix = sin(uv.x * 3.14159 + time) * 0.5 + 0.5;
                
                // Blend between neon colors (Unity color interpolation samples)
                float4 color1 = lerp(_NeonPink, _NeonBlue, colorMix);
                float4 color2 = lerp(_NeonPurple, _NeonTeal, colorMix);
                
                return lerp(color1, color2, sin(uv.y * 3.14159 + time * 0.5) * 0.5 + 0.5);
            }
            
            // Fragment shader (cloned and adapted from Unity glass fragment samples)
            float4 frag(Varyings input) : SV_Target
            {
                // Normalize vectors (Unity standard practice)
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                
                // Calculate fresnel effect (cloned from Unity fresnel samples)
                float fresnel = CalculateFresnel(normalWS, viewDirWS, _FresnelPower);
                
                // Get animated time for color variation (Unity time samples)
                float time = _Time.y;
                
                // Blend neon colors (adapted from Unity color samples)
                float4 neonColor = BlendNeonColors(input.uv, time);
                
                // Sample Cymatics texture
                float4 cymatics = SAMPLE_TEXTURE2D(_CymaticsTex, sampler_CymaticsTex, input.uv);

                // Combine base color with neon variation (Unity color blending)
                float4 finalColor = lerp(_BubbleColor, neonColor, 0.5);

                // Blend in Cymatics pattern
                finalColor.rgb = lerp(finalColor.rgb, cymatics.rgb, cymatics.a * 0.5);
                
                // Apply fresnel to create glass-like rim lighting (Unity glass effect)
                finalColor.rgb += fresnel * _GlowIntensity * finalColor.rgb;
                
                // Apply transparency with breathing opacity
                finalColor.a *= _Transparency * _LODFade * _BreathOpacity;
                
                // Ensure alpha is properly set for transparency
                finalColor.a = saturate(finalColor.a);
                
                return finalColor;
            }
            
            ENDHLSL
        }
        
        // Shadow pass (cloned from Unity URP shadow samples)
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            
            ENDHLSL
        }
    }
    
    // Fallback for older hardware (Unity standard practice)
    FallBack "Universal Render Pipeline/Lit"
}
