// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

Shader "Utility/Foliage Mask" {
	Properties{
		_ShadowTex("Texture", 2D) = "gray" {}
		_Alpha("Alpah", float) = 1
	}
	Subshader{
		Tags{ "Queue" = "Transparent" }
		Pass{
			ZWrite Off
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			//Offset - 1, -1
			CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma multi_compile_fog
	#include "UnityCG.cginc"
			struct v2f {
				float4 uvShadow : TEXCOORD0;
				float4 pos : SV_POSITION;
			};
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			v2f vert(float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				o.uvShadow = mul(unity_Projector, vertex);
				return o;
			}
			sampler2D _ShadowTex;
			float _Alpha;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 texS = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
				texS.a *= _Alpha;
				return texS;
			}
			ENDCG
		}
	}
}