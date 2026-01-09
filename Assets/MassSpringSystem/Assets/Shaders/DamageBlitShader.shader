Shader "Hidden/DamageBlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // This will be our current damageMaskTexture
        _DamageDecal ("Decal", 2D) = "white" {} // The sprite we want to draw
        _HitUV ("Hit UV", Vector) = (0,0,0,0) // xy for position, zw unused
        _Radius ("Radius", Float) = 0.05
        _Intensity ("Intensity", Float) = 0.2 // How strong the new damage is
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" } // RenderType Opaque is fine, Queue Overlay
        LOD 100

        Pass
        {
            Blend One One // Additive blending (new damage adds to old damage)
            // Or Blend SrcAlpha OneMinusSrcAlpha for regular alpha blend if your decal has alpha
            // For a mask, 'Max' blending is often best to ensure values only increase:
            // BlendOp Max, Blend One One // Need to write custom Blending in fragment for this if not using BlendOp Max.

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

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

            sampler2D _MainTex; // The RenderTexture itself
            float4 _MainTex_TexelSize; // Unity automatically provides texel size

            sampler2D _DamageDecal;
            float4 _HitUV;   // xy = hit UV, zw = unused
            float _Radius;   // Radius of the decal in UV space
            float _Intensity; // Intensity of the damage (opacity)

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; // Pass through the full-screen UVs
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Current value of the damage mask at this pixel
                // i.uv then represents the UV coordinate of the current pixel (fragment) that the fragment shader is processing on the damageMaskTexture
                fixed4 currentMask = tex2D(_MainTex, i.uv);

                // Calculate distance from the hit point in UV space
                float dist = distance(i.uv, _HitUV.xy);

                // Determine the alpha/opacity of the decal based on distance
                // Use smoothstep for a softer edge
                float decalAlpha = smoothstep(_Radius, _Radius * 0.5, dist); // Fades out from center

                // Sample the damage decal sprite (if it has complex shape)
                // For a simple white circle, decalAlpha is enough
                // each color component returns nubmer from 0-1
                fixed4 decalColor = tex2D(_DamageDecal, (i.uv - _HitUV.xy) / (2.0 * _Radius) + 0.5); // Sample decal based on hit UV, scale it by radius

                // Blend the decal onto the mask. We want to ADD damage, not replace.
                // Max blend mode is ideal for masks: take the max of current mask and new decal.
                // Since BlendOp Max is not directly supported on all platforms or with standard CGPROGRAM,
                // we'll simulate it by adding with intensity.
                // Alternatively, use CommandBuffer.Blit with specific blend options.

                // this multiplication will never exceed 1
                fixed newDamage = decalColor.r * decalAlpha * _Intensity; // Assuming decal is grayscale
                fixed finalMaskValue = max(currentMask.r, newDamage); // Take the maximum value at each pixel
                return fixed4(finalMaskValue, finalMaskValue, finalMaskValue, 1.0); // Output grayscale mask
            }
            ENDCG
        }
    }
}


