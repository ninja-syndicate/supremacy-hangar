using SupremacyHangar.Runtime.Environment.Connections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyHangar.Runtime.Environment
{
    public class EnvironmentPrefab : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField] private ConnectivityNode prefabName;
        public ConnectivityNode PrefabName => prefabName;
        
        [SerializeField] private List<Joiner> joins = new();

        private readonly Dictionary<ConnectivityJoin, Transform> joinsByName = new Dictionary<ConnectivityJoin, Transform>();

        public IReadOnlyDictionary<ConnectivityJoin, Transform> Joins;

        public GameObject connectedTo;

        public List<Collider> ColliderList { get; set; } = new();

        public bool wasConnected { get; internal set; }

        public void ToggleDoor()
        {
            foreach (Collider c in ColliderList)
                c.enabled = !c.enabled;
        }

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            foreach (var join in joins)
            {
                joinsByName[join.name] = join.position;
            }
            Joins = new Dictionary<ConnectivityJoin, Transform>(joinsByName);
        }

        [Serializable]
        public class Joiner
        {
            [SerializeField] public ConnectivityJoin name;
            [SerializeField] public Transform position;
        }

        public void JoinTo(ConnectivityJoin name, Transform otherHalf)
        {
            if (!joinsByName.TryGetValue(name, out var connection)) {
                Debug.LogError($"Could not find connection called {name}", this);
                return;
            }
            var delta = otherHalf.position - connection.position;

            transform.position += delta;
        }
    }
}