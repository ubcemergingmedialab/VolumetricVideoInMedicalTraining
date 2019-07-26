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

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace Depthkit
{
    public enum PlayerType {
        AVProVideo = 3,
        UnityVideoPlayer = 4,
        Invalid = -1
    };

    /// <summary>
    /// The type of player that will be used as the backend for Depthkit playback 
    /// </summary>
    /// <remarks>
    /// Users can choose any of these, but playback will not be successful unless
    /// the user has imported a given player into the project.
    /// </remarks>
    public enum AvailablePlayerType {
#if DK_USING_AVPRO
        AVProVideo = PlayerType.AVProVideo,
#endif
        UnityVideoPlayer = PlayerType.UnityVideoPlayer
    }

    public enum GammaCorrection
    {
        None = 0,
        LinearToGammaSpace = 1,
        GammaToLinearSpace = 2,
        //Unity 2017.1 - 2018.2 has a video player bug where Linear->Gamma needs to be applied twice before texture look up in depth
        LinearToGammaSpace2x = 3

    }

    /// <summary>
    /// A version struct to contain a verison number in major.minor.patch format 
    /// </summary>
    /// <remarks>
    /// Version objects are equitable, compareable and implicitly convertable to string
    /// </remarks>
    public struct Version : System.IEquatable<Version>
    {
        // Read/write auto-implemented properties.
        public byte major { get; private set; }
        public byte minor { get; private set; }
        public byte patch { get; private set; }

        public Version(byte major, byte minor = 0, byte patch = 0)
            : this()
        {
            this.major = major;
            this.minor = minor;
            this.patch = patch;
        }

        public override string ToString()
        {
            return major.ToString() + "." + minor.ToString() + "." + patch.ToString();
        }

        public static implicit operator string(Version v)
        {
            return v.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is Version)
            {
                return this.Equals((Version)obj);
            }
            return false;
        }

        public bool Equals(Version other)
        {
            return (major == other.major) && (minor == other.minor) && (patch == other.patch);
        }

        public override int GetHashCode()
        {
            return (int)(major ^ minor ^ patch);
        }

        public static bool operator ==(Version lhs, Version rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Version lhs, Version rhs)
        {
            return !(lhs.Equals(rhs));
        }

        public static bool operator <(Version lhs, Version rhs)
        {
            if (lhs.major < rhs.major) { return true; }
            else if(lhs.major == rhs.major)
            {
                if (lhs.minor < rhs.minor) { return true; }
                else if(lhs.minor == rhs.minor)
                {
                    if (lhs.patch < rhs.patch) { return true; }
                }
            }
            return false;
        }

        public static bool operator >(Version lhs, Version rhs)
        { 
            return !(lhs < rhs);
        }

        public static bool operator <=(Version lhs, Version rhs)
        {
            if (lhs == rhs) return true;
            return lhs < rhs;
        }

        public static bool operator >=(Version lhs, Version rhs)
        {
            if (lhs == rhs) return true;
            return lhs > rhs;
        }
    }

    public class Depthkit_Info {

        public static Version Version = new Version(0, 2, 6);

        public const string AVPRO_DEFINE = "DK_USING_AVPRO";
        public const string ZERODAYSLOOK_DEFINE = "DK_USING_ZERODAYSLOOK";

        // Mapping of defines to their implemented player values 
        public static Dictionary<string, PlayerType> DirectiveDict = new Dictionary<string, PlayerType>(){
            {AVPRO_DEFINE, PlayerType.AVProVideo},
        };

        // Which asset should be searched for to see if a player has been added
        // For example, if someone adds AVProVideo, they will have a file called MediaPlayer.cs now in their project
        public static Dictionary<string, string> AssetSearchDict = new Dictionary<string, string>() { 
            {"MediaPlayer", Depthkit_Info.AVPRO_DEFINE}, 
            {"Depthkit_ZeroDaysLook", Depthkit_Info.ZERODAYSLOOK_DEFINE} 
        };

#if UNITY_EDITOR
        public static BuildTargetGroup[] SupportedPlatforms = new BuildTargetGroup[] {
            BuildTargetGroup.Android,
            BuildTargetGroup.Standalone,
            BuildTargetGroup.iOS,
            BuildTargetGroup.PS4,
            BuildTargetGroup.tvOS,
            BuildTargetGroup.XboxOne,
            BuildTargetGroup.WSA
        };
#endif
    }
}