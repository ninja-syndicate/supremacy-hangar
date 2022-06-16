using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor.Importers
{
    public class MechSkins : Base
    {
        private static readonly string[] MechSkinHeaders =
        {   
            "id",
            "collection",
            "mech_model",
            "label",
            "tier",
            "image_url",
            "animation_url",
            "card_animation_url",
            "large_image_url",
            "avatar_url",
            "created_at",
            "background_color",
            "youtube_url",
            "mech_type",
            "stat_modifier",
        };

        protected override string Filename => "mech_skins.csv";
        protected override string[] Headers => MechSkinHeaders;
        public override string ImporterName => "Mech Skins";

        public MechSkins(ILogInterface logger, string staticDataDir) : base(logger, staticDataDir) { }
        protected override void SetupForImport(Runtime.Data data)
        {
            data.mechSkins ??= new List<Runtime.MechSkin>();
        }

        protected override void ProcessRecord(Runtime.Data data, int index, string[] fields)
        {
            if (!TryParseGuid(index, fields[0], "id", out var id)) return;
            if (!TryParseGuid(index, fields[2], "mech model id", out var mechModelId)) return;

            var mechModel = data.mechModels.Find(x => x.Id == mechModelId);
            if (mechModel == null)
            {
                logger.LogError($"{ImporterName} data - could not find mech model with GUID {mechModelId} from {dataPath}:{index}");
                return;
            }
            
            var mechSkin = data.mechSkins.Find(x => x.Id == id);
            if (mechSkin == null)
            {
                mechSkin = ScriptableObject.CreateInstance<Runtime.MechSkin>();
                AssetDatabase.AddObjectToAsset(mechSkin, data);
                mechSkin.Id = id;
                data.mechSkins.Add(mechSkin);
            }

            mechSkin.mechModel = mechModel;
            mechSkin.humanName = fields[3];
            mechSkin.name = $"Mech Skin - {mechModel.humanName} - {mechSkin.humanName}";
        }
    }
}