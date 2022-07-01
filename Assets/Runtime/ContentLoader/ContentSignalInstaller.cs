using Zenject;

namespace SupremacyHangar.Runtime.ContentLoader
{
    public class InventoryLoadedSignal { }
    public class InventoryRecievedSignal { }

    public class ContentSignalHandler
    {
        readonly SignalBus _signalBus;

        public ContentSignalHandler(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void InventoryLoaded()
        {
            _signalBus.Fire<InventoryLoadedSignal>();
        }
        
        public void InventoryRecieved()
        {
            _signalBus.Fire<InventoryRecievedSignal>();
        }
    }

    public class ContentSignalInstaller : Installer<ContentSignalInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<ContentSignalHandler>().AsSingle().NonLazy();
            Container.DeclareSignal<InventoryLoadedSignal>();
            Container.DeclareSignal<InventoryRecievedSignal>().RunAsync();
        }
    }
}