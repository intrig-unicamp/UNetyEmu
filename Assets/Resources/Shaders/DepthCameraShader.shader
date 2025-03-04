Shader "Custom/DepthCameraShader" //This Shader is based on the work done in https://github.com/dahburj/unity-depth-camera/tree/master
{
    
    // Properties of the shader
    Properties
    {
        _Color ("Background", COLOR) = (0,0,0,1) // Background color 
        _R_State("R_State", Int) = 0 // Red state (No need to use other channels)
        _R_Min("R_Min", Float) = 0 // Red minimum value
        _R_Max("R_Max", Float) = 1 // Red maximum value
    }

    // Subshader of the shader
    SubShader
    {
        
        Pass
        {
            
            // Allow the object to always pass the depth test
            // Disable culling to render on both sides of the object
            // Do not write to depth buffer
            ZTest Always Cull Off ZWrite Off
            
            // Start a shading code block in CG
            CGPROGRAM

            #include "UnityCG.cginc" // Import utilities and macros from Unity

            sampler2D_float _CameraDepthTexture; //Depth texture of the chamber, where the depth values are stored
            half4 _Color; // Color used to render the object (modified by the depth)
            int _R_State; // State parameter to manage the depth in the red channel
            float _R_Min; // Minimum parameter to manage the depth in the red channel
            float _R_Max; // Maximum parameter to manage the depth in the red channel

            // Define vertex attributes, such as position and UV coordinates
            struct CommonAttributes
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD;
            };

            // Structure containing the interpolated values between the vertex shader and the fragment shader, including the UV coordinates
            struct CommonVaryings
            {
                float4 position : SV_POSITION;
                half2 uv0 : TEXCOORD0;
                half2 uv1 : TEXCOORD1;
            };

            // Vertex shader that receives the input attributes and returns the interpolated values
            CommonVaryings CommonVertex(CommonAttributes input)
            {
                CommonVaryings o;
                o.position = UnityObjectToClipPos(input.position);
                o.uv0 = input.uv;
                o.uv1 = input.uv;
                return o;
            }

            // Fragment shader that receives the interpolated values and returns the color of the object
            half4 DepthToRGBFragment(CommonVaryings input) : SV_Target
            {
                
                // Reads the linear depth of the chamber depth buffer
                float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, input.uv1));

                // Assigns the depth value to the red channel, using the minimum and maximum values to normalize the result
                if (_R_State < 2)
                {
                    float d = _R_State == 1 ? 1 - depth : depth;
                    if (d >= _R_Min && d < _R_Max)
                    {
                        _Color.r = 1 - (d - _R_Min) * (1 / (_R_Max - _R_Min)); 
                        _Color.a = 1;
                    }
                }

                // Returns the color of the object
                return half4(_Color.r, _Color.r, _Color.r, _Color.a);

            }

            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA //Allows to compile the shader for different color space configurations
            #pragma vertex CommonVertex // Specifies which functions are the vertex shaders
            #pragma fragment DepthToRGBFragment // Specifies which functions are the fragment shaders
            #pragma target 3.0 // Sets the target version of the shader (Shader Model 3.0)

            ENDCG

        }

    }

}
