using System;
using System.Collections.Generic;
using SupremacyData.Editor;
using SupremacyHangar.Runtime.ContentLoader.Types;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Runtime.ContentLoader
{
    [CreateAssetMenu(fileName = "AssetMappings.asset", menuName = "Supremacy/Create Asset Mapping")]
    public class AssetMappings : ScriptableObject
    {
        public IReadOnlyDictionary<Guid, FactionMapping> FactionHallwayByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, MechChassisMapping> MechChassisPrefabByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, MechSkinMapping> MechSkinAssetByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, MysteryCrateMapping> MysteryCrateAssetByGuid { get; private set; }

        public List<FactionMapping> Factions => factions;
        public List<MechChassisMapping> MechChassis => mechChassis;
        public List<MechSkinMapping> MechSkins => mechSkins;
        public List<MysteryCrateMapping> MysteryCrates => mysteryCrates;

        [SerializeField] private List<FactionMapping> factions;
        [SerializeField] private List<MechChassisMapping> mechChassis;
        [SerializeField] private List<MechSkinMapping> mechSkins;
        [SerializeField] private List<MysteryCrateMapping> mysteryCrates;

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

            var mysteryCrateByGuid = new Dictionary<Guid, MysteryCrateMapping>();
            PopulateDictionary(mysteryCrateByGuid, mysteryCrates, "mysteryCrates", GetMysteryCrateMapping);
            MysteryCrateAssetByGuid = mysteryCrateByGuid;
        }

        private SupremacyData.Runtime.Faction GetFactionMapping(FactionMapping arg) => arg.DataFaction;

        private SupremacyData.Runtime.MechModel GetMechMapping(MechChassisMapping arg) => arg.DataMechModel;

        private SupremacyData.Runtime.MechSkin GetSkinMapping(MechSkinMapping arg) => arg.DataMechSkin;

        private SupremacyData.Runtime.MysteryCrate GetMysteryCrateMapping(MysteryCrateMapping arg) => arg.DataMysteryCrate;
        
        private void PopulateDictionary<TMapping, TRecord>(
            Dictionary<Guid, TMapping> dict, List<TMapping> list, string typeName,
            Func<TMapping, TRecord> getRecord) where TRecord : SupremacyData.Runtime.BaseRecord
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

                if(!dict.ContainsKey(record.Id))
                { 
                    dict.Add(record.Id, mapping);
                    index++;
                }
            }
        }
    }
    
    [Serializable]
    public class FactionMapping
    {
        public bool ContainsError { get { return containsError; } set { containsError = value; } }
        [SerializeField] private bool containsError = false;

        public SupremacyData.Runtime.Faction DataFaction => dataFaction;
        public AssetReferenceEnvironmentConnectivity ConnectivityGraph => connectivityGraph; 
            
        [SerializeField][SerializeReference] private SupremacyData.Runtime.Faction dataFaction;
        [SerializeField] private AssetReferenceEnvironmentConnectivity connectivityGraph;
    }

    [Serializable]
    public class MechChassisMapping
    {
        public bool ContainsError { get { return containsError; } set { containsError = value; } }
        [SerializeField] private bool containsError = false;
        public SupremacyData.Runtime.MechModel DataMechModel => dataMechModel;
        public AssetReference MechReference => mechReference;
        
        [SerializeField][SerializeReference] private SupremacyData.Runtime.MechModel dataMechModel;
        [SerializeField] private AssetReference mechReference;

    }

    [Serializable]
    public class MechSkinMapping
    {
        public bool ContainsError { get { return containsError; } set { containsError = value; } }
        [SerializeField] private bool containsError = false;
        public SupremacyData.Runtime.MechSkin DataMechSkin => dataMechSkin;
        public AssetReferenceSkin SkinReference => skinReference;
        
        [SerializeField][SerializeReference] private SupremacyData.Runtime.MechSkin dataMechSkin;
        [SerializeField] private AssetReferenceSkin skinReference;
    }
    
    [Serializable]
    public class MysteryCrateMapping
    {

        public bool ContainsError { get { return containsError; } set { containsError = value; } }
        [SerializeField] private bool containsError = false;
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
