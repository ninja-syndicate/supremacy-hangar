using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace SupremacyData.Editor.Importers
{
    public class Brands : Base
    {
        private static readonly string[] BrandHeaders =
        {   
            "id",
            "faction_id",
            "label",
            "deleted_at",
            "updated_at",
            "created_at",
        };

        public override string ImporterName => "Brands";

        protected override string Filename => "brands.csv";
        protected override string[] Headers => BrandHeaders;
        
        public Brands(ILogInterface logger, string staticDataDir) : base(logger, staticDataDir) { }

        protected override void SetupForImport(Runtime.Data data)
        {
            data.brands ??= new List<Runtime.Brand>();
        }
        
        protected override void ProcessRecord(Runtime.Data data, int index, string[] fields)
        {
            var id = ParseGuid(index, fields[0], "id");
            if (id == Guid.Empty) return;

            var factionId = ParseGuid(index, fields[1], "faction id");
            if (factionId == Guid.Empty) return;

            var faction = data.factions.Find(x => x.id == factionId);
            if (faction == null)
            {
                logger.LogError($"{ImporterName} data - could not find faction with ID {factionId} referenced by {dataPath}:{index}");
                return;
            }
            
            var brand = data.brands.Find(x => x.Id == id);
            if (brand == null)
            {
                brand = ScriptableObject.CreateInstance<Runtime.Brand>();
                AssetDatabase.AddObjectToAsset(brand, data);
                brand.id = id;
                data.brands.Add(brand);
            }

            brand.humanName = fields[2];
            brand.faction = faction;
            brand.name = $"Brand - {brand.humanName}";
        }
    }
}