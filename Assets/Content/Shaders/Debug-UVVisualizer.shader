Shader "Supremacy/Debug/UVVisualizer"
{
    Properties
    {
        [UVIndex] _UVIndex("UV Index", int) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1: TEXCOORD1;
                float2 uv2: TEXCOORD2;
                float2 uv3: TEXCOORD3;
                float2 uv4: TEXCOORD4;
                float2 uv5: TEXCOORD5;
                float2 uv6: TEXCOORD6;
                float2 uv7: TEXCOORD7;
            };

            struct VertexOutput
            {
                float4 vertex : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1: TEXCOORD1;
                float2 uv2: TEXCOORD2;
                float2 uv3: TEXCOORD3;
                float2 uv4: TEXCOORD4;
                float2 uv5: TEXCOORD5;
                float2 uv6: TEXCOORD6;
                float2 uv7: TEXCOORD7;
            };

            CBUFFER_START(UnityPerMaterial)
                int _UVIndex;
            CBUFFER_END
            
            VertexOutput vert (VertexInput input)
            {
                VertexOutput o;
                o.vertex = TransformObjectToHClip(input.vertex.xyz);
                o.uv0 = input.uv0;
                o.uv1 = input.uv1;
                o.uv2 = input.uv2;
                o.uv3 = input.uv3;
                o.uv4 = input.uv4;
                o.uv5 = input.uv5;
                o.uv6 = input.uv6;
                o.uv7 = input.uv7;
                return o;
            }

            half4 frag(VertexOutput input) : SV_Target
            {
                float2 uv = input.uv0;
                uv = _UVIndex > 0. ? input.uv1 : uv;
                uv = _UVIndex > 1. ? input.uv2 : uv;
                uv = _UVIndex > 2. ? input.uv3 : uv;
                uv = _UVIndex > 3. ? input.uv4 : uv;
                uv = _UVIndex > 4. ? input.uv5 : uv;
                uv = _UVIndex > 5. ? input.uv6 : uv;
                uv = _UVIndex > 6. ? input.uv7 : uv;
                
                half4 col = half4(uv.x, uv.y, 0, 1);
                return col;
            }
            ENDHLSL
        }
    }
}
