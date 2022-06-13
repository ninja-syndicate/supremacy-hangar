using System;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyHangar.Runtime.Environment
{
    public class EnvironmentPrefab : MonoBehaviour, ISerializationCallbackReceiver
    {
        private string prefabName;
        public string PrefabName => prefabName;
        
        [SerializeField] private List<Joiner> joins = new();

        private readonly Dictionary<string, Transform> joinsByName = new Dictionary<string, Transform>();

        public IReadOnlyDictionary<string, Transform> Joins;

        public GameObject connectedTo;

        public List<Collider> ColliderList  = new();

        public bool wasConnected { get; internal set; }

        public void ToggleDoor()
        {
            foreach (Collider c in ColliderList)
                c.enabled = !c.enabled;
        }

        private void Start()
        {
            prefabName = name.Replace("(Clone)", "").Trim();    
        }

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            foreach (var join in joins)
            {
                joinsByName[join.name] = join.position;
            }
            Joins = new Dictionary<string, Transform>(joinsByName);
        }

        [Serializable]
        public class Joiner
        {
            [SerializeField] public string name;
            [SerializeField] public Transform position;
        }

        public void JoinTo(string name, Transform otherHalf)
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