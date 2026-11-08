Shader "Silent/Clear Water 3 Extras/Camera Depth to Alpha" 
// Originally written by Lyuma. Thanks Lyuma!
{
    Properties
    {
    }

    SubShader
    {
        // Render queue is set to Overlay, rendering after normal geometry.
        Tags 
        { 
            "Queue" = "Overlay" 
            "RenderType" = "Opaque" 
            "IgnoreProjector" = "True" 
        }

        // Render states optimized for writing specific data to the alpha channel.
        Cull Off
        ZWrite Off
        ZTest Always
        ColorMask A                   // Only write to the Alpha channel.
        Blend One Zero, One Zero      // Direct overwrite of color and alpha values.

        Pass
        {
            CGPROGRAM
            #pragma fragment frag
            #pragma vertex vert
            #include "UnityCG.cginc"

            sampler2D _CameraDepthTexture;

            struct v2f 
            {
                float4 pos       : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            // Mirror detection: Checks for projection matrix asymmetry.
            // In VR/Stereo rendering, mirror detection is bypassed.
            #ifdef USING_STEREO_MATRICES
            static bool isInMirror = 0;
            #else
            static bool isInMirror = (unity_CameraProjection[2][0] != 0.f || unity_CameraProjection[2][1] != 0.f);
            #endif

            // Vertex Shader
            v2f vert(in float3 pos : POSITION) 
            {
                v2f o;
                
                // Set up a full-screen quad coordinate system.
                o.screenPos = float4(pos.xy * 2.0, 1.0, 1.0);
                o.pos = o.screenPos;
                
                // If not rendering from a mirror camera, collapse the vertices 
                // to clip/discard the geometry.
                if (!isInMirror) 
                {
                    o.pos = float4(1.0, 1.0, 1.0, 1.0);
                }
                
                return o;
            }

            static float4x4 projMat = UNITY_MATRIX_P;

            // Reconstructs camera-space coordinates using screen UV and depth.
            float3 computeCameraSpacePosFromDepthAndInvProjMat(float zdepth, float2 screenUV) 
            {
                #if defined(UNITY_REVERSED_Z)
                    zdepth = 1.0 - zdepth;
                #endif

                // Reconstruct Normalized Device Coordinates (NDC) space.
                float4 clipPos = float4(screenUV.xy, zdepth, 1.0);
                clipPos.xyz = 2.0 * clipPos.xyz - 1.0;

                // Transform back to camera space using the inverse projection matrix.
                float4 camPos = mul(unity_CameraInvProjection, clipPos);
                camPos.xyz /= camPos.w;
                camPos.z *= -1.0; // Correct for Unity's camera forward direction.

                return camPos.xyz;
            }

            // Fragment Shader
            float4 frag(in v2f o) : SV_Target 
            {
                // Convert screen position to normalized UV space [0, 1].
                float2 screenUV = (o.screenPos.xy / o.screenPos.w) * 0.5 + 0.5;

                #ifdef UNITY_UV_STARTS_AT_TOP
                    screenUV.y = 1.0 - screenUV.y;
                #endif

                // Handle single-pass instanced / multi-pass stereo UV adjustments.
                screenUV = UnityStereoTransformScreenSpaceTex(screenUV);

                // Retrieve the raw depth value from the camera depth texture.
                float sampledDepth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, screenUV));

                // Reconstruct the 3D position in view space.
                float3 viewSpacePosition = computeCameraSpacePosFromDepthAndInvProjMat(sampledDepth, screenUV);

                // Write coordinates to the target. Note that ColorMask A limits output 
                // to the alpha channel, storing the negative Z depth (-viewSpacePosition.z).
                return float4(viewSpacePosition.xyy, -viewSpacePosition.z);
            }
            ENDCG
        }
    } 
}