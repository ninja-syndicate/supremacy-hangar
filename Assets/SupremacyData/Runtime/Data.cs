using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("SupremacyData.Editor")]
namespace SupremacyData.Runtime
{
    public class Data : ScriptableObject
    {
//        public IReadOnlyList<BattleAbility> BattleAbilities { get; private set; }
        public IReadOnlyList<Brand> Brands { get; private set; }
        public IReadOnlyList<Faction> Factions { get; private set; }

//        [SerializeField] internal List<BattleAbility> battleAbilities = new();
        [SerializeField] internal List<Brand> brands = new();
        [SerializeField] internal List<Faction> factions = new();
        
        public void Awake()
        {
//            if (battleAbilities != null) BattleAbilities = battleAbilities.AsReadOnly();
            if (brands != null) Brands = brands.AsReadOnly();
            if (factions != null) Factions = factions.AsReadOnly();
        }
    }
    
    public abstract class BaseRecord : ScriptableObject
    {
        public Guid Id => id;
        
        [SerializeField] internal Guid id;
    }
}