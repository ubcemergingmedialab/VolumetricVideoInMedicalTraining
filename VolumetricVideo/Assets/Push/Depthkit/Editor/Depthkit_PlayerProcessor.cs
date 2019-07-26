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
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;
namespace Depthkit
{
    [InitializeOnLoad]
    public class Depthkit_PlayerProcesser : AssetPostprocessor
    {


        static void AddPlayerDefine(string target)
        {
            
            //set the target across all supported platforms
            for (int pIndex = 0; pIndex < Depthkit_Info.SupportedPlatforms.Length; pIndex++)
            {
                //get the exisiting defines
                string existingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(Depthkit_Info.SupportedPlatforms[pIndex]);
                int defineIndex;
                List<string> defineList;
                if(!DefineExistsInPlatformDefines(existingDefines, target, out defineList, out defineIndex))
                {
                    //add the new define
                    defineList.Add(target);

                    //combine the strings back into the proper define style
                    string newDefines = string.Join(";", defineList.ToArray());
                    //add the defines
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(Depthkit_Info.SupportedPlatforms[pIndex], newDefines);
                }
            }
        }

        static void RemovePlayerDefine(string target)
        {
            //remove the target across all supported platforms
            for (int pIndex = 0; pIndex < Depthkit_Info.SupportedPlatforms.Length; pIndex++)
            {
                //get the exisiting defines
                string existingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(Depthkit_Info.SupportedPlatforms[pIndex]);
                int defineIndex;
                List<string> defineList;
                if(DefineExistsInPlatformDefines(existingDefines, target, out defineList, out defineIndex))
                {
                    defineList.RemoveAt(defineIndex); 

                    //combine the strings back into the proper define style
                    string newDefines = string.Join(";", defineList.ToArray());

                    //add the defines
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(Depthkit_Info.SupportedPlatforms[pIndex], newDefines);
                }
            }
            
            if(target != Depthkit_Info.ZERODAYSLOOK_DEFINE)
            {
                ResetClipsAffectedByDefineChange(Depthkit_Info.DirectiveDict[target]);
            }
        }

        static bool DefineExistsInPlatformDefines(string platformDefines, string targetDefine, out List<string> defineList, out int index)
        {
            //assign index a bum value
            index = 0;

            //split the platform defines
            string[] defines = platformDefines.Split(';');

            //make the new define list
            defineList = new List<string>(defines);

            //check if the define exists
            for (int i = defineList.Count-1; i >= 0; i--)
            {
                if(defines[i].Contains(targetDefine))
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        //unused
        public static List<PlayerType> GetSupportedPlayersInAssembly()
        {
            string existingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string[] defines = existingDefines.Split(';');
            List<PlayerType> supportedPlayers = new List<PlayerType>();

            //find out if this has already been definied
            for (int i = 0; i < defines.Length; i++ )
            {
                if(defines[i].Contains("DK_"))
                {
                    supportedPlayers.Add(Depthkit_Info.DirectiveDict[defines[i]]);
                }
            }

            return supportedPlayers;
        } 

        public static void UpdateDefines()
        {
            foreach (KeyValuePair<string,string> item in Depthkit_Info.AssetSearchDict)
            {
                
                string[] assets = AssetDatabase.FindAssets(item.Key);                
                bool playerFound = false;
                if(assets.Length > 0) //the file exists
                {
                    foreach (string guid in assets)
                    {
                        //need to do this because FindAssets can return files where string is only part of the name, i.e. MediaPlayer
                        if(item.Key == Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)))
                        {
                            playerFound = true;
                            AddPlayerDefine(item.Value);
                            break; 
                        }
                    }                    
                }
                
                if(!playerFound)
                {
                    //asset doesn't exist at all so remove the define
                    RemovePlayerDefine(item.Value);             
                }
            }
        }

        static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
        {       
            UpdateDefines();
        }
        

        static void ResetClipsAffectedByDefineChange(PlayerType targetType)
        {
            Depthkit_Clip[] clips = Resources.FindObjectsOfTypeAll<Depthkit_Clip>();
            for (int i = 0; i < clips.Length; i++)
            {
                //if this clip matches the type being updated by the directive shift
                if(clips[i]._playerType == (AvailablePlayerType)targetType)
                {
                    clips[i]._playerType = Depthkit_Clip._defaultPlayerType;
                    clips[i]._needToResetPlayerType = true;
                    clips[i].ResetPlayer();
                }
            }
        }
    }
}

