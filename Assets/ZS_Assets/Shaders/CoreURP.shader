Shader "Custom/CoreURP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.3, 0.7, 1.0, 0.6)
        _Transparency ("Transparency", Range(0.0, 1.0)) = 0.4
        _WaveSpeed ("Wave Speed", Float) = 2.0
        _WaveAmount ("Wave Amount", Float) = 0.1
        _EmissionIntensity ("Emission Intensity", Float) = 0.5
        _EdgeGlow ("Edge Glow", Float) = 1.0
        
        [Header(Texture Animation)]
        _TexRotationSpeed ("Texture Rotation Speed", Float) = 0.0
        _TexScrollSpeedX ("Texture Scroll Speed X", Float) = 0.0
        _TexScrollSpeedY ("Texture Scroll Speed Y", Float) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float edgeGlow : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST; // Add UV tiling and offset support
            float4 _Color;
            float _Transparency;
            float _WaveSpeed;
            float _WaveAmount;
            float _EmissionIntensity;
            float _EdgeGlow;
            float _TexRotationSpeed;
            float _TexScrollSpeedX;
            float _TexScrollSpeedY;

            v2f vert (appdata v)
            {
                v2f o;

                // Add wave effect
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float wave = sin(worldPos.y * 3.0 + _Time.y * _WaveSpeed) * _WaveAmount;
                float3 newPos = v.vertex.xyz + v.normal * wave;

                o.vertex = UnityObjectToClipPos(float4(newPos, 1.0));
                
                // Apply UV tiling and offset
                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // UV rotation
                float angle = _Time.y * _TexRotationSpeed;
                float cosAngle = cos(angle);
                float sinAngle = sin(angle);
                float2 pivot = float2(0.5, 0.5); // Rotate around center
                uv -= pivot;
                float2 rotatedUV;
                rotatedUV.x = uv.x * cosAngle - uv.y * sinAngle;
                rotatedUV.y = uv.x * sinAngle + uv.y * cosAngle;
                uv = rotatedUV + pivot;
                
                // UV translation
                uv += float2(_TexScrollSpeedX, _TexScrollSpeedY) * _Time.y;
                
                o.uv = uv;
                o.worldPos = worldPos;
                o.normal = UnityObjectToWorldNormal(v.normal);

                // Calculate edge glow
                float3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                o.edgeGlow = 1.0 - saturate(dot(viewDir, o.normal));

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Base color
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                // Add energy flow effect
                float energyPulse = sin(i.worldPos.y * 4.0 + _Time.y * _WaveSpeed * 2.0) * 0.3 + 0.7;
                col.rgb *= energyPulse;

                // Edge glow
                float edgeGlow = pow(i.edgeGlow, 2.0) * _EdgeGlow;
                col.rgb += edgeGlow * _Color.rgb * _EmissionIntensity;

                // Set transparency
                col.a = _Transparency;

                return col;
            }
            ENDCG
        }
    }

    Fallback "Mobile/Particles/Alpha Blended"
}