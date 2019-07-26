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

//INCLUDE THIS IN YOUR INPUT STRUCT
#define DEPTHKIT_TEX_COORDS(idx, idx2, idx3) \
	float2 uv_MainTex : TEXCOORD##idx; \
	float2 uv2_MainTex2 : TEXCOORD##idx2; \
    float3 worldPos : TEXCOORD##idx3;

//INCLUDE THIS IF YOU ARE NOT USING A SURFACE SHADER
#define DEPTHKIT_TEX_ST float4 _MainTex_ST; \
			            float4 _MainTex2_ST;

float _DepthBrightnessThreshold = 0.5;  // per-pixel brightness threshold, used to refine edge geometry from eroneous edge depth samples
float _SheerAngleThreshold = 0.0;       // per-pixel internal edge threshold (sheer angle of geometry at that pixel)

//All Depthkit params
sampler2D _MainTex;
sampler2D _MainTex2;
float4 _MainTex_TexelSize;
float4 _MainTex2_TexelSize;

float4 _Crop;
float _ClipEpsilon;
float2 _ImageDimensions;
float2 _FocalLength;
float2 _PrincipalPoint;
float _NearClip;
float _FarClip;
float4x4 _Extrinsics;
float4x4 _InverseExtrinsics;
float4x4 _ExtrinsicsToObject;
float2 _MeshScalar;
int _TextureFlipped;
int _ColorSpaceCorrectionDepth;
int _ColorSpaceCorrectionColor;

// blend modes reflected from scripts
#define BLEND_ALPHA 0
#define BLEND_ADD 1
#define BLEND_MULTIPLY 2
#define BLEND_SCREEN 3

#define CORRECT_NONE 0
#define CORRECT_LINEAR_TO_GAMMA 1
#define CORRECT_GAMMA_TO_LINEAR 2
//Unity 2017.1 - 2018.2 has a video player bug where Linear->Gamma needs to be applied twice before texture look up in depth
#define CORRECT_LINEAR_TO_GAMMA_2X 3

#define BRIGHTNESS_THRESHOLD_OFFSET 0.01f

#define FLOAT_EPS 0.00001f

