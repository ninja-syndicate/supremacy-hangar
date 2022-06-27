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
        public Guid OwnershipID;
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class Mech : SiloItem
    {
        public Guid MechID;
        public Guid SkinID;
        public MechChassisMapping MechChassisDetails;
        public MechSkinMapping MechSkinDetails;
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class MysteryBox : SiloItem
    {        
        public Guid MysteryCrateID;
        public DateTime CanOpenOn;
        public MysteryCrateMapping MysteryCrateDetails;
    }
}