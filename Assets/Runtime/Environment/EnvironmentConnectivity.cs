using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Runtime.Environment
{
    [CreateAssetMenu(fileName = "Settings/Environment", menuName = "Supremacy/Environment Connectivity Graph")]
    public class EnvironmentConnectivity : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private List<EnvironmentPartAddressable> parts = new();

        public Dictionary<string, EnvironmentPartAddressable> MyJoins { get; private set; } = new();

        public void OnAfterDeserialize()
        {
            if (parts != null)
            {
                int n = parts.Count;
                MyJoins.Clear();
                for (int i = 0; i < n; ++i)
                {
                    MyJoins[parts[i].ReferenceName] = parts[i];
                }
                //parts = null;
            }
        }

        public void OnBeforeSerialize()
        {
        }
    }

    [Serializable]
    public class AssetReferenceEnvironmentPrefab : AssetReferenceT<EnvironmentPrefab>
    {
        public AssetReferenceEnvironmentPrefab(string guid) : base(guid) {}
    }

    [Serializable]
    public class EnvironmentPartAddressable : ISerializationCallbackReceiver
    {
        [SerializeField] private string referenceName;

        public string ReferenceName => referenceName;

        public EnvironmentPrefab Part => part;

        public AssetReferenceGameObject Reference;

        [SerializeField][SerializeReference] private EnvironmentPrefab part;

        [SerializeField] private List<PartJoin> joins = new();

        public Dictionary<string, PartJoin> MyJoinsByConnector = new();

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (joins != null)
            {
                int n = joins.Count;
                MyJoinsByConnector.Clear();
                for (int i = 0; i < n; ++i)
                {
                    MyJoinsByConnector[joins[i].Connector] = joins[i];
                }
            }
        }
    }

    [Serializable]
    public class PartJoin
    {
        public string Connector => connector;
        public List<EnvironmentPrefab> Destinations => destinations;

        [SerializeField] private string connector;

        [SerializeField][SerializeReference] private List<EnvironmentPrefab> destinations;
    }
}