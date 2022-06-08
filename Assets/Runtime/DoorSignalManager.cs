using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class DoorOpenSignal 
{
    public GameObject TriggerDoor;
}

public class DoorClosedSignal
{}

public class DoorSignalHandler
{
    readonly SignalBus _signalBus;

    public DoorSignalHandler(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    public void DoorOpen(GameObject triggerDoor)
    {
        _signalBus.Fire(new DoorOpenSignal() { TriggerDoor = triggerDoor});
    }
    public void DoorClosed()
    {
        _signalBus.Fire<DoorClosedSignal>();
    }
}

public class DoorSignalManager : MonoInstaller<DoorSignalManager>
    {
        public override void InstallBindings()
        {
            //Signal bus installed by repositioner
            Container.Bind<DoorSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<DoorOpenSignal>();
            Container.DeclareSignal<DoorClosedSignal>();
        }
}
