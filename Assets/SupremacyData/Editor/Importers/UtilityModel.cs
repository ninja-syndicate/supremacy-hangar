using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SupremacyData.Editor.Importers
{
    public class UtilityModels : Base
    {
        private static readonly string[] CsvHeaders =
        {
            "id",
            "brand_id",
            "label",
            "utility_type",
            "default_skin_id",
            "deleted_at",
            "updated_at",
            "created_at",
        };

        public UtilityModels(ILogInterface logger, string staticDataDir) : base(logger, staticDataDir) { }
        protected override string Filename => "utility_models.csv";
        protected override string[] Headers => CsvHeaders;
        public override string ImporterName => "Utility Models";
        protected override void SetupForImport(Runtime.Data data)
        {
            data.weaponModels ??= new List<Runtime.WeaponModel>();
        }

        protected override void ProcessRecord(Runtime.Data data, int index, string[] fields)
        {
            if (!TryParseGuid(index, fields[0], "id", out var id)) return;

            var utilityModel = data.utilityModels.Find(x => x.Id == id);
            if (utilityModel == null)
            {
                utilityModel = ScriptableObject.CreateInstance<Runtime.UtilityModel>();
                AssetDatabase.AddObjectToAsset(utilityModel, data);
                utilityModel.Id = id;
                data.utilityModels.Add(utilityModel);
            }

            utilityModel.humanName = fields[2];
            utilityModel.name = $"Utility Model - {utilityModel.humanName}";
            if (!ParseType(fields[3], out utilityModel.type))
            {
                logger.LogError($"{ImporterName} data - unknown utility type {fields[3]} from {dataPath}:{index}");
            }

            if (string.IsNullOrWhiteSpace(fields[1])) return;

            if (!TryParseGuid(index, fields[1], "brand ID", out var brandId)) return;
            var brand = data.brands.Find(x => x.Id == brandId);
            if (brand == null)
            {
                logger.LogError($"{ImporterName} data - could not find brand with GUID {brandId} from {dataPath}:{index}");
            }
            else
            {
                utilityModel.brand = brand;
            }
        }

        internal static bool ParseType(string field, out Runtime.UtilityModel.ModelType type)
        {
            type = Runtime.UtilityModel.ModelType.ShieldGenerator;
            switch (field.ToLowerInvariant())
            {
                case "shield generator":
                    type = Runtime.UtilityModel.ModelType.ShieldGenerator;
                    return true;
                case "accelorator":
                    type = Runtime.UtilityModel.ModelType.Accelorator;
                    return true;
                default:
                    return false;
            }
        }

    }
}