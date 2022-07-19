using System;
using SupremacyHangar.Runtime.ContentLoader.Types;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Runtime.ContentLoader
{
    [Serializable]
    public class MappingWithError
    {
        public bool ContainsError { 
            get => containsError;
            set => containsError = value;
        }
        
        [SerializeField] private bool containsError;
    }
    
    [Serializable]
    public class MappingWithError<TData, TRef> : MappingWithError 
        where TData : SupremacyData.Runtime.BaseRecord
        where TRef : AssetReference
    {
        public TData Data => data;
        public TRef Reference => reference; 
            
        [SerializeField][SerializeReference] private TData data;
        [SerializeField] private TRef reference;
    }
    
    [Serializable]
    public class FactionMapping : MappingWithError
    {
        public SupremacyData.Runtime.Faction DataFaction => dataFaction;
        public AssetReferenceEnvironmentConnectivity ConnectivityGraph => connectivityGraph; 
            
        [SerializeField][SerializeReference] private SupremacyData.Runtime.Faction dataFaction;
        [SerializeField] private AssetReferenceEnvironmentConnectivity connectivityGraph;
    }

    [Serializable]
    public class MechChassisMapping : MappingWithError
    {
        public SupremacyData.Runtime.MechModel DataMechModel => dataMechModel;
        public AssetReference MechReference => mechReference;
        
        [SerializeField][SerializeReference] private SupremacyData.Runtime.MechModel dataMechModel;
        [SerializeField] private AssetReference mechReference;

    }

    [Serializable]
    public class MechSkinMapping : MappingWithError
    {
        public SupremacyData.Runtime.MechSkin DataMechSkin => dataMechSkin;
        public AssetReferenceSkin SkinReference => skinReference;
        
        [SerializeField][SerializeReference] private SupremacyData.Runtime.MechSkin dataMechSkin;
        [SerializeField] private AssetReferenceSkin skinReference;
    }
    
    [Serializable]
    public class MysteryCrateMapping : MappingWithError
    {

        public SupremacyData.Runtime.MysteryCrate DataMysteryCrate => dataMysteryCrate;
        public AssetReference MysteryCrateReference => mysteryCrateReference;
        
        [SerializeField][SerializeReference] private SupremacyData.Runtime.MysteryCrate dataMysteryCrate;
        [SerializeField] private AssetReference mysteryCrateReference;
    }

    [Serializable]
    public class WeaponModelMapping : MappingWithError<SupremacyData.Runtime.WeaponModel, AssetReference> { }

    [Serializable]
    public class WeaponSkinMapping : MappingWithError<SupremacyData.Runtime.WeaponSkin, AssetReferenceSkin> { }

    [Serializable]
    public class PowerCoreMapping : MappingWithError<SupremacyData.Runtime.PowerCore, AssetReference> { }

    [Serializable]
    public class UtilityModelMapping : MappingWithError<SupremacyData.Runtime.UtilityModel, AssetReference> { }
    
    [Serializable]
    public class UtilitySkinMapping : MappingWithError<SupremacyData.Runtime.UtilitySkin, AssetReferenceSkin> { }
}