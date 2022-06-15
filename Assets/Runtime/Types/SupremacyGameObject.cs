using System.Collections.Generic;

namespace SupremacyHangar.Runtime.Types
{
    public class SupremacyGameObject
    {
        public string faction;
        public List<SiloItem> Silos = new();
    }
}