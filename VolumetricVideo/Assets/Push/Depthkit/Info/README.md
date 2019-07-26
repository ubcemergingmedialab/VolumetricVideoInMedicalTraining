# **Depthkit Unity Plugin**
Copyright 2018 Scatter All Rights reserved.

***

## **Quickstart**

To quckly get up and running with your own Depthkit capture, click the Depthkit menu item in Unity and select the "Create Depthkit Clip" option. Inside the inspector, select your target movie player, then drag in your video file, metadata file, and poster frame. Ensure your camera is facing the proper direction, then press play. You should see your capture in motion!

For more documentation on getting started, those who have registered for the beta can access the Unity tutorials through the Beta portal.

For further support, feel free to reach out to us at support@depthkit.tv.


## **Using other players**

The Depthkit Unity plugin allows for users to use other player backends for Depthkit captures so that they can leverage different video players's features without needing to totally rely on the native Unity VideoPlayer component. We currently support AVProVideo and MPMP as external backends. To add these to your project, simply add them to a Depthkit project, then change the PlayerType setting on your Clip. 

## **Changelog**
### 0.2.0
- Added the Hologram render type accessible from the Depthkit Clip object, Render Options.
- Various fixes to the playback functionality.
- UI interface to load player meta data file dynamically from a path of your choosing.

### 0.1.9 - BREAKING CHANGE
- Enables support for Unity 2017
- Fixes issue where VideoPlayer wasn't being properly subscribing to the Prepare() callback
- Full deprecates use of MovieTexture and SpriteSequence in favor of having Unity's VideoClip become the primary playback method for clips. Use of MovieTexture/SpriteSequence Players will only work in plugin version 0.1.8f2 and older and require Unity 5.6.2 or older.

### 0.1.8
- Fixes issue where implemented player values were being reset on undo/redo. Note that if you change your player type, your player values will still be cleared. 
- Got rid of Depthkit prefab. Now, more easily add a Depthkit Clip to your Scene by using either the Depthkit menu at the top of the screen, or right-clicking in the project heirarchy, and going to Depthkit > Depthkit Clip. This method is also now platform agnostic.

### 0.1.7
- Added support for Unity 5.6 Beta Video Player - this player plays on a lot of platforms and should now be seen as the default player type.
- Due to the above, have now deprecated support for SpriteSequence based players.
- Fixes problem with "lasers" being cast back from Depthkit clips


### 0.1.6
- Clip is interfaced with through properties on the Clip. Clip.Events returns the events emitted by playback/loading, and Clip.Controller is used to control playback functionality.
- Added APIExample.cs in Scripts/Examples for scripting examples on how to interface with Depthkit from code. 
- Makes plugin component more aesthetically pleasing
- Gives a better signal if the clip is setup yet or not, as well as resolves issues with it giving an improper read
- Adds volume slider to Clip as well as SetVolume() fuction
- Adds ability to listen to player events. Access listenable events by calling Clip.Events.
- Fixes issue where MovieTextures weren't properly playing sound when their player was selected.
- Fixes issue where existing player options weren't detected when Depthkit was imported into a project.

### 0.1.5
- Fixes issue with new player define method where building wasn't possible.

### 0.1.4
- Added ability to use MPMP, AVPro, or SpriteSequence as player backend for Depthkit clips.
- Added new API to be able to genericaly interface with a given clip's playback parameters without needing to directly access the player itself. These APIs can be accessed through the Clip.GetPlayer() function.

