using SupremacyHangar.Runtime.ScriptableObjects;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Runtime.Environment
{
    [CreateAssetMenu(fileName = "Settings/Environment", menuName = "Supremacy/Environment Connectivity Graph")]
    public class EnvironmentConnectivity : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private int initialSection = 0;
        [SerializeField] private List<ConnectivityJoin> requiredJoins;

        public List<ConnectivityJoin> RequiredJoins => requiredJoins;

        [SerializeField] private List<EnvironmentPartAddressable> parts = new();

        public Dictionary<Guid, EnvironmentPartAddressable> MyJoins { get; private set; } = new();

        public EnvironmentPartAddressable GetInitialSection()
        {
            return parts[initialSection];
        }

        public void OnAfterDeserialize()
        {
            if (parts != null)
            {
                int n = parts.Count;
                MyJoins.Clear();
                for (int i = 0; i < n; ++i)
                {
                    MyJoins[parts[i].ReferenceName.Id] = parts[i];
                }
                //parts = null;
            }

            //if (requiredJoins != null)
            //{
            //    RequiredJoins.Clear();
            //    foreach (var part in requiredJoins)
            //        RequiredJoins.Add(part.Id);
            //}
        }

        public void OnBeforeSerialize() {}
    }

    [Serializable]
    public class EnvironmentPartAddressable : ISerializationCallbackReceiver
    {
        [SerializeField] private ConnectivityJoin referenceName;

        public ConnectivityJoin ReferenceName => referenceName;

        public AssetReferenceGameObject Reference;

        [SerializeField] private List<PartJoin> joins = new();

        public Dictionary<Guid, PartJoin> MyJoinsByConnector = new();

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            if (joins != null)
            {
                int n = joins.Count;
                MyJoinsByConnector.Clear();
                for (int i = 0; i < n; ++i)
                {
                    MyJoinsByConnector[joins[i].Connector.Id] = joins[i];
                }
            }
        }
    }

    [Serializable]
    public class PartJoin
    {
        public ConnectivityJoin Connector => connector;
        public List<ConnectivityJoin> Destinations => destinations;

        [SerializeField] private ConnectivityJoin connector;

        [SerializeField] private List<ConnectivityJoin> destinations;
    }
}