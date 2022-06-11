using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Zenject;

[assembly: InternalsVisibleTo("SupremacyData.Editor")]
namespace SupremacyData.Runtime
{
    public class Data : ScriptableObject, ISerializationCallbackReceiver
    {
        public IReadOnlyCollection<BattleAbility> BattleAbilities { get; private set; }
        public IReadOnlyCollection<Brand> Brands { get; private set; }
        public IReadOnlyCollection<Faction> Factions { get; private set; }
        public IReadOnlyCollection<GameAbility> GameAbilities { get; private set; }
        public IReadOnlyCollection<MechModel> MechModels { get; private set; }
        public IReadOnlyCollection<MechSkin> MechSkins { get; private set; }

        [SerializeField] internal List<BattleAbility> battleAbilities = new();
        [SerializeField] internal List<Brand> brands = new();
        [SerializeField] internal List<Faction> factions = new();
        [SerializeField] internal List<GameAbility> gameAbilities = new();
        [SerializeField] internal List<MechModel> mechModels = new();
        [SerializeField] internal List<MechSkin> mechSkins = new();

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (battleAbilities != null) BattleAbilities = battleAbilities.AsReadOnly();
            if (brands != null) Brands = brands.AsReadOnly();
            if (factions != null) Factions = factions.AsReadOnly();
            if (gameAbilities != null) GameAbilities = gameAbilities.AsReadOnly();
            if (mechModels != null) MechModels = mechModels.AsReadOnly();
            if (mechSkins != null) MechSkins = mechSkins.AsReadOnly();
        }
    }
    
    public abstract class BaseRecord : ScriptableObject, ISerializationCallbackReceiver
    {
        public Guid Id { get; internal set; } = Guid.Empty;
        public string HumanName => humanName;
 
        [SerializeField] internal string humanName;
        [SerializeField] private byte[] idBytes;

        public virtual void OnBeforeSerialize()
        {
            if (Id == Guid.Empty) return;
            idBytes = Id.ToByteArray();
        }

        public virtual void OnAfterDeserialize()
        {
            Id = idBytes != null ? new Guid(idBytes) : Guid.Empty;
        }
    }
}