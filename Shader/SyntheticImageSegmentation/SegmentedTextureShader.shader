Shader "AAI/SegmentedTextureShader"
{
    Properties
    {
        _TagLookUp ("TagLookUp", 2D) = "white" {}
		_IndexedTexture ("IndexedTexture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Cull Off ZWrite Off  ZClip Off 
			//Blend One OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };


            struct v2f
            {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
            };

            uniform sampler2D _TagLookUp;
			uniform sampler2D _IndexedTexture;
			uniform float _NumberOfSegments;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
			    fixed4 index = tex2D(_IndexedTexture, i.uv);
				clip( index.r == 0. ? -1:1 );
                uint lookupIndex = uint(index.r * 255.0f) - 1;// << 24 | uint(index.g * 255.0f) << 16 | uint(index.b * 255.0f) << 8 | uint(index.a * 255.0f);
				float2 cord = float2((lookupIndex / _NumberOfSegments)+((1 / _NumberOfSegments) / 2), 0);
				fixed4 color = tex2D(_TagLookUp, cord); 
				return color;
            }
			ENDCG
        }
    }
}
