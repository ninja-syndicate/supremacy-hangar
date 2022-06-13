using UnityEngine;
using Zenject;
using SupremacyHangar.Runtime.ContentLoader;

namespace SupremacyHangar.Runtime.Silo
{
    public class SiloFiller : MonoBehaviour
    {
        [SerializeField]
        private Transform mechSpawner;

        AddressablesManager _addressablesManager;

        [Inject]
        public void Construct(AddressablesManager addressablesManager)
        {
            _addressablesManager = addressablesManager;
        }

        private void Start()
        {
            _addressablesManager.SpawnMech(mechSpawner);
        }
    }
}