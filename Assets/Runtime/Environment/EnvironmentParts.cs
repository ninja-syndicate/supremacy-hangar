using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Environment
{
    [CreateAssetMenu(fileName = "Settings/Environment", menuName = "Supremacy/Environment Loader", order = 0)]
    public class EnvironmentParts : ScriptableObjectInstaller
    {
        [SerializeField] private List<EnvironmentPrefab> parts;
    }
}