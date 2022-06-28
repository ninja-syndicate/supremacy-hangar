using SupremacyHangar.Runtime.ContentLoader.Types;
using SupremacyHangar.Runtime.Environment;
using SupremacyHangar.Runtime.ScriptableObjects;
using SupremacyHangar.Runtime.Silo;
using SupremacyHangar.Runtime.Types;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace SupremacyHangar.Runtime.ContentLoader
{ 
    public class AddressablesManager : MonoInstaller
    {
        [Inject]
        private HangarData _playerInventory;

        [Inject]
        private ContentSignalHandler _contentSignalHandler;

        public AssetReference TargetMech { get; set; }
        public AssetReferenceSkin TargetSkin { get; set; }

        public SupremacyData.Runtime.Faction CurrentFaction
        {
            get
            {
                if (_playerInventory == null) return null;
                if (_playerInventory.faction == Guid.Empty) return null;
                if (!mappingsLoaded) return null;
                return mappings.FactionMappingByGuid[_playerInventory.faction].DataFaction;
            }
        }
        
        private AssetReference previousMech;
        private AssetReference previousCrate;

        private LoadedAsset myMech { get; set; } = new LoadedAsset();

        [SerializeField] private AssetReferenceAssetMappings assetMappingsReference;

        private AssetMappings mappings;
        private bool mappingsLoaded;

        private SignalBus _bus;
        private bool _subscribed;

        private SiloSignalHandler _siloSignalHandler;
        private CrateSignalHandler _crateSignalHandler;

        private Transform prevTransform;
        private bool sameMechChassis = false;

        [Inject]
        public void Construct(SignalBus bus, SiloSignalHandler siloHandler, CrateSignalHandler crateHandler)
        {
            _bus = bus;
            _siloSignalHandler = siloHandler;
            _crateSignalHandler = crateHandler;
        }
        private void OnEnable()
        {
            SubscribeToSignal();
        }

        private void OnDisable()
        {
            if (!_subscribed) return;
            _bus.Unsubscribe<InventoryRecievedSignal>(SetPlayerInventory);

            _subscribed = false;
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<InventoryRecievedSignal>(SetPlayerInventory);

            _subscribed = true;
        }

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

            mappings = operation.Result;
            mappingsLoaded = true;
            if (!TryGetComponent(out BridgeScript bridge))
            {
                Debug.LogError("Bridgescript isn't on this component!", this);
                return;
            } 
            
#if UNITY_EDITOR
            bridge.SetPlayerInventoryFromFragment();
#elif UNITY_WEBGL
            Plugins.WebGL.WebGLPluginJS.SiloReady();
#endif
        }

        private void SetPlayerInventory()
        {
            _playerInventory.factionGraph = mappings.FactionMappingByGuid[_playerInventory.faction].ConnectivityGraph;
            foreach (var silo in _playerInventory.Silos)
            {
                MapSiloToAsset(silo);
            }
            _contentSignalHandler.InventoryLoaded();
        }
        
        public void MapSiloToAsset(SiloItem silo)
        {
            switch (silo)
            {
                case Mech mech:
                    if (!mappings.MechChassisMappingByGuid.TryGetValue(mech.StaticID, out mech.MechChassisDetails))
                    {
                        Debug.LogError($"No Chassis mapping found for {mech.StaticID}");
                        break;
                    }
                    if (!mappings.MechSkinMappingByGuid.TryGetValue(mech.Skin.StaticID, out mech.MechSkinDetails))
                    {
                        Debug.LogError($"No Skin mapping found for {mech.Skin.StaticID}");
                    }
                    break;
                case MysteryCrate crate:
                    if (!mappings.MysteryCrateMappingByGuid.TryGetValue(crate.StaticID, out crate.MysteryCrateDetails))
                    {
                        Debug.LogError($"No Crate mapping found for {crate.StaticID}");
                    }
                    break;
                default:
                    Debug.LogError($"Unknown silo of type {silo}.");
                    break;
            }
        }

        private bool ValidateMappings()
        {
            if (assetMappingsReference != null) return true;
            Debug.LogError("No asset mapping file assigned to AddressablesManager!", this);
            return false;
        }

        private void LoadSkinReference(Action<ScriptableObjects.Skin> callBack)
        {
            if (myMech.skin == null && TargetSkin != null)
            {
                TargetSkin.LoadAssetAsync<ScriptableObjects.Skin>().Completed += (skin) =>
                {
                    myMech.skin = skin.Result;
                    callBack(myMech.skin);
                };
            }
            else
            {
                //Skin alreasy loaded
                callBack(myMech.skin);
            }
        }

        private void LoadMechReference(Action<GameObject> callBack)
        {
            if (myMech.mech == null)
            {
                previousMech = TargetMech;
                mechOperationHandler = TargetMech.LoadAssetAsync<GameObject>();
                StartCoroutine(LoadingAsset());
                mechOperationHandler.Completed += (mech) =>
                {
                    myMech.mech = mech.Result as GameObject;

                    callBack(myMech.mech);
                };
            }
            else
            {//Mech already loaded
                callBack(myMech.mech);
            }
        }

        private IEnumerator LoadingAsset()
        {
            float percentageComplete = 0;
            do
            {
                percentageComplete = skinOperationHandler.IsValid() ? mechOperationHandler.PercentComplete + skinOperationHandler.PercentComplete /2 : mechOperationHandler.PercentComplete;
                percentageComplete *= 100;

                Debug.Log("Downloading Asset: " + percentageComplete);
                yield return null;
            } while (!mechOperationHandler.IsDone);
            Debug.Log("Downloading Asset: 100%");
        }

#if UNITY_EDITOR
        public void QuickSpawn()
        {
            if (!prevTransform)
            {
                Debug.LogError("No prior spawn set");
                return;
            }

            SpawnMech(prevTransform, true);
        }
#endif
        public void SpawnMech(Transform spawnLocation, bool insideCrate = false, bool quickLoad = false)
        {
            prevTransform = spawnLocation;

            //When new mech is spawned remove previous unless inside crate
            if (!insideCrate)
                UnloadMech(quickLoad);
            else
                previousCrate = previousMech;

            if (sameMechChassis)
            {
                SetLoadedSkin(myMech.mech);
                return;
            }

            //Load new Mech & Skin
            LoadMechReference(
                (result) =>
                {
                    TargetMech.InstantiateAsync(spawnLocation.position, spawnLocation.rotation, spawnLocation).Completed += (mech) =>
                    {
                        myMech.mech = mech.Result;
                        SetLoadedSkin(myMech.mech, insideCrate);
                    };
                });
        }

        private void SetLoadedSkin(GameObject mech, bool insideCrate = false)
        {
            LoadSkinReference(
                (skin) =>
                {
                    MeshRenderer mechMesh = myMech.mech.GetComponentInChildren<MeshRenderer>();
                    var mechRepostion = myMech.mech.GetComponentInChildren<SiloPlatformRepositioner>();
                    if (skin != null)
                        mechMesh.sharedMaterials = skin.mats;

                    Container.InjectGameObject(myMech.mech);

                    if (insideCrate)
                        _crateSignalHandler.OpenCrate();
                    else
                    {
                        mechRepostion.RepositionObject();
                        _siloSignalHandler.SiloFilled();
                    }
                }
            );
        }

        public void UnloadMech(bool quickLoad = false)
        {
            if (myMech.skin != null)
            {
                Addressables.Release(myMech.skin);
                myMech.skin = null;
            }

            if (previousMech != null)
            {
                if (!quickLoad || previousMech != TargetMech)
                {
                    if(previousCrate != null) previousCrate.ReleaseAsset();

                    previousMech.ReleaseAsset();
                }
#if UNITY_EDITOR
                if (previousMech != TargetMech &&
                    myMech.mech != null)
                {
                    sameMechChassis = false;
                    Destroy(myMech.mech);
                    myMech.mech = null;
                }

                if (previousMech == TargetMech && myMech.mech)
                    sameMechChassis = true;
#endif
            }

        }
    }
}