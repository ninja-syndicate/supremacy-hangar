using SupremacyHangar.Runtime.Interaction;
using SupremacyHangar.Runtime.Environment;
using Zenject;
using SupremacyHangar.Runtime.Silo;

public class SignalInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);
        RepositionSignalInstaller.Install(Container);
        InteractionSignalInstaller.Install(Container);
        SiloSignalInstaller.Install(Container);
    }
}
