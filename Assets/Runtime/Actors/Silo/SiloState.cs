using SupremacyHangar.Runtime.Types;
using UnityEngine;

namespace SupremacyHangar.Runtime.Actors.Silo
{
    public class SiloState : MonoBehaviour
    {
        public StateName CurrentState => state;
        public SiloItem Contents => contents;
        
        public enum StateName
        {
            NotLoaded,
            Loading,
            Loaded,
            LoadedWithCrate,
        }

        private SiloItem contents;
        private StateName state;
    }
}