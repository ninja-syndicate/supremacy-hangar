using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Zenject;

namespace SupremacyHangar.Runtime.Silo
{
    public class SiloFiller : MonoBehaviour
    {
        [SerializeField]
        private Transform MechSpawner;

        AddressablesManager _addressablesManager;

        [Inject]
        public void Constrct(AddressablesManager addressablesManager)
        {
            _addressablesManager = addressablesManager;
        }

        private void Start()
        {
            _addressablesManager.spawnMech(MechSpawner);
        }
    }
}