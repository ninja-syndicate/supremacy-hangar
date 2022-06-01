using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime
{
    public class repositioner : MonoInstaller
    {
    //    private repositionManager _manager;
        
    //    [Inject]
    //    public void IntallBindings(repositionManager repositionManager)
    //    {
    //        _manager = repositionManager;
    //    }

        public void moveToZero()
        {
            Debug.Log("move");
            transform.position = Vector3.zero;
        }
    }
}
