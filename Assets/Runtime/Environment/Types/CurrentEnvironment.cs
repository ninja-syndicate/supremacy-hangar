using UnityEngine;

namespace SupremacyHangar.Runtime.Environment.Types
{
    public class CurrentEnvironment
    {
        public EnvironmentPart CurrentPart { get; set; } = null;
        public EnvironmentPartAddressable CurrentPrefabAsset { get; set; } = null;
        public GameObject CurrentGameObject { get; set; } = null;
    }
}
