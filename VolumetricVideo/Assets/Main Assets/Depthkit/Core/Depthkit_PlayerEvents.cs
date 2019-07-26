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

namespace Depthkit
{
    public delegate void DepthkitClipEventHandler();

    /// <summary>
    /// Class that contains events a given player could potentially emit for listening. </summary>
    [System.Serializable]
    public class Depthkit_PlayerEvents 
    {
        public event DepthkitClipEventHandler PlaybackStarted;
        public event DepthkitClipEventHandler PlaybackPaused;
        public event DepthkitClipEventHandler PlaybackStopped;
        public event DepthkitClipEventHandler LoadingStarted;
        public event DepthkitClipEventHandler LoadingFinished;

        public virtual void OnClipPlaybackStarted()
        {   
            if(PlaybackStarted != null) { PlaybackStarted(); }
        }
 
        public virtual void OnClipPlaybackPaused()
        {   
            if(PlaybackPaused != null) { PlaybackPaused(); }
        }

        public virtual void OnClipPlaybackStopped()
        {   
            if(PlaybackStopped != null) { PlaybackStopped(); }
        }

        public virtual void OnClipLoadingStarted()
        {
            if(LoadingStarted != null) { LoadingStarted(); } 
        }

        public virtual void OnClipLoadingFinished()
        {
            if(LoadingFinished != null) { LoadingFinished(); }
        }
    }
}