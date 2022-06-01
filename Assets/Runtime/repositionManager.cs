using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime
{
    public struct saveRelativePositionSignal
    {
        public Vector3 position;
    }

    public struct repositionObjectSignal
    {}

    public struct updatePositionSignal
    {}

    public class repositionSignalHandler
    {
        readonly SignalBus _signalBus;

        public repositionSignalHandler(SignalBus signalBus)
        {
            Debug.Log("init");
            _signalBus = signalBus;
        }

        public void saveRelativePosition()
        {
            _signalBus.Fire(new saveRelativePositionSignal() { position = Vector3.zero });
        }

        public void repositionObject()
        {
            _signalBus.Fire<repositionObjectSignal>();
        }

        public void updatePosition()
        {

            _signalBus.Fire<updatePositionSignal>();
        }
    }

    public class repositionManager : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            Container.Bind<repositionSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<saveRelativePositionSignal>().OptionalSubscriber();
            //Container.BindSignal<saveRelativePositionSignal>().ToMethod(Debug.Log("save rel pos"));
            Container.DeclareSignal<repositionObjectSignal>();
            Container.BindSignal<repositionObjectSignal>().ToMethod<repositioner>(x => x.moveToZero).FromResolve();
            Container.DeclareSignal<updatePositionSignal>();
        }

    }
}
