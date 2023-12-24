#if UNITY_EDITOR
using System;
using System.IO;
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

        private static Camera CurrentSelectedCamera()
        {
            GameObject active = Selection.activeGameObject;
            if (!active) return null;
            return active.GetComponent<Camera>();
        }

        private static string ModsRootDirectory => Path.GetFullPath(Path.Combine(Application.dataPath, _modsFolderName));
        private static bool ModsFolderExists => Directory.Exists(ModsRootDirectory);
        
        static string SavePath()
        {
            ModManager manager = ModManager.Instance;
            if (!ModManager.Instance) manager = FindAnyObjectByType<ModManager>();

            if (!manager)
                throw new Exception("Unable to find the mods path, please add a ModManager component somewhere");
            
            return Path.Combine(ModsRootDirectory, "thumbnail.png");
        }
        
        [MenuItem("Gann4Games/Mod Support")]
        static void ShowWindow()
        {
            GetWindow(typeof(ModCreationWindow));
        }

        static Texture2D GetCameraViewTexture()
        {
            Camera camera = CurrentSelectedCamera();
            var targetTexture = camera.targetTexture;
            
            Texture2D image = new Texture2D(targetTexture.width, targetTexture.height);
            image.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
            image.Apply();
            return image;
        }
        
        static void CaptureThumbnail()
        {
            #region Create folder if it doesn't exist
            if (!ModsFolderExists)
                Directory.CreateDirectory(ModsRootDirectory);
            #endregion
            
            RenderTexture activeRenderTexture = RenderTexture.active;
            RenderTexture.active = CurrentSelectedCamera().targetTexture;
            Texture2D image = GetCameraViewTexture();
            RenderTexture.active = activeRenderTexture;
 
            byte[] bytes = image.EncodeToPNG();
            DestroyImmediate(image);
 
            File.WriteAllBytes(SavePath(), bytes);
            
            EditorUtility.RevealInFinder(SavePath());
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
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("Your files will be stored in the following directory:\n" + ModsRootDirectory);
        }
        
        private void ThumbnailSection()
        {
            EditorGUILayout.LabelField("Thumbnail creation", EditorStyles.boldLabel);

            if (!CurrentSelectedCamera())
            {
                EditorGUILayout.HelpBox("Please select a camera to capture the thumbnail from.", MessageType.Warning);
                return;
            }
            
            if (!CurrentSelectedCamera().targetTexture)
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
                    CurrentSelectedCamera().targetTexture = new RenderTexture(_thumbnailWidth, _thumbnailHeight, 24);
            }
            
            if (CurrentSelectedCamera().targetTexture)
            {
                if(GUILayout.Button("Save Thumbnail", GUILayout.Height(50)))
                    CaptureThumbnail();

                EditorGUILayout.Separator();
                
                CurrentSelectedCamera().Render();

                var lastRect = GUILayoutUtility.GetLastRect();
                var guiAspectRatio = Mathf.Min(position.width, position.height - lastRect.yMax) / CurrentSelectedCamera().targetTexture.height;
                
                var desiredPosition = new Vector2(
                    (position.width - CurrentSelectedCamera().targetTexture.width * guiAspectRatio) / 2,
                    lastRect.yMax    
                );
                
                EditorGUI.DrawTextureTransparent(
                    new Rect(
                        desiredPosition,
                        new(CurrentSelectedCamera().targetTexture.width * guiAspectRatio, CurrentSelectedCamera().targetTexture.height * guiAspectRatio)),
                    CurrentSelectedCamera().targetTexture
                );
            }
        }
    }
}
#endif