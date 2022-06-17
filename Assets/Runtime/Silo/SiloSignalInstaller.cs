using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Silo
{
    public class CloseSiloSignal { }
    public class SiloUnloadedSignal { }

    public class SiloSignalHandler
    {
        readonly SignalBus _signalBus;

        public SiloSignalHandler(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void CloseSilo()
        {
            _signalBus.Fire<CloseSiloSignal>();
        }
        public void SiloUnloaded()
        {
            _signalBus.Fire<SiloUnloadedSignal>();
        }
    }

    public class SiloSignalInstaller : Installer<SiloSignalInstaller>
    {       
        public override void InstallBindings()
        {
            Container.Bind<SiloSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<CloseSiloSignal>();
            Container.DeclareSignal<SiloUnloadedSignal>();
        }
    }
}