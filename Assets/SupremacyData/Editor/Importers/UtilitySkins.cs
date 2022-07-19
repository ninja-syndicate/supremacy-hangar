using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor.Importers
{
    public class UtilitySkins : Base
    {
        private static readonly string[] CsvHeaders =
        {   
            "id",
            "label",
            "utility_type",
            "tier",
            "created_at",
            "image_url",
            "card_animation_url",
            "avatar_url",
            "large_image_url",
            "background_color",
            "animation_url",
            "youtube_url",
            "collection",
            "weapon_model_id",
            "stat_modifier",
        };

        protected override string Filename => "utility_skins.csv";
        protected override string[] Headers => CsvHeaders;
        public override string ImporterName => "Utility Skins";

        public UtilitySkins(ILogInterface logger, string staticDataDir) : base(logger, staticDataDir) { }
        protected override void SetupForImport(Runtime.Data data)
        {
            data.utilitySkins ??= new List<Runtime.UtilitySkin>();
        }

        protected override void ProcessRecord(Runtime.Data data, int index, string[] fields)
        {
            if (!TryParseGuid(index, fields[0], "id", out var id)) return;
            if (!TryParseGuid(index, fields[13], "utility model id", out var weaponModelId)) return;

            var weaponModel = data.utilityModels.Find(x => x.Id == weaponModelId);
            if (weaponModel == null)
            {
                logger.LogError($"{ImporterName} data - could not find utility skin with GUID {weaponModelId} from {dataPath}:{index}");
                return;
            }
            
            var utilitySkin = data.utilitySkins.Find(x => x.Id == id);
            if (utilitySkin == null)
            {
                utilitySkin = ScriptableObject.CreateInstance<Runtime.UtilitySkin>();
                AssetDatabase.AddObjectToAsset(utilitySkin, data);
                utilitySkin.Id = id;
                data.utilitySkins.Add(utilitySkin);
            }

            utilitySkin.utilityModel = weaponModel;
            utilitySkin.humanName = fields[1];
            if (!UtilityModels.ParseType(fields[2], out utilitySkin.type))
            {
                logger.LogError($"{ImporterName} data - unknown weapon type {fields[3]} from {dataPath}:{index}");
            }
            utilitySkin.name = $"Weapon Skin - {weaponModel.humanName} - {utilitySkin.humanName}";
        }
    }
}