using SupremacyHangar.Runtime.ContentLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;
using static TMPro.TMP_Dropdown;
using UnityEngine.AddressableAssets;
using SupremacyHangar.Runtime.ContentLoader.Types;
using SupremacyHangar.Editor.ContentLoader;

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
            assetLoaderWindow.AllMaps = allAssets;
            assetLoaderWindow.MyAddressablesManager = addressableManager;
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
