using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrefabNameGetter
{
    public GameObject GetOriginalName(GameObject go)
    {
        return PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
    }
}
