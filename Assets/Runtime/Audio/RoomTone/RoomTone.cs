using UnityEngine;

namespace SupremacyHangar.Runtime.Audio.RoomTone
{
    [CreateAssetMenu(fileName = "NewRoomTone", menuName = "Supremacy/Audio/Create Room Tone", order = 0)]
    public class RoomTone : ScriptableObject
    {
        public AudioClip Clip => clip;

        [SerializeField] private AudioClip clip;
    }
}