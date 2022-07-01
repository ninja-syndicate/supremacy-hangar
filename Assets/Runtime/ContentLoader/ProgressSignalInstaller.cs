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
    }

    public class ProgressSignalHandler
    {
        readonly SignalBus _signalBus;
        private Dictionary<AsyncOperationHandle, float> progressAmounts = new();

        public ProgressSignalHandler(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void ProgressBar(AsyncOperationHandle key, float completePercentage)
        {
            if (!progressAmounts.ContainsKey(key))
                progressAmounts.Add(key, completePercentage);
            else
                progressAmounts[key] = completePercentage;

            AssetLoadingProgress();
        }

        private void AssetLoadingProgress()
        {
            float totalProgress = 0;
            float progress = 0;
            foreach (var percentage in progressAmounts.Values)
            {
                progress += percentage;
            }

            if (progress == progressAmounts.Count)
                totalProgress = 1;
            else
                totalProgress = progress / progressAmounts.Count;

            Debug.Log(totalProgress);
            _signalBus.Fire(new AssetLoadingProgressSignal() { PercentageComplete = totalProgress });
        }
    }

    public class ProgressSignalInstaller : Installer<ProgressSignalInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<ProgressSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<AssetLoadingProgressSignal>().OptionalSubscriber().RunAsync();
        }
    }
}
