using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using System.IO;
using TMPro;
using UnityEngine.Serialization;

namespace Gann4Games.ModSupport
{
    //TODO: Bring mod loading logic into a mod manager class which is going to have utilities to load mods and use them.
    public class UIModBrowser : MonoBehaviour
    {
        public static UIModBrowser Instance;

        [Header("General settings")]
        [Tooltip("The folder name to look for mods, the project will also try to search for this folder into the <gamename>_Data folder.\nMake sure the folder is also found in the Addressables profile settings.")]
        [SerializeField] private string modRootFolderName = "Mods";
        
        [Header("Label to load")]
        [SerializeField] private AssetLabelReference findByLabel;
        
        [Header("UI Elements")]
        [FormerlySerializedAs("DisplayMod")] 
        [Tooltip("Text element that will display the selected mod's name.")]
        [SerializeField] private TextMeshProUGUI uiSelectedModTextDisplay;

        [FormerlySerializedAs("PrefabParent")] 
        [Tooltip("The container of mods found (that have title and description), you can use a vertical layout container for example.")]
        [SerializeField] private RectTransform modCatalogContainer;
        
        [FormerlySerializedAs("ModButtonPrefab")] 
        [Tooltip("UI prefab that's used to load mod information.")]
        [SerializeField] private GameObject uiModLoaderButtonPrefab;

        private ModInfo _selectedMod;
        private List<ModInfo> _installedModList = new List<ModInfo>();
        public string ModsRootDirectory => Path.Combine(Application.dataPath, modRootFolderName);

        /// <summary>
        /// Stores a mod to load it/spawn it later. Ideal to use with UI buttons.
        /// </summary>
        /// <param name="mod"></param>
        public static void SelectMod(ModInfo mod)
        {
            Instance.uiSelectedModTextDisplay.text = mod.GetModName();
            Instance._selectedMod = mod;
        }

        /// <summary>
        /// Spawns the previously selected mod. Ideal to use with UI buttons.
        /// </summary>
        public static void SpawnSelectedMod()
        {
            if (Instance._selectedMod == null)
                throw new NullReferenceException("Tried to spawn a mod without selecting it! Make sure you're selecting it first.");
            
            SpawnMod(Instance._selectedMod);
        }

        /// <summary>
        /// Loads the given mod asynchronously
        /// </summary>
        /// <param name="mod"></param>
        public static void SpawnMod(ModInfo mod)
        {
            if(mod == null) 
                throw new NullReferenceException("Please provide a mod to load!");
            
            //load from the directory were spawning from
            AsyncOperationHandle<IResourceLocator> loadContentCatalogAsync = Addressables.LoadContentCatalogAsync(@"" + mod.ModFile);

            //call this when were done loading in the content
            loadContentCatalogAsync.Completed += Instance.OnCompleted;
        }
        
        private void Start()
        {
            if(!Instance) Instance = this;
            
            // Find all mods found in the base folder
            // EDITOR: Default Assets/Mods unless you have modified the variables
            // BUILD: GameName_Data/Mods unless you have modified the variables
            List<string> files = FindAllJsonFiles(ModsRootDirectory);
            files.ForEach(i =>
            {
                // Iterate over each json file and store its data using the ModInfo class.
                ModInfo mod = new ModInfo(FormatPathAccordingly(i));
                
                // Fix the bundle paths in the mod file
                mod.FixBundlePathsInModFile();
                
                // Add the mod to the list of installed mods
                _installedModList.Add(mod);
            });
            
            DisplayInstalledModsOnUI();
        }

        /// <summary>
        /// Displays the catalog of mods in the UI.
        /// </summary>
        private void DisplayInstalledModsOnUI()
        {
            foreach (ModInfo mod in _installedModList)
            {
                RectTransform modButton = Instantiate(uiModLoaderButtonPrefab, modCatalogContainer.transform.position, modCatalogContainer.transform.rotation, modCatalogContainer.transform).GetComponent<RectTransform>();
                modButton.GetComponent<UIModItemLoaderButton>().SetupModPrefab(mod);
            }
        }
        
        /// <summary>
        /// Called when the addressable finishes loading.
        /// </summary>
        /// <param name="obj"></param>
        private void OnCompleted(AsyncOperationHandle<IResourceLocator> obj)
        {

            //find prefabs in the addressable with the tag specified in the first parameter
            IResourceLocator resourceLocator = obj.Result;
            resourceLocator.Locate(findByLabel.labelString, typeof(GameObject), out IList<IResourceLocation> locations);

            //if there are loactions in the adressable spawn them
            if (locations != null)
            {
                foreach (IResourceLocation resourceLocation in locations)
                {
                    GameObject resourceLocationData = (GameObject)resourceLocation.Data;

                    AsyncOperationHandle<GameObject> prefab = Addressables.InstantiateAsync(resourceLocation);                        
                    prefab.Completed += OnModInstantiated;
                }
            }
        }

        /// <summary>
        /// Called when the prefab spawns in the scene.
        /// </summary>
        /// <param name="obj"></param>
        private void OnModInstantiated(AsyncOperationHandle<GameObject> obj)
        {
            Debug.Log("Prefab Spawned");
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

        /// <summary>
        /// Replaces backslashes by forward slashes.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string FormatPathAccordingly(string path)
        {
            return path.Replace(@"\", "/");
        }
    }
}
