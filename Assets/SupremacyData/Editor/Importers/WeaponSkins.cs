using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor.Importers
{
    public class WeaponSkins : Base
    {
        private static readonly string[] CsvHeaders =
        {   
            "id",
            "label",
            "weapon_type",
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

        protected override string Filename => "weapon_skins.csv";
        protected override string[] Headers => CsvHeaders;
        public override string ImporterName => "Weapon Skins";

        public WeaponSkins(ILogInterface logger, string staticDataDir) : base(logger, staticDataDir) { }
        protected override void SetupForImport(Runtime.Data data)
        {
            data.weaponSkins ??= new List<Runtime.WeaponSkin>();
        }

        protected override void ProcessRecord(Runtime.Data data, int index, string[] fields)
        {
            if (!TryParseGuid(index, fields[0], "id", out var id)) return;
            if (!TryParseGuid(index, fields[13], "weapon model id", out var weaponModelId)) return;

            var weaponModel = data.weaponModels.Find(x => x.Id == weaponModelId);
            if (weaponModel == null)
            {
                logger.LogError($"{ImporterName} data - could not find mech model with GUID {weaponModelId} from {dataPath}:{index}");
                return;
            }
            
            var weaponSkin = data.weaponSkins.Find(x => x.Id == id);
            if (weaponSkin == null)
            {
                weaponSkin = ScriptableObject.CreateInstance<Runtime.WeaponSkin>();
                AssetDatabase.AddObjectToAsset(weaponSkin, data);
                weaponSkin.Id = id;
                data.weaponSkins.Add(weaponSkin);
            }

            weaponSkin.weaponModel = weaponModel;
            weaponSkin.humanName = fields[1];
            if (!WeaponModels.ParseType(fields[2], out weaponSkin.type))
            {
                logger.LogError($"{ImporterName} data - unknown weapon type {fields[3]} from {dataPath}:{index}");
            }            
            weaponSkin.name = $"Weapon Skin - {weaponModel.humanName} - {weaponSkin.humanName}";
        }
    }
}