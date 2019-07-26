/************************************************************************************

Depthkit Unity SDK License v1
Copyright 2016-2018 Scatter All Rights reserved.  

Licensed under the Scatter Software Development Kit License Agreement (the "License"); 
you may not use this SDK except in compliance with the License, 
which is provided at the time of installation or download, 
or which otherwise accompanies this software in either electronic or hard copy form.  

You may obtain a copy of the License at http://www.depthkit.tv/license-agreement-v1

Unless required by applicable law or agreed to in writing, 
the SDK distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and limitations under the License. 

************************************************************************************/

Shader "Depthkit/Photo" 
{
    Properties
    {
    //		//Main texture is the combined color and depth video frame
            _MainTex ("Texture", 2D) = "white" {}
    //		_MainTex2 ("Texture", 2D) = "white" {} //we currently set the same texture twice due to a bug in unity to pass multiple texture coordinates
    //		//Size of the actual texture being passed in
    //		_TextureDimensions ("Texture Dimension", Vector) = (0, 0, 0, 0)
    //		//Crop factor that shows where from the original depth frame the texture is sampling
    //		_Crop ("Crop", Vector) = (0,0,0,0)
    //		//Original depth frame image dimensions
    //		_ImageDimensions ("Image Dimensions", Vector) = (0,0,0,0)
    //		//Focal length X/Y in terms of pixels from the original depth image (_ImageDimensions)
    //		_FocalLength ("Focal Length", Vector) = (0,0,0,0)
    //		//Principal Point in terms of pixels from the original depth image (_ImageDimensions)
    //		_PrincipalPoint ("Principal Point", Vector) = (0,0,0,0)
    //		//Near and Far bounds of depth data range for this frame
    //		_NearClip ("Near Clip", Float) = 0.0
    //		_FarClip  ("Far Clip", Float) = 0.0
    //		//Number of vertices (x/y) in textured mesh
    //		_MeshDensity ("Mesh Density", Range(0,255)) = 128
    //		//is the texture flipped
    //		_TextureFlipped ("Texture Flipped", Range(0,1) = 0
    }

    SubShader
    {

    Tags{ "Queue" = "Transparent" "RenderType" = "Opaque" "IgnoreProjector" = "True" }

    Cull Off
    Blend Off

    CGPROGRAM

    #pragma surface surf Lambert vertex:vert addshadow
    #pragma exclude_renderers d3d11_9x

    #include "../../../Resources/Depthkit.cginc"

    struct Input
    {
        DEPTHKIT_TEX_COORDS(0,1,2)
    };
    
    void vert (inout appdata_full v, out Input o)
    {
        UNITY_INITIALIZE_OUTPUT(Input, o);

        float2 colorTexCoord;
        float2 depthTexCoord;
        float4 vertOut;

        dkVertexPass(v.vertex, colorTexCoord, depthTexCoord, vertOut);

        v.texcoord.xy = colorTexCoord;
        v.texcoord1.xy = depthTexCoord;
        v.vertex = vertOut;

        o.worldPos = vertOut.xyz;

    }
        
    void surf (Input IN, inout SurfaceOutput o)
    {
        float3 col;

        dkFragmentPass(IN.uv2_MainTex2, IN.uv_MainTex, IN.worldPos, col);

        o.Emission = col.rgb;
        o.Alpha = 1;
    }

    ENDCG

    }

    Fallback "Diffuse"
}