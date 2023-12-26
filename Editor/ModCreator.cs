#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gann4Games.ModSupport.Editor
{
    public class ModCreator
    {
        public string modName = "Your mod name";
        public string modDescription = "";
        public string UnbundledModPath => Path.Combine(ModsRootDirectory, "MOD_NAME");
        public bool IsUnbundledModPresent => Directory.Exists(UnbundledModPath);
        public bool IsModFolderPresent => Directory.Exists(ModDestinationDirectory);
        public string ModDestinationDirectory => Path.Combine(ModsRootDirectory, modName);
        public string ModsRootDirectory => ModCreationWindow.ModsRootDirectory;

        private void ValidateFields()
        {
            if (string.IsNullOrEmpty(modName))
                throw new Exception("Please specify a mod name!");

            if (string.IsNullOrEmpty(ModsRootDirectory))
                throw new Exception("Please specify a mods root directory!");

            if (!IsModFolderPresent)
                Directory.CreateDirectory(ModDestinationDirectory);
        }

        public void CreateMod()
        {
            ValidateFields();
            
            BundleModFiles(ModDestinationDirectory);
            // MoveThumbnail(newModDirectory);
            WriteDescription(modDescription, ModDestinationDirectory);
            
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Creates a new directory with the mod name and move the content from MOD_FILES to the new path.
        /// If the directory already exists, it will move the files from MOD_FILES to the existing directory.
        /// If there are already files in the directory, it will overwrite them.
        /// </summary>
        /// <param name="destinationDirectory"></param>
        public void BundleModFiles(string destinationDirectory)
        {
            if (!IsUnbundledModPresent)
            {
                Debug.Log("There are no files to bundle!");
                return;
            }

            if(!IsModFolderPresent)
                Directory.Move(UnbundledModPath, destinationDirectory);
            else
            {
                Directory.EnumerateFileSystemEntries(UnbundledModPath).ToList().ForEach(path =>
                {
                    string newPath = Path.Combine(destinationDirectory, Path.GetFileName(path));
                    if(File.Exists(path))
                        File.Move(path, newPath);
                    else if(Directory.Exists(path))
                        Directory.Move(path, newPath);
                });
            }
        }
        
        /// <summary>
        /// Moves the thumbnail taken in the 
        /// </summary>
        /// <param name="destinationDirectory"></param>
        [Obsolete] public void MoveThumbnail(string destinationDirectory)
        {
            string thumbnailPath = Path.Combine(ModsRootDirectory, "thumbnail.png");
            if (File.Exists(thumbnailPath))
            {
                string newThumbnailPath = Path.Combine(destinationDirectory, "thumbnail.png");
                File.Move(thumbnailPath, newThumbnailPath);
            }
        }
        
        /// Creates a description.txt file inside the mod path if the description is not empty.
        public void WriteDescription(string descriptionContent, string destinationDirectory)
        {
            if(!string.IsNullOrEmpty(descriptionContent))
            {
                string descriptionFilePath = Path.Combine(destinationDirectory, "description.txt");
                File.WriteAllText(descriptionFilePath, descriptionContent);
            }
        }
    }
}

#endif