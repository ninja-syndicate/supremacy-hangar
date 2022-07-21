using SupremacyData.Runtime;
using SupremacyHangar.Runtime.ContentLoader;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SupremacyHangar.Runtime.Types
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class SiloItem
    {
        public Guid? OwnershipID;
        public Guid StaticID;
    }

    public class EmptySilo : SiloItem { }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class MysteryCrate : SiloItem
    {
        public bool Openable => DateTime.UtcNow >= CanOpenOn;
        public DateTime CanOpenOn;

        public MysteryCrateMapping MysteryCrateDetails;
    }    
    
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class Mech : SiloItem
    {
        public Skin Skin;
        public SiloItem[] Accessories;

        public MechChassisMapping MechChassisDetails;
        public MechSkinMapping MechSkinDetails;
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class Weapon : SiloItem
    {
        public Skin Skin;
        public WeaponModelMapping WeaponModelDetails;
        public WeaponSkinMapping WeaponSkinDetails;
    }    

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class PowerCore : SiloItem
    {
        public PowerCoreMapping PowerCoreDetails;
    }        
    
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class Skin : SiloItem
    {        
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class Utility : SiloItem
    {
        //public UtilityMapping UtilityDetails;
    }
}