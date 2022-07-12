using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor.Importers
{
    public class PowerCores : Base
    {
        private static readonly string[] CsvHeaders =
        {   
            "id",
            "collection",
            "label",
            "size",
            "capacity",
            "max_draw_rate",
            "recharge_rate",
            "armour",
            "max_hitpoints",
            "tier",
            "created_at",
            "image_url",
            "card_animation_url",
            "avatar_url",
            "large_image_url",
            "background_color",
            "animation_url",
            "youtube_url",
        };

        public override string ImporterName => "Power Cores";
        protected override string[] Headers => CsvHeaders;
        protected override string Filename => "power_cores.csv";
        
        public PowerCores(ILogInterface logger, string staticDataDir) : base(logger, staticDataDir) { }
        
        protected override void SetupForImport(Runtime.Data data)
        {
            data.powerCores ??= new List<Runtime.PowerCore>();
        }

        protected override void ProcessRecord(Runtime.Data data, int index, string[] fields)
        {
            if (!TryParseGuid(index, fields[0], "id", out var id)) return;

            var powerCore = data.powerCores.Find(x => x.Id == id);
            if (powerCore == null)
            {
                powerCore = ScriptableObject.CreateInstance<Runtime.PowerCore>();
                AssetDatabase.AddObjectToAsset(powerCore, data);
                powerCore.Id = id;
                data.powerCores.Add(powerCore);
            }
                
            powerCore.humanName = fields[2];
            powerCore.name = $"Power Core - {powerCore.humanName}";
        }
    }
}