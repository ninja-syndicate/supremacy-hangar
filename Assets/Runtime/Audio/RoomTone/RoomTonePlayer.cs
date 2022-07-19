using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Audio.RoomTone
{
    public class RoomTonePlayer : MonoBehaviour
    {
        private SignalBus bus;

        [Inject]
        public void InjectDependencies(SignalBus bus)
        {
            this.bus = bus;
            bus.Subscribe<RoomTone>(OnRoomToneChanged);
        }

        public void OnDisable()
        {
            if (bus != null)
            {
                bus.TryUnsubscribe<RoomTone>(OnRoomToneChanged);
            }
        }

        private void OnRoomToneChanged(RoomTone newRoomTone)
        {
            Debug.Log($"Change Room Tone to: {newRoomTone.name} with clip {newRoomTone.Clip.name}");
        }
    }
}