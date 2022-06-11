using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Zenject;

[assembly: InternalsVisibleTo("SupremacyData.Editor")]
namespace SupremacyData.Runtime
{
    public abstract class BaseData : ScriptableObject
    {
        public Guid Id => id;
        
        [SerializeField] internal Guid id;
    }

    public class BattleAbility : BaseData
    {
        
    }
    
    public class Brand : BaseData
    {
        public string HumanName => humanName;
        private Faction Faction => faction;

        [SerializeField] internal string humanName;
        [SerializeField] internal Faction faction;
    }
    
    public class Faction : BaseData
    {
        public string HumanName => humanName;

        [SerializeField] internal string humanName;
    }    
    
}