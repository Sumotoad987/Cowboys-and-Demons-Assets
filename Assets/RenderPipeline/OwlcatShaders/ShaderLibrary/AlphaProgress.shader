// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PF/GUI/AlphaProgress" 
{
	Properties
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_MaskTex("Mask Tex", 2D) = "" {}
		_Progress("Progress", Range(0, 1)) = 0.1
	}
	
	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _MainTex;
	sampler2D _MaskTex;
	float _Progress;

	struct appdata_t
	{
		float4 vertex   : POSITION;
		float4 color    : COLOR;
		float2 texcoord : TEXCOORD0;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD;
		float4 color : COLOR0;
	};

	v2f vert(appdata_t i)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(i.vertex);
		o.uv = i.texcoord.xy;
		o.color = i.color;
		return o;
	}

	float4 frag(v2f i) : COLOR
	{
		float4 c = tex2D(_MainTex, i.uv) * i.color;
		float4 mask = tex2D(_MaskTex, i.uv);
		float progress = lerp(-.01, 1.01, _Progress);
		c.a = (progress - mask.a) > 0 ? c.a : 0;
		
		return c;
	}

	ENDCG
	
	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200
		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	} 

	FallBack "Diffuse"
}
