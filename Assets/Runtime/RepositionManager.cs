using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime
{
    public struct SaveRelativePositionSignal
    {
        public Vector3 Position;
    }

    public struct RepositionObjectSignal
    {}

    public struct UpdatePositionSignal
    {}

    public class RepositionSignalHandler
    {
        readonly SignalBus _signalBus;

        public RepositionSignalHandler(SignalBus signalBus)
        {
            Debug.Log("init");
            _signalBus = signalBus;
        }

        public void saveRelativePosition()
        {
            _signalBus.Fire(new SaveRelativePositionSignal() { Position = Vector3.zero });
        }

        public void repositionObject()
        {
            _signalBus.Fire<RepositionObjectSignal>();
        }

        public void updatePosition()
        {

            _signalBus.Fire<UpdatePositionSignal>();
        }
    }

    public class RepositionManager : MonoInstaller<RepositionManager>
    {
        public override void InstallBindings()
        {
            Debug.Log("My bindings are installed");
            SignalBusInstaller.Install(Container);
            Container.Bind<RepositionSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<SaveRelativePositionSignal>().OptionalSubscriber();
            Container.DeclareSignal<RepositionObjectSignal>();
            Container.DeclareSignal<UpdatePositionSignal>();
        }
    }
}
