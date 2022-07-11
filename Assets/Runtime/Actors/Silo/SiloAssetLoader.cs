using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.ContentLoader.Types;
using SupremacyHangar.Runtime.Silo;
using SupremacyHangar.Runtime.Types;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.Silo
{
    public class SiloAssetLoader : MonoBehaviour
    {
        private HashSet<GameObject> loadedInstances = new();
        private HashSet<AssetReference> loadedReferences = new();

        private AddressablesManager addressablesManager;
        private SiloState siloState;
        private SignalBus _bus;
        private bool _subscribed;
        private CrateSignalHandler _crateSignalHandler;
        private SiloSignalHandler _siloSignalHandler;

        [Inject]
        public void Construct(AddressablesManager addressablesManager, SiloState siloState, SignalBus bus, CrateSignalHandler crateSignalHandler, SiloSignalHandler siloSignalHandler)
        {
            _bus = bus;
            this.addressablesManager = addressablesManager;
            this.siloState = siloState;
            this.siloState.OnStateChanged += OnSiloStateChanged;
            _crateSignalHandler = crateSignalHandler;
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

        private void FinishedLoadingSiloAsset()
        {
            switch (siloState.CurrentState)
            {
                case SiloState.StateName.LoadingCrateContent:
                case SiloState.StateName.LoadingSiloContent:
                    if(!loadedInstances.Contains(addressablesManager.myMech.mech))
                        loadedInstances.Add(addressablesManager.myMech.mech);

                    if (addressablesManager.TargetSkin != null && !loadedReferences.Contains(addressablesManager.TargetSkin))
                        loadedReferences.Add(addressablesManager.TargetSkin);
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
            foreach (Transform spawn in signal.SpawnPoints)
            {
                SetLoadContents(siloState.Contents);
                addressablesManager.SpawnMech(spawn);
            }

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
                    _crateSignalHandler.NeedCrateContent();
#if UNITY_WEBGL
                    //Plugins.WebGL.WebGLPluginJS.GetCrateContent(siloState.Contents.OwnershipID.ToString());
#endif
                    break;
                default:
                    break;
            }
        }

        public void LoadHangarContents(Transform spawnPoint)
        {
            SetLoadContents(siloState.Contents);
            addressablesManager.SpawnMech(spawnPoint);
        }

        public void LoadCrateContents(Transform spawnPoint, SiloItem crateContent)
        {
            addressablesManager.MapSiloToAsset(crateContent);
            SetLoadContents(crateContent);
            addressablesManager.SpawnMech(spawnPoint, true);
        }

        public void Unload()
        {
            foreach(var instance in loadedInstances)
                Addressables.ReleaseInstance(instance);

            foreach (var asset in loadedReferences)
                asset.ReleaseAsset();

            loadedInstances.Clear();
            loadedReferences.Clear();
            addressablesManager.myMech.skin = null;
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
                default:
                    Debug.LogWarning($"Unexpected type of {siloState.Contents.GetType()} - cowardly refusing to fill the silo", this);
                    break;
            }
        }

        private void PopulateWithMech(Mech mech)
        {
            //bool populated = true;
            if (mech.MechChassisDetails == null)
            {
                Debug.LogWarning($"Unmapped Mech ID {mech.StaticID} can't load silo", this);
                //populated = false;
            }
            else
            {
                addressablesManager.TargetMech = mech.MechChassisDetails.MechReference;
            }

            if (mech.MechSkinDetails == null)
            {
                Debug.LogWarning($"Unmapped Skin ID {mech.StaticID} can't load silo", this);
                //populated = false;
            }
            else
            {
                addressablesManager.TargetSkin = mech.MechSkinDetails.SkinReference;
            }
        }
    }
}