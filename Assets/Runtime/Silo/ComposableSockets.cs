using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyHangar.Runtime.Silo
{
    public class ComposableSockets : MonoBehaviour
    {
        [SerializeField] public List<Transform> WeaponSockets;
        [SerializeField] public List<Transform> UtilitySockets;
        [SerializeField] public Transform PowerCoreSocket;
    }
}
