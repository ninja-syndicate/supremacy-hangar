using SupremacyHangar.Runtime.Interaction;
using SupremacyHangar.Runtime.Reposition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SignalInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);
        RepositionSignalInstaller.Install(Container);
        InteractionSignalInstaller.Install(Container);
    }
}
