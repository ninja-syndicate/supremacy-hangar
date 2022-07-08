using SupremacyHangar.Runtime.Silo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace SupremacyHangar.Runtime.ContentLoader
{
    public class AssetLoadingProgressSignal
    {
        public float PercentageComplete;
        public string Description;
    }

    public class AssetLoadedSignal { }
    public class AssetLoadedWithSpawnSignal 
    {
        public Transform SpawnPoint;
    }

    public class ProgressSignalHandler
    {
        readonly SignalBus _signalBus;
        private Dictionary<AsyncOperationHandle, string> progressAmounts = new();

        public ProgressSignalHandler(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void ProgressBar(AsyncOperationHandle key, string message)
        { 
            if (!progressAmounts.ContainsKey(key))
                progressAmounts.Add(key, message);

            AssetLoadingProgress();
        }

        public void FinishedLoading(GameObject res)
        { 
            if ( res && res.TryGetComponent(out SpawnPointLocation loc))
                _signalBus.Fire(new AssetLoadedWithSpawnSignal() { SpawnPoint = loc.SpawnPoint });
            else
                _signalBus.Fire<AssetLoadedSignal>();
        }

        private void AssetLoadingProgress()
        {
            float totalProgress = 0;
            float progress = 0;
            string currentlyLoading = "";

            foreach (var handle in progressAmounts)
            {
                progress += handle.Key.PercentComplete;
                currentlyLoading = handle.Key.PercentComplete < 1 ? currentlyLoading = handle.Value : "";
            }

            if (progress == progressAmounts.Count)
                totalProgress = 1;
            else
                totalProgress = progress / progressAmounts.Count;

            Debug.Log(totalProgress);
            _signalBus.Fire(new AssetLoadingProgressSignal() { PercentageComplete = totalProgress, Description = currentlyLoading });

            if (totalProgress == 1) progressAmounts.Clear();
        }
    }

    public class ProgressSignalInstaller : Installer<ProgressSignalInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<ProgressSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<AssetLoadingProgressSignal>().OptionalSubscriber().RunAsync();
            Container.DeclareSignal<AssetLoadedSignal>().OptionalSubscriber().RunAsync();
            Container.DeclareSignal<AssetLoadedWithSpawnSignal>().OptionalSubscriber().RunAsync();
        }
    }
}