fixed3 rgb2hsv(fixed3 c)
{
    fixed4 K = fixed4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    fixed4 p = lerp(fixed4(c.bg, K.wz), fixed4(c.gb, K.xy), step(c.b, c.g));
    fixed4 q = lerp(fixed4(p.xyw, c.r), fixed4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    return fixed3(abs(q.z + (q.w - q.y) / (6.0 * d + FLOAT_EPS)), d / (q.x + FLOAT_EPS), q.x);
}

float depthForPoint(float2 texturePoint)
{   
    float2 centerpix = _MainTex_TexelSize.xy * .5;
    texturePoint += centerpix;

    // clamp to texture bounds - 0.5 pixelsize so we do not sample outside our texture
    texturePoint = _TextureFlipped == 1 ? clamp(texturePoint, float2(0, 0.5) + centerpix, float2(1.0, 1.0) - centerpix) : clamp(texturePoint, float2(0, 0) + centerpix, float2(1.0, 0.5) - centerpix);

    float4 textureSample = float4(texturePoint.x, texturePoint.y, 0.0, 0.0);
    // if Unity is rendering in Linear space and we are using unity video player (which must encode frames in gamma space), we apply inverse gamma (1/2.2) to depth frame to get a linear sample
    float4 depthsample = tex2Dlod(_MainTex, textureSample);

    if (_ColorSpaceCorrectionDepth == CORRECT_LINEAR_TO_GAMMA)
    {
        depthsample.rgb = float3(LinearToGammaSpaceExact(depthsample.r),
                                 LinearToGammaSpaceExact(depthsample.g),
                                 LinearToGammaSpaceExact(depthsample.b));
    }
    else if (_ColorSpaceCorrectionDepth == CORRECT_GAMMA_TO_LINEAR)
    {
        depthsample.rgb = float3(GammaToLinearSpaceExact(depthsample.r), 
                                 GammaToLinearSpaceExact(depthsample.g), 
                                 GammaToLinearSpaceExact(depthsample.b));
    }
    else if (_ColorSpaceCorrectionDepth == CORRECT_LINEAR_TO_GAMMA_2X)
    {
        depthsample.rgb = float3(
            LinearToGammaSpaceExact(LinearToGammaSpaceExact(depthsample.r)),
            LinearToGammaSpaceExact(LinearToGammaSpaceExact(depthsample.g)),
            LinearToGammaSpaceExact(LinearToGammaSpaceExact(depthsample.b)));
    }
    half3 depthsamplehsv = rgb2hsv(depthsample.rgb);

    // make the brightness filter ease out throughout the normalized 0 .. 1 range
    // we also add an offset to the brightness threshold to get rid of additional edge geometry by default
    float filtereddepth = pow(depthsamplehsv.b, 6);
    
    return filtereddepth > _DepthBrightnessThreshold + BRIGHTNESS_THRESHOLD_OFFSET ? depthsamplehsv.r : 0.0;
}


//#define FLATTEN_PROJECTION  // flatten the projection so that we can debug the per pixel edge classification (num valid neighbors)
#define NUM_NEIGHBORS 8

void dkVertexPass(float4 vertIn, inout float2 colorTexCoord, inout float2 depthTexCoord, inout float4 vertOut)
{
    float2 basetex = vertIn.xy;

    // we align our depth pixel centers with the center of each quad, so we do not require a half pixel offset
    if (_TextureFlipped == 1)
    {
        basetex.y = 1.0 - basetex.y;
        depthTexCoord = basetex * float2(1.0, 0.5) + float2(0.0, 0.5);
        colorTexCoord = basetex * float2(1.0, 0.5);
    }
    else
    {
        depthTexCoord = basetex * float2(1.0, 0.5);
        colorTexCoord = basetex * float2(1.0, 0.5) + float2(0.0, 0.5);
    }

    // coordinates are always aligned to a multiple of texture sample widths, no need to clamp to topleft
    // unlike per-pixel sampling..
    float depth = depthForPoint(depthTexCoord);

    if (depth <= _ClipEpsilon || ((1.0 - _ClipEpsilon) >= depth))
    {
        // we use a 3x3 kernel, so sampling 8 neighbors
        float2 textureStep = float2(_MainTex_TexelSize.x *_MeshScalar.x, _MainTex_TexelSize.y *_MeshScalar.y);   // modify our texture step according to decimation level
        
        const float2 neighbors[NUM_NEIGHBORS] = {
            float2(-textureStep.x, -textureStep.y),
            float2(0, -textureStep.y),
            float2(textureStep.x, -textureStep.y),
            float2(-textureStep.x, 0),
            float2(textureStep.x, 0),
            float2(-textureStep.x, textureStep.y),
            float2(0, textureStep.y),
            float2(textureStep.x, textureStep.y)
        };
        
        // if this depth sample is not valid then check neighbors
        int validNeighbors = 0;
        float maxDepth = 0.0;
        for (int i = 0; i < NUM_NEIGHBORS; i++)
        {
            float depthNeighbor = depthForPoint(depthTexCoord + neighbors[i]);
            maxDepth = max(maxDepth, depthNeighbor);
            validNeighbors += (depthNeighbor > _ClipEpsilon || ((1.0 - _ClipEpsilon) < depthNeighbor)) ? 1 : 0;
        }

        // clip to near plane if we and all our neighbors are invalid
        depth = validNeighbors > 0 ? maxDepth : 0;
    }

#if defined(FLATTEN_PROJECTION)
    vertOut = float4(vertIn.x, vertIn.y, 0, 1);
#else
    // project depth
    float2 imageCoordinates = _Crop.xy + (basetex * _Crop.zw);

    // transform from 0..1 space to near-far space Z
    float z = depth * (_FarClip - _NearClip) + _NearClip;

    float2 ortho = imageCoordinates * _ImageDimensions - _PrincipalPoint;
    float2 proj = ortho * z / _FocalLength;

    proj.y = _TextureFlipped ? -proj.y : proj.y;

    vertOut = float4(proj, z, vertIn.w);
    vertOut = mul(_Extrinsics, vertOut);
#endif
}

void dkFragmentPass(float2 depthTexCoord, float2 colorTexCoord, float3 vertPos, inout float3 col, bool bSheerCulling = true)
{
    float2 centerpix = _MainTex_TexelSize.xy * .5;

    float2 centerDepthSampleCoord = depthTexCoord - fmod(depthTexCoord, _MainTex_TexelSize.xy); // clamp to start of pixel
    float depth = depthForPoint(centerDepthSampleCoord);

    // clamp to texture bounds - 0.5 pixelsize so we do not sample outside our texture
    colorTexCoord = _TextureFlipped == 1 ? clamp(colorTexCoord, float2(0, 0) + centerpix, float2(1.0, 0.5) - centerpix) : clamp(colorTexCoord, float2(0, .5) + centerpix, float2(1.0, 1.0) - centerpix);

    // if unity is rendering in linear space and we are using external gamma corrected assets, then encode to gamma space pow(2.2) to match the input assets
    col = tex2D(_MainTex, colorTexCoord).rgb;

    if (_ColorSpaceCorrectionColor == CORRECT_LINEAR_TO_GAMMA)
    {
        col.rgb = LinearToGammaSpace(col);
    }
    else if (_ColorSpaceCorrectionColor == CORRECT_GAMMA_TO_LINEAR)
    {
        col.rgb = GammaToLinearSpace(col);
    }

    //convert back from worldspace to local space
    float4 localPos = mul(_ExtrinsicsToObject, float4(vertPos,1.0));

    //convert to homogenous coordinate space
    localPos.xy = localPos.xy / localPos.z;

    //find local space normal for triangle surface
    float3 dx = ddx(localPos);
    float3 dy = ddy(localPos);
    float3 n = normalize(cross(dx, dy));

    // make sure to handle dot product of the whole hemisphere by taking the absolute of range -1 to 0 to 1
    float sheerAngle = abs(dot(n, float3(0,0,1)));
    //Leave this in for quick debugging
    //col.rgb = float3(sheerAngle, sheerAngle, sheerAngle);
    //col.rgb = n * .5 + .5;
    //col.rgb = dy;

#if !defined(FLATTEN_PROJECTION)

    // we filter the _SheerAngleThreshold value on CPU so that we have an ease in over the 0..1 range, removing internal geometry at grazing angles
    // we also apply near and far clip clipping, the far clipping plane is pulled back to remove geometry wrapped to the far plane from the near plane

    if ( depth < _ClipEpsilon ||
         depth > (1.0f - _ClipEpsilon) ||
        (sheerAngle < (_SheerAngleThreshold + FLOAT_EPS) && bSheerCulling == true))
    {
        discard;
    }
#endif
}

