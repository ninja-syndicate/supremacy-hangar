using SupremacyHangar.Runtime.Environment;
using SupremacyHangar.Runtime.ScriptableObjects;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Runtime.ContentLoader.Types
{
    [Serializable]
    public class AssetReferenceAudioClip : AssetReferenceT<AudioClip>
    {
        public AssetReferenceAudioClip(string guid) : base(guid) { }
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