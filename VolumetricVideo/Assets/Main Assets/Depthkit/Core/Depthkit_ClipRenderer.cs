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

using UnityEngine;
using System.Collections;

namespace Depthkit
{
    [ExecuteInEditMode]
    /// <summary>
    /// The base class that any Depthkit Renderer implementation will derrive from 
    /// </summary>
    /// <remarks>
    /// This class provides methods that are implemented in child classes to allow
    /// a way for clip to be rendered in different ways
    /// </remarks>
    public abstract class Depthkit_ClipRenderer : MonoBehaviour
    {
        public enum MeshDensity
        {
            Highest,
            High,
            Medium,
            Low
        };

        protected bool _geometryDirty;
        protected bool _materialDirty;
        protected bool _textureUpdated;
        protected bool _metadataChanged;

        /// <summary>
        /// Texture that represents the current frame
        /// <summary>
        protected Texture _texture;
        // use the public getter/setter only when we need to mark the mesh dirty
        public Texture Texture
        {
            get { return _texture; }
            set
            {
                _textureUpdated = true; // use this flag to refresh other values in the renderer
                if (_texture == null && value != null)
                {
                    _geometryDirty = true;
                }
                if(_texture != null && value != null && (value.width != _texture.width || value.height != _texture.height))
                {
                    _geometryDirty = true;
                }
                _texture = value;
            }
        }

        /// <summary>
        /// Mesh Density sets the fidelity level of the current mesh. Higher means more polygons
        /// <summary>
        [SerializeField]
        protected MeshDensity _meshDensity = MeshDensity.Medium;
        // use the public getter/setter only when we need to mark the mesh dirty
        public MeshDensity Density
        {
            get { return _meshDensity; }
            set
            {
                _geometryDirty = true;
                _meshDensity = value;
            }
        }

        protected int MeshScalar
        {
            get
            {
                if (_meshDensity == MeshDensity.Highest)
                {
                    return 1;
                }
                else if (_meshDensity == MeshDensity.High)
                {
                    return 2;
                }
                else if (_meshDensity == MeshDensity.Medium)
                {
                    return 4;
                }
                else if (_meshDensity == MeshDensity.Low)
                {
                    return 8;
                }
                return 1;
            }
        }

        /// <summary>
        /// Boundaries of the mesh as determined by the metadata file.
        /// <summary>
        protected Bounds _bounds;
        public Bounds Bounds
        {
            get { return _bounds; }
            set
            {
                _bounds = value;
            }
        }

        /// <summary>
        /// Set to true if the Texture is flipped from what default unity Textures would expect
        /// <summary>
        protected bool _textureIsFlipped;
        // use the public getter/setter only when we need to mark the mesh dirty
        public bool TextureIsFlipped
        {
            get { return _textureIsFlipped; }
            set
            {
                _materialDirty = (_textureIsFlipped != value);
                _textureIsFlipped = value;
            }
        }


        /// <summary>
        /// Set to apply gamma correction to this texture in the render shader
        /// <summary>
        protected GammaCorrection _gammaCorrectDepth;
        public GammaCorrection GammaCorrectDepth
        {
            get { return _gammaCorrectDepth; }
            set
            {
                _materialDirty = (_gammaCorrectDepth != value);
                _gammaCorrectDepth = value;
            }
        }

        /// <summary>
        /// Set to apply gamma correction to this texture in the render shader
        /// <summary>
        protected GammaCorrection _gammaCorrectColor;
        public GammaCorrection GammaCorrectColor
        {
            get { return _gammaCorrectColor; }
            set
            {
                _materialDirty = (_gammaCorrectColor != value);
                _gammaCorrectColor = value;
            }
        }

        /// <summary>
        /// Metadata contains information about the current clip
        /// <summary>
        [SerializeField, HideInInspector]
        protected Depthkit_Metadata _metadata;
        // use the public getter/setter only when we need to mark the mesh dirty
        public Depthkit_Metadata Metadata
        {
            get { return _metadata; }
            set
            {
                _geometryDirty = true;
                _metadataChanged = true;
                _metadata = value;
                if (_metadata != null)
                {
                    Bounds = new Bounds (_metadata.boundsCenter, _metadata.boundsSize);
                }

            }
        }

        [HideInInspector]
        public float _depthBrightnessThreshold;
        [HideInInspector]
        public float _internalEdgeCutoffAngle;

        public virtual void SetGeometryDirty()
        {
            _geometryDirty = true;
        }

        public virtual void SetMaterialDirty()
        {
            _materialDirty = true;
        }

        /// <summary>
        /// Render type returns the version of the DepthKitSDK this implementation was released against
        /// <summary>
        public abstract Version GetSDKVersion();

        /// <summary>
        /// Render type returns the version of the implementation
        /// <summary>
        public abstract Version GetVersion();

        /// <summary>
        /// Render type returns the appropriate enum for each subclass implementation
        /// <summary>
        public abstract RenderType GetRenderType();

