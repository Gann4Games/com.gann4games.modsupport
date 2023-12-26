using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using TMPro;
using UnityEngine.Serialization;

namespace Gann4Games.ModSupport
{
    public class UIModBrowser : MonoBehaviour
    {
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
        [SerializeField] private UIModLoadButton uiModLoaderButtonPrefab;

        private static ModProvider _modProvider;
        
        private List<UIModLoadButton> _installedModButtons = new List<UIModLoadButton>();

        /// <summary>
        /// Stores a mod to load it/spawn it later. Ideal to use with UI buttons.
        /// </summary>
        /// <param name="mod"></param>
        public void SelectMod(ModInfo mod)
        {
            uiSelectedModTextDisplay.text = mod.GetModName();
            _modProvider.SelectMod(mod);
        }

        /// <summary>
        /// Spawns the previously selected mod. Ideal to use with UI buttons.
        /// </summary>
        public void SpawnSelectedMod()
        {
            _modProvider.LoadSelectedCatalog()
                .Completed += OnCompleted;
        }

        /// <summary>
        /// Loads the given mod asynchronously
        /// </summary>
        /// <param name="mod"></param>
        public void SpawnMod(ModInfo mod)
        {
            _modProvider.LoadCatalog(mod)
                .Completed += OnCompleted;
        }
        
        private void Start()
        {
            _modProvider = new ModProvider(modRootFolderName);
            ScanAndDisplayMods();
        }

        /// <summary>
        /// Displays the catalog of mods in the UI.
        /// </summary>
        public void ScanAndDisplayMods()
        {
            _modProvider.ScanForMods();
            ClearModButtons();
            DisplayModButtons();
        }

        private void ClearModButtons()
        {
            _installedModButtons.ForEach(i => Destroy(i.gameObject));
            _installedModButtons.Clear();
        }

        private void DisplayModButtons()
        {
            foreach (ModInfo mod in _modProvider.InstalledModList)
            {
                Vector3 position = modCatalogContainer.position;
                Quaternion rotation = modCatalogContainer.rotation;
                RectTransform container = modCatalogContainer;

                UIModLoadButton button = Instantiate(uiModLoaderButtonPrefab, position, rotation, container);
                button.SetModToLoad(mod, this);
                _installedModButtons.Add(button);
            }
        }
        
        /// <summary>
        /// Called when the addressable finishes loading.
        /// </summary>
        /// <param name="obj"></param>
        private void OnCompleted(AsyncOperationHandle<IResourceLocator> obj)
        {
            _modProvider
                .InstantiateCatalogPrefab(obj, findByLabel.labelString)
                .ForEach(prefab => prefab.Completed += OnModInstantiated);
        }

        /// <summary>
        /// Called when the prefab spawns in the scene.
        /// </summary>
        /// <param name="obj"></param>
        private void OnModInstantiated(AsyncOperationHandle<UnityEngine.GameObject> obj)
        {
            Debug.Log("Prefab Spawned");
        }
    }
}
