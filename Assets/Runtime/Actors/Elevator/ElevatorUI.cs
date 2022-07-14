using System;
using UnityEngine;
using UnityEngine.UI;

namespace SupremacyHangar.Runtime.Actors.Elevator
{
    public class ElevatorUI : MonoBehaviour
    {
        [SerializeField] private ElevatorMotor motor;

        [SerializeField] private Image motor;
        
        public void Awake()
        {
            SetupMotor();
        }
        
    }
}