using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.ContentLoader
{
    public class InventoryLoadedSignal { }

    public class ContentSignalHandler
    {
        readonly SignalBus _signalBus;

        public ContentSignalHandler(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void InventoryLoaded()
        {
            _signalBus.Fire<InventoryLoadedSignal>();
        }
    }

    public class ContentSignalInstaller : Installer<ContentSignalInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<ContentSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<InventoryLoadedSignal>();
        }
    }
}