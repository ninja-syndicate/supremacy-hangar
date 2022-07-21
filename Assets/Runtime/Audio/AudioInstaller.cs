using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Audio
{
    public class AudioInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Debug.Log("Bindings Installed");
            Container.DeclareSignal<RoomTone.RoomTone>().OptionalSubscriber();
        }
    }
}