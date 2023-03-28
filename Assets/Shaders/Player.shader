Shader "Custom/Player" {
    Properties{
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (1, 0, 0, 1)
        _OutlineWidth("Outline Width", Range(0, 0.1)) = 0.02
    }

        SubShader{
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 100

            CGPROGRAM
            #pragma surface surf Lambert noambient nolightmap

            sampler2D _MainTex;
            half _OutlineWidth;
            fixed4 _OutlineColor;

            struct Input {
                float2 uv_MainTex;
                float4 screenPos;
                float3 worldNormal;
                float3 worldPos;
            };

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            #include "UnityCG.cginc"

            void surf(Input IN, inout SurfaceOutput o) {
                o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
                o.Alpha = 1;

                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos)));

                float depthDiff = sceneDepth - IN.screenPos.z;
                float edgeDetection = step(_OutlineWidth, depthDiff);

                float3 worldViewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);
                float outline = dot(worldViewDir, IN.worldNormal);
                outline = 1 - saturate((outline - 0.5) * 20);

                o.Emission = _OutlineColor.rgb * outline * edgeDetection;
            }
            ENDCG
        }
            FallBack "Diffuse"
}
