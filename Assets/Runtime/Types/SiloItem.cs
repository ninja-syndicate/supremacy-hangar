using SupremacyData.Runtime;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.ContentLoader.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Runtime.Types
{
    public enum SupremacyType
    {
        empty,
        mech,
        mystery_crate
    }

    public class SiloItem
    {
        public SupremacyType Type = SupremacyType.empty;
        public Guid ownership_id;
    }

    public class Mech : SiloItem
    {
        public Guid mech_id;
        public Guid skin_id;
        public MechChassisMapping MechChassisDetails;
        public MechSkinMapping MechSkinDetails;
    }

    public class MysteryBox : SiloItem
    {        
        public Guid mystery_crate_id;
        public string can_open_on;
        public MysteryCrateMapping MysteryCrateDetails;
    }
}