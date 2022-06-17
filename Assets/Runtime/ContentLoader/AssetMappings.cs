using System;
using System.Collections.Generic;
using SupremacyHangar.Runtime.Environment;
using SupremacyHangar.Runtime.ScriptableObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Runtime.ContentLoader
{
    [CreateAssetMenu(fileName = "AssetMappings.asset", menuName = "Supremacy/Create Asset Mapping")]
    public class AssetMappings : ScriptableObject
    {
        public IReadOnlyDictionary<Guid, EnvironmentConnectivity> FactionHallwayByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, AssetReference> MechChassisPrefabByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, AssetReference> MechSkinAssetByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, AssetReference> MysteryCrateAssetByGuid { get; private set; }

        [SerializeField] private List<FactionMapping> factions;
        [SerializeField] private List<MechChassisMapping> mechChassis;
        [SerializeField] private List<MechSkinMapping> mechSkins;
        [SerializeField] private List<MysteryCrateMapping> mysteryCrates;

        //For some unity-lifecycle related reason, we can't do this as part of deserialization, which is why it's in enable.
        //Hopefully this is early enough for consumers of this data...
        public void OnEnable()
        {
            var hallwaysByGuid = new Dictionary<Guid, EnvironmentConnectivity>();
            PopulateDictionary(hallwaysByGuid, factions, "factions", GetFactionMapping);
            FactionHallwayByGuid = hallwaysByGuid;

            var mechChassisByGuid = new Dictionary<Guid, AssetReference>();
            PopulateDictionary(mechChassisByGuid, mechChassis, "mechModels", GetMechMapping);
            MechChassisPrefabByGuid = mechChassisByGuid;                
            
            var mechSkinByGuid = new Dictionary<Guid, AssetReference>();
            PopulateDictionary(mechSkinByGuid, mechSkins, "mechSkins", GetSkinMapping);
            MechSkinAssetByGuid = mechSkinByGuid;

            var mysteryCrateByGuid = new Dictionary<Guid, AssetReference>();
            PopulateDictionary(mechSkinByGuid, mysteryCrates, "mysteryCrates", GetMysteryCrateMapping);
            MysteryCrateAssetByGuid = mechSkinByGuid;
        }

        private (SupremacyData.Runtime.Faction, EnvironmentConnectivity) GetFactionMapping(FactionMapping arg)
        {
            return (arg.DataFaction, arg.ConnectivityGraph);
        }
        
        private (SupremacyData.Runtime.MechModel, AssetReference) GetMechMapping(MechChassisMapping arg)
        {
            return (arg.DataMechModel, arg.MechReference);
        }
        
        private (SupremacyData.Runtime.MechSkin, AssetReference) GetSkinMapping(MechSkinMapping arg)
        {
            return (arg.DataMechSkin, arg.SkinReference);
        }
        
        private (SupremacyData.Runtime.MysteryCrate, AssetReference) GetMysteryCrateMapping(MysteryCrateMapping arg)
        {
            return (arg.DataMysteryCrate, arg.MysteryCrateReference);
        }

        private void PopulateDictionary<TMapping, TValue, TRecord>(
            Dictionary<Guid, TValue> dict, List<TMapping> list, string typeName, 
            Func<TMapping,(TRecord, TValue)> getRecord) where TRecord: SupremacyData.Runtime.BaseRecord
        {
            if (list == null) return;

            int index = 0;
            foreach (var mapping in list)
            {
                var tuple = getRecord(mapping);
                if (tuple.Item1 == null)
                {
                    Debug.LogError($"No static data set for {typeName} at index {index}", this);
                    index++;
                    continue;
                }

                if (tuple.Item2 == null)
                {
                    Debug.LogError($"No mapping data set for {typeName} at index {index}", this);
                    index++;
                    continue;                    
                }
                
                dict.Add(tuple.Item1.Id, tuple.Item2);
                index++;
            }
        }
    }
    
    [Serializable]
    public class FactionMapping
    {
        public SupremacyData.Runtime.Faction DataFaction => dataFaction;
        public EnvironmentConnectivity ConnectivityGraph => connectivityGraph; 
            
        [SerializeField][SerializeReference] private SupremacyData.Runtime.Faction dataFaction;
        [SerializeField][SerializeReference] private EnvironmentConnectivity connectivityGraph;
    }

    [Serializable]
    public class MechChassisMapping
    {
        public SupremacyData.Runtime.MechModel DataMechModel => dataMechModel;
        public AssetReference MechReference => mechReference;
        
        [SerializeField][SerializeReference] private SupremacyData.Runtime.MechModel dataMechModel;
        [SerializeField] private AssetReference mechReference;

    }

    [Serializable]
    public class MechSkinMapping
    {
        public SupremacyData.Runtime.MechSkin DataMechSkin => dataMechSkin;
        public AssetReference SkinReference => skinReference;
        
        [SerializeField][SerializeReference] private SupremacyData.Runtime.MechSkin dataMechSkin;
        [SerializeField] private AssetReference skinReference;
    }
    
    [Serializable]
    public class MysteryCrateMapping
    {
        public SupremacyData.Runtime.MysteryCrate DataMysteryCrate => dataMysteryCrate;
        public AssetReference MysteryCrateReference => mysteryCrateReference;
        
        [SerializeField][SerializeReference] private SupremacyData.Runtime.MysteryCrate dataMysteryCrate;
        [SerializeField] private AssetReference mysteryCrateReference;
    }
 
    [Serializable]
    public class AssetReferenceAssetMappings : AssetReferenceT<AssetMappings>
    {
        public AssetReferenceAssetMappings(string guid) : base(guid) { }
    }
}
