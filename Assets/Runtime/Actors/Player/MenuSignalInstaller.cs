using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.Player
{
    public class PauseGameSignal { }
    public class ResumeGameSignal { }

    public class MenuSignalHandler
    {
        readonly SignalBus _signalBus;

        public MenuSignalHandler(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void PauseGame()
        {
            _signalBus.Fire<PauseGameSignal>();
        }

        public void ResumeGame()
        {
            _signalBus.Fire<ResumeGameSignal>();
        }
    }

    public class MenuSignalInstaller : Installer<MenuSignalInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<MenuSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<PauseGameSignal>().OptionalSubscriber();
            Container.DeclareSignal<ResumeGameSignal>().OptionalSubscriber();
        }
    }
}
