Shader "Unlit/Terrain"
{
    Properties
    {
        _DisplacementMap ("Displacement Map", 2D) = "white" {}
        _Albedo ("Albedo", 2D) = "white" {}
        _Normal ("Normal", 2D) = "white" {}
        _Slope ("Slope", 2D) = "white" {}
        _Erosion ("Erosion Map", 2D) = "white" {}
        _Displacement ("Displacement", Float) = 1
        _DisplaceAlongNormal("Displace Along Normal", float) = 0
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

            sampler2D _Slope;
            float4 _Slope_ST;

            sampler2D _Erosion;
            float4 _Erosion_ST;

            float _Displacement;
            float _Size;

            float _DisplaceAlongNormal;
            
            v2f vert (appdata v)
            {
                v2f o;
                float d = tex2Dlod(_DisplacementMap, v.uv);
                v.vertex.xyz += v.normal.xyz * d * _Displacement / _Size;
                v.normal.xyz = tex2Dlod(_Normal, v.uv).xzy * 2 - 1;
                v.vertex.xyz += float3(v.normal.x, 0, v.normal.y) * _DisplaceAlongNormal;
                
                o.uv = TRANSFORM_TEX(v.uv, _DisplacementMap);
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_Albedo, i.uv);
                float slope = tex2D(_Slope, i.uv);
                float3 normal = tex2D(_Normal, i.uv).xyz * 2 - 1;
                float erosionMap = tex2D(_Erosion, i.uv);

                return erosionMap;
                return dot(normal, normalize(float3(1, 2, 4)));
                return float4(normal.xyz, 1);
                return normal.z;
                return slope;
                UNITY_APPLY_FOG(i.fogCoord, col);
            }
            ENDCG
        }
    }
}
