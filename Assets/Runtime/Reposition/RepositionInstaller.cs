using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

<<<<<<<< HEAD:Assets/Runtime/Environment/RepositionInstaller.cs
namespace SupremacyHangar.Runtime.Environment
========
namespace SupremacyHangar.Runtime.Reposition
>>>>>>>> develop:Assets/Runtime/Reposition/RepositionInstaller.cs
{
    public struct RepositionObjectSignal
    {
        public Vector3 Position;
    }

    public class RepositionSignalHandler
    {
        readonly SignalBus _signalBus;

        public RepositionSignalHandler(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void RepositionObject(Vector3 pos)
        {
            _signalBus.Fire(new RepositionObjectSignal() { Position = pos});
        }
    }

    public class RepositionSignalInstaller : Installer<RepositionSignalInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<RepositionSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<RepositionObjectSignal>();
        }
    }
}
