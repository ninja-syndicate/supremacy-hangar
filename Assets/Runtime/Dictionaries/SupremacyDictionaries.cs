using System;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Runtime.Dictionaries
{
    [Serializable]
    public class StringAssetDictionary : SerializableDictionary<string, AssetReference> { }

    [Serializable]
    public class StringAssetStorage : SerializableDictionary.Storage<StringAssetDictionary> { }

    [Serializable]
    public class StringStringAssetDictionary : SerializableDictionary<string, StringAssetDictionary, StringAssetStorage> { }
}