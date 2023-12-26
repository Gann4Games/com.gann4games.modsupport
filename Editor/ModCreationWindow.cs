#if UNITY_EDITOR
using System.IO;
using Gann4Games.ModSupport.Editor;
using UnityEditor;
using UnityEngine;

namespace Gann4Games.ModSupport
{
    
    public class ModCreationWindow : EditorWindow
    {
        private static string _modsFolderName = "Mods";
        private static int _thumbnailWidth = 1024;
        private static int _thumbnailHeight = 1024;

        public static string ModsRootDirectory => Path.GetFullPath(Path.Combine(Application.dataPath, _modsFolderName));
        public static bool ModsFolderExists => Directory.Exists(ModsRootDirectory);

        private ModCreationThumbnailCapturer _thumbnailCapturer = new(ModsRootDirectory);
        private ModCreator _modCreator = new();
        
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
            ModDeliverySection();
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
        
        private void ModDeliverySection()
        {
            EditorGUILayout.LabelField("Mod Delivery", EditorStyles.boldLabel);

            TextField("Mod name: ", ref _modCreator.modName);
            TextField("Mod description: ", ref _modCreator.modDescription);
            
            if(_modCreator.IsModFolderPresent)
                EditorGUILayout.HelpBox("A mod with this name already exists, files will be overriden.", MessageType.Info);

            if (GUILayout.Button("Save changes", GUILayout.Height(50)))
                _modCreator.CreateMod();
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
                EditorGUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button("Change resolution", GUILayout.Height(50)))
                        _thumbnailCapturer.CameraTargetTexture = null;
                    if (GUILayout.Button("Save thumbnail", GUILayout.Height(50)))
                        _thumbnailCapturer.CaptureThumbnail(_modCreator.ModDestinationDirectory);
                }
                EditorGUILayout.EndHorizontal();
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
        
        private void TextField(string name, ref string value)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(name);
                value = EditorGUILayout.TextField(value);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif