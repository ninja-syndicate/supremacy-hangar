using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor.Importers
{
    public class BattleAbilities : Base 
    {
        private static readonly string[] BattleAbilityHeaders =
        {   
            "id",
            "label",
            "cooldown_duration_second",
            "description",
        };
        public override string ImporterName => "Battle Abilities";
        protected override string Filename => "battle_abilities.csv";
        protected override string[] Headers => BattleAbilityHeaders;
        public BattleAbilities(ILogInterface logger, string staticDataDir) : base(logger, staticDataDir) { }
        
        protected override void SetupForImport(Runtime.Data data)
        {
            data.battleAbilities ??= new List<Runtime.BattleAbility>();
        }

        protected override void ProcessRecord(Runtime.Data data, int index, string[] fields)
        {
            if (!TryParseGuid(index, fields[0], "id", out var id)) return;
            
            var ability = data.battleAbilities.Find(x => x.Id == id);
            if (ability == null)
            {
                ability = ScriptableObject.CreateInstance<Runtime.BattleAbility>();
                AssetDatabase.AddObjectToAsset(ability, data);
                ability.Id = id;
                data.battleAbilities.Add(ability);
            }
            
            ability.humanName = fields[1];
            ability.name = $"BattleAbility - {ability.humanName}";
            if (TryParseInt(index, fields[2], "cooldown duration", out var intValue))
            {
                ability.cooldown = intValue;
            }

            ability.description = fields[3];
        }
    }
}