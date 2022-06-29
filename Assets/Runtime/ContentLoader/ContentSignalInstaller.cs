using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.ContentLoader
{
    public class InventoryLoadedSignal { }
    public class InventoryRecievedSignal { }

    public class AssetLoadingProgressSignal
    {
        public float PercentageComplete;
    }

    public class ContentSignalHandler
    {
        readonly SignalBus _signalBus;

        private float siloLoadProgress = 0;
        private float assetLoadProgress = 0;
        private float skinLoadProgress = 0;
        private Dictionary<string, float> progressAmounts = new();

        public ContentSignalHandler(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void InventoryLoaded()
        {
            _signalBus.Fire<InventoryLoadedSignal>();
        }
        
        public void InventoryRecieved()
        {
            _signalBus.Fire<InventoryRecievedSignal>();
        }

        public void SiloLoadProgress(float completePercentage)
        {
            if (!progressAmounts.ContainsKey("silo"))
                progressAmounts.Add("silo", completePercentage);
            else
                progressAmounts["silo"] = completePercentage;

            //   siloLoadProgress = completePercentage;
            AssetLoadingProgress();
        }

        public void AssetLoadProgress(float completePercentage)
        {
            if (!progressAmounts.ContainsKey("asset"))
                progressAmounts.Add("asset", completePercentage);
            else
                progressAmounts["asset"] = completePercentage;

            //assetLoadProgress = completePercentage;
            AssetLoadingProgress();
        }

        public void SkinLoadProgress(float completePercentage)
        {
            if (!progressAmounts.ContainsKey("skin"))
                progressAmounts.Add("skin", completePercentage);
            else
                progressAmounts["skin"] = completePercentage;

            //skinLoadProgress = completePercentage;
            AssetLoadingProgress();
        }

        private void AssetLoadingProgress()
        {
            float totalProgress = 0;
            float progress = 0;
            foreach(var percentage in progressAmounts.Values)
            {
                progress += percentage;
            }

            if(progress == progressAmounts.Count)
                totalProgress = 1;
            else
                totalProgress = progress / progressAmounts.Count;

            Debug.Log(totalProgress);
            _signalBus.Fire(new AssetLoadingProgressSignal() { PercentageComplete = totalProgress});
        }
    }

    public class ContentSignalInstaller : Installer<ContentSignalInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<ContentSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<InventoryLoadedSignal>();
            Container.DeclareSignal<InventoryRecievedSignal>().RunAsync();
            Container.DeclareSignal<AssetLoadingProgressSignal>().RunAsync();
        }
    }
}