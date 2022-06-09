using UnityEngine;
using Zenject;
using UnityEngine.AddressableAssets;
using System;
using SupremacyHangar.Runtime.Dictionaries;

namespace SupremacyHangar.Runtime.ScriptableObjects
{

    [CreateAssetMenu(fileName = "DictionaryInstaller", menuName = "Installers/DictionaryInstaller")]
    public class DictionaryInstaller : ScriptableObjectInstaller
    {
        [SerializeField]
        private SupremacyDictionary mechSkinDictionary;

        public override void InstallBindings()
        {
            mechSkinDictionary.Initialize();
            // Create one definitive instance of supremacyDictionary and re-use that for every class that asks for it
            Container.Bind<SupremacyDictionary>().FromInstance(mechSkinDictionary).AsSingle().NonLazy();
        }
    }

    [Serializable]
    public class SupremacyDictionary : Settings, IInitializable
    {
        public void Initialize()
        {
            redMountainSkinDictionary.Clear();
            bostonCyberneticsSkinDictionary.Clear();
            zaibatsuSkinDictionary.Clear();
            MechDictionary.Clear();
            AllSkinsDictionary.Clear();
            LootBoxDictionary.Clear();
            FactionDictionary.Clear();
            
            for (int i = 0; i < lootBoxKeys.Length; i++)
            {
                LootBoxDictionary.Add(lootBoxKeys[i], lootBoxAssets[i]);
            }

            for (int i = 0; i < redMountainSkinKeys.Length; i++)
            {
                redMountainSkinDictionary.Add(redMountainSkinKeys[i], redMountainSkins[i]);
            }

            for (int i = 0; i < bostonCyberneticsSkinKeys.Length; i++)
            {
                bostonCyberneticsSkinDictionary.Add(bostonCyberneticsSkinKeys[i], bostonCyberneticsSkins[i]);
            }

            for (int i = 0; i < zaibatsuSkinKeys.Length; i++)
            {
                zaibatsuSkinDictionary.Add(zaibatsuSkinKeys[i], zaibatsuSkins[i]);
            }

            for (int i = 0; i < mechKeys.Length; i++)
            {
                MechDictionary.Add(mechKeys[i], mechList[i]);
            }

            for (int i = 0; i < factionKeys.Length; i++)
            {
                FactionDictionary.Add(factionKeys[i], factionGraphList[i]);
            }

            AllSkinsDictionary.Add("BostonCybernetics", bostonCyberneticsSkinDictionary);
            AllSkinsDictionary.Add("RedMountain", redMountainSkinDictionary);
            AllSkinsDictionary.Add("Zaibatsu", zaibatsuSkinDictionary);
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
        protected AssetReference[] redMountainSkins;

        [SerializeField]
        protected string[] redMountainSkinKeys;

        [SerializeField]
        protected AssetReference[] bostonCyberneticsSkins;

        [SerializeField]
        protected string[] bostonCyberneticsSkinKeys;

        [SerializeField]
        protected AssetReference[] zaibatsuSkins;

        [SerializeField]
        protected string[] zaibatsuSkinKeys;

        [SerializeField]
        protected AssetReferenceGameObject[] mechList;

        [SerializeField]
        protected string[] mechKeys;

        [SerializeField]
        protected AssetReference[] factionGraphList;

        [SerializeField]
        protected string[] factionKeys;

        protected StringAssetDictionary redMountainSkinDictionary = new StringAssetDictionary();
        protected StringAssetDictionary bostonCyberneticsSkinDictionary = new StringAssetDictionary();
        protected StringAssetDictionary zaibatsuSkinDictionary = new StringAssetDictionary();

        public StringAssetDictionary MechDictionary { get; protected set; } = new StringAssetDictionary();

        public StringAssetDictionary LootBoxDictionary { get; protected set; } = new StringAssetDictionary();

        public StringStringAssetDictionary AllSkinsDictionary { get; protected set; } = new StringStringAssetDictionary();

        public StringAssetDictionary FactionDictionary { get; protected set; } = new StringAssetDictionary();
    }
}