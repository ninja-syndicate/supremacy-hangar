using System;
using UnityEngine;
using UnityEngine.UI;

namespace SupremacyHangar.Runtime.Actors.Elevator
{
    [Serializable]
    public class LevelIndicator
    {
        public bool Valid => Hilighted != null && Unhilighted != null;

        public GameObject Hilighted;
        public GameObject Unhilighted;
    }
    
    public class ElevatorUI : MonoBehaviour
    {
        //TODO: this needs to be changed to ElevatorMotor or something, so we could use it on any elevator
        [SerializeField] private PlayerElevator motor;

        [SerializeField] private LevelIndicator[] levelIndicators;
        
        public void Awake()
        {
            SetupMotor();
            SetupLevelIndicators();
        }

        public void OnEnable()
        {
            if (motor == null) return;
            motor.OnStopChanged += SetIndicator;
        }

        public void Start()
        {
            SetIndicator(motor.CurrentStop);
        }

        public void OnDisable()
        {
            if (motor == null) return;
            motor.OnStopChanged -= SetIndicator;
        }

        private void SetIndicator(int motorCurrentStop)
        {
            for (int index = 0; index < levelIndicators.Length; index++)
            {
                levelIndicators[index].Hilighted.SetActive(index == motorCurrentStop);
                levelIndicators[index].Unhilighted.SetActive(index != motorCurrentStop);
            }
        }

        private void SetupMotor()
        {
            if (motor != null) return;
            if (TryGetComponent(out motor)) return;
            Debug.LogError("Motor is not set, or on this gameobject", this);
            enabled = false;
        }

        private void SetupLevelIndicators()
        {
            for (int index = 0; index < levelIndicators.Length; index++)
            {
                if (levelIndicators[index] != null)
                {
                    if (levelIndicators[index].Valid) continue;

                    Debug.LogError($"Level indicator at index {index} is not valid - check that it has everything assigned", this);
                    enabled = false;
                }
                else
                {
                    Debug.LogError($"Level indicator at index {index} is null", this);
                    enabled = false;
                }
            }

            if (motor == null)
            {
                Debug.LogError($"Motor is null, can't check levelIndicator length", this);
                return;
            }

            if (levelIndicators.Length != motor.Stops.Length)
            {
                if (levelIndicators.Length < motor.Stops.Length)
                {
                    Debug.LogError($"Not enough level indicators", this);
                    enabled = false;
                }
                else
                {
                    Debug.LogError($"Motor has {motor.Stops.Length} but we only have {levelIndicators.Length} level indicators", this);
                }
            }
        }
    }
}