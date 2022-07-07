using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar
{
    public class SiloHallwayConsoleButtonChangeSignal {
        public string Text;
    }

    public class SiloHallwayConsoleSignalHandler
    {
        readonly SignalBus _signalBus;

        public SiloHallwayConsoleSignalHandler(SignalBus bus)
        {
            _signalBus = bus;
        }

        public void ChangeButtonValue(string text)
        {
            _signalBus.Fire(new SiloHallwayConsoleButtonChangeSignal() { Text = text });
        }
    }

    public class SiloHallwayConsoleSignalInstaller : Installer<SiloHallwayConsoleSignalInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<SiloHallwayConsoleSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<SiloHallwayConsoleSignalInstaller>();
        }
    }
}
