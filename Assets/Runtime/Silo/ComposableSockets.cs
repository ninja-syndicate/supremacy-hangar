using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyHangar.Runtime.Silo
{
    public class ComposableSockets : MonoBehaviour
    {
        [SerializeField] public List<Transform> WeaponSockets = new();
        [SerializeField] public List<Transform> UtilitySockets = new();
        [SerializeField] public Transform PowerCoreSocket;
    }
}
