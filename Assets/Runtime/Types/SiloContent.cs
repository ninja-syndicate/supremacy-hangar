using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyHangar.Runtime.Types
{
    public class SiloContent
    {
        public string type;
        public string chassisId;
        public string skinId;
        public string id;
        public string expires;

        public SiloContent()
        {
            type = null;
            id = null;
            expires = null;
            chassisId = null;
            skinId = null;
        }
    }

    public class Mech
    {
        public string chassisId;
        public string skinId;
    }

    public class LootBox
    {
        public string expires;
    }
}