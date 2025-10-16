Shader "ASE/Image/TexRamp" {
	Properties {
		_Color1 ("Color1", Vector) = (1,1,1,1)
		_Color2 ("Color2", Vector) = (0,0,0,1)
		_ColorBoundry ("Color Boundry", Range(0, 1)) = 0.5
		_ColorSmooth ("Color Smooth", Range(0, 1)) = 1
		_solidColor ("solidColor", Vector) = (1,1,1,1)
		_MainTex ("MainTex", 2D) = "white" {}
		_solidColorInt ("solidColorInt", Range(0, 1)) = 0
		[KeywordEnum(AlphaBlend,Multiply,Additive)] _TexBlendMode ("TexBlendMode", Float) = 0
		_BlendInit ("BlendInit", Range(0, 1)) = 1
		_SpeedScale ("SpeedScale", Vector) = (0,0,0,0)
		[Toggle] _ToggleSwitch0 ("Toggle Switch0", Float) = 0
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[HideInInspector] __dirty ("", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;
			float4 _MainTex_ST;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy);
			}

			ENDHLSL
		}
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}