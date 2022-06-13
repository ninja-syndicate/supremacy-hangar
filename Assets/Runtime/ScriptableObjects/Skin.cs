using UnityEngine;

namespace SupremacyHangar.Runtime.ScriptableObjects
{
    public class Skin : ScriptableObject
    {
        [SerializeField]
        public Material[] mats;
    }
}