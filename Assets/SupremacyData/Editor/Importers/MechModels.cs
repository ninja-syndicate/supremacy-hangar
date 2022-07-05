using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor.Importers
{
    public class MechModels : Base
    {
        private static readonly string[] MechModelHeaders =
        {   
            "id",
            "label",
            "created_at",
            "default_chassis_skin_id",
            "brand_id",
            "mech_type",
        };
        
        public MechModels(ILogInterface logger, string staticDataDir) : base(logger, staticDataDir) { }
        protected override string Filename => "mech_models.csv";
        protected override string[] Headers => MechModelHeaders;
        public override string ImporterName => "Mech Models";
        protected override void SetupForImport(Runtime.Data data)
        {
            data.mechModels ??= new List<Runtime.MechModel>();
        }

        protected override void ProcessRecord(Runtime.Data data, int index, string[] fields)
        {
            if (!TryParseGuid(index, fields[0], "id", out var id)) return;
            
            var mechModel = data.mechModels.Find(x => x.Id == id);
            if (mechModel == null)
            {
                mechModel = ScriptableObject.CreateInstance<Runtime.MechModel>();
                AssetDatabase.AddObjectToAsset(mechModel, data);
                mechModel.Id = id;
                data.mechModels.Add(mechModel);
            }

            mechModel.humanName = fields[1];
            mechModel.name = $"Mech Model - {mechModel.humanName}";
            mechModel.type = ParseType(index, fields[5]);
            
            if (string.IsNullOrWhiteSpace(fields[4])) return;

            if (!TryParseGuid(index, fields[4], "brand ID", out var brandId)) return;
            var brand = data.brands.Find(x => x.Id == brandId);
            if (brand == null)
            {
                logger.LogError($"{ImporterName} data - could not find brand with GUID {brandId} from {dataPath}:{index}");
            }
            else
            {
                mechModel.brand = brand;
            }
        }

        private Runtime.MechModel.ModelType ParseType(int index, string field)
        {
            switch (field.ToLowerInvariant())
            {
                case "humanoid":
                    return Runtime.MechModel.ModelType.Humanoid;
                case "platform":
                    return Runtime.MechModel.ModelType.Platform;
                default:
                    logger.LogError($"{ImporterName} data - unknown mech type {field} from {dataPath}:{index}");
                    break;
            }
            return Runtime.MechModel.ModelType.Humanoid;
        }
    }
}