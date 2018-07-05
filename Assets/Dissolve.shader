Shader "Custom/Dissolve"
{
    Properties {
        _LineColor("Line Color", Color) = (1.0, 1.0, 1.0, 1.0)
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0,0)
        _DissolvePercentage("DissolvePercentage", Range(0,1)) = 0.0
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _DissolveTex("Dissolve Texture", 2D) = "white" {}

        _LineWidth("Line Width", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _ShowTexture("ShowTexture", Range(0,1)) = 0.0

    }
    SubShader {
        // Rendering order
        //http://marupeke296.com/UNI_S_No2_ShaderLab.html
        Tags{
            "RenderType" = "Opaque"
        }  //不透明

        //画質設定（中）
        //https://docs.unity3d.com/ja/540/Manual/SL-ShaderLOD.html
        LOD 200
 
        CGPROGRAM

		// Debugger
		//#pragma enable_d3d11_debug_symbols

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
 
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
 
        sampler2D _MainTex;
        sampler2D _DissolveTex;

        struct Input {
            float2 uv_MainTex;
            float2 uv_DissolveTex;
            float3 worldPos;
        };
 
        half _Glossiness;
        half _Metallic;
        half _DissolvePercentage;
        half _ShowTexture;
        fixed4 _Color;
        fixed4 _EmissionColor;
        float4 _LineColor;
        float _LineWidth;

        // 組み込み関数の詳細（tex2D, clip, lerpなど）
        // http://developer.download.nvidia.com/cg/lerp.html
        void surf(Input IN, inout SurfaceOutputStandard o) {

            //グレースケールの場合，rgbは全て同一の値になる
            half gradient = tex2D(_DissolveTex, IN.uv_DissolveTex).r; //tex2D->fixed4
            //half gradient = tex2D(_DissolveTex, IN.worldPos.xy).r; //tex2D->fixed4
            half4 clear = half4(0.0, 0.0, 0.0, 0.0);

            //0より小さい場合描画をしない（_DissolvePercentageの方が大きい時，描画しない）
            clip(gradient - _DissolvePercentage); 

            int isClear = int(gradient - (_DissolvePercentage + _LineWidth) + 0.99);
            int isAtLeastLine = int(gradient - (_DissolvePercentage));
            half4 altCol = lerp(_LineColor, clear, isClear); //光らせる部分

            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
            o.Albedo = lerp(o.Albedo, altCol, isAtLeastLine);
            o.Alpha = lerp(1.0, clear, isClear);
            o.Emission = altCol;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
