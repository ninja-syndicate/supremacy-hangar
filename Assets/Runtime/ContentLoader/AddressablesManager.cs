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
        
        private LoadingProgressContext loadingProgressContext = new();

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

        public LoadedAsset myMech { get; set; } = new LoadedAsset();

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
            Container.Inject(loadingProgressContext);
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
            StartCoroutine(loadingProgressContext.LoadingAssetProgress(mappingsOp, "Loading Asset Map"));
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
                var skinOperationHandler = TargetSkin.LoadAssetAsync(); 
                StartCoroutine(loadingProgressContext.LoadingAssetProgress(skinOperationHandler));
                skinOperationHandler.Completed += (skin) =>
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
        
#if UNITY_EDITOR
        public void QuickSpawn()
        {
            if (!prevTransform)
            {
                Debug.LogError("No prior spawn set");
                return;
            }

            SpawnMech(prevTransform, false, true);
        }
#endif
        public void SpawnMech(Transform spawnLocation, bool insideCrate = false, bool quickLoad = false)
        {
            prevTransform = spawnLocation;

            //When new mech is spawned remove previous unless inside crate
            if (quickLoad)
            {
                UnloadMech(quickLoad);
            }

            previousMech = TargetMech;
            if (sameMechChassis)
            {
                SetLoadedSkin(myMech.mech);
                return;
            }

            var mechOperationHandler = TargetMech.InstantiateAsync(spawnLocation.position, spawnLocation.rotation, spawnLocation);
            StartCoroutine(loadingProgressContext.LoadingAssetProgress(mechOperationHandler, "Loading Mesh"));
            mechOperationHandler.Completed += (mech) =>
                {
                    myMech.mech = mech.Result;
                    loadingProgressContext.ProgressSignalHandler.FinishedLoading(mech.Result);
                    SetLoadedSkin(insideCrate);
                };
        }

        private void SetLoadedSkin(bool insideCrate = false)
        {
            LoadSkinReference(
                (skin) =>
                {
                    var mechMesh = myMech.mech.GetComponentInChildren<Renderer>();
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
                    previousMech.ReleaseAsset();
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