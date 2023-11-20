Shader "Unlit/Terrain"
{
    Properties
    {
        _DisplacementMap ("Displacement Map", 2D) = "white" {}
        _Displacement ("Displacement", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float3 normal : NORMAL;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _DisplacementMap;
            float4 _DisplacementMap_TexelSize;
            float4 _DisplacementMap_ST;
            float _Displacement;
            

            v2f vert (appdata v)
            {
                v2f o;
                float d = tex2Dlod(_DisplacementMap, v.uv);
                v.vertex.xyz += v.normal.xyz * d * _Displacement;

                o.uv = TRANSFORM_TEX(v.uv, _DisplacementMap);
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                i.uv = (uint2)(i.uv * _DisplacementMap_TexelSize.zw) / _DisplacementMap_TexelSize.zw;
                float4 col = tex2D(_DisplacementMap, i.uv);
                float4 slope = 100 * float4(
                    col.x - tex2D(_DisplacementMap, i.uv + _DisplacementMap_TexelSize.x).x, 
                    col.x - tex2D(_DisplacementMap, i.uv - _DisplacementMap_TexelSize.x).x,
                    col.x - tex2D(_DisplacementMap, i.uv + _DisplacementMap_TexelSize.y).x, 
                    col.x - tex2D(_DisplacementMap, i.uv - _DisplacementMap_TexelSize.y).x
                    );


                UNITY_APPLY_FOG(i.fogCoord, col);
                return slope;
            }
            ENDCG
        }
    }
}
