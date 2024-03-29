using SupremacyHangar.Runtime.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Silo
{
    public class OpenCrateSignal { }
    public class FillCrateSignal 
    {
        public SiloItem CrateContents;
    }

    public class CanOpenCrateSignal { }

    public class CrateSignalHandler
    {
        readonly SignalBus _signalBus;

        public CrateSignalHandler(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void OpenCrate()
        {
            _signalBus.Fire<OpenCrateSignal>();
        }

        public void FillCrate(SiloItem crateContent)
        {
            _signalBus.Fire(new FillCrateSignal() { CrateContents = crateContent});
        }

        public void CanOpenCrate()
        {
            _signalBus.Fire<CanOpenCrateSignal>();
        }
    }

    public class CrateSignalInstaller : Installer<CrateSignalInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<CrateSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<OpenCrateSignal>().OptionalSubscriber();
            Container.DeclareSignal<FillCrateSignal>().OptionalSubscriber();
            Container.DeclareSignal<CanOpenCrateSignal>().OptionalSubscriber();
        }
    }
}
