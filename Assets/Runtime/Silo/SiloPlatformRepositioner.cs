using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Silo
{
    public class SiloPlatformRepositioner : MonoBehaviour
    {
        private SiloSignalHandler _siloSignalHandler;

        [Inject]
        public void Construct(SiloSignalHandler siloSignalHandler)
        {
            _siloSignalHandler = siloSignalHandler;
            _siloSignalHandler.RepositionPlatform(transform.position);
        }
    }
}
