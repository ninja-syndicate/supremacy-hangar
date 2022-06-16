Shader "Supremacy/Debug/UV0Visualizer"
{
    Properties
    {
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
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            VertexOutput vert (VertexInput input)
            {
                VertexOutput o;
                o.vertex = TransformObjectToHClip(input.vertex.xyz);
                o.uv = input.uv;
                return o;
            }

            half4 frag(VertexOutput input) : SV_Target
            {
                half4 col = half4(input.uv.x, input.uv.y, 0, 1);
                return col;
            }
            ENDHLSL
        }
    }
}
