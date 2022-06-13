using SupremacyHangar.Runtime.Environment;
using SupremacyHangar.Runtime.ScriptableObjects;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Runtime.ContentLoader.Types
{
    public class LoadedAsset
    {
        public GameObject mech = null;
        public Skin skin = null;
        public AssetReference assetReference = null;
    }
}