using System.Collections.Generic;
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
        
        protected override void SetupForImport(Runtime.Data data)
        {
            data.factions ??= new List<Runtime.Faction>();
        }

        protected override void ProcessRecord(Runtime.Data data, int index, string[] fields)
        {
            if (!TryParseGuid(index, fields[0], "id", out var id)) return;

            var faction = data.factions.Find(x => x.Id == id);
            if (faction == null)
            {
                faction = ScriptableObject.CreateInstance<Runtime.Faction>();
                AssetDatabase.AddObjectToAsset(faction, data);
                faction.Id = id;
                data.factions.Add(faction);
            }
                
            faction.humanName = fields[3];
            if (TryParseColor(index, fields[8], "primary color", out var color))
            {
                faction.primaryColor = color;
            }
            if (TryParseColor(index, fields[9], "secondary color", out color))
            {
                faction.secondaryColor = color;
            }
            if (TryParseColor(index, fields[10], "background color", out color))
            {
                faction.backgroundColor = color;
            }

            faction.logoURL = fields[11];
            faction.backgroundURL = fields[12];
            faction.description = fields[13];

            faction.name = $"Faction - {faction.humanName}";
        }
    }
}