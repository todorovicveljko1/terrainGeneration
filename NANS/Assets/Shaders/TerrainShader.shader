Shader "Custom/TerrainShader"
{
    Properties
    {
        _MinHeight ("MinHeight", float) = 0
        _MaxHeight ("MaxHeight", float) = 32
        _Texture("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        half _MinHeight;
        half _MaxHeight;
        sampler2D _Texture;


        struct Input
        {
            float3 worldPos;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float r = saturate(1 - (IN.worldPos.y - _MinHeight)*(1/(_MaxHeight-_MinHeight)));
            o.Albedo = tex2D(_Texture, r);
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
