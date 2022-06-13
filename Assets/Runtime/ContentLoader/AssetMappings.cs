using System;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyHangar.Runtime.ContentLoader
{
    [CreateAssetMenu(fileName = "AssetMappings.asset", menuName = "Supremacy/Create Asset Mapping")]
    public class AssetMappings : ScriptableObject
    {
        //TODO: these should just return the actual destination objects, rather than the mappings.
        public IReadOnlyDictionary<Guid, FactionMapping> FactionHallwayByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, MechChassisMapping> MechChassisPrefabByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, MechSkinMapping> MechSkinAssetByGuid { get; private set; }

        [SerializeField] private List<FactionMapping> factions;
        [SerializeField] private List<MechChassisMapping> mechChassis;
        [SerializeField] private List<MechSkinMapping> mechSkins;

        //For some unity-lifecycle related reason, we can't do this as part of deserialization, which is why it's in enable.
        //Hopefully this is early enough for consumers of this data...
        public void OnEnable()
        {
            var hallwaysByGuid = new Dictionary<Guid, FactionMapping>();
            PopulateDictionary(hallwaysByGuid, factions, "factions", GetFactionMapping);
            FactionHallwayByGuid = hallwaysByGuid;

            var mechChassisByGuid = new Dictionary<Guid, MechChassisMapping>();
            PopulateDictionary(mechChassisByGuid, mechChassis, "mechModels", GetMechMapping);
            MechChassisPrefabByGuid = mechChassisByGuid;                
            
            var mechSkinByGuid = new Dictionary<Guid, MechSkinMapping>();
            PopulateDictionary(mechSkinByGuid, mechSkins, "mechSkins", GetSkinMapping);
            MechSkinAssetByGuid = mechSkinByGuid;
        }

        private SupremacyData.Runtime.Faction GetFactionMapping(FactionMapping arg)
        {
            return arg.DataFaction;
        }
        
        private SupremacyData.Runtime.MechModel GetMechMapping(MechChassisMapping arg)
        {
            return arg.DataMechModel;
        }
        
        private SupremacyData.Runtime.MechSkin GetSkinMapping(MechSkinMapping arg)
        {
            return arg.DataMechSkin;
        }

        private void PopulateDictionary<T,U>(Dictionary<Guid, T> dict, List<T> list, string typeName, Func<T,U> getRecord) where U: SupremacyData.Runtime.BaseRecord
        {
            if (list == null) return;

            int index = 0;
            foreach (var mapping in list)
            {
                var record = getRecord(mapping);
                if (record == null)
                {
                    Debug.LogError($"No static data set for {typeName} at index {index}", this);
                    index++;
                    continue;
                }
                dict.Add(record.Id, mapping);
                index++;
            }
        }
    }
    
    [Serializable]
    public class FactionMapping
    {
        public SupremacyData.Runtime.Faction DataFaction => dataFaction;
        
        [SerializeField][SerializeReference] private SupremacyData.Runtime.Faction dataFaction;
    }

    [Serializable]
    public class MechChassisMapping
    {
        public SupremacyData.Runtime.MechModel DataMechModel => dataMechModel;
        
        [SerializeField][SerializeReference] private SupremacyData.Runtime.MechModel dataMechModel;
        //TODO: add the environment connectivity graph here.
    }

    [Serializable]
    public class MechSkinMapping
    {
        public SupremacyData.Runtime.MechSkin DataMechSkin => dataMechSkin;
        
        [SerializeField][SerializeReference] private SupremacyData.Runtime.MechSkin dataMechSkin;
        //TODO: add the environment connectivity graph here.
    }    
    
}