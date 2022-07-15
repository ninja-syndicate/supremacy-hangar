using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Silo
{
    public class CloseSiloSignal { }
    public class SiloUnloadedSignal { }
    public class UnloadSiloContentSignal { }

    public class SiloFilledSignal { }

    public class UnlockOtherSilo { }

    public class PlatformRepositionSignal
    {
        public Vector3 Position;
    }

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
        public void UnloadSiloContent()
        {
            _signalBus.Fire<UnloadSiloContentSignal>();
        }
        public void SiloUnloaded()
        {
            _signalBus.Fire<SiloUnloadedSignal>();
        }

        public void SiloFilled()
        {
            _signalBus.Fire<SiloFilledSignal>();
        }

        public void UnlockOtherSilo()
        {
            _signalBus.Fire<UnlockOtherSilo>();
        }
        
        public void RepositionPlatform(Vector3 pos)
        {
            _signalBus.Fire(new PlatformRepositionSignal() { Position = pos});
        }
    }

    public class SiloSignalInstaller : Installer<SiloSignalInstaller>
    {       
        public override void InstallBindings()
        {
            Container.Bind<SiloSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<CloseSiloSignal>();
            Container.DeclareSignal<SiloUnloadedSignal>();
            Container.DeclareSignal<SiloFilledSignal>();
            Container.DeclareSignal<UnloadSiloContentSignal>();
            Container.DeclareSignal<PlatformRepositionSignal>();
            Container.DeclareSignal<UnlockOtherSilo>();
        }
    }
}