using SupremacyHangar.Runtime.Interaction;
using SupremacyHangar.Runtime.Environment;
using Zenject;
using SupremacyHangar.Runtime.Silo;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.Actors.Player;


namespace SupremacyHangar.Runtime
{
    public class SignalInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            RepositionSignalInstaller.Install(Container);
            InteractionSignalInstaller.Install(Container);
            SiloSignalInstaller.Install(Container);
            ContentSignalInstaller.Install(Container);
            CrateSignalInstaller.Install(Container);
			ProgressSignalInstaller.Install(Container);
            MenuSignalInstaller.Install(Container);
		}
    }
}