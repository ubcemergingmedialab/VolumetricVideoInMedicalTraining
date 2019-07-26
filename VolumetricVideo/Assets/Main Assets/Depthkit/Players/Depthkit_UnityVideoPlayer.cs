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
using System;
using System.Collections;

namespace Depthkit
{
    /// <summary>
    /// Implmentation of the Depthkit player with the Unity VideoPlayer-based backend.
    /// </summary>
    public class Depthkit_UnityVideoPlayer : Depthkit_ClipPlayer
    {
        //reference to the MovieTexture passed in through Clip
        [SerializeField, HideInInspector]
        protected UnityEngine.Video.VideoPlayer _mediaPlayer;
        [SerializeField, HideInInspector]
        protected AudioSource _audioSource;

        public override void CreatePlayer()
        {

            _mediaPlayer = gameObject.GetComponent<UnityEngine.Video.VideoPlayer>();

            if (_mediaPlayer == null)
            {
                _mediaPlayer = gameObject.AddComponent<UnityEngine.Video.VideoPlayer>();
            }

            _audioSource = gameObject.GetComponent<AudioSource>();

            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            _mediaPlayer.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.AudioSource;
            _mediaPlayer.SetTargetAudioSource(0, _audioSource);
            _mediaPlayer.renderMode = UnityEngine.Video.VideoRenderMode.APIOnly;
            _mediaPlayer.prepareCompleted += OnVideoLoadingComplete;
            _mediaPlayer.EnableAudioTrack(0, true);
        }

        public override bool IsPlayerCreated()
        {
            return _mediaPlayer != null;
        }

        public override bool IsPlayerSetup()
        {
            if(!IsPlayerCreated())
            {
                return false;
            }

            if(_mediaPlayer.source == UnityEngine.Video.VideoSource.VideoClip)
            {
                return _mediaPlayer.clip != null;
            }
            else
            {
                return _mediaPlayer.url != null && _mediaPlayer.url != "";
            }
        }

        /// <summary>
        /// Sets the video from a path. Assumed relative to data foldder file path.</summary>
        public override void SetVideoPath(string path)
        {
            if (!IsPlayerCreated())
            {
                return;
            }
            path = path.Replace("\\", "/");
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }
            _mediaPlayer.source = UnityEngine.Video.VideoSource.Url;
            _mediaPlayer.url = path;
        }

        /// <summary>
        /// Get the absolute path to the video.</summary>
        public override string GetVideoPath()
        {
            if (!IsPlayerSetup())
            {
                return "";
            }

            if (_mediaPlayer.source == UnityEngine.Video.VideoSource.VideoClip)
            {
                return _mediaPlayer.clip.originalPath;
            }
            else
            {
                return _mediaPlayer.url;
            }
        }

        public override void StartVideoLoad()
        {
            
            StartCoroutine(Load());
        }

        public override IEnumerator Load()
        {
            Events.OnClipLoadingStarted();
            _mediaPlayer.Prepare();
            yield return null;
        }

        public void OnVideoLoadingComplete(UnityEngine.Video.VideoPlayer player)
        {
            VideoLoaded = true;
            Events.OnClipLoadingFinished();
        }

        public override IEnumerator LoadAndPlay()
        {
            StartVideoLoad();
            while (!VideoLoaded)
            {
                yield return null;
            }
            Play();
            yield return null;
        }

        public override void Play()
        {
            _mediaPlayer.Play();
            Events.OnClipPlaybackStarted();
        }
        public override void Pause()
        {
            _mediaPlayer.Pause();
            Events.OnClipPlaybackPaused();
        }
        public override void Stop()
        {
            _mediaPlayer.Stop();
            Events.OnClipPlaybackStopped();
        }

        public override int GetCurrentFrame()
        {
            return (int)_mediaPlayer.frame;
        }
        public override double GetCurrentTime()
        {
            return _mediaPlayer.time;
        }

        public override double GetDuration()
        {
            return _mediaPlayer.clip.length;
        }

        public override Texture GetTexture()
        {
            return _mediaPlayer.texture;
        }
        public override bool IsTextureFlipped ()
        {
            return false;
        }
        public override GammaCorrection GammaCorrectDepth()
        {
            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
#if UNITY_2018_2_OR_NEWER
                return GammaCorrection.LinearToGammaSpace;
#elif UNITY_2017_1_OR_NEWER
                //https://issuetracker.unity3d.com/issues/regression-videos-are-dark-when-using-linear-color-space?page=1
                Debug.LogWarning("Unity Video Player does not display correct color on Windows between version 2017.1 and 2018.2. Use AVPro, switch to Gamma Color Space, or upgrade Unity to use Depthkit with this project.");
                return GammaCorrection.LinearToGammaSpace2x;
#else
                return GammaCorrection.LinearToGammaSpace;
#endif
            }
            else
            {
                return GammaCorrection.None;
            }
        }
        public override GammaCorrection GammaCorrectColor()
        {
            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
#if UNITY_2018_2_OR_NEWER
                return GammaCorrection.None;
#elif UNITY_2017_1_OR_NEWER
                return GammaCorrection.LinearToGammaSpace;
#else
                return GammaCorrection.None;
#endif
            }
            else
            {
                return GammaCorrection.None;
            }
        }
        public override bool IsPlaying()
        {
            return _mediaPlayer.isPlaying;
        }

        public override void RemoveComponents()
        {
            if(!Application.isPlaying)
            {
                DestroyImmediate(_mediaPlayer, true);
                DestroyImmediate(_audioSource, true);
                DestroyImmediate(this, true);
            }
            else
            {
                Destroy(_mediaPlayer);
                Destroy(_audioSource);
                Destroy(this);
            }
        }

        public override AvailablePlayerType GetPlayerType()
        {
            return AvailablePlayerType.UnityVideoPlayer;
        }

        public UnityEngine.Video.VideoPlayer GetPlayerBackend()
        {
            return _mediaPlayer;
        }
    }
}