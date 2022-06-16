using System.Collections.Generic;

namespace SupremacyHangar.Runtime.Types
{
    public class SupremacyGameObject
    {
        public string faction;
        public List<SiloItem> Silos = new();

        public void CopyFrom(SupremacyGameObject other)
        {
            faction = other.faction;
            Silos = other.Silos;
        }
    }
}