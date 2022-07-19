using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Runtime.ContentLoader
{
    [CreateAssetMenu(fileName = "AssetMappings.asset", menuName = "Supremacy/Create Asset Mapping")]
    public class AssetMappings : ScriptableObject
    {
        public IReadOnlyDictionary<Guid, FactionMapping> FactionMappingByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, MechChassisMapping> MechChassisMappingByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, MechSkinMapping> MechSkinMappingByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, MysteryCrateMapping> MysteryCrateMappingByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, WeaponModelMapping> WeaponModelMappingByGuid { get; private set; }
        
        public IReadOnlyDictionary<Guid, WeaponSkinMapping> WeaponSkinMappingByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, PowerCoreMapping> PowerCoreMappingByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, UtilityModelMapping> UtilityModelMappingByGuid { get; private set; }
        public IReadOnlyDictionary<Guid, UtilitySkinMapping> UtilitySkinMappingByGuid { get; private set; }

        public List<FactionMapping> Factions => factions;
        public List<MechChassisMapping> MechChassis => mechChassis;
        public List<MechSkinMapping> MechSkins => mechSkins;
        public List<MysteryCrateMapping> MysteryCrates => mysteryCrates;
        public List<WeaponModelMapping> WeaponModels => weaponModels;
        public List<WeaponSkinMapping> WeaponSkins => weaponSkins;
        public List<PowerCoreMapping> PowerCores => powerCores;
        public List<UtilityModelMapping> UtilityModels => utilityModels;
        public List<UtilitySkinMapping> UtilitySkins => utilitySkins;

        [SerializeField] private List<FactionMapping> factions;
        [SerializeField] private List<MechChassisMapping> mechChassis;
        [SerializeField] private List<MechSkinMapping> mechSkins;
        [SerializeField] private List<MysteryCrateMapping> mysteryCrates;
        [SerializeField] private List<WeaponModelMapping> weaponModels;
        [SerializeField] private List<WeaponSkinMapping> weaponSkins;
        [SerializeField] private List<PowerCoreMapping> powerCores;
        [SerializeField] private List<UtilityModelMapping> utilityModels;
        [SerializeField] private List<UtilitySkinMapping> utilitySkins;

        //For some unity-lifecycle related reason, we can't do this as part of deserialization, which is why it's in enable.
        //Hopefully this is early enough for consumers of this data...
        public void OnEnable()
        {
            var hallwaysByGuid = new Dictionary<Guid, FactionMapping>();
            PopulateDictionary(hallwaysByGuid, factions, "factions", GetFactionMapping);
            FactionMappingByGuid = hallwaysByGuid;

            var mechChassisByGuid = new Dictionary<Guid, MechChassisMapping>();
            PopulateDictionary(mechChassisByGuid, mechChassis, "mechModels", GetMechMapping);
            MechChassisMappingByGuid = mechChassisByGuid;                
            
            var mechSkinByGuid = new Dictionary<Guid, MechSkinMapping>();
            PopulateDictionary(mechSkinByGuid, mechSkins, "mechSkins", GetSkinMapping);
            MechSkinMappingByGuid = mechSkinByGuid;

            var mysteryCrateByGuid = new Dictionary<Guid, MysteryCrateMapping>();
            PopulateDictionary(mysteryCrateByGuid, mysteryCrates, "mysteryCrates", GetMysteryCrateMapping);
            MysteryCrateMappingByGuid = mysteryCrateByGuid;
            
            var weaponModelByGuid = new Dictionary<Guid, WeaponModelMapping>();
            PopulateDictionary(weaponModelByGuid, weaponModels, "weaponModels", GetRecord);
            WeaponModelMappingByGuid = weaponModelByGuid;

            var weaponSkinByGuid = new Dictionary<Guid, WeaponSkinMapping>();
            PopulateDictionary(weaponSkinByGuid, weaponSkins, "weaponSkins", GetRecord);
            WeaponSkinMappingByGuid = weaponSkinByGuid;
            
            var powerCoresByGuid = new Dictionary<Guid, PowerCoreMapping>();
            PopulateDictionary(powerCoresByGuid, powerCores, "powerCores", GetRecord);
            PowerCoreMappingByGuid = powerCoresByGuid;

            var utilityModelsByGuid = new Dictionary<Guid, UtilityModelMapping>();
            PopulateDictionary(utilityModelsByGuid, utilityModels, "utilityModels", GetRecord);
            UtilityModelMappingByGuid = utilityModelsByGuid;

            var utilitySkinsByGuid = new Dictionary<Guid, UtilitySkinMapping>();
            PopulateDictionary(utilitySkinsByGuid, utilitySkins, "utilitySkins", GetRecord);
            UtilitySkinMappingByGuid = utilitySkinsByGuid;
        }

        private SupremacyData.Runtime.Faction GetFactionMapping(FactionMapping arg) => arg.DataFaction;

        private SupremacyData.Runtime.MechModel GetMechMapping(MechChassisMapping arg) => arg.DataMechModel;

        private SupremacyData.Runtime.MechSkin GetSkinMapping(MechSkinMapping arg) => arg.DataMechSkin;

        private SupremacyData.Runtime.MysteryCrate GetMysteryCrateMapping(MysteryCrateMapping arg) => arg.DataMysteryCrate;

        private SupremacyData.Runtime.BaseRecord GetRecord<TData, TRef>(MappingWithError<TData, TRef> arg)
            where TData : SupremacyData.Runtime.BaseRecord
            where TRef : AssetReference 
            => arg.Data;

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

                if (!dict.ContainsKey(record.Id))
                {
                    dict.Add(record.Id, mapping);
                    index++;
                }
                else Debug.LogWarning($"Record not added due to duplicate key {record.HumanName} at index {index}");
            }
        }
    }
}
