using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.ContentLoader.Types;
using SupremacyHangar.Runtime.Silo;
using SupremacyHangar.Runtime.Types;
using System;
using System.Collections.Generic;
using SupremacyData.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using MysteryCrate = SupremacyHangar.Runtime.Types.MysteryCrate;
using PowerCore = SupremacyHangar.Runtime.Types.PowerCore;

namespace SupremacyHangar.Runtime.Actors.Silo
{
    public class SiloAssetLoader : MonoBehaviour
    {
        private HashSet<GameObject> loadedInstances = new();
        private HashSet<AssetReference> loadedReferences = new();

        private BridgeScript bridgeScript;

        private AddressablesManager addressablesManager;
        private SiloState siloState;
        private SignalBus _bus;
        private bool _subscribed;
        private SiloSignalHandler _siloSignalHandler;

        [Inject]
        public void Construct(AddressablesManager addressablesManager, SiloState siloState, BridgeScript bridgeScript)
        {
            this.addressablesManager = addressablesManager;
            this.siloState = siloState;
            this.siloState.OnStateChanged += OnSiloStateChanged;
            this.bridgeScript = bridgeScript;
        }

        [Inject]
        public void ConstructSignals(SignalBus bus, SiloSignalHandler siloSignalHandler)
        {
            _bus = bus;

            _siloSignalHandler = siloSignalHandler;
            SubscribeToSignal();
        }

        private void OnEnable()
        {
            SubscribeToSignal();
        }

        private void OnDisable()
        {
            if (!_subscribed) return;
            _bus.Unsubscribe<AssetLoadedSignal>(FinishedLoadingSiloAsset);
            _bus.Unsubscribe<ComposableLoadedSignal>(LoadComposable);
            _bus.Unsubscribe<AssetLoadedWithSpawnSignal>(ChangeSpawnLocation);
            _bus.Unsubscribe<FillCrateSignal>(FillCrate);
            _bus.Unsubscribe<UnloadSiloContentSignal>(Unload);
            _subscribed = false;
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<AssetLoadedSignal>(FinishedLoadingSiloAsset);
            _bus.Subscribe<ComposableLoadedSignal>(LoadComposable);
            _bus.Subscribe<AssetLoadedWithSpawnSignal>(ChangeSpawnLocation);
            _bus.Subscribe<FillCrateSignal>(FillCrate);
            _bus.Subscribe<UnloadSiloContentSignal>(Unload);
            _subscribed = true;
        }

        private void FinishedLoadingSiloAsset(AssetLoadedSignal signal)
        {
            switch (siloState.CurrentState)
            {
                case SiloState.StateName.LoadingCrateContent:
                case SiloState.StateName.LoadingSiloContent:
                    if(!loadedInstances.Contains(addressablesManager.myMech.mech))
                        loadedInstances.Add(signal.Asset);
                    break;
            }

            NextSiloState();
        }

        private void ChangeSpawnLocation(AssetLoadedWithSpawnSignal signal)
        {
            siloState.SpawnLocation = signal.SpawnPoint; 
            NextSiloState();
        }

        private void LoadComposable(ComposableLoadedSignal signal)
        {
            if (siloState.CurrentState == SiloState.StateName.NotLoaded) return;
            if (siloState.Contents is not Mech mech)
            {
                Debug.LogError("Mech was null!");
                return;
            }

            if (mech.Accessories == null || mech.Accessories.Length == 0) return;
            var accessoryList = mech.Accessories;
            
            //TODO: this should probably be done after parsing. 
            Queue<Weapon> weaponAccessories = new ();
            Queue<Utility> utilityAccessories = new ();
            Queue<PowerCore> powerCoreAccessories = new ();

            foreach (var accessory in accessoryList)
            {
                switch (accessory)
                {
                    case Weapon weapon:
                        weaponAccessories.Enqueue(weapon);
                        break;
                    case Utility utility:
                        utilityAccessories.Enqueue(utility);
                        break;
                    case PowerCore powerCore:
                        powerCoreAccessories.Enqueue(powerCore);
                        break;
                    default:
                        Debug.Log($"Unknown accessory! {accessory}");
                        break;
                }
            }
            
            //Load Weapons
            foreach (Transform spawn in signal.SocketsLists.WeaponSockets)
            {
                if (!weaponAccessories.TryDequeue(out var weapon)) continue;
                SetLoadContents(weapon);
                addressablesManager.SpawnMech(spawn);
            }
            
            if (weaponAccessories.Count > 0) Debug.LogError("More weapons in payload than slots!");

            //Load Utilities
            foreach (Transform spawn in signal.SocketsLists.UtilitySockets)
            {
                if (!utilityAccessories.TryDequeue(out var utility)) continue;
                SetLoadContents(utility);
                addressablesManager.SpawnMech(spawn);
            }

            if (utilityAccessories.Count > 0) Debug.LogError("More utilities in payload than slots!");

            if (!powerCoreAccessories.TryDequeue(out var aPowerCore))
            {
                Debug.LogError("No powercore in payload!");
                return;
            }
            
            SetLoadContents(aPowerCore);
            addressablesManager.SpawnMech(signal.SocketsLists.PowerCoreSocket);

            if (powerCoreAccessories.Count > 0) Debug.LogError("More PowerCores in payload than slots!");
            
            NextSiloState();
        }

