Shader "Custom/FrozenSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Main Texture", 2D) = "white" {}
        _IceColor ("Ice Tint", Color) = (0.7, 0.9, 1, 0.6)
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _FrostAmount ("Frost Amount", Range(0, 1)) = 0.5
        _CrackTex ("Crack Texture", 2D) = "black" {}
        _CrackSpeed ("Crack Speed", Float) = 0.1
        _SparkleIntensity ("Sparkle Intensity", Range(0, 5)) = 2
        _SparkleSpeed ("Sparkle Speed", Float) = 1
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "RenderType"="Opaque"
        }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            sampler2D _CrackTex;
            float4 _IceColor;
            float _FrostAmount;
            float _CrackSpeed;
            float _SparkleIntensity;
            float _SparkleSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Основная текстура
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // Шум для эффекта инея
                float2 noiseUV = i.worldPos * 0.5;
                fixed4 noise = tex2D(_NoiseTex, noiseUV + _Time.y * 0.1);
                
                // Трещины льда (анимированные)
                float2 crackUV = i.uv * 2.0 + float2(0, _Time.y * _CrackSpeed);
                fixed4 cracks = tex2D(_CrackTex, crackUV);
                
                // Блики (sparkles)
                float sparkle = sin(i.worldPos.x * 50 + _Time.y * _SparkleSpeed) *
                              cos(i.worldPos.y * 50 + _Time.y * _SparkleSpeed * 1.3);
                sparkle = saturate(sparkle) * _SparkleIntensity;
                
                // Смешивание эффектов
                float frost = noise.r * _FrostAmount;
                float iceFactor = saturate(frost + cracks.r * 0.5 + sparkle);
                
                // Финальный цвет
                col.rgb = lerp(col.rgb, _IceColor.rgb, _IceColor.a * iceFactor);
                col.rgb += sparkle * 0.3;
                col.a = max(col.a, cracks.r * 0.3);
                
                return col;
            }
            ENDCG
        }
    }
}