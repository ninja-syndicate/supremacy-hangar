using SupremacyHangar.Runtime.Environment;
using SupremacyHangar.Runtime.ScriptableObjects;
using SupremacyHangar.Runtime.Silo;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Runtime.ContentLoader.Types
{
    [Serializable]
    public class AssetReferenceSkin : AssetReferenceT<Skin>
    {
        public AssetReferenceSkin(string guid) : base(guid) { }
    }

    [Serializable]
    public class AssetReferenceEnvironmentConnectivity : AssetReferenceT<EnvironmentConnectivity>
    { 
        public AssetReferenceEnvironmentConnectivity(string guid) : base(guid) { }
    }

    public class LoadedAsset
    {
        public GameObject mech = null;
        public Skin skin = null;
        public AssetReference assetReference = null;
    }

    [Serializable]
    public class IntLoadedAssetDictionary : SerializableDictionary<int, LoadedAsset> { }
}