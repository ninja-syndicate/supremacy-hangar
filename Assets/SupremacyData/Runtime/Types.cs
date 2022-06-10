using System;
using UnityEngine;

namespace SupremacyData.Runtime
{
    [Serializable]
    public abstract class BaseData
    {
        public Guid Id => id;
        
        [SerializeField] private Guid id;
    }
    
    [Serializable]
    public class BattleAbility : BaseData
    {
        
    }

    [Serializable]
    public class Brand : BaseData
    {
        public string Name => Name;
        private Faction Faction => faction;

        private string name;
        private Faction faction;
    }
    
    [Serializable]
    public class Faction : BaseData
    {
        public string Name => Name;

        private string name;
    }    
    
}