using UnityEngine;
using Zenject;
using UnityEngine.AddressableAssets;
using System;

namespace SupremacyHangar.Runtime.Scriptable
{

    [CreateAssetMenu(fileName = "DictionaryInstaller", menuName = "Installers/DictionaryInstaller")]
    public class DictionaryInstaller : ScriptableObjectInstaller
    {
        [SerializeField]
        private SupremacyDictionary mechSkinDictionary;

        public override void InstallBindings()
        {
            mechSkinDictionary.Initialize();
            Debug.Log("init " + mechSkinDictionary.mechDictionary.Keys.Count);
            // Create one definitive instance of supremacyDictionary and re-use that for every class that asks for it
            Container.Bind<SupremacyDictionary>().FromInstance(mechSkinDictionary).AsSingle().NonLazy();
        }
    }

    [Serializable]
    public class SupremacyDictionary : Settings, IInitializable
    {
        public void Initialize()
        {
            RedMountainSkinDictionary.Clear();
            BostonCyberneticsSkinDictionary.Clear();
            ZaibatsuSkinDictionary.Clear();
            mechDictionary.Clear();
            AllSkinsDictionary.Clear();
            lootBoxDictionary.Clear();
            
            for (int i = 0; i < lootBoxKeys.Length; i++)
            {
                lootBoxDictionary.Add(lootBoxKeys[i], lootBoxAssets[i]);
            }

            for (int i = 0; i < RedMountainSkinKeys.Length; i++)
            {
                RedMountainSkinDictionary.Add(RedMountainSkinKeys[i], RedMountainSkins[i]);
            }

            for (int i = 0; i < BostonCyberneticsSkinKeys.Length; i++)
            {
                BostonCyberneticsSkinDictionary.Add(BostonCyberneticsSkinKeys[i], BostonCyberneticsSkins[i]);
            }

            for (int i = 0; i < ZaibatsuSkinKeys.Length; i++)
            {
                ZaibatsuSkinDictionary.Add(ZaibatsuSkinKeys[i], ZaibatsuSkins[i]);
            }

            for (int i = 0; i < MechKeys.Length; i++)
            {
                mechDictionary.Add(MechKeys[i], MechList[i]);
            }

            AllSkinsDictionary.Add("BostonCybernetics", BostonCyberneticsSkinDictionary);
            AllSkinsDictionary.Add("RedMountain", RedMountainSkinDictionary);
            AllSkinsDictionary.Add("Zaibatsu", ZaibatsuSkinDictionary);
        }
    }

    [Serializable]
    public class Settings
    {
        [SerializeField]
        protected AssetReference[] lootBoxAssets;

        [SerializeField]
        protected string[] lootBoxKeys;

        [SerializeField]
        protected AssetReference[] RedMountainSkins;

        [SerializeField]
        protected string[] RedMountainSkinKeys;

        [SerializeField]
        protected AssetReference[] BostonCyberneticsSkins;

        [SerializeField]
        protected string[] BostonCyberneticsSkinKeys;

        [SerializeField]
        protected AssetReference[] ZaibatsuSkins;

        [SerializeField]
        protected string[] ZaibatsuSkinKeys;

        [SerializeField]
        protected AssetReference[] MechList;

        [SerializeField]
        protected string[] MechKeys;

        protected StringAssetDictionary RedMountainSkinDictionary = new StringAssetDictionary();
        protected StringAssetDictionary BostonCyberneticsSkinDictionary = new StringAssetDictionary();
        protected StringAssetDictionary ZaibatsuSkinDictionary = new StringAssetDictionary();

        public StringAssetDictionary mechDictionary { get; protected set; } = new StringAssetDictionary();

        public StringAssetDictionary lootBoxDictionary { get; protected set; } = new StringAssetDictionary();

        public StringStringAssetDictionary AllSkinsDictionary { get; protected set; } = new StringStringAssetDictionary();





    }
}