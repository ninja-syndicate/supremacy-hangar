using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using Zenject;

namespace SupremacyHangar.Runtime.Environment
{
    public class EnvironmentManager : MonoInstaller<EnvironmentManager>
    {
        [SerializeField] private List<EnvironmentPart> parts;

        private readonly Dictionary<string, EnvironmentPart> prefabsByName = new();

        public override void InstallBindings()
        {
            Container.Bind<EnvironmentManager>().FromInstance(this);
        }

        public void Awake()
        {
            foreach (var part in parts) prefabsByName[part.ReferenceName] = part;

            //TODO: replace this with json loader code and editor.
            var hallDoorway = prefabsByName["Hallway-Connector"];
            var hallSilo = prefabsByName["Hallway-Silo"];
            var hallJoinRoom = prefabsByName["Hallway-JoinRoom"];
            
            hallSilo.AddJoin("Connection-Hallway-Connector-1", new []{hallDoorway});
            hallSilo.AddJoin("doorway-connector-2", new []{hallDoorway});
            hallJoinRoom.AddJoin("Connection-Hallway-Connector-1", new []{hallDoorway});
            hallJoinRoom.AddJoin("doorway-connector-2", new []{hallDoorway});

            hallDoorway.AddJoin("Connection-Hallway-Connector-1", new []{hallSilo, hallJoinRoom});
            hallDoorway.AddJoin("doorway-connector-2", new []{hallSilo, hallJoinRoom});

            Instantiate(hallSilo.Reference);
        }
    }
}