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
        private SupremacyGameObject _playerInventory;

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
                return mappings.FactionHallwayByGuid[_playerInventory.faction].DataFaction;
            }
        }
        
        private AssetReference previousMech;
        
        private LoadedAsset myMech { get; set; } = new LoadedAsset();

        [SerializeField] private AssetReferenceAssetMappings assetMappingsReference;

        private AssetMappings mappings;
        private bool mappingsLoaded;

        private SignalBus _bus;
        private bool _subscribed;

        private SiloSignalHandler _signalHandler;


        private Transform prevTransform;
        private bool sameMechChassis = false;

        private AsyncOperationHandle mechOperationHandler;
        private AsyncOperationHandle skinOperationHandler;

        [Inject]
        public void Construct(SignalBus bus, SiloSignalHandler siloHandler)
        {
            _bus = bus;
            _signalHandler = siloHandler;
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
            _playerInventory.factionGraph = mappings.FactionHallwayByGuid[_playerInventory.faction].ConnectivityGraph;
            foreach (var silo in _playerInventory.Silos)
            {
                switch (silo)
                {
                    case Mech mech:
                        mech.MechChassisDetails = mappings.MechChassisPrefabByGuid[mech.MechID];
                        mech.MechSkinDetails = mappings.MechSkinAssetByGuid[mech.SkinID];
                        break;
                    case MysteryBox box:
                        box.MysteryCrateDetails = mappings.MysteryCrateAssetByGuid[box.MysteryCrateID];
                        break;
                    default:
                        Debug.LogError($"Unknown silo of type {silo}.");
                        break;
                }
            }
            _contentSignalHandler.InventoryLoaded();
        }
        
        private bool ValidateMappings()
        {
            if (assetMappingsReference != null) return true;
            Debug.LogError("No asset mapping file assigned to AddressablesManager!", this);
            return false;
        }

        private void LoadSkinReference(Action<Skin> callBack)
        {
            if (myMech.skin == null && TargetSkin != null)
            {
                skinOperationHandler = TargetSkin.LoadAssetAsync(); 
                StartCoroutine(LoadingSkin());
                skinOperationHandler.Completed += (skin) =>
                {
                    myMech.skin = skin.Result as Skin;
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


#if UNITY_EDITOR
        public void QuickSpawn()
        {
            if (!prevTransform)
            {
                Debug.LogError("No prior spawn set");
                return;
            }

            SpawnMech(prevTransform);
        }
#endif

        private IEnumerator LoadingAsset()
        {
            do
            {
                _contentSignalHandler.AssetLoadProgress(mechOperationHandler.PercentComplete);
                yield return null;
            } while (!mechOperationHandler.IsDone);
            _contentSignalHandler.AssetLoadProgress(1);
        }

        private IEnumerator LoadingSkin()
        {
            do
            {
                _contentSignalHandler.SkinLoadProgress(skinOperationHandler.PercentComplete);
                yield return null;
            } while (!mechOperationHandler.IsDone);
            _contentSignalHandler.SkinLoadProgress(1);
        }

        public void SpawnMech(Transform spawnLocation)
        {
            prevTransform = spawnLocation;
            //When mech out of view release addressables
            UnloadMech();

            if(sameMechChassis)
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
                        SetLoadedSkin(myMech.mech);
                    };
                });
        }

        private void SetLoadedSkin(GameObject mech)
        {
            LoadSkinReference(
                (skin) =>
                {
                    MeshRenderer mechMesh = myMech.mech.GetComponentInChildren<MeshRenderer>();
                    if (skin != null)
                        mechMesh.sharedMaterials = skin.mats;

                    mechMesh.enabled = true; 
                    Container.InjectGameObject(myMech.mech);
                    _signalHandler.SiloFilled();
                }
            );
        }

        public void UnloadMech()
        {
            if (myMech.skin != null)
            {
                Addressables.Release(myMech.skin);
                myMech.skin = null;
            }

            if (previousMech != null &&
                myMech.mech != null && previousMech != TargetMech)
            {
                previousMech.ReleaseAsset();
#if UNITY_EDITOR
                sameMechChassis = false;
                Destroy(myMech.mech);
                myMech.mech = null;
#endif
            }

            if (previousMech != null && previousMech == TargetMech)
                sameMechChassis = true;
        }
    }
}