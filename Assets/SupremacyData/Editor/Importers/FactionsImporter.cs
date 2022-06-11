using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SupremacyData.Runtime;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor.Importers
{
    public class Factions : Base
    {
        private static readonly string[] FactionHeaders =
            {   
                "id",
                "vote_price",
                "contract_reward",
                "label",
                "guild_id",
                "deleted_at",
                "updated_at",
                "created_at",
                "primary_color",
                "secondary_color",
                "background_color",
                "logo_url",
                "background_url",
                "description",
            };

        public override string ImporterName => "Factions";

        protected override string Filename => "factions.csv";
        protected override string[] Headers => FactionHeaders;        
        
        public Factions(ILogInterface logger, string staticDataDir) : base(logger, staticDataDir)
        {
        }
        
        protected override void SetupForImport(Data data)
        {
            data.factions ??= new List<Faction>();
        }

        protected override void ProcessRecord(Data data, int index, string[] fields)
        {
            var id = ParseGuid(index, fields[0], "id");
            if (id == Guid.Empty) return;

            var faction = data.factions.Find(x => x.Id == id);
            if (faction == null)
            {
                faction = ScriptableObject.CreateInstance<Faction>();
                AssetDatabase.AddObjectToAsset(faction, data);
                faction.id = id;
                data.factions.Add(faction);
            }
                
            faction.humanName = fields[3];
            faction.name = $"Faction - {faction.humanName}";
        }
    }
}