        private void FillCrate(FillCrateSignal signal)
        {
            if(siloState.CurrentState == SiloState.StateName.LoadingCrateContent)
                LoadCrateContents(siloState.SpawnLocation, signal.CrateContents);
        }

        private void NextSiloState()
        {
            switch(siloState.CurrentState)
            {
                case SiloState.StateName.LoadingSiloContent:
                    switch(siloState.Contents)
                    {
                        case MysteryCrate crate:
                            siloState.NextState(SiloState.StateName.LoadedWithCrate);
                            break;
                        default:
                            siloState.NextState(SiloState.StateName.Loaded);
                            break;
                    }
                    break;
                case SiloState.StateName.LoadingCrateContent:
                    siloState.NextState(SiloState.StateName.Loaded);
                    break;
            }
        }

        private void OnSiloStateChanged(SiloState.StateName newState)
        {
            switch (newState)
            {
                case SiloState.StateName.EmptySiloLoaded:
                    LoadHangarContents(siloState.SpawnLocation);
                    siloState.NextState(SiloState.StateName.LoadingSiloContent);
                    break;
                case SiloState.StateName.LoadingCrateContent:
                    bridgeScript.RequestCrateContent(siloState.Contents.OwnershipID.ToString());
                    break;
                default:
                    break;
            }
        }

        public void LoadHangarContents(Transform spawnPoint)
        {
            SetLoadContents(siloState.Contents);
            if(siloState.Contents is Weapon)
                addressablesManager.SpawnMech(spawnPoint, false, true);
            else
                addressablesManager.SpawnMech(spawnPoint);
        }

        public void LoadCrateContents(Transform spawnPoint, SiloItem crateContent)
        {
            addressablesManager.MapSiloToAsset(crateContent);
            siloState.ChangeSiloContentToCrate(crateContent);
            SetLoadContents(crateContent);
            if(crateContent is Weapon)
                addressablesManager.SpawnMech(spawnPoint, true, true);
            else
                addressablesManager.SpawnMech(spawnPoint, true);
        }

        public void Unload()
        {
            foreach (var asset in loadedReferences)
                asset.ReleaseAsset();

            foreach (var instance in loadedInstances)
                Addressables.ReleaseInstance(instance);

            loadedInstances.Clear();
            loadedReferences.Clear();
            addressablesManager.myMech.skin = null;
            addressablesManager.ResetSkinMapping();
            _siloSignalHandler.SiloUnloaded();
        }

        private void SetLoadContents(SiloItem contents)
        {
            switch (contents)
            {
                case Mech mech:
                    PopulateWithMech(mech);
                    break;
                case MysteryCrate box:
                    addressablesManager.TargetMech = box.MysteryCrateDetails.MysteryCrateReference;
                    addressablesManager.TargetSkin = null;
                    break;
                case Weapon weapon:
                    addressablesManager.TargetMech = weapon.WeaponModelDetails?.Reference;
                    addressablesManager.TargetSkin = weapon.WeaponSkinDetails?.Reference;
                    if (weapon.WeaponSkinDetails == null)
                    {
                        Debug.LogError("Weapon Model Skin has no reference so not loading.");
                    }
                    else
                    {
                        loadedReferences.Add(weapon.WeaponSkinDetails.Reference);
                    }
                    break;
                case PowerCore powerCore:
                    addressablesManager.TargetMech = powerCore.PowerCoreDetails.Reference;
                    addressablesManager.TargetSkin = null;
                    break;
                case Utility utility:
                    addressablesManager.TargetMech = null;
                    addressablesManager.TargetSkin = null;
                    break;
                default:
                    Debug.LogError($"Unexpected type of {siloState.Contents.GetType()} - cowardly refusing to fill the silo", this);
                    break;
            }
        }

        private void PopulateWithMech(Mech mech)
        {
            //bool populated = true;
            if (mech.MechChassisDetails == null)
            {
                Debug.LogError($"Unmapped Mech ID {mech.StaticID} can't load silo", this);
                //populated = false;
            }
            else
            {
                addressablesManager.TargetMech = mech.MechChassisDetails.MechReference;
            }

            if (mech.MechSkinDetails == null)
            {
                Debug.LogError($"Unmapped Skin ID {mech.StaticID} can't load silo", this);
                //populated = false;
            }
            else
            {
                addressablesManager.TargetSkin = mech.MechSkinDetails.SkinReference;
                loadedReferences.Add(mech.MechSkinDetails.SkinReference);
            }
        }
    }
}