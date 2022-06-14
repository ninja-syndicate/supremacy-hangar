using SupremacyHangar.Runtime.Environment.Connections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Runtime.Environment
{
    [CreateAssetMenu(fileName = "Settings/Environment", menuName = "Supremacy/Environment Connectivity Graph")]
    public class EnvironmentConnectivity : ScriptableObject
    {
        [SerializeField] private int initialSection = 0;
        [SerializeField] private List<ConnectivityJoin> requiredJoins;

        public List<ConnectivityJoin> RequiredJoins => requiredJoins;

        [SerializeField] private List<EnvironmentPartAddressable> parts = new();

        public Dictionary<ConnectivityNode, EnvironmentPartAddressable> MyJoins { get; private set; } = new();

        public EnvironmentPartAddressable GetInitialSection()
        {
            return parts[initialSection];
        }

        public void OnEnable()
        {
            if (parts == null) return;
            MyJoins.Clear();

            foreach (var part in parts)
            {
                MyJoins.Add(part.ReferenceName, part);
                part.InitJoins();
            }
        }
    }

    [Serializable]
    public class EnvironmentPartAddressable
    {
        [SerializeField] private ConnectivityNode referenceName;

        public ConnectivityNode ReferenceName => referenceName;

        public AssetReferenceGameObject Reference;

        [SerializeField] private List<PartJoin> joins = new();

        public Dictionary<ConnectivityJoin, PartJoin> MyJoinsByConnector = new();
        
        public void InitJoins()
        {
            if (joins == null) return;
            MyJoinsByConnector.Clear();

            foreach(var join in joins)
                MyJoinsByConnector.Add(join.Connector, join);
        }
    }

    [Serializable]
    public class PartJoin
    {
        public ConnectivityJoin Connector => connector;
        public List<ConnectivityNode> Destinations => destinations;

        [SerializeField] private ConnectivityJoin connector;

        [SerializeField] private List<ConnectivityNode> destinations;
    }
}