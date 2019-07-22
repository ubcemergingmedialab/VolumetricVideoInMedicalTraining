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

namespace Depthkit
{

    [CustomEditor(typeof(Depthkit_Clip))]
    [CanEditMultipleObjects]
    public class Depthkit_ClipEditor : Editor
    {

        //PLAYER PROPERTIES
        SerializedProperty _playerTypeProp;
        SerializedProperty _posterProp;
        SerializedProperty _metaDataFileProp;
        SerializedProperty _metaDataFilePathProp;
        SerializedProperty _metaDataSourceTypeProp;

        //RENDERER PROPERTIES
        SerializedProperty _renderTypeProp;
        SerializedProperty _depthBrightnessThresholdProp;
        SerializedProperty _internalEdgeCutoffAngleProp;

        //CLIP REFRESH PROPERTIES
        SerializedProperty _resetPlayerTypeProp;
        SerializedProperty _refreshPlayerValuesProp;
        SerializedProperty _refreshMetadataProp;
        Texture2D logo;

        int cachedPlayerType;
        int cachedRenderType;
        bool _needToUndoRedo;

        void OnEnable()
        {
            // subscribe to the undo event
            Undo.undoRedoPerformed += OnUndoRedo;
            _needToUndoRedo = false;

            //set the property types
            _playerTypeProp = serializedObject.FindProperty("_playerType");
            _posterProp = serializedObject.FindProperty("_poster");
            _metaDataFileProp = serializedObject.FindProperty("_metaDataFile");
            _metaDataFilePathProp = serializedObject.FindProperty("_metaDataFilePath");
            _metaDataSourceTypeProp = serializedObject.FindProperty("_metaDataSourceType");
 
            _renderTypeProp = serializedObject.FindProperty("_renderType");
            _depthBrightnessThresholdProp = serializedObject.FindProperty("_depthLuminanceFilter");
            _internalEdgeCutoffAngleProp = serializedObject.FindProperty("_internalEdgeCutoffAngle");

            cachedPlayerType = _playerTypeProp.enumValueIndex;
            cachedRenderType = _renderTypeProp.enumValueIndex;

            _resetPlayerTypeProp = serializedObject.FindProperty("_needToResetPlayerType");
            _refreshPlayerValuesProp = serializedObject.FindProperty("_needToRefreshPlayerValues");
            _refreshMetadataProp = serializedObject.FindProperty("_needToRefreshMetadata");

            logo = Resources.Load("dk-logo-32", typeof(Texture2D)) as Texture2D;
        }

        void OnUndoRedo()
        {
            _needToUndoRedo = true;
        }

        public override void OnInspectorGUI()
        {
            //update the object with the object variables
            serializedObject.Update();

            //set the clip var as the target of this inspector
            Depthkit_Clip clip = (Depthkit_Clip)target;

            // DK INFO
            OnInspectorGUI_DepthKitInfo();

            EditorGUILayout.BeginVertical("Box");
            {
                // PLAYER INFO
                OnInspectorGUI_PlayerSettings(clip);
                // META INFO
                OnInspectorGUI_PlayerMetaInfo();
                EditorGUILayout.Space();
                // PLAYER SETUP FEEDBACK
                OnInspectorGUI_PlayerSetupInfo(clip);

            }
            EditorGUILayout.EndVertical();

            // RENDERER OPTIONS
            OnInspectorGUI_CleanupFilters();

            EditorGUILayout.Space();
            OnInspectorGUI_CheckForUndo();

            // APPLY PROPERTY MODIFICATIONS
            serializedObject.ApplyModifiedProperties();
        }

        void OnInspectorGUI_DepthKitInfo()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            Rect rect = GUILayoutUtility.GetRect(logo.width, logo.height); GUI.DrawTexture(rect, logo);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Depthkit Clip Editor", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Version " + Depthkit_Info.Version);
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        void OnInspectorGUI_PlayerSetupInfo(Depthkit_Clip clip)
        {
            if (clip.IsSetup)
            {
                GUI.backgroundColor = Color.green;
                EditorGUILayout.BeginVertical();
                
                EditorGUILayout.HelpBox("Depthkit clip is setup and ready for playback",
                                        MessageType.Info);
            }

            else
            {
                GUI.backgroundColor = Color.red;
                EditorGUILayout.BeginVertical();
                
                EditorGUILayout.HelpBox("Depthkit clip is not setup. \n"
                                        + string.Format("Player Setup: {0} | Metadata Setup: + {1} | Renderer Setup: {2}",
                                        clip.PlayerSetup, clip.MetaSetup, clip.RendererSetup),
                                        MessageType.Error);
            }
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }

        void OnInspectorGUI_CleanupFilters()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Clean Up Filters", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(_internalEdgeCutoffAngleProp, new GUIContent("Spike Removal"));
            EditorGUILayout.PropertyField(_depthBrightnessThresholdProp, new GUIContent("Edge Choke"));
            EditorGUILayout.EndVertical();
        }

        // https://answers.unity.com/questions/413101/invalidoperationexception-operation-is-not-valid-d.html
        void OnInspectorGui_UserConfirmRendererSwitch()
        {
            if(_renderTypeProp.enumValueIndex != cachedRenderType)
            {
                Depthkit_Clip clip = (Depthkit_Clip)target;

                if (cachedRenderType != (int)RenderType.Photo)
                {
                    if (EditorUtility.DisplayDialog("Changing Renderer type", "WARNING: you will lose all render layers if you change renderer type, would you like to change renderer?", "Yes", "No"))
                    {
                        cachedRenderType = _renderTypeProp.enumValueIndex;
                        clip._needToResetRenderType = true;
                    }
                    else
                    {
                        clip._renderType = (RenderType)cachedRenderType;
                    }            
                }
                else
                {
                    cachedRenderType = _renderTypeProp.enumValueIndex;
                    clip._needToResetRenderType = true;
                }
            }
        }

        void OnInspectorGUI_PlayerSettings(Depthkit_Clip clip)
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_playerTypeProp, new GUIContent("Video Player"));
            if (EditorGUI.EndChangeCheck() || (_playerTypeProp.enumValueIndex != cachedPlayerType))
            {
                _resetPlayerTypeProp.boolValue = true;
                cachedPlayerType = _playerTypeProp.enumValueIndex;
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_renderTypeProp, new GUIContent("Look"));
            if (EditorGUI.EndChangeCheck())
            {
                EditorApplication.delayCall += OnInspectorGui_UserConfirmRendererSwitch;
            }
        }

        void OnInspectorGUI_PlayerMetaInfo()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_metaDataSourceTypeProp);
            if (_metaDataSourceTypeProp.enumValueIndex == (int)Depthkit_Clip.MetadataSourceType.FilePath)
            {
                EditorGUILayout.DelayedTextField(_metaDataFilePathProp);
            }
            else
            {
                EditorGUILayout.PropertyField(_metaDataFileProp);
            }
            EditorGUILayout.PropertyField(_posterProp);
            if (EditorGUI.EndChangeCheck())
            {
                _refreshMetadataProp.boolValue = true;
            }
        }

        void OnInspectorGUI_CheckForUndo()
        {
            if (_needToUndoRedo)
            {
                _refreshPlayerValuesProp.boolValue = true;
                _needToUndoRedo = false;
            }
        }
    }
}