using SupremacyHangar.Runtime.Scriptable;
using SupremacyHangar.Runtime.Silo;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace SupremacyHangar.Runtime.Interaction
{
    public class InteractionZone : MonoBehaviour
    {
        private bool hasCollided = false;

        [SerializeField]
        private Text labelText;

        [SerializeField]
        private SiloPositioner spawner;

        private PlayerInput _playerInput;

        [Inject]
        private AddressablesManager _addressablesManager;
        
        [Inject]
        private SiloContent[] _siloContent;

        [Inject]
        private SupremacyDictionary _supremacyDictionary;

        [SerializeField]
        private int siloIndex = 0;

        private SignalBus _bus;

        [Inject]
        public void Constuct(SignalBus bus)
        {
            _bus = bus;
            _bus.Subscribe<InteractionSignal>(OnInteraction);
        }

        private void OnDisable()
        {
            _bus.Unsubscribe<InteractionSignal>(OnInteraction);
        }

        private void OnInteraction(InteractionSignal signal)
        {
            if (hasCollided == false) return;
            Debug.Log(signal.message);
            //Check message then do corresponding task
            fillSilo();
        }

        private void fillSilo()
        {
            //Todo: accept loot boxes
            if (_siloContent[siloIndex].type.Contains("mech"))
            {
                _addressablesManager.targetMech = _supremacyDictionary.mechDictionary[_siloContent[siloIndex].chassisId];
                _addressablesManager.targetSkin = _supremacyDictionary.AllSkinsDictionary[_siloContent[siloIndex].chassisId][_siloContent[siloIndex].skinId];
            }
            else
            {
                _addressablesManager.targetMech = _supremacyDictionary.lootBoxDictionary[_siloContent[siloIndex].id];
                _addressablesManager.targetSkin = null;
            }
            spawner.spawnSilo();
        }

        private void OnTriggerEnter(Collider other)
        {
            labelText.enabled = true;
            hasCollided = true;
        }

        private void OnTriggerExit(Collider other)
        {
            labelText.enabled = false;
            hasCollided = false;
        }
    }
}