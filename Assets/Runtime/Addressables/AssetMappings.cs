using System;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyHangar.Runtime.Addressables
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
            foreach (var factionMapping in factions) hallwaysByGuid.Add(factionMapping.DataFaction.Id, factionMapping);
            FactionHallwayByGuid = hallwaysByGuid;
            
            var mechChassisByGuid = new Dictionary<Guid, MechChassisMapping>();
            foreach (var chassisMapping in mechChassis) mechChassisByGuid.Add(chassisMapping.DataMechModel.Id, chassisMapping);
            MechChassisPrefabByGuid = mechChassisByGuid;

            var mechSkinByGuid = new Dictionary<Guid, MechSkinMapping>();
            foreach (var skinMapping in mechSkins) mechSkinByGuid.Add(skinMapping.DataMechSkin.Id, skinMapping);
            MechSkinAssetByGuid = mechSkinByGuid;            
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