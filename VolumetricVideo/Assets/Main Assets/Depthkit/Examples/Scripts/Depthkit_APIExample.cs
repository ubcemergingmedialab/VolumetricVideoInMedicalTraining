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
using Depthkit;

// This is an example class that shows you how to interact with a Depthkit Clip
// The code example here shows you how to trigger a loading action for the clip, and then begin playback once clip is loaded.
public class Depthkit_APIExample : MonoBehaviour
{
    // Assign your Depthkit GameObject to this field in the inspector (or assign the clip directly)
    public Depthkit_Clip clip;
    void Start()
    {
        //You can subscribe to clip events by accessing the Events property on the clip.
        //Here we subscribe to LoadingStarted to make sure our loading function is being called
        clip.Events.LoadingStarted += OnClipLoadingStart;
        //Then we are subscribing our OnClipLoadingFinished function to the LoadingFinished event of the clip so we know when to safely trigger playback.
        clip.Events.LoadingFinished += OnClipLoadingFinished;

        //there are a few other events you can subscribe to
        clip.Events.PlaybackStarted += OnClipPlaybackStarted; // called when playback has started
        clip.Events.PlaybackPaused += OnClipPlaybackPaused; //called when playback has paused
        clip.Events.PlaybackStopped += OnClipPlaybackStopped; // called when playback has stopped  
        
        // Once we have subscribed to the loading events, we can start loading the clip
        clip.Player.StartVideoLoad();

        //You can call other methods of your controller like this:

        // Loading Methods
        // clip.Controller.StartVideoLoad(); // load the video - very important! should be called before trying playback
        // StartCoroutine(clip.Controller.Load()); // load the clip manually through a coroutine
        // StartCoroutine(clip.Controller.LoadAndPlay()); // load the clip in a coroutine and then play it. not recommended.
        
        // Methods to call once clip has loaded
        // clip.Controller.Play(); // tell the clip to play
        // clip.Controller.IsPlaying(); // check if the video is currently playing
        // clip.Controller.SeekTo(1.0f); // seek to a specific location in the clip
        // clip.Controller.SetLoop(true); // set the loop status of the clip
        // clip.Controller.SetVolume(5.0f); // set the volume of the clip
        // clip.Controller.GetCurrentTime(); //get the current time in playback
        // clip.Controller.GetDuration(); // get the length of the clip
        // clip.Controller.GetPlayerType(); //get the type of player playing the clip

        // Methods to call once clip has started playing
        // clip.Controller.Pause(); // tell the clip to pause
        // clip.Controller.Stop(); // tell the clip to stop
    }

    void OnClipLoadingStart()
    {
        // If we are inside this block, we can confirm that our controller function worked and we are subscribed to the loading event; 
        Debug.Log("APIExample: Loading Started");
    }

    void OnClipLoadingFinished()
    {
        // When this code is reached we know that the Clip has been loaded, so we can safely call play
        // If Play is called without a clip being properly loaded, it will return null and not play the clip.
        Debug.Log("APIExample: Loading Finished");
        clip.Player.Play();
    }

    void OnClipPlaybackPaused()
    {
        // Debug.Log("APIExample: Playback Paused");
    }

    void OnClipPlaybackStarted()
    {
        // Debug.Log("APIExample: Playback Started");
    }

    void OnClipPlaybackStopped()
    {
        // Debug.Log("APIExample: Playback Stopped");
    }
}
