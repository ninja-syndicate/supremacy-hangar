using SupremacyHangar.Runtime.ContentLoader;
using UnityEngine;
using Zenject;
#if UNITY_EDITOR
    using SupremacyHangar.Editor.ContentLoader;
#endif

namespace SupremacyHangar
{
    public class AssetPreviewer : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private AssetMappings allAssets;

        private AssetLoaderWindow assetLoaderWindow;

        [Inject]
        public void Construct(AddressablesManager addressableManager)
        {
            assetLoaderWindow = UnityEditor.EditorWindow.GetWindow<AssetLoaderWindow>();
            assetLoaderWindow.AllMaps(ref allAssets);
            assetLoaderWindow.MyAddressablesManager(ref addressableManager);
            assetLoaderWindow.Close();
        }

        public void OnDisable() 
        {
            assetLoaderWindow.Close();
        }
#else
        private void Start()
        {
            Destroy(this);
        }
#endif
    }
}
