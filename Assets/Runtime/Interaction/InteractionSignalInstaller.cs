using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Interaction
{
    public enum InteractionType
    {
        Silo,
        Elevator
    }

    public class InteractionSignal {
        public InteractionType Type;
    }

    public class PlayerInteractionChangeSignal {
        public InteractionType Type;
    }

    public class InteractionSignalHandler
    {
        readonly SignalBus _signalBus;

        public InteractionSignalHandler(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void LoadSilo(InteractionType type)
        {
            _signalBus.Fire(new InteractionSignal() { Type = type });
        }

        public void InteractWithElevator(InteractionType type)
        {
            _signalBus.Fire(new InteractionSignal() { Type = type });
        }

        public void InteractWithCrate()
        {

        }

        public void ChangePlayerInteraction(InteractionType type)
        {
            _signalBus.Fire(new PlayerInteractionChangeSignal() { Type = type });
        }
    }

    public class InteractionSignalInstaller : Installer<InteractionSignalInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<InteractionSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<InteractionSignal>();
            Container.DeclareSignal<PlayerInteractionChangeSignal>();
        }
    }
}