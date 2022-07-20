using SupremacyHangar.Runtime.Interaction;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Audio.RoomTone
{
    public class RoomToneChanger : InteractionZoneResponder
    {
        [SerializeField] private RoomTone roomTone;
        private SignalBus bus;

        public override void Awake()
        { 
            base.Awake();
            SetupRoomTone();
        }

        [Inject]
        public void InjectDependencies(SignalBus bus)
        {
            this.bus = bus;
        }

        
        public override void OnPlayerExited()
        {
        }

        public override void OnPlayerEntered(GameObject go, FirstPersonController controller)
        {
            bus.Fire(roomTone);
        }
        
        private void SetupRoomTone()
        {
            if (roomTone != null) return;
            enabled = false;
            Debug.LogError("Roomtone attribute not set!", this);
        }
       
    }
}