using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor.Importers
{
    public class MysteryCrates : Base
    {
        private static readonly string[] MysteryCrateHeaders =
        {   

            "id",
            "mystery_crate_type",
            "price",
            "amount",
            "amount_sold",
            "faction_id",
            "deleted_at",
            "updated_at",
            "created_at",
            "label",
            "description",
            "image_url",
            "card_animation_url",
            "avatar_url",
            "large_image_url",
            "background_color",
            "animation_url",
            "youtube_url",
        };

        protected override string Filename => "mystery_crates.csv";
        protected override string[] Headers => MysteryCrateHeaders;
        public override string ImporterName => "Mystery Crates";

        public MysteryCrates(ILogInterface logger, string staticDataDir) : base(logger, staticDataDir) { }
        protected override void SetupForImport(Runtime.Data data)
        {
            data.mysteryCrates ??= new List<Runtime.MysteryCrate>();
        }

        protected override void ProcessRecord(Runtime.Data data, int index, string[] fields)
        {
            if (!TryParseGuid(index, fields[0], "id", out var id)) return;

            var mysteryCrate = data.mysteryCrates.Find(x => x.Id == id);
            if (mysteryCrate == null)
            {
                mysteryCrate = ScriptableObject.CreateInstance<Runtime.MysteryCrate>();
                AssetDatabase.AddObjectToAsset(mysteryCrate, data);
                mysteryCrate.Id = id;
                data.mysteryCrates.Add(mysteryCrate);
            }

            mysteryCrate.humanName = fields[9];
            mysteryCrate.name = $"Mystery Crate Model - {mysteryCrate.humanName}";

            switch (fields[1].ToLowerInvariant())
            {
                case "weapon":
                    mysteryCrate.type = Runtime.MysteryCrate.ModelType.Weapon;
                    break;
                case "mech":
                    mysteryCrate.type = Runtime.MysteryCrate.ModelType.Mech;
                    break;
                default:
                    logger.LogError($"{ImporterName} data - unknown mystery crate type {fields[1]} from {dataPath}:{index}");
                    break;
            }

            if (string.IsNullOrWhiteSpace(fields[5])) return;

            if (!TryParseGuid(index, fields[5], "faction ID", out var factionID)) return;
            var faction = data.factions.Find(x => x.Id == factionID);
            if (faction == null)
            {
                logger.LogError($"{ImporterName} data - could not find faction with GUID {factionID} from {dataPath}:{index}");
            }
            else
            {
                mysteryCrate.faction = faction;
            }
        }
    }
}