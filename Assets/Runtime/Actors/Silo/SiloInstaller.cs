using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.Silo
{
    public class SiloInstaller : MonoInstaller
    {
        [SerializeField] private SiloState siloState;
        
        public override void InstallBindings()
        {
            Container.Bind<SiloState>().FromInstance(siloState);
        }        
        
    }
}