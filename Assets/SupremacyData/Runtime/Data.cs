using System;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyData.Runtime
{
    public class Data : ScriptableObject
    {
        public IReadOnlyList<BattleAbility> BattleAbilities { get; private set; }
        public IReadOnlyList<Brand> Brands { get; private set; }
        public IReadOnlyList<Faction> Factions { get; private set; }

        [SerializeField] private List<BattleAbility> battleAbilities = new List<BattleAbility>();
        [SerializeField] private List<Brand> brands = new List<Brand>();
        [SerializeField] private List<Faction> factions = new List<Faction>();
        
        public void Awake()
        {
            if (battleAbilities != null) BattleAbilities = battleAbilities.AsReadOnly();
            if (brands != null) Brands = brands.AsReadOnly();
            if (factions != null) Factions = factions.AsReadOnly();
        }
    }
}