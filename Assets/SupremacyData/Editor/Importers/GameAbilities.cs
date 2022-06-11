using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor.Importers
{
    public class GameAbilities: Base 
    {
        private static readonly string[] GameAbilityHeaders =
        {   
            "id",
            "game_client_ability_id",
            "faction_id",
            "battle_ability_id",
            "label",
            "colour",
            "image_url",
            "sups_cost",
            "description",
            "text_colour",
            "current_sups",
            "level",
        };
        
        protected override string Filename => "game_abilities.csv";
        protected override string[] Headers => GameAbilityHeaders;
        public override string ImporterName => "Game Abilities";

        public GameAbilities(ILogInterface logger, string staticDataDir) : base(logger, staticDataDir) { }
        
        protected override void SetupForImport(Runtime.Data data)
        {
            data.gameAbilities ??= new List<Runtime.GameAbility>();
        }

        protected override void ProcessRecord(Runtime.Data data, int index, string[] fields)
        {
            if (!TryParseGuid(index, fields[0], "id", out var id)) return;
            if (!TryParseGuid(index, fields[2], "faction ID", out var factionId)) return;

            var faction = data.factions.Find(x => x.Id == factionId);
            if (faction == null)
            {
                logger.LogError($"{ImporterName} data - could not find faction with GUID {factionId} from {dataPath}:{index}");
                return;
            }
            
            var gameAbility = data.gameAbilities.Find(x => x.Id == id);
            if (gameAbility == null)
            {
                gameAbility = ScriptableObject.CreateInstance<Runtime.GameAbility>();
                AssetDatabase.AddObjectToAsset(gameAbility, data);
                gameAbility.Id = id;
                data.gameAbilities.Add(gameAbility);
            }

            gameAbility.humanName = fields[4];
            gameAbility.name = $"Game Ability - {gameAbility.humanName}";

            gameAbility.faction = faction;
            gameAbility.description = fields[8];
            gameAbility.imageURL = fields[6];
            if (TryParseColor(index, fields[5], "color", out var color))
            {
                gameAbility.color = color;
            }

            if (TryParseColor(index, fields[9], "text color", out color))
            {
                gameAbility.textColor = color;
            }

            if (!string.IsNullOrWhiteSpace(fields[3]))
            {
                if (TryParseGuid(index, fields[3], "battle ability ID", out var battleAbilityId))
                {
                    var battleAbility = data.battleAbilities.Find(x => x.Id == battleAbilityId);
                    if (battleAbility == null)
                    {
                        logger.LogError($"{ImporterName} data - could not find battle ability with GUID {battleAbilityId} from {dataPath}:{index}");
                    }
                    else
                    {
                        gameAbility.battleAbility = battleAbility;
                    }
                }
            }

            switch (fields[11].ToLowerInvariant())
            {
                case "mech":
                    gameAbility.level = Runtime.GameAbility.AbilityLevel.Mech;
                    break;
                case "faction":
                    gameAbility.level = Runtime.GameAbility.AbilityLevel.Faction;
                    break;
                default:
                    logger.LogError($"{ImporterName} data - unknown level {fields[11]} from {dataPath}:{index}");
                    break;
            }
        }
    }
}