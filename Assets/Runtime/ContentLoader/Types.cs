using SupremacyHangar.Runtime.Environment;
using SupremacyHangar.Runtime.ScriptableObjects;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Runtime.ContentLoader.Types
{
    
    [Serializable]
    public class AssetReferenceAssetMappings : AssetReferenceT<AssetMappings>
    {
        public AssetReferenceAssetMappings(string guid) : base(guid) { }
    }
    
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