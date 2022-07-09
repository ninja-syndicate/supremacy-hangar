using System;
using Unity.Mathematics;

namespace SupremacyHangar.Runtime.Actors.Player
{
    public interface IPlayerController
    {
        public event Action OnInteractionTriggered;

        public float3 PlatformVelocity { get; set; }

        public void IncrementInteractionPromptRequests();
        public void DecrementInteractionPromptRequests();
    }
}