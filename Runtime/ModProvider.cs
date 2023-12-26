using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Gann4Games.ModSupport
{
    /// <summary>
    /// The class is in charge of loading and providing mods to use from the specified folders.
    /// </summary>
    public class ModProvider
    {
        private List<string> _modCatalogs = new List<string>();
        private string _folderName;
        
        public List<ModInfo> InstalledModList { get; private set; }
        public string ModsRootDirectory => Path.Combine(Application.dataPath, _folderName);

        public ModInfo SelectedMod { get; private set; }
        
        public ModProvider(string folderName)
        {
            _folderName = folderName;
        }
        
        /// <summary>
        /// Scans for all json files in the mods root directory and saves them back into the installed mod list. (strings -> ModInfo)
        /// </summary>
        public void ScanForMods()
        {
            if (string.IsNullOrEmpty(ModsRootDirectory))
            {
                Debug.LogError("You must specify a mods root directory!");
                return;
            }
            
            _modCatalogs = FindAllJsonFiles(ModsRootDirectory);
            InstalledModList = new List<ModInfo>();
            _modCatalogs.ForEach(modPath =>
            {
                ModInfo mod = new ModInfo(modPath);
                mod.FixBundlePathsInModFile();
                InstalledModList.Add(mod);
            });
        }

        public void SelectMod(ModInfo mod)
        {
            SelectedMod = mod;
        }

        public AsyncOperationHandle<IResourceLocator> LoadSelectedCatalog()
        {
            if(SelectedMod == null)
                throw new NullReferenceException("Tried to load a mod without selecting it! Make sure you're selecting it first.");
            return LoadCatalog(SelectedMod);
        }
        
        public AsyncOperationHandle<IResourceLocator> LoadCatalog(ModInfo mod)
        {
            if(mod == null)
                throw new NullReferenceException("Please provide a mod to load!");
            return Addressables.LoadContentCatalogAsync(mod.ModFile);
        }

        public List<AsyncOperationHandle<GameObject>> InstantiateCatalogPrefab(AsyncOperationHandle<IResourceLocator> catalog, string label)
        {
            //find prefabs in the addressable with the tag specified in the first parameter
            IResourceLocator locator = catalog.Result;
            locator.Locate(label, typeof(GameObject), out IList<IResourceLocation> locations);

            if (locations == null)
                throw new Exception("No prefabs found in the catalog!");
            
            // Instantiate every location and return them as list
            return locations
                .Select(resourceLocation => Addressables.InstantiateAsync(resourceLocation))
                .ToList();
        }

        /// <summary>
        /// Finds all json files inside the specified path no matter where they're located thanks to recursion.
        /// </summary>
        /// <param name="path">The directory/path to search in</param>
        /// <returns>List of found json files</returns>
        private List<string> FindAllJsonFiles(string path)
        {
            List<string> jsonFiles = new List<string>();
            
            // Scan for json files in the given path
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                if (file.EndsWith("json"))
                    jsonFiles.Add(file);
            }
            
            // Scan subfolders in the given path recursively
            var directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                List<string> innerLevelJsonFiles = FindAllJsonFiles(directory);
                foreach (string file in innerLevelJsonFiles)
                    jsonFiles.Add(file);
            }

            return jsonFiles;
        }
    }
}