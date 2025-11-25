Shader "Custom/CreatureURP"
{
    Properties
    {
        [Header(Base Properties)]
        _BaseColor ("Base Color", Color) = (0.3, 0.7, 1.0, 0.6)
        _MainTex ("Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.8
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.1
        
        [Header(Transparency)]
        _Alpha ("Alpha", Range(0.0, 1.0)) = 0.6
        
        [Header(Vertex Animation)]
        _WaveSpeed ("Wave Speed", Float) = 2.0
        _WaveAmplitude ("Wave Amplitude", Float) = 0.1
        _WaveFrequency ("Wave Frequency", Float) = 3.0
        _EnergyFlowSpeed ("Energy Flow Speed", Float) = 1.0
        
        [Header(Fresnel Rim Light)]
        _FresnelColor ("Fresnel Color", Color) = (0.5, 0.9, 1.0, 1.0)
        _FresnelPower ("Fresnel Power", Range(0.1, 10.0)) = 3.0
        _FresnelIntensity ("Fresnel Intensity", Range(0.0, 5.0)) = 2.0
        
        [Header(Emission and Glow)]
        _EmissionColor ("Emission Color", Color) = (0.3, 0.7, 1.0, 1.0)
        _EmissionIntensity ("Emission Intensity", Range(0.0, 5.0)) = 1.0
        _GlowPulseSpeed ("Glow Pulse Speed", Float) = 1.5
        _GlowPulseAmount ("Glow Pulse Amount", Range(0.0, 1.0)) = 0.3
        
        [Header(Energy Flow)]
        _EnergyFlowColor ("Energy Flow Color", Color) = (0.4, 0.8, 1.0, 1.0)
        _EnergyFlowIntensity ("Energy Flow Intensity", Range(0.0, 2.0)) = 0.5
        
        [Header(Texture Animation)]
        _TexRotationSpeed ("Texture Rotation Speed", Float) = 0.0
        _TexScrollSpeedX ("Texture Scroll Speed X", Float) = 0.0
        _TexScrollSpeedY ("Texture Scroll Speed Y", Float) = 0.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 300
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                float fogFactor : TEXCOORD4;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _MainTex_ST;
                float _Smoothness;
                float _Metallic;
                float _Alpha;
                
                float _WaveSpeed;
                float _WaveAmplitude;
                float _WaveFrequency;
                float _EnergyFlowSpeed;
                
                float4 _FresnelColor;
                float _FresnelPower;
                float _FresnelIntensity;
                
                float4 _EmissionColor;
                float _EmissionIntensity;
                float _GlowPulseSpeed;
                float _GlowPulseAmount;
                
                float4 _EnergyFlowColor;
                float _EnergyFlowIntensity;
                
                float _TexRotationSpeed;
                float _TexScrollSpeedX;
                float _TexScrollSpeedY;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Vertex animation - wave effect
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float time = _Time.y;
                
                // Multi-directional waves
                float waveX = sin((positionWS.x + time * _WaveSpeed) * _WaveFrequency) * _WaveAmplitude;
                float waveY = sin((positionWS.y + time * _WaveSpeed * 0.7) * _WaveFrequency) * _WaveAmplitude * 0.5;
                float waveZ = sin((positionWS.z + time * _WaveSpeed * 0.5) * _WaveFrequency) * _WaveAmplitude * 0.3;
                
                // Energy flow effect (from bottom to top)
                float energyWave = sin((positionWS.y + time * _EnergyFlowSpeed) * 4.0) * 0.05;
                
                // Apply deformation
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                positionWS += normalWS * (waveX + waveY + waveZ);
                positionWS.y += energyWave;
                
                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.normalWS = normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                
                // Apply UV tiling and offset
                float2 uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                // UV rotation
                float angle = time * _TexRotationSpeed;
                float cosAngle = cos(angle);
                float sinAngle = sin(angle);
                float2 pivot = float2(0.5, 0.5); // Rotate around center
                uv -= pivot;
                float2 rotatedUV;
                rotatedUV.x = uv.x * cosAngle - uv.y * sinAngle;
                rotatedUV.y = uv.x * sinAngle + uv.y * cosAngle;
                uv = rotatedUV + pivot;
                
                // UV translation
                uv += float2(_TexScrollSpeedX, _TexScrollSpeedY) * time;
                
                output.uv = uv;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Base color
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _BaseColor;
                
                // Energy flow effect
                float time = _Time.y;
                float energyFlow = sin(input.positionWS.y * 4.0 + time * _EnergyFlowSpeed * 2.0) * 0.5 + 0.5;
                half3 energyColor = lerp(baseColor.rgb, _EnergyFlowColor.rgb, energyFlow * _EnergyFlowIntensity);
                
                // Fresnel rim light
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);
                half3 fresnelGlow = fresnel * _FresnelColor.rgb * _FresnelIntensity;
                
                // Glow pulse
                float glowPulse = sin(time * _GlowPulseSpeed) * _GlowPulseAmount + (1.0 - _GlowPulseAmount);
                half3 emission = _EmissionColor.rgb * _EmissionIntensity * glowPulse;
                
                // Combine all effects
                half3 finalColor = energyColor + fresnelGlow + emission;
                
                // Simple lighting
                Light mainLight = GetMainLight();
                half3 lighting = mainLight.color * mainLight.distanceAttenuation;
                finalColor *= (lighting * 0.5 + 0.5); // Soft lighting
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, _Alpha);
            }
            ENDHLSL
        }
        
        // Shadow casting Pass (optional)
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}

