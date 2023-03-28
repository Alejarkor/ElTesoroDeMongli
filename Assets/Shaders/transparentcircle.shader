Shader "Custom/transparentCircle"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _MousePos("MousePos", Vector) = (0,0,0,0)
        _Resolution("Resolution", Vector) = (800,600,0,0)
        _CircleRadius("Circle Radius", Range(0, 400)) = 100
        _FeatherAmount("Feather Amount", Range(0, 100)) = 10
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows alpha:fade

            #pragma target 3.0

            sampler2D _MainTex;

            struct Input
            {
                float2 uv_MainTex;
                float4 screenPos;
            };

            half _Glossiness;
            half _Metallic;
            fixed4 _Color;
            float2 _MousePos;
            float2 _Resolution;
            float _CircleRadius;
            float _FeatherAmount;

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;

                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                screenUV.xy *= _Resolution;
                float2 MousePos = float2(_MousePos.x, _MousePos.y);
                float d = distance(MousePos, screenUV);

                float maxd = _CircleRadius;
                float feather = _FeatherAmount;

                d = smoothstep(maxd - feather, maxd, d);

                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;

                o.Alpha = d;
            }
            ENDCG
        }
            FallBack "Diffuse"
}
