using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime
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

        public void repositionObject(Vector3 pos)
        {
            _signalBus.Fire(new RepositionObjectSignal() { Position = pos});
        }
    }

    public class RepositionManager : MonoInstaller<RepositionManager>
    {
        public override void InstallBindings()
        {
            Debug.Log("My bindings are installed");
            SignalBusInstaller.Install(Container);
            Container.Bind<RepositionSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<RepositionObjectSignal>();
        }
    }
}
