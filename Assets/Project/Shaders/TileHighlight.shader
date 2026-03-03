Shader "StarSaga/TileHighlight"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
        _GlowSpeed ("Glow Speed", Float) = 5.0
        _BorderSize ("Border Size", Range(0, 0.5)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR; 
                float2 texcoord : TEXCOORD0; 
                float2 texcoord1: TEXCOORD1; 
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1: TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _GlowColor;
            float _GlowSpeed;
            float _BorderSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.texcoord1 = IN.texcoord1;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                if (IN.texcoord1.x > 0.5)
                {
                    // Calculate if we are in the border area
                    float2 borderDist = min(IN.texcoord, 1.0 - IN.texcoord);
                    float edgeDist = min(borderDist.x, borderDist.y);
                    
                    // Simple border detection using step
                    float isBorder = step(edgeDist, _BorderSize);
                    
                    if (isBorder > 0.5)
                    {
                        float pulse = (sin(_Time.y * _GlowSpeed) + 1.0) * 0.5;
                        // For the border, we blend strongly towards white/GlowColor
                        c.rgb = lerp(c.rgb, _GlowColor.rgb, pulse);
                        c.a = max(c.a, pulse * 0.8); // Ensure the border is visible even if texture is transparent
                    }
                }
                
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
