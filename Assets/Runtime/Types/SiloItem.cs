using SupremacyData.Runtime;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.ContentLoader.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
    }    

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class PowerCore : SiloItem
    {
    }        
    
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class Skin : SiloItem
    {        
    }
}