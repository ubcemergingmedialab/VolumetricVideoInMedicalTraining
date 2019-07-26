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
    /// Implmentation of the Depthkit player with an AVProVideo-based backend </summary>
#if (!DK_USING_AVPRO)
    public class Depthkit_AVProVideoPlayer
    {
#else
    public class Depthkit_AVProVideoPlayer : Depthkit_ClipPlayer
    {
        [SerializeField, HideInInspector]
        /// <summary>
        /// Reference to the AVProVideo Component </summary>
        protected RenderHeads.Media.AVProVideo.MediaPlayer _mediaPlayer;

        public override void CreatePlayer()
        {
            _mediaPlayer = gameObject.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>();
            if (_mediaPlayer == null)
            {
                // no media component already added to this component, try adding a MediaPlayer component
                try
                {
                    _mediaPlayer = gameObject.AddComponent<RenderHeads.Media.AVProVideo.MediaPlayer>();
                }
                catch (Exception e)
                {
                    Debug.LogError("AVProVideo not found in project: " + e.ToString());
                }
            }
        }

        public override bool IsPlayerCreated()
        {
            return _mediaPlayer != null;
        }

        public override bool IsPlayerSetup()
        {
            if (!IsPlayerCreated())
            {
                return false;
            }

            return _mediaPlayer.m_VideoPath != null && _mediaPlayer.m_VideoPath != "";               
        }

        /// <summary>
        /// Sets the video from a path. Assumed relative to data folder path.</summary>
        public override void SetVideoPath(string path)
        {
            if (!IsPlayerCreated())
            {
                return;
            }

            path = path.Replace("\\", "/");
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring( Application.dataPath.Length);
            }

            _mediaPlayer.m_VideoLocation = RenderHeads.Media.AVProVideo.MediaPlayer.FileLocation.RelativeToProjectFolder;
            _mediaPlayer.m_VideoPath = path;
        }

        /// <summary>
        /// Get the absolute path to the video.</summary>
        public override string GetVideoPath()
        {
            if (!IsPlayerSetup())
            {
                return "";
            }

            return RenderHeads.Media.AVProVideo.MediaPlayer.GetFilePath(_mediaPlayer.m_VideoPath, _mediaPlayer.m_VideoLocation);
        }

        public override IEnumerator Load()
        {
            //start the loading operation
            _mediaPlayer.OpenVideoFromFile(_mediaPlayer.m_VideoLocation, _mediaPlayer.m_VideoPath, false);
            Events.OnClipLoadingStarted();

            //while the video is loading you can't play it
            while (!_mediaPlayer.Control.CanPlay())
            {
                VideoLoaded = false;
                yield return null;
            }
            VideoLoaded = true;
            Events.OnClipLoadingFinished();
            yield return null;
        }

        public override void StartVideoLoad()
        {
            StartCoroutine(Load());
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
            if(VideoLoaded)
            {
                _mediaPlayer.Control.Play();
                Events.OnClipPlaybackStarted();
            }
        }
        public override void Pause()
        {
            _mediaPlayer.Control.Pause();
            Events.OnClipPlaybackPaused();
        }
        public override void Stop()
        {
            _mediaPlayer.Control.Stop();
            Events.OnClipPlaybackStopped();
        }

        public override double GetCurrentTime()
        {
            return _mediaPlayer.Control.GetCurrentTimeMs() / 1000;
        }

        public override int GetCurrentFrame()
        {
            if (_mediaPlayer != null && _mediaPlayer.TextureProducer != null)
            {
                return _mediaPlayer.TextureProducer.GetTextureFrameCount();
            }
            return -1;
        }

        public override double GetDuration()
        {
            return _mediaPlayer.Info.GetDurationMs() / 1000;
        }

        public override Texture GetTexture()
        {
            if (_mediaPlayer != null && _mediaPlayer.TextureProducer != null)
            {
                return _mediaPlayer.TextureProducer.GetTexture ();
            }
            return null;
        }
        public override bool IsTextureFlipped ()
        {
            //#if (UNITY_ANDROID && !UNITY_EDITOR)
            //            return false;
            //#endif
            //            return true;
            if (_mediaPlayer != null && _mediaPlayer.TextureProducer != null)
            {
                return _mediaPlayer.TextureProducer.RequiresVerticalFlip();
            }
            return false;
        }

        public override GammaCorrection GammaCorrectDepth()
        {
            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                if (_mediaPlayer.Info != null && !_mediaPlayer.Info.PlayerSupportsLinearColorSpace())
                {
                    return GammaCorrection.None;
                }
                else
                {
                    return GammaCorrection.LinearToGammaSpace;
                }
            }
            else // ColorSpace.Gamma
            {
                return GammaCorrection.None;
            }
        }

        public override GammaCorrection GammaCorrectColor()
        {

            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                if (_mediaPlayer.Info != null && !_mediaPlayer.Info.PlayerSupportsLinearColorSpace())
                {
                    return GammaCorrection.GammaToLinearSpace;
                }
                else {
                    return GammaCorrection.None;
                }
            }
            else // ColorSpace.Gamma
            {
                return GammaCorrection.None;
            }
        }

        public override bool IsPlaying()
        {
            return _mediaPlayer.Control.IsPlaying();
        }

        public override void RemoveComponents()
        {
            if(!Application.isPlaying)
            {
                DestroyImmediate(_mediaPlayer, true);
                DestroyImmediate(this, true);
            }
            else
            {
                Destroy(_mediaPlayer);
                Destroy(this);
            }
        }

        public override AvailablePlayerType GetPlayerType()
        {
            return AvailablePlayerType.AVProVideo;
        }

        public RenderHeads.Media.AVProVideo.MediaPlayer GetPlayerBackend()
        {
            return _mediaPlayer;
        }
#endif
        }
    }