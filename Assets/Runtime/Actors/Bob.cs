using System;
using Unity.Mathematics;
using UnityEngine;

namespace SupremacyHangar.Runtime.Actors
{
    public class Bob : MonoBehaviour
    {
        [SerializeField] private float3 floatVector;
        [SerializeField] private float period;
        
        private float3 startPosition;

        public void Awake()
        {
            startPosition = transform.localPosition;
        }

        public void Update()
        {
            float multiplier = math.sin(Time.time / period * 2 * math.PI);
            float3 newPosition = startPosition + floatVector * multiplier;
            transform.localPosition = newPosition;
        }
    }
}