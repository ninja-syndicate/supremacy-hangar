using SupremacyHangar.Runtime.ContentLoader.Types;
using System;
using System.Collections.Generic;

namespace SupremacyHangar.Runtime.Types
{
    public class HangarData
    {
        public Guid faction;
        public AssetReferenceEnvironmentConnectivity factionGraph;
        public List<SiloItem> Silos = new();

        public void CopyFrom(HangarData other)
        {
            faction = other.faction;
            Silos = other.Silos;
        }
    }
}