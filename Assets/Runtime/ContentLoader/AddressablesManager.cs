using SupremacyHangar.Runtime.ContentLoader.Types;
using SupremacyHangar.Runtime.Silo;
using SupremacyHangar.Runtime.Types;
using System;
using System.Collections.Generic;
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

        public LoadedAsset myMech { get; set; } = new LoadedAsset();

        [SerializeField] private AssetReferenceAssetMappings assetMappingsReference;

        private AssetMappings mappings;
        private bool mappingsLoaded;

        private SignalBus _bus;
        private bool _subscribed;

        private SiloSignalHandler _siloSignalHandler;
        private CrateSignalHandler _crateSignalHandler;

        //These are used in editor only
        private Transform prevTransform;
        private bool sameMechChassis = false;
        private AssetReference previousMech;
        private bool crateOpened = false;
        private GameObject crateInstance;

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
            
#if !UNITY_WEBGL || UNITY_EDITOR
            bridge.SetPlayerInventoryFromFragment();
#else
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

                    if (mech.Accessories == null) break;
                    foreach(var item in mech.Accessories)
                    {
                        switch(item)
                        {
                            case Weapon weapon:
                                if (!mappings.WeaponModelMappingByGuid.TryGetValue(weapon.StaticID, out weapon.WeaponModelDetails))
                                {
                                    Debug.LogError($"No Weapon mapping found for {weapon.StaticID}");
                                    break;
                                }
                                if (!mappings.WeaponSkinMappingByGuid.TryGetValue(weapon.Skin.StaticID, out weapon.WeaponSkinDetails))
                                {
                                    Debug.LogError($"No Weapon Skin mapping found for {weapon.Skin.StaticID}");
                                    break;
                                }
                                break;
                            case PowerCore powerCore:
                                if (!mappings.PowerCoreMappingByGuid.TryGetValue(powerCore.StaticID, out powerCore.PowerCoreDetails))
                                {
                                    Debug.LogError($"No Power Core mapping found for {powerCore.StaticID}");
                                }
                                break;
                            case Utility utility:
                                //if (!mappings.UtilityMappingByGuid.TryGetValue(utility.StaticID, out utility.UtilityDetails))
                                //{
                                //    Debug.LogError($"No Utility mapping found for {utility.StaticID}");
                                //}
                                break;
                            default:
                                Debug.LogError($"Unknown accessory {item}");
                                break;
                        }
                    }
                    break;
                case MysteryCrate crate:
                    if (!mappings.MysteryCrateMappingByGuid.TryGetValue(crate.StaticID, out crate.MysteryCrateDetails))
                    {
                        Debug.LogError($"No Crate mapping found for {crate.StaticID}");
                    }
                    break;
                case Weapon weapon:
                    if (!mappings.WeaponModelMappingByGuid.TryGetValue(weapon.StaticID, out weapon.WeaponModelDetails))
                    {
                        Debug.LogError($"No Weapon mapping found for {weapon.StaticID}");
                        break;
                    }
                    if (weapon?.Skin?.StaticID != null && !mappings.WeaponSkinMappingByGuid.TryGetValue(weapon.Skin.StaticID, out weapon.WeaponSkinDetails))
                    {
                        Debug.LogError($"No Weapon Skin mapping found for {weapon.Skin.StaticID}");
                        break;
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

        private void LoadSkinReference(AssetReferenceSkin skinReference, Action<ScriptableObjects.Skin> callBack)
        {
            if (skinReference != null)
            {
                var skinOperationHandler = skinReference.LoadAssetAsync(); 
                StartCoroutine(loadingProgressContext.LoadingAssetProgress(skinOperationHandler, "Loading Skin"));
                skinOperationHandler.Completed += (skin) =>
                {
                    myMech.skin = skin.Result;
                    callBack(myMech.skin);
                };
            }
            else
            {
                //Skin already loaded
                callBack(null);
            }
        }
        
#if UNITY_EDITOR
        public void QuickSpawn(Transform newSpawn = null, bool isWeapon = false, bool inCrate = false)
        {
            if (!prevTransform && !newSpawn)
            {
                Debug.LogError("No prior spawn set");
                return;
            }

            if (newSpawn)
                SpawnMech(newSpawn, inCrate, isWeapon, true);
            else
                SpawnMech(prevTransform, false, isWeapon, true);
        }
#endif
        private Dictionary<AsyncOperationHandle, AssetReferenceSkin> skinToMeshMap = new();

        public void ResetSkinMapping()
        {
            skinToMeshMap.Clear();
        }

        private Transform siloDirection;
        public void SpawnMech(Transform spawnLocation, bool insideCrate = false, bool isWeaponOnly = false, bool quickLoad = false)
        {
            if(!insideCrate)
                prevTransform = spawnLocation;

            if (!siloDirection) skinToMeshMap.Clear();

            //When new mech is spawned remove previous unless inside crate
            if (quickLoad)
                UnloadMech(quickLoad, insideCrate);

            previousMech = TargetMech;

            if (sameMechChassis)
            {
                SetLoadedSkin(myMech.mech, TargetSkin, insideCrate);
                return;
            }
                        
            if (skinToMeshMap.Count == 0 && !insideCrate)
                siloDirection = spawnLocation;

            //Rotate items (primarily used for composables & in crate spawning)
            Vector3 spawnPosition = spawnLocation.position;
            Quaternion spawnRotation = Quaternion.LookRotation(spawnLocation.forward, spawnLocation.up);

            /*            if (skinToMeshMap.Count == 0 && insideCrate)
                newRotation = siloDirection;*/

            //Load and spawn mech
            var mechOperationHandler = TargetMech.InstantiateAsync(spawnPosition, spawnRotation, spawnLocation);
            StartCoroutine(loadingProgressContext.LoadingAssetProgress(mechOperationHandler, "Loading Mesh"));
            
            if(TargetSkin != null)
                skinToMeshMap.Add(mechOperationHandler, TargetSkin);

            mechOperationHandler.Completed += (mech) =>
                {
                    if (insideCrate && !crateInstance)
                        crateInstance = myMech.mech;

                    myMech.mech = mech.Result;

                    if (isWeaponOnly && !insideCrate)
                    {
                        mech.Result.transform.Rotate(Vector3.right, -90.0f);
                        mech.Result.transform.Rotate(Vector3.forward, -90.0f);
                    }
                    
                    SetLoadedSkin(mech.Result, FindMeshSkin(mech.Result), insideCrate);
                };
        }

        private AssetReferenceSkin FindMeshSkin(GameObject currentMeshObj)
        {
            foreach(var item in skinToMeshMap)
            {
                var t = item.Key.Result as GameObject;
                if (t == currentMeshObj)
                    return item.Value;
            }
            return null;
        }

        private void SetLoadedSkin(GameObject result, AssetReferenceSkin skinReference, bool insideCrate = false)
        {
            LoadSkinReference(skinReference,
                (skin) =>
                {
                    Renderer mechMesh = result.GetComponentInChildren<Renderer>();
                    var platformRepositioner = result.GetComponentInChildren<SiloPlatformRepositioner>();
                    if (skin != null)
                        mechMesh.sharedMaterials = skin.mats;

                    Container.InjectGameObject(result);

                    loadingProgressContext.ProgressSignalHandler.FinishedLoading(result);

                    if (insideCrate)
                        _crateSignalHandler.OpenCrate();
                    else
                    {
                        if (platformRepositioner && skinToMeshMap.Count <= 1)
                            platformRepositioner.RepositionObject();

                        _siloSignalHandler.SiloFilled();
                    }
                }
            );
        }
#if UNITY_EDITOR
        public void UnloadMech(bool quickLoad = false, bool insideCrate = false)
        {
            if (myMech.skin != null)
            {
                Addressables.Release(myMech.skin);
                myMech.skin = null;
            }

            if (previousMech != null && (!insideCrate || crateOpened))
            {
                if (!quickLoad || previousMech != TargetMech)
                {
                    if(!insideCrate && crateOpened)
                        Addressables.ReleaseInstance(crateInstance);
                    
                    Addressables.ReleaseInstance(myMech.mech);
                    skinToMeshMap.Clear();
                }

                if (previousMech != TargetMech &&
                    myMech.mech != null)
                {
                    sameMechChassis = false;
                }

                if (previousMech == TargetMech && myMech.mech)
                    sameMechChassis = true;

            }
            crateOpened = insideCrate;
        }
#endif
    }
}