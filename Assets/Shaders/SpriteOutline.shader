Shader "Custom/SpriteOutlineIgnoreBorder"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (0,1,0,1)
        [Range(1,20)]
        _OutlineSize("Outline Size (Pixels)", float) = 2.0
        [Range(0,1)]
        _AlphaThreshold("Alpha Threshold", float) = 0.1
        [Range(0,20)]
        _IgnoreBorder("Ignore border (Pixels)", float) = 10.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Lighting Off
        ZWrite Off
        Cull Off
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Outline"
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _OutlineColor;
            float _OutlineSize;
            float _AlphaThreshold;
            float _IgnoreBorder;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1) 원본 텍스처 샘플
                fixed4 col = tex2D(_MainTex, i.uv);

                // 2) UV 기준 바깥 테두리를 무시하기 위해,
                //    _IgnoreBorder 픽셀만큼 안쪽 범위만 Outline 검사
                float2 borderUV = float2(_IgnoreBorder / _ScreenParams.x, _IgnoreBorder / _ScreenParams.y);

                // 만약 현재 픽셀이 스프라이트 가장자리(borderUV) 밖이라면, Outline을 적용하지 않음
                if (i.uv.x < borderUV.x || i.uv.x > 1.0 - borderUV.x ||
                    i.uv.y < borderUV.y || i.uv.y > 1.0 - borderUV.y)
                {
                    return col; // 원래 픽셀 색상 그대로 반환 (테두리 영역 무시)
                }

                // 3) 불투명 픽셀이면 바로 반환
                if (col.a >= _AlphaThreshold) 
                    return col;

                // 4) OutlineSize 픽셀 단위 → UV 오프셋
                float2 texelSize = float2(_OutlineSize / _ScreenParams.x, _OutlineSize / _ScreenParams.y);

                // 5) 주변 8방향 알파값 샘플링
                float a1 = tex2D(_MainTex, i.uv + float2(texelSize.x, 0)).a;
                float a2 = tex2D(_MainTex, i.uv - float2(texelSize.x, 0)).a;
                float a3 = tex2D(_MainTex, i.uv + float2(0, texelSize.y)).a;
                float a4 = tex2D(_MainTex, i.uv - float2(0, texelSize.y)).a;
                float a5 = tex2D(_MainTex, i.uv + float2(texelSize.x, texelSize.y)).a;
                float a6 = tex2D(_MainTex, i.uv + float2(-texelSize.x, texelSize.y)).a;
                float a7 = tex2D(_MainTex, i.uv + float2(texelSize.x, -texelSize.y)).a;
                float a8 = tex2D(_MainTex, i.uv + float2(-texelSize.x, -texelSize.y)).a;

                // 6) 주변 8방향 중 하나라도 불투명 픽셀이 있으면 Outline 색상
                if (a1 >= _AlphaThreshold || a2 >= _AlphaThreshold ||
                    a3 >= _AlphaThreshold || a4 >= _AlphaThreshold ||
                    a5 >= _AlphaThreshold || a6 >= _AlphaThreshold ||
                    a7 >= _AlphaThreshold || a8 >= _AlphaThreshold)
                {
                    return _OutlineColor;
                }

                // 7) 나머지는 원래 픽셀 색상
                return col;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
