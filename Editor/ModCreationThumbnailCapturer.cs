using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Gann4Games.ModSupport.Editor
{
    public class ModCreationThumbnailCapturer
    {
        public string ThumbnailSavePath => Path.Combine(_modsRootDirectory, "thumbnail.png");
        private bool ModsFolderExists => Directory.Exists(_modsRootDirectory);
        private string _modsRootDirectory;
        
        public ModCreationThumbnailCapturer(string modsRootDirectory)
        {
            SetModsRootDirectory(modsRootDirectory);
            
            if (!Directory.Exists(modsRootDirectory))
                Directory.CreateDirectory(modsRootDirectory);
        }
        
        public void SetModsRootDirectory(string modsRootDirectory)
        {
            _modsRootDirectory = modsRootDirectory;
        }

        public Camera CurrentSelectedCamera()
        {
            UnityEngine.GameObject active = Selection.activeGameObject;
            if (!active) return null;
            return active.GetComponent<Camera>();
        }

        public RenderTexture CameraTargetTexture
        {
            get => CurrentSelectedCamera().targetTexture;
            set => CurrentSelectedCamera().targetTexture = value;
        }

        public Texture2D GetCameraViewTexture()
        {
            Camera camera = CurrentSelectedCamera();
            var targetTexture = camera.targetTexture;
            
            Texture2D image = new Texture2D(targetTexture.width, targetTexture.height);
            image.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
            image.Apply();
            return image;
        }
        
        public void CaptureThumbnail()
        {
            #region Create folder if it doesn't exist
            if (!ModsFolderExists)
                throw new Exception("Mods folder doesn't exist, please create it first.");
            #endregion
            
            RenderTexture activeRenderTexture = RenderTexture.active;
            RenderTexture.active = CurrentSelectedCamera().targetTexture;
            Texture2D image = GetCameraViewTexture();
            RenderTexture.active = activeRenderTexture;
 
            byte[] bytes = image.EncodeToPNG();
            // MonoBehaviour.DestroyImmediate(image);
 
            File.WriteAllBytes(ThumbnailSavePath, bytes);
            
            EditorUtility.RevealInFinder(ThumbnailSavePath);
        }
        
        public void RenderCameraView()
        {
            CurrentSelectedCamera().Render();
        }
    }
}