        /// <summary>
        /// Cleans the scene of all scripts and game objects generated by this renderer
        /// <summary>
        public abstract void RemoveComponents();


        /// <summary>
        /// Submits Draw calls to the current state of the renderer
        /// <summary>
        public abstract void Draw();

        /// <summary>
        /// Sets all the parameters on materials built using the Depthkit.cginc convenience functions
        /// call this from the subclasses
        /// <summary>
        protected virtual void SetMaterialProperties(Material material)
        {
            if (material == null || _metadata == null)
            {
                Debug.LogWarning("Couldn't set material props");
                return;
            }

            material.SetTexture("_MainTex", _texture);
            material.SetTexture("_MainTex2", _texture);
            material.SetInt("_TextureFlipped", _textureIsFlipped ? 1 : 0);
            material.SetInt("_ColorSpaceCorrectionDepth", (int)_gammaCorrectDepth);
            material.SetInt("_ColorSpaceCorrectionColor", (int)_gammaCorrectColor);

            //dynamic color space switching
            material.SetVector("_TextureDimensions", new Vector4(_metadata.textureWidth, _metadata.textureHeight, 0.0f, 0.0f));

            //tweak props
            material.SetFloat("_DepthBrightnessThreshold", _depthBrightnessThreshold);
            material.SetFloat("_SheerAngleThreshold", Mathf.Pow(_internalEdgeCutoffAngle, 6.0f));
            material.SetVector("_MeshScalar", new Vector4( (float)MeshScalar, (float)MeshScalar, 0, 0) );

            // only supporting 1 perspective for now, can extend for multi-perspective in the future
            if (_metadata.perspectives != null && _metadata.perspectives.Length >= 1)
            {
                var perspective = _metadata.perspectives[0];
                material.SetVector("_Crop", perspective.crop);
                material.SetFloat("_ClipEpsilon", perspective.clipEpsilon);
                material.SetVector("_ImageDimensions", perspective.depthImageSize);
                material.SetVector("_FocalLength", perspective.depthFocalLength);
                material.SetVector("_PrincipalPoint", perspective.depthPrincipalPoint);
                material.SetFloat("_NearClip", perspective.nearClip);
                material.SetFloat("_FarClip", perspective.farClip);
                material.SetMatrix("_Extrinsics", perspective.extrinsics);
                material.SetMatrix("_InverseExtrinsics", perspective.extrinsics.inverse);
                material.SetMatrix("_ExtrinsicsToObject", perspective.extrinsics.inverse * transform.worldToLocalMatrix );
            }
        }

        public void GetMeshLattice(ref Mesh mesh)
        {
            if (_texture == null) {
                return;
            }

            if (_texture.width == 0 || _texture.height < 2) {
                Debug.Log ("texture too small");
                return;
            }

            int textureWidth = _texture.width;
            //divide by two for combined per pixel format
            int textureHeight = _texture.height / 2;

            //don't scale below the size of the actual mesh
            while (MeshScalar > textureWidth || MeshScalar > textureHeight)
            {
                Debug.LogError("texture too small for Mesh Density. Increase Mesh Density to show textures this size.");
                return;
            }

            int vertsWide = (textureWidth / MeshScalar) + 1;
            int	vertsTall = (textureHeight / MeshScalar) + 1;

            //////////////////////////////////////////////
            /// Build mesh
            //////////////////////////////////////////////

            mesh.Clear ();

            //Index format MUST be set before any vertices / indices are set on the mesh
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            Vector3[] verts = new Vector3[vertsWide * vertsTall];
            int[] indices = new int[(vertsWide - 1) * (vertsTall - 1) * 2 * 3];
            int curIndex = 0;
            for (int y = 0; y < vertsTall - 1; y++) {
                for (int x = 0; x < vertsWide - 1; x++) {
                    indices [curIndex++] = x + y * vertsWide;
                    indices [curIndex++] = x + (y + 1) * vertsWide;
                    indices [curIndex++] = (x + 1) + y * vertsWide;

                    indices [curIndex++] = (x + 1) + (y) * vertsWide;
                    indices [curIndex++] = x + (y + 1) * vertsWide;
                    indices [curIndex++] = (x + 1) + (y + 1) * vertsWide;
                }
            }

            Vector4 vertexStep = new Vector4 ((float)MeshScalar / (float)(textureWidth), (float)MeshScalar / (float)(textureHeight), 0.0f, 0.0f);
            curIndex = 0;

            for (int y = 0; y < vertsTall; y++) {
                for (int x = 0; x < vertsWide; x++) {
                    verts [curIndex].x = x * vertexStep.x;
                    verts [curIndex].y = y * vertexStep.y;
                    verts [curIndex].z = 0;
                    curIndex++;
                }
            }

            mesh.vertices = verts;
            mesh.SetIndices (indices, MeshTopology.Triangles, 0);
            mesh.bounds = _bounds;
        }
    }
}