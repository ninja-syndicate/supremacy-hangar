using System;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyHangar.Runtime.Environment.Types
{
    [Serializable]
    public class EnvironmentPart
    {
        [SerializeField] private string referenceName;

        public string ReferenceName => referenceName;
        
        [SerializeField] private EnvironmentPrefab reference;
        
        public EnvironmentPrefab Reference => reference;

        private Dictionary<string, List<EnvironmentPart>> joins = new();
        public IReadOnlyDictionary<string, List<EnvironmentPart>> MyJoins;

        public void AddJoin(string connectorName, EnvironmentPart[] destinations)
        {
            joins[connectorName] = new List<EnvironmentPart>(destinations);

            MyJoins = new Dictionary<string, List<EnvironmentPart>>(joins);
        }
    }

    struct DoorTuple
    {
        public EnvironmentSpawner Door1;
        public EnvironmentSpawner Door2;
    }

    public class CurrentEnvironment
    {
        public EnvironmentPart CurrentPart { get; set; } = null;
        public EnvironmentPartAddressable CurrentPrefabAsset { get; set; } = null;
        public GameObject CurrentGameObject { get; set; } = null;
    }
}