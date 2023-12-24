#if UNITY_EDITOR
using System;
using System.IO;
using Gann4Games.ModSupport.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gann4Games.ModSupport
{
    
    public class ModCreationWindow : EditorWindow
    {
        private static string _modsFolderName = "Mods";
        private static int _thumbnailWidth = 1024;
        private static int _thumbnailHeight = 1024;

        private static string ModsRootDirectory => Path.GetFullPath(Path.Combine(Application.dataPath, _modsFolderName));
        private static bool ModsFolderExists => Directory.Exists(ModsRootDirectory);

        private ModCreationThumbnailCapturer _thumbnailCapturer = new(ModsRootDirectory);
        
        [MenuItem("Gann4Games/Mod Support")]
        static void ShowWindow()
        {
            GetWindow(typeof(ModCreationWindow));
        }

        private void OnGUI()
        {
            titleContent.text = "Mod Creation";
            
            SettingsSection();
            EditorGUILayout.Separator();
            ThumbnailSection();
        }

        private void SettingsSection()
        {
            if(!ModsFolderExists)
                EditorGUILayout.HelpBox("The mods folder doesn't exist, please create it first.", MessageType.Warning);
            
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Mods root folder name:");
                _modsFolderName = EditorGUILayout.TextField(_modsFolderName);
                _thumbnailCapturer.SetModsRootDirectory(ModsRootDirectory);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("Your files will be stored in the following directory:\n" + ModsRootDirectory);
        }
        
        private void ThumbnailSection()
        {
            EditorGUILayout.LabelField("Thumbnail creation", EditorStyles.boldLabel);

            if (!_thumbnailCapturer.CurrentSelectedCamera())
            {
                EditorGUILayout.HelpBox("Please select a camera to capture the thumbnail from.", MessageType.Warning);
                return;
            }
            
            if (!_thumbnailCapturer.CurrentSelectedCamera().targetTexture)
            {
                EditorGUILayout.HelpBox("Your camera needs a target texture.", MessageType.Warning);
                GUILayout.Label("Image size (in pixels):");
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Width:");
                    _thumbnailWidth = EditorGUILayout.IntField(Mathf.Clamp(_thumbnailWidth, 1, 4096));
                    GUILayout.Label("Height: ");
                    _thumbnailHeight = EditorGUILayout.IntField(Mathf.Clamp(_thumbnailHeight, 1, 4096));
                
                }
                EditorGUILayout.EndHorizontal();
                if(GUILayout.Button("Create target texture", GUILayout.Height(50)))
                    _thumbnailCapturer.CameraTargetTexture = new RenderTexture(_thumbnailWidth, _thumbnailHeight, 24);
            }
            
            if (_thumbnailCapturer.CameraTargetTexture)
            {
                if(GUILayout.Button("Save Thumbnail", GUILayout.Height(50)))
                    _thumbnailCapturer.CaptureThumbnail();

                EditorGUILayout.Separator();
                
                _thumbnailCapturer.RenderCameraView();

                var lastRect = GUILayoutUtility.GetLastRect();
                var guiAspectRatio = Mathf.Min(position.width, position.height - lastRect.yMax) / _thumbnailCapturer.CurrentSelectedCamera().targetTexture.height;
                
                var desiredPosition = new Vector2(
                    (position.width - _thumbnailCapturer.CameraTargetTexture.width * guiAspectRatio) / 2,
                    lastRect.yMax    
                );
                
                EditorGUI.DrawTextureTransparent(
                    new Rect(
                        desiredPosition,
                        new(
                            _thumbnailCapturer.CurrentSelectedCamera().targetTexture.width * guiAspectRatio, 
                            _thumbnailCapturer.CurrentSelectedCamera().targetTexture.height * guiAspectRatio)),
                    _thumbnailCapturer.CurrentSelectedCamera().targetTexture
                );
            }
        }
    }
}
#endif