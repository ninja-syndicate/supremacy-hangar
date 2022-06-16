using SupremacyHangar.Runtime.ContentLoader.Types;
using SupremacyHangar.Runtime.Environment;
using SupremacyHangar.Runtime.ScriptableObjects;
using SupremacyHangar.Runtime.Types;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace SupremacyHangar.Runtime.ContentLoader
{ 
    public class AddressablesManager : MonoInstaller
    {
        public AssetReference TargetMech { get; set; }
        public AssetReference TargetSkin { get; set; }

        private AssetReference previousMech;

        public string FactionName { get; set; }

        private LoadedAsset myMech { get; set; } = new LoadedAsset();

        [SerializeField] private AssetReferenceAssetMappings assetMappingsReference;

        private AssetMappings mappings;
        private bool mappingsLoaded = false;

        public override void InstallBindings()
        {
            Container.Bind<AddressablesManager>().FromInstance(this).AsSingle().NonLazy();
        }

        public void Awake()
        {
            bool valid = true;
            valid &= ValidateMappings();

            if (!valid) enabled = false;
        }
        
        // Start is called before the first frame update
        public override void Start()
        {
            Addressables.InitializeAsync().Completed += AddressablesManager_Completed;
        }

        private void AddressablesManager_Completed(AsyncOperationHandle<IResourceLocator> obj)
        {
            var mappingsOp = assetMappingsReference.LoadAssetAsync();
            mappingsOp.Completed += LoadMappings;
        }

        private void LoadMappings(AsyncOperationHandle<AssetMappings> operation)
        {
            if (operation.Result == null)
            {
                Debug.LogError("Mappings were null!");
                return;
            }

            Debug.Log("I AM LOAD");
            mappingsLoaded = true;
            mappings = operation.Result;
        }
        
        private bool ValidateMappings()
        {
            if (assetMappingsReference != null) return true;
            Debug.LogError("No asset mapping file assigned to AddressablesManager!", this);
            return false;
        }

        private void LoadSkinReference(Action<Skin> callBack)
        {
            if (myMech.skin == null)
            {
                Debug.Log("loading skin");
                TargetSkin.LoadAssetAsync<Skin>().Completed += (skin) =>
                {
                    Debug.Log("saving Skin" + skin.Result);
                    myMech.skin = skin.Result;
                    callBack(myMech.skin);
                };
            }
            else
            {
                Debug.Log("existing skin");
                //Skin alreasy loaded
                callBack(myMech.skin);
            }
        }

        private void LoadMechReference(Action<GameObject> callBack)
        {
            if (myMech.mech == null)
            {
                previousMech = TargetMech;
                Debug.Log("loading mech");
                TargetMech.LoadAssetAsync<GameObject>().Completed += (mech) =>
                {
                    Debug.Log("saving Mech");
                    myMech.mech = mech.Result;

                    callBack(myMech.mech);
                };
            }
            else
            {//Mech already loaded
                Debug.Log("Existing mech");
                callBack(myMech.mech);
            }
        }

        public void SpawnMech(Transform spawnLocation)
        {
            //When mech out of view release addressables
            UnloadMech();

            //Load new Mech & Skin
            LoadMechReference(
                (result) =>
                {
                    TargetMech.InstantiateAsync(spawnLocation.position, spawnLocation.rotation, spawnLocation).Completed += (mech) =>
                    {
                        myMech.mech = mech.Result;
                        if (TargetSkin == null) return;
                        LoadSkinReference(
                            (skin) =>
                            {
                                Debug.Log("Setting Skin");
                                MeshRenderer mechMesh = myMech.mech.GetComponentInChildren<MeshRenderer>();
                                mechMesh.sharedMaterials = skin.mats;
                                mechMesh.enabled = true;
                            }
                        );
                    };
                });
        }

        public void UnloadMech()
        {
            if (myMech.skin != null)
            {
                Addressables.Release(myMech.skin);
                myMech.skin = null;
            }

            if (TargetMech != previousMech &&
                previousMech != null)
            {
                previousMech.ReleaseAsset();
                myMech.mech = null;
            }
            else
            {
                Destroy(myMech.mech);
            }
        }
    }
}