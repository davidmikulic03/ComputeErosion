Shader "Unlit/Terrain"
{
    Properties
    {
        _DisplacementMap ("Displacement Map", 2D) = "white" {}
        _Albedo ("Albedo", 2D) = "white" {}
        _Normal ("Normal", 2D) = "white" {}
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
                float3 normal : NORMAL;
            };

            sampler2D _DisplacementMap;
            float4 _DisplacementMap_ST;

            sampler2D   _Normal;
            float4      _Normal_ST;
            
            sampler2D _Albedo;
            float4 _Albedo_ST;

            float _Displacement;
            float _Size;
            
            v2f vert (appdata v)
            {
                v2f o;
                float d = tex2Dlod(_DisplacementMap, v.uv);
                v.vertex.xyz += v.normal.xyz * d * _Displacement / _Size;
                o.normal = (tex2Dlod(_Normal, v.uv) * 2 - 1).xzy;

                o.uv = TRANSFORM_TEX(v.uv, _DisplacementMap);
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_Albedo, i.uv)  * float4(2, 2, 1, 0) - float4(1, 1, 0, 0);
                float3 normal = tex2D(_Normal, i.uv).xyz * 2 - 1;
                //col = dot(normal, -normalize(float3(2, 1.0, 4.0)));
                //col = 1.0 - dot(normal, float3(0, 0, 1));

                return col.z;
                return float4(col.xy, 0, 0);
                return float4(normal.xy, 0, 1);
                UNITY_APPLY_FOG(i.fogCoord, col);
            }
            ENDCG
        }
    }
}
