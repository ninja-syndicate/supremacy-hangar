using SupremacyHangar.Runtime.ContentLoader.Types;
using System;
using System.Collections.Generic;

namespace SupremacyHangar.Runtime.Types
{
    public class SupremacyGameObject
    {
        public Guid faction;
        public AssetReferenceEnvironmentConnectivity factionGraph;
        public List<SiloItem> Silos = new();

        public void CopyFrom(SupremacyGameObject other)
        {
            faction = other.faction;
            Silos = other.Silos;
        }
    }
}