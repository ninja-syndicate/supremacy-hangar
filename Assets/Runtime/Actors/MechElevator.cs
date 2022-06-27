using SupremacyData.Runtime;
using SupremacyHangar.Runtime.ContentLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityMath = Unity.Mathematics;
using Zenject;
using SupremacyHangar.Runtime.Silo;

namespace SupremacyHangar.Runtime.Actors
{

    //todo Change to dynamic elevator (calulated stops)
    public class MechElevator : ElevatorMovement
    {
        [Inject]
        public void Construct(BaseRecord targetData, PlatformStops platformStops)
        {
            nextStop = 1;
            speed = platformStops.Velocity;
            currentPos = platformStops.StopsList[targetData];

            stops = new[] { currentPos, platformStops.FinalStop };
            transform.localPosition = currentPos;
        }

        public override void Move(float deltaTime)
        {
            if (UnityMath.math.distancesq(currentPos, stops[nextStop]) < Mathf.Epsilon) return;
            base.Move(deltaTime);
        }
    }
}
