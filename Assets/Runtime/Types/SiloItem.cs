using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public string ownership_id;
    }

    public class Mech : SiloItem
    {
        public string mech_id;
        public string skin_id;
    }

    public class MysteryBox : SiloItem
    {        
        public string can_open_on;
    }
}