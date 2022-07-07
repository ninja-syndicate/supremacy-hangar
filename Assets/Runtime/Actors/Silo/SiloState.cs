using System;
using SupremacyHangar.Runtime.Types;
using UnityEngine;
using Zenject;

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

        [SerializeField] private int siloOffset;
        
        private SiloItem contents;
        
        [SerializeField] private StateName state;

        public event Action<StateName> OnStateChanged;
        
        [Inject]
        public void Construct(SiloItem[] hallwayContents)
        {
            contents = hallwayContents[siloOffset];
        }
    }
}