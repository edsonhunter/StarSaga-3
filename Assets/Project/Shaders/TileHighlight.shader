Shader "StarSaga/TileHighlight"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
        _GlowSpeed ("Glow Speed", Float) = 5.0
        _BorderSize ("Border Size", Range(0, 0.5)) = 0.1
        
        [Header(Shockwave)]
        _ShockwaveType ("Shockwave Type", Int) = 0
        _ExplosionCenter ("Explosion Center", Vector) = (0,0,0,0)
        _ExplosionTime ("Explosion Time", Range(0, 1)) = 0.0
        _ShockwaveStrength ("Shockwave Strength", Float) = 0.5
        _ShockwaveRadius ("Shockwave Radius", Float) = 15.0
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
            #pragma target 3.0
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
            
            // Shockwave Properties
            int _ShockwaveType;
            float4 _ExplosionCenter; // x, y (world position), z (unused), w (unused)
            float _ExplosionTime; // 0 to 1
            float _ShockwaveStrength;
            float _ShockwaveRadius;

            float4 _MultiCenters[50];
            int _MultiCenterCount;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                // --- Vertex Displacement (Shockwave) ---
                float3 worldPos = mul(unity_ObjectToWorld, IN.vertex).xyz;
                float4 displacedVertex = IN.vertex;
                float currentRadius = _ExplosionTime * _ShockwaveRadius;

                if (_ShockwaveType == 1) // Radial Explode
                {
                    float dist = distance(worldPos.xy, _ExplosionCenter.xy);
                    float ripple = 0;
                    if (_ExplosionTime > 0.0 && dist < currentRadius && dist > currentRadius - 3.0) 
                    {
                        float diff = currentRadius - dist;
                        ripple = sin(diff * 3.14159) * _ShockwaveStrength * (1.0 - _ExplosionTime);
                    }
                    if (dist > 0.001) 
                    {
                        float2 dir = normalize(worldPos.xy - _ExplosionCenter.xy);
                        displacedVertex.xy += dir * ripple;
                    }
                }
                else if (_ShockwaveType == 2) // StripLine (Horizontal Wave up and down)
                {
                    float dist = abs(worldPos.y - _ExplosionCenter.y);
                    float ripple = 0;
                    if (_ExplosionTime > 0.0 && dist < currentRadius && dist > currentRadius - 3.0) 
                    {
                        float diff = currentRadius - dist;
                        ripple = sin(diff * 3.14159) * _ShockwaveStrength * (1.0 - _ExplosionTime);
                    }
                    float dirY = sign(worldPos.y - _ExplosionCenter.y);
                    if (dirY == 0) dirY = 1.0;
                    displacedVertex.y += dirY * ripple;
                }
                else if (_ShockwaveType == 3) // Color Scatter (Multi-Radial)
                {
                    float totalRippleX = 0;
                    float totalRippleY = 0;
                    float cRadius = _ExplosionTime * (_ShockwaveRadius * 0.5); // Smaller radius for multiple points
                    
                    for (int i = 0; i < _MultiCenterCount; i++)
                    {
                        float d = distance(worldPos.xy, _MultiCenters[i].xy);
                        if (_ExplosionTime > 0.0 && d < cRadius && d > cRadius - 1.5)
                        {
                            float diff = cRadius - d;
                            float r = sin(diff * 3.14159) * (_ShockwaveStrength * 0.5) * (1.0 - _ExplosionTime);
                            if (d > 0.001) 
                            {
                                float2 dir = normalize(worldPos.xy - _MultiCenters[i].xy);
                                totalRippleX += dir.x * r;
                                totalRippleY += dir.y * r;
                            }
                        }
                    }
                    
                    displacedVertex.x += totalRippleX;
                    displacedVertex.y += totalRippleY;
                }
                
                OUT.vertex = UnityObjectToClipPos(displacedVertex);
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
