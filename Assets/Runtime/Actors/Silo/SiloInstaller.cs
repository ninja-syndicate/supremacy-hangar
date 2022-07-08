using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.Silo
{
    public class SiloInstaller : MonoInstaller
    {
        [SerializeField] private SiloState siloState;
        [SerializeField] private SiloAssetLoader siloAssetLoader;
        
        public override void InstallBindings()
        {
            Container.Bind<SiloState>().FromInstance(siloState);
            Container.Bind<SiloAssetLoader>().FromInstance(siloAssetLoader);
        }        
        
    }
}