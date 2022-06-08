using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyHangar.Runtime.Scriptable
{
    public class SiloContent
    {
        public string type;
        public string id;
        public string chassisId;
        public string skinId;
        public string expires;

        public SiloContent()
        {
            type = null;
            id = null;
            chassisId = null;
            skinId = null;
            expires = null;
        }
    }
}