using SupremacyHangar.Runtime.ContentLoader;
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
        private HashSet<AssetReference> loaded;

        private AddressablesManager addressablesManager;
        private SiloState siloState;
        private SignalBus _bus;
        private bool _subscribed;
        private CrateSignalHandler _crateSignalHandler;

        [Inject]
        public void Construct(AddressablesManager addressablesManager, SiloState siloState, SignalBus bus, CrateSignalHandler crateSignalHandler)
        {
            _bus = bus;
            this.addressablesManager = addressablesManager;
            this.siloState = siloState;
            this.siloState.OnStateChanged += OnSiloStateChanged;
            _crateSignalHandler = crateSignalHandler;
            SubscribeToSignal();
        }

        private void OnEnable()
        {
            SubscribeToSignal();
        }

        private void OnDisable()
        {
            if (!_subscribed) return;
            _bus.Unsubscribe<AssetLoadedWithSpawnSignal>(ChangeSpawnLocation);
            _bus.Subscribe<AssetLoadedSignal>(FinishedLoading);
            _bus.Unsubscribe<FillCrateSignal>(FillCrate);
            _subscribed = false;
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<AssetLoadedWithSpawnSignal>(ChangeSpawnLocation);
            _bus.Subscribe<AssetLoadedSignal>(FinishedLoading);
            _bus.Subscribe<FillCrateSignal>(FillCrate);
            _subscribed = true;
        }

        private void FinishedLoading()
        {
            if(siloState.CurrentState == SiloState.StateName.LoadingSiloContent)
                siloState.NextState(SiloState.StateName.Loaded);
        }

        private void ChangeSpawnLocation(AssetLoadedWithSpawnSignal signal)
        {
            siloState.SpawnLocation = signal.SpawnPoint; 
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