using UnityEngine;

namespace SupremacyHangar.Scriptable
{
    public class Skin : ScriptableObject
    {
        [SerializeField]
        public Material[] mats;
    }
}