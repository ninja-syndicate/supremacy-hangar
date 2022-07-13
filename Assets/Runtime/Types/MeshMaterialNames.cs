using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyHangar
{
    [Serializable]
    public class MeshMaterialNameMap
    {
        public GameObject Mesh;
        public List<string> MaterialNames;
    }

    public class MeshMaterialNames : ScriptableObject
    {
        public List<MeshMaterialNameMap> MaterialNameMap;
    }
}
