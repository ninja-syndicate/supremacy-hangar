using System;
using SupremacyHangar.Runtime.Silo;
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
            LoadingSilo,
            EmptySiloLoaded,
            LoadingSiloContent,
            Loaded,
            LoadedWithCrate,
            LoadingCrateContent,
        }

        [SerializeField] private int siloOffset;
        
        private SiloItem contents;
        
        [SerializeField] private StateName state;

        public event Action<StateName> OnStateChanged;

        public Transform SpawnLocation { get; set; }

        [Inject]
        public void Construct(SiloItem[] hallwayContents)
        {
            contents = hallwayContents[siloOffset];
        }

        public void UserInteraction()
        {
            var nextState = state;
            switch (CurrentState)
            {
                case StateName.NotLoaded:
                    nextState = StateName.LoadingSilo;
                    break;
                case StateName.LoadedWithCrate:
                    nextState = StateName.LoadingCrateContent;
                    break;
                default:
                    Debug.LogError($"Current state has no action {CurrentState}", this);
                    break;
            }

            if (nextState != state)
            {
                state = nextState;
                OnStateChanged?.Invoke(nextState);
            }
        }

        public void NextState(StateName nextState)
        {
            state = nextState; 
            OnStateChanged?.Invoke(nextState);
        }
    }
}