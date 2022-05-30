using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SupremacyHangar.Runtime.Environment
{
    [Serializable]
    public class EnvironmentPart
    {
        [SerializeField] private string referenceName;

        public string ReferenceName => referenceName;
        
        [SerializeField] private EnvironmentPrefab reference;
        
        public EnvironmentPrefab Reference => reference;

        private Dictionary<string, List<EnvironmentPart>> joins = new();

        public void AddJoin(string connectorName, EnvironmentPart[] destinations)
        {
            joins[connectorName] = new List<EnvironmentPart>(destinations);
        }
    }
}