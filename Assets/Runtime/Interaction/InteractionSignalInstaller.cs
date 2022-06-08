using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Interaction
{
    public class InteractionSignal {
        public string message;
    }

    public class InteractionSignalHandler
    {
        readonly SignalBus _signalBus;

        public InteractionSignalHandler(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void PressurizeSilo()
        {
            _signalBus.Fire(new InteractionSignal() { message = "pressurize"});
        }

        public void InteractWithElevator()
        {
            _signalBus.Fire(new InteractionSignal() { message = "elevator" });
        }

        public void InteractWithCrate()
        {

        }
    }

    public class InteractionSignalInstaller : Installer<InteractionSignalInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<InteractionSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<InteractionSignal>();
        }
    }
}