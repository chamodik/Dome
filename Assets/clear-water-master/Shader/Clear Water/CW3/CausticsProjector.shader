Shader "Silent/Clear Water 3 Caustics Projector"
// UNITY_SHADER_NO_UPGRADE 
{
    Properties
    {
        [HeaderEx(Caustics Settings)]
        _CausticsTex ("Caustics Texture", 2D) = "white" {}
        _CausticsSpeed ("Caustics Speed", Float) = 0.04
        _CausticsScale ("Caustics Scale", Float) = 0.2
        _CausticsStrength ("Caustics Strength", Float) = 2.4
        _EdgeFadeRadius ("Edge Fade Radius", Float) = 0.3
        _EdgeFadeOffset ("Edge Fade Offset", Float) = 0.2
        
        [HeaderEx(Distortion Settings)]
        [Normal]_WarpTex ("Distortion Map Texture (optional)", 2D) = "bump" {}
        _WarpPow ("Distortion Power", Vector) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags {  
            "RenderType"="Transparent" 
            "Queue"="Transparent-10"
            "DisableBatching" = "True" 
            "IgnoreProjector" = "True"
            "ForceNoShadowCasting " = "True"
            "PreviewType" = "Skybox"
        }
        LOD 100
        ZWrite Off
        ZTest Always
        Cull Off
        Blend One One

        Pass
        {
            Name "FORWARD"
            Lighting On
            Tags 
            {
                "LightMode"="ForwardBase"
                "PassFlags"="OnlyDirectional"
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc" // for _LightColor0

            #if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC)//ASE Sampler Macros
            #define UNITY_SAMPLE_TEX2D_LOD(tex,coord,lod) tex.SampleLevel (sampler##tex,coord, lod)
            #else
            #define UNITY_SAMPLE_TEX2D_LOD(tex,coord,lod) tex2Dlod(tex,float4(coord,0,lod))
            #endif

            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            sampler2D _CausticsTex;
            sampler2D _WarpTex;
            
            CBUFFER_START(UnityPerMaterial)
            uniform float _CausticsSpeed;
            uniform float _CausticsScale;
            uniform float _CausticsStrength;
            uniform float _EdgeFadeRadius;
            uniform float _EdgeFadeOffset;
            uniform float4 _WarpTex_ST;
            uniform float4 _WarpPow;
            CBUFFER_END

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float4 _CameraDepthTexture_TexelSize;

            struct appdata
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID 
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 depthTextureUv : TEXCOORD0;
                float4 rayFromCamera : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Dj Lukis.LT's oblique view frustum correction 
            // https://github.com/lukis101/VRCUnityStuffs/blob/master/Shaders/DJL/Overlays/WorldPosOblique.shader
            inline float4 CalculateObliqueFrustumCorrection()
            {
                float x1 = -UNITY_MATRIX_P._31 / (UNITY_MATRIX_P._11 * UNITY_MATRIX_P._34);
                float x2 = -UNITY_MATRIX_P._32 / (UNITY_MATRIX_P._22 * UNITY_MATRIX_P._34);
                return float4(x1, x2, 0, UNITY_MATRIX_P._33 / UNITY_MATRIX_P._34 + x1 * UNITY_MATRIX_P._13 + x2 * UNITY_MATRIX_P._23);
            }
            inline float CorrectedLinearEyeDepth(float z, float correctionFactor)
            {
                return 1.f / (z / UNITY_MATRIX_P._34 + correctionFactor);
            }

            bool SceneZDefaultValue()
            {
                #if UNITY_REVERSED_Z
                    return 0.f;
                #else
                    return 1.f;
                #endif
            }

            inline float get01Depth(float4 projPos)
            {
                return Linear01Depth 
                (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(projPos)));
            }

            inline float get01DepthVS(float4 projPos)
            {
                if(projPos.x > 1 || projPos.x < 0 || projPos.y > 1 || projPos.y < 0) return 0;
                return Linear01Depth 
                (SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, float4(projPos.xy / projPos.w, 0, 0)));
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                float4 worldPosition = mul(unity_ObjectToWorld, v.vertex);
                const fixed3 baseWorldPos = unity_ObjectToWorld._m03_m13_m23;
                
                o.pos = mul(UNITY_MATRIX_VP, worldPosition);
                o.depthTextureUv = ComputeGrabScreenPos(o.pos);
                
                // Warp ray by the base world position, so it's possible to have reoriented fog
                o.rayFromCamera.xyz = worldPosition.xyz - _WorldSpaceCameraPos.xyz;
                o.rayFromCamera.w = dot(o.pos, CalculateObliqueFrustumCorrection()); 

                UNITY_TRANSFER_INSTANCE_ID(v, o);
                return o;
            }
            
            float4x4 lookAt(float3 Eye, float3 Center, float3 Up)
            {
                float4x4 Matrix;
            
                float3 X, Y, Z;
            
                Z = Eye - Center;
                Z = normalize(Z);
                Y = Up;
                X = cross(Y, Z);
                Y = cross(Z, X);
            
                X = normalize(X);
                Y = normalize(Y);
            
                Matrix[0][0] = X.x;
                Matrix[1][0] = X.y;
                Matrix[2][0] = X.z;
                Matrix[3][0] = dot(-X, Eye);
                Matrix[0][1] = Y.x;
                Matrix[1][1] = Y.y;
                Matrix[2][1] = Y.z;
                Matrix[3][1] = dot(-Y, Eye);
                Matrix[0][2] = Z.x;
                Matrix[1][2] = Z.y;
                Matrix[2][2] = Z.z;
                Matrix[3][2] = dot(-Z, Eye);
                Matrix[0][3] = 0;
                Matrix[1][3] = 0;
                Matrix[2][3] = 0;
                Matrix[3][3] = 1.0f;
            
                return Matrix;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                UNITY_SETUP_INSTANCE_ID(i);
                
                float perspectiveDivide = 1.f / i.pos.w;
                float4 rayFromCamera = i.rayFromCamera * perspectiveDivide;
                float2 depthTextureUv = i.depthTextureUv.xy * perspectiveDivide;
                
                float sceneZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, depthTextureUv);
                
                bool isSkybox = (sceneZ == SceneZDefaultValue());
                if (isSkybox)
                {
                    // This is skybox, depth texture has default value
                    // It's scene dependant on whether we want the fog to be clipped by the skybox
                    clip(-1);
                    sceneZ = 0;
                }
                
                // Linearize depth and use it to calculate background world position
                float sceneDepth = CorrectedLinearEyeDepth(sceneZ, rayFromCamera.w*perspectiveDivide);
                float3 worldPosition = rayFromCamera.xyz * sceneDepth + _WorldSpaceCameraPos.xyz;

                /* Not used.
                float4 localPosition = mul(unity_WorldToObject, float4(worldPosition, 1));
                float localDepth = CorrectedLinearEyeDepth(i.pos.z, rayFromCamera.w);
                float clipAt = (localDepth < sceneDepth);
                localPosition.xyz /= localPosition.w;
                */ 
                
                float3 objPos = mul(unity_WorldToObject, float4(worldPosition, 1.0)).xyz;
            
                // Fade out at the edges
                float3 edgeFadeMaskXYZ = 1.0 - smoothstep(1.0 - _EdgeFadeRadius, 1.0, abs(objPos) + _EdgeFadeOffset);
                float edgeFadeMask = edgeFadeMaskXYZ.x * edgeFadeMaskXYZ.y * edgeFadeMaskXYZ.z;
                if (edgeFadeMask <= 0) discard;
            
                // Light space transformation for directional caustics
                float4x4 sunToWorld = lookAt(_WorldSpaceLightPos0, 0, float3(0, 1, 0));
            
                // Sample caustics texture
                float2 uv = mul(float4(worldPosition, 1.0), sunToWorld).xy * _CausticsScale;
                
                // Distortion / Normal distortion warping
                float2 warpUVs = UnpackNormal(tex2D(_WarpTex, TRANSFORM_TEX(uv,_WarpTex)+frac(_Time.x*_WarpPow.xy))) * _WarpPow.zw;
            
                float2 uv1 = uv + _Time.y * _CausticsSpeed;
                float2 uv2 = uv * -1.0 + _Time.y * _CausticsSpeed * 0.5;
            
                // Sample animated caustics layers
                float3 causticstex = tex2D(_CausticsTex, uv1+warpUVs).rgb;
                float3 causticstex2 = tex2D(_CausticsTex, uv2+warpUVs).rgb;
            
                float3 caustics = min(causticstex, causticstex2) * edgeFadeMask;
            
                caustics *= caustics;
                float4 finalColor = float4(caustics * _LightColor0.xyz * _CausticsStrength, 1.0);
            
                // Apply fog
                UNITY_CALC_FOG_FACTOR_RAW(length(rayFromCamera.xyz * sceneDepth));
                finalColor *= saturate(unityFogFactor);
            
                return finalColor;
            }
            ENDCG
        }
    }
}
