using SupremacyData.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityMath = Unity.Mathematics;

namespace SupremacyHangar.Runtime.Silo
{
    [Serializable]
    public class PlatformPosition
    {
        public BaseRecord MechType => mechType;
        [SerializeField] private BaseRecord mechType;

        public UnityMath.float3 InitialStop => initialStop;
        [SerializeField] private UnityMath.float3 initialStop;
    }

    public class PlatformStops : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private PlatformPosition[] stopsArray;

        public UnityMath.float3 FinalStop => finalStop;
        [SerializeField] private UnityMath.float3 finalStop;

        public IReadOnlyDictionary<BaseRecord, UnityMath.float3> StopsList;
        private Dictionary<BaseRecord, UnityMath.float3> stops = new();

        public float Velocity => speed;
        [FormerlySerializedAs("velocity"), SerializeField] private float speed = 2f;

        public void OnAfterDeserialize()
        {
            foreach (var stop in stopsArray)
            {
                if (stop.MechType)
                    stops[stop.MechType] = stop.InitialStop;
            }
            StopsList = new Dictionary<BaseRecord, UnityMath.float3>(stops);
        }

        public void OnBeforeSerialize() {}
    }
}
