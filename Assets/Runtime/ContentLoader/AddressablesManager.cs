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
        [Inject]
        private SupremacyGameObject _playerInventory;

        [Inject]
        private ContentSignalHandler _contentSignalHandler;

        public AssetReference TargetMech { get; set; }
        public AssetReference TargetSkin { get; set; }

        private AssetReference previousMech;

        public string FactionName { get; set; }

        private LoadedAsset myMech { get; set; } = new LoadedAsset();

        [SerializeField] private AssetReferenceAssetMappings assetMappingsReference;

        private AssetMappings mappings;

        private SignalBus _bus;
        private bool _subscribed;

        [Inject]
        public void Construct(SignalBus bus)
        {
            _bus = bus;
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
        }

        private void SetPlayerInventory()
        {
            _playerInventory.factionGraph = mappings.FactionHallwayByGuid[_playerInventory.faction].ConnectivityGraph;
            foreach (var silo in _playerInventory.Silos)
            {
                switch (silo)
                {
                    case Mech mech:
                        mech.MechChassisDetails = mappings.MechChassisPrefabByGuid[mech.mech_id];
                        mech.MechSkinDetails = mappings.MechSkinAssetByGuid[mech.skin_id];
                        break;
                    case MysteryBox box:
                        box.MysteryCrateDetails = mappings.MysteryCrateAssetByGuid[box.mystery_crate_id];
                        break;
                    default:
                        Debug.LogError($"Unknown silo of type {silo}.");
                        break;
                }
            }
            Debug.Log("Inventory saved");
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

            if (previousMech != null &&
                myMech.mech == null)
            {
                previousMech.ReleaseAsset();
            }
        }
    }
}