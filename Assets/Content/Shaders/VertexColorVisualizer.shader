Shader "Supremacy/Debug/VertexColorVisualizer"
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
                float4 color : COLOR;
            };

            struct VertexOutput
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
            
            VertexOutput vert (VertexInput input)
            {
                VertexOutput o;
                o.vertex = TransformObjectToHClip(input.vertex.xyz);
                o.color = input.color;
                return o;
            }

            half4 frag (VertexOutput input) : SV_Target
            {
                half4 col = input.color;
                return col;
            }
            ENDHLSL
        }
    }
}
