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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.Reflection;
using System.ComponentModel;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Depthkit
{

    /// <summary>
    /// The type of rendering that should be used for Depthkit clip </summary>
    /// <remarks>
    /// Users can extend rendering by placing a new renderer in here. </remarks>
    public enum RenderType
    {
        [Description("PhotoLook")]
        Photo,
#if DK_USING_ZERODAYSLOOK
        [Description("ZeroDaysLook")]
        ZeroDays,
#endif
#if DK_USING_MULTIPERSPECTIVE
        [Description("MultiPerspectiveLook")]
        MultiPerspective,
#endif
#if DK_USING_SIMPLE_COMPUTE
        [Description("SimpleComputeLook")]
        SimpleCompute
#endif
    }

    public static class RenderTypeExt
    {
        public static string Name(this RenderType value)
        {
            DescriptionAttribute[] da = (DescriptionAttribute[])(value.GetType().GetField(value.ToString())).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return da.Length > 0 ? da[0].Description : value.ToString();
        }
        /// <summary>
        /// RenderType extension to check compatible version </summary>
        /// <remarks>
        /// The version of the installed look must be greater or equal to the version returned or an error will be logged and the look will probabaly not work. </remarks>
        public static Version CompatibleVersion(this RenderType value)
        {
            switch (value)
            {
                case RenderType.Photo:
                {
                    return new Version(0, 1, 0);
                }
#if DK_USING_ZERODAYSLOOK
                case RenderType.ZeroDays:
                    {
                        return new Version(0, 1, 0);
                    }
#endif
#if DK_USING_MULTIPERSPECTIVE
                case RenderType.MultiPerspective:
                    {
                        return new Version(0, 1, 0);
                    }
#endif
#if DK_USING_SIMPLE_COMPUTE
                case RenderType.SimpleCompute:
                    {
                        return new Version(0, 1, 0);
                    }
#endif
                default: return new Version(0, 0, 0);
            }
        }
    }

    /// <summary>
    /// A Depthkit clip </summary>
    /// <remarks>
    /// Class that holds Depthkit data and prepares clips for playback in the editor. </remarks>
    [RequireComponent(typeof(BoxCollider))]
    [ExecuteInEditMode]
    public class Depthkit_Clip : MonoBehaviour
    {

        /// <summary>
        ///  What kind of player backend is playing the Clip.</summary>
        [SerializeField]
        protected Depthkit_ClipPlayer _player;
        public Depthkit_ClipPlayer Player
        {
            get
            {
                return _player;
            }

            protected set
            {
                _player = value;
            }
        }

        public Depthkit_PlayerEvents Events
        {
            get
            {
                if (Player != null)
                {
                    return Player.Events;
                }
                Debug.LogError("Unable to access events as player is currently null");
                return null;
            }
        }

        /// <summary>
        ///  What kind of renderer backend is playing the Clip.</summary>
        [SerializeField]
        protected Depthkit_ClipRenderer _renderer;
        public Depthkit_ClipRenderer ClipRenderer
        {
            get
            {
                return _renderer;
            }

            protected set
            {
                _renderer = value;
            }
        }

        /// <summary>
        /// Set the brightness threshold for the depth texture  </summary>
        [Range(0.0f,1.0f), Tooltip("Filter out depth map samples that have a brightness lower than this value. Should be as high as possible without introducing holes in the clip during playback. This helps cleanup edge artifacts.")] 
        public float _depthLuminanceFilter = 0.5f;
        
        /// <summary>
        /// Set the internal edge filter threshold </summary>
        [Range(0.0f, 1.0f), Tooltip("Set the maximum size a randered triangle should be in order to be displayed. Triangles whose size are higher than this value won't be displayed. Should be as as low as possible to remove unwanted internal edge geometry.")]
        public float _internalEdgeCutoffAngle = 0.5f;

        /// <summary>
        /// The bounding box collider</summary>
        [SerializeField]
        protected BoxCollider _collider;

        public static AvailablePlayerType _defaultPlayerType = AvailablePlayerType.UnityVideoPlayer;

        /// <summary>
        /// The type of player, as expressed through the Unity Inspector.</summary>
        public AvailablePlayerType _playerType;

        /// <summary>
        /// The type of renderer, as expressed through the Unity Inspector.</summary>
        [Tooltip("The type of render style you want to use for your clip. Specific render parameters can be accessed in the render's component.")]
        public RenderType _renderType = RenderType.Photo;


        #region Imported Depthkit Data
        public enum MetadataSourceType {
            TextAsset,
            FilePath
        }
        /// <summary>
        /// The metadata path, relative to StreamingAssets that cooresponds to a given clip. This is exported from Visualize.</summary>
        [Tooltip("The path to your metadata file, relative to StreamingAssets")]
        public string _metaDataFilePath;
        /// <summary>
        /// The metadata file that cooresponds to a given clip. This is exported from Visualize.</summary>
        [Tooltip("Your metadata TextAsset file, generated when you bring your metadata file anywhere into Unity's Assets/ folder")]
        public TextAsset _metaDataFile;
        /// <summary>
        /// The Type of Metadata file you're provided to the Depthkit renderer</summary>
        [Tooltip("The type of Metadata file you're providing for this clip.")]
        public MetadataSourceType _metaDataSourceType = MetadataSourceType.TextAsset;


        /// <summary>
        /// The poster frame for a Depthkit capture.</summary>
        public Texture2D _poster;

        /// <summary> Reference to the metadata object fromed from the imported metadata file</summary>
        [SerializeField]
        private Depthkit_Metadata _metaData;
        #endregion

        /// <summary>Should the player backend be updated</summary>
        public bool _needToResetPlayerType;
        /// <summary>Should the player values be updated</summary>
        public bool _needToRefreshPlayerValues;
        /// <summary>Should the renderer backend be updated</summary>
        public bool _needToResetRenderType;
        /// <summary>Should the renderer backend be updated</summary>
        public bool _needToRefreshMetadata;

        /// <summary>True when the Clip Player is sucessfully configured.</summary>
        protected bool _playerSetup;
        public bool PlayerSetup
        {
            get 
            {
                return _playerSetup;
            }
        }

        /// <summary>True when valid metadata is loaded into Metada object.</summary>
        protected bool _metaSetup;
        public bool MetaSetup
        {
            get 
            {
                return _metaSetup;
            }
        }

        /// <summary>true when the renderer is configured properly.</summary>
        protected bool _rendererSetup;
        public bool RendererSetup
        {
            get 
            {
                return _rendererSetup;
            }
        }

        private int _lastFrame = -1;

        /// <summary>Whether or not the clip is fully setup</summary>
        public bool IsSetup
        {
            get
            {
                return _playerSetup && _rendererSetup && _metaSetup;
            }
        }

        void Start()
        {
            _needToResetPlayerType = false;
            _needToRefreshPlayerValues = false;
            _needToRefreshMetadata = false;
            _needToResetRenderType = false;

            _collider = GetComponent<BoxCollider>();
        }

        void Reset() //native monobehavior call
        {
            _playerType = _defaultPlayerType;
        } 

        /// <summary>      
        /// Configures the player with a TextAsset resource
        /// </summary>
        public void Setup(AvailablePlayerType playerType, RenderType renderType, TextAsset metadata)
        {
            Setup(playerType, renderType);
            _metaDataSourceType = MetadataSourceType.TextAsset;
            _metaDataFile = metadata;
            RefreshMetaData();
        }

        /// <summary>      
        /// Configures the player with a dynamic metadata file 
        /// </summary>
        public void Setup(AvailablePlayerType playerType, RenderType renderType, string metadataPath)
        {
            Setup(playerType, renderType);
            _metaDataSourceType = MetadataSourceType.FilePath;
            _metaDataFilePath = metadataPath;
            RefreshMetaData();
        }

        /// <summary>      
        /// Configures the player with a render type and player type, but no path
        /// </summary>
        protected void Setup(AvailablePlayerType playerType, RenderType renderType)
        {
            _playerType = playerType;
            _renderType = renderType;

            //build the components
            ResetPlayer();
            ResetRenderer();
        }

        void Update()
        {

            //safety checks
            if (_renderer == null || !_rendererSetup)
            {
                ResetRenderer();
            }

            if (_player == null)
            {
                ResetPlayer();
            }

            if (!_playerSetup)
            {
                _needToRefreshPlayerValues = true;
            }
            
            if(!_metaSetup)
            {
                _needToRefreshMetadata = true;
            }

            if (_needToResetRenderType)
            {
                ResetRenderer();
                _needToResetRenderType = false;
            }

            if (_needToResetPlayerType)
            {
                ResetPlayer();
                _needToResetPlayerType = false;
            }

            if (_needToRefreshPlayerValues || !_player.IsPlayerCreated())
            {
                RefreshPlayerValues();
                _needToRefreshPlayerValues = false;
            }

            if (_needToRefreshMetadata)
            {
                RefreshMetaData();
                _needToRefreshMetadata = false;
            }

            RefreshRendererValues();

        }

        public void OnValidate()
        {
            ValidateRendererVersion();
            RefreshRendererValues();
        }

        /// <summary>
        /// Called when the player itself changes </summary>
        public void ResetPlayer()
        {
            //try to set the player variable to any Player type component on this script
            Player = gameObject.GetComponent<Depthkit_ClipPlayer>();

            //Short circuit if we are setting the same player as before
            if (Player != null && Player.GetPlayerType() == _playerType)
            {
                return;
            }

            //destroy the components that player references
            //use a for loop to get around the component potentially shifting in the event of an undo
            Depthkit_ClipPlayer[] attachedPlayers = GetComponents<Depthkit_ClipPlayer>();
            for (int i = 0; i < attachedPlayers.Length; i++)
            {
                attachedPlayers[i].RemoveComponents();
            }
            Player = null;

            //add the new components
            switch (_playerType)
            {
#if DK_USING_AVPRO
                case AvailablePlayerType.AVProVideo:
                    {
                        Player = gameObject.AddComponent<Depthkit_AVProVideoPlayer>();
                        break;
                    }
#endif
                case AvailablePlayerType.UnityVideoPlayer:
                    {
                        Player = gameObject.AddComponent<Depthkit_UnityVideoPlayer>();
                        break;
                    }
            }

            Player.CreatePlayer();

            RefreshPlayerValues();
        }

        /// <summary>
        /// Called in the editor to ensure the renderer is compatible with this SDK version</summary>

        private void ValidateRendererVersion()
        {
            if (_renderer != null && _renderer.GetVersion() < _renderType.CompatibleVersion())
            {
                Debug.LogError(_renderType.Name() + "-" + _renderer.GetVersion() +
                    " is not compatible with current SDK version (DepthkitSDK-" + Depthkit_Info.Version +
                    "). Please upgrade your Look to at least version: " +
                    _renderType.Name() + "-" + _renderType.CompatibleVersion() + ". Latest looks can be found at https://www.depthkit.tv/downloads ");
            }
        }

        /// <summary>
        /// Called when player vars are changed but player itself isn't changed </summary>
        public void RefreshPlayerValues()
        {
            _playerSetup = Player.IsPlayerSetup();
        }

        public void ResetRenderer()
        {
            _renderer = gameObject.GetComponent<Depthkit_ClipRenderer>();

            if (_renderer != null)
            {
                //ensure the new render type isn't the same as the one we already have
                if (_renderType == _renderer.GetRenderType())
                {
                    _renderer.Metadata = _metaData;
                    _rendererSetup = true;
                    return;
                }

                Depthkit_ClipRenderer[] attachedRenderers = GetComponents<Depthkit_ClipRenderer>();
                for (int i = 0; i < attachedRenderers.Length; i++)
                {
                    attachedRenderers[i].RemoveComponents();
                }
            }

            _renderer = null;

            //add the new components
            switch (_renderType)
            {
                case RenderType.Photo:
                    _renderer = gameObject.AddComponent<Depthkit_PhotoLook>();
                    break;
                
#if DK_USING_MULTIPERSPECTIVE
                case RenderType.MultiPerspective:
                    _renderer = gameObject.AddComponent<MultiPerspectiveRenderer>();
                    break;
#endif
#if DK_USING_ZERODAYSLOOK
                case RenderType.ZeroDays:
                    _renderer = gameObject.AddComponent<Depthkit_ZeroDaysLook>();
                    break;
#endif
#if DK_USING_SIMPLE_COMPUTE
                case RenderType.SimpleCompute:
                    _renderer = gameObject.AddComponent<SimpleComputeRenderer>();
                    break;
#endif
                default:
                    Debug.LogError("Renderer Not Found");
                    _renderer = gameObject.AddComponent<Depthkit_PhotoLook>();
                    break;
            }

            ValidateRendererVersion();

            _renderer.Metadata = _metaData;

            _rendererSetup = true;
            
            RefreshRendererValues();
            #if UNITY_EDITOR
            SceneView.RepaintAll();
            #endif

        }

        /// <summary>
        /// Call to ensure the renderer has the proper texture </summary>
        protected void RefreshRendererValues()
        {
            if(_renderer == null)
            {
                return;
            }

            if (Application.isPlaying && IsSetup)
            {

                int frame = Player.GetCurrentFrame();

                if (frame == -1 || _lastFrame != frame)
                {
                    //Get the texture from the provider!
                    _renderer.Texture = Player.GetTexture();
                    _renderer.TextureIsFlipped = Player.IsTextureFlipped();
                    _renderer.GammaCorrectDepth = Player.GammaCorrectDepth();
                    _renderer.GammaCorrectColor = Player.GammaCorrectColor();
                    _lastFrame = frame;
                }
            }
            else
            {
                if (_renderer.Texture != _poster) //protect from triggering updates
                {
                    _renderer.Texture = _poster; //may be null
                }
                _renderer.TextureIsFlipped = false;

                _renderer.GammaCorrectDepth = QualitySettings.activeColorSpace == ColorSpace.Linear ? GammaCorrection.LinearToGammaSpace : GammaCorrection.None;
                _renderer.GammaCorrectColor = GammaCorrection.None;
            }

            //set the render tweak values
            _renderer._depthBrightnessThreshold = _depthLuminanceFilter;
            _renderer._internalEdgeCutoffAngle = _internalEdgeCutoffAngle;

            //If we are in the unity editor mode we need to do a force draw here to ensure any changes are seen in the viewprt
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _renderer.Draw();
            }
#endif
        }

        void RefreshMetaData()
        {
            string metaDataJson = "";
            _metaSetup = false;
            switch (_metaDataSourceType)
            {
                case MetadataSourceType.FilePath:
                    if(!string.IsNullOrEmpty(_metaDataFilePath))
                    {
                        //TODO: allow for more than just streaming assets
                        metaDataJson = System.IO.File.ReadAllText(Path.Combine(Application.streamingAssetsPath, _metaDataFilePath));
                    }
                    break;
                case MetadataSourceType.TextAsset:
                    if(_metaDataFile != null)
                    {
                        metaDataJson = _metaDataFile.text;
                    }
                    break;
            }

            //If there is no metadata, bail!
            if (metaDataJson == "")
            {
                return;
            }

            try
            {
                _metaData = Depthkit_Metadata.CreateFromJSON(metaDataJson);
            }
            catch (System.Exception)
            {
                Debug.LogError("Invaid Depthkit Metadata Format. Make sure you are using the proper metadata export from Depthkit Visualize.");
                return;
            }

            if (_collider == null)
            {
                _collider = GetComponent<BoxCollider>();
            }

            _collider.center = _metaData.boundsCenter;
            _collider.size = _metaData.boundsSize;

            if (_renderer != null)
            {
                _renderer.Metadata = _metaData;
            }

            _metaSetup = true;

        }

        void OnDrawGizmos()
        {

            if (Application.isPlaying && _metaData != null)
            {
                Gizmos.color = new Color(.5f, 1.0f, 0, 0.5f);
                Gizmos.DrawWireSphere(
                    transform.localToWorldMatrix * new Vector4(_metaData.boundsCenter.x, _metaData.boundsCenter.y, _metaData.boundsCenter.z, 1.0f),
                    transform.localScale.x * _metaData.boundsSize.x * .5f);
            }
        }

        void OnApplicationQuit()
        {
            if (Player != null)
            {
                Player.Stop();
            }
        }

    }

}