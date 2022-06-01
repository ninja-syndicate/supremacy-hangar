using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using Zenject;

namespace SupremacyHangar.Runtime.Environment
{
    public class EnvironmentManager : MonoInstaller
    {
        [Inject]
        private repositionSignalHandler _repositionSignalHandler;

        [SerializeField] private List<EnvironmentPart> parts;

        private readonly Dictionary<string, EnvironmentPart> prefabsByName = new();

        public currentEnvironment currentEnvironment { get; set; } = new();

        public List<GameObject> loadedObjects = new();
        public List<GameObject> objectsToUnload = new();
        public List<GameObject> newlyLoadedObjects = new();
        private bool roomChanged = false;

        private EnvironmentPrefab interactedDoor = null;

        public override void InstallBindings()
        {
            Container.Bind<EnvironmentManager>().FromInstance(this);
        }

        public void Awake()
        {
            foreach (var part in parts) prefabsByName[part.ReferenceName] = part;

            //TODO: replace this with json loader code and editor.
            var hallDoorway = prefabsByName["Hallway-Connector"];
            var hallDoubleSilo = prefabsByName["Hallway-DoubleSilo"];
            var hallJoinRoom = prefabsByName["Hallway-SmallStraightJoin"];
            var siloRoomLeft = prefabsByName["Hallway - Silo_Left"];
            var siloRoomRight = prefabsByName["Hallway - Silo_Right"];

            //Initialize public connector references
            hallDoorway.Reference.initialize();
            hallDoubleSilo.Reference.initialize();
            hallJoinRoom.Reference.initialize();
            siloRoomLeft.Reference.initialize();
            siloRoomRight.Reference.initialize();

            //Create Joins (connectivity graph)
            siloRoomRight.AddJoin("ConnectionPoint_Silo_01", new[] { hallDoubleSilo });
            siloRoomLeft.AddJoin("ConnectionPoint_Silo_02", new[] { hallDoubleSilo });

            hallDoorway.AddJoin("ConnectionPoint_HallwayConnector_01", new[] { hallDoubleSilo, hallJoinRoom });
            hallDoorway.AddJoin("ConnectionPoint_HallwayConnector_02", new[] { hallDoubleSilo, hallJoinRoom });

            hallDoubleSilo.AddJoin("ConnectionPoint_HallwayConnector_01", new[] { hallDoorway });
            hallDoubleSilo.AddJoin("ConnectionPoint_HallwayConnector_02", new[] { hallDoorway });
            hallDoubleSilo.AddJoin("ConnectionPoint_Silo_02", new[] { siloRoomLeft });
            hallDoubleSilo.AddJoin("ConnectionPoint_Silo_01", new[] { siloRoomRight });

            hallJoinRoom.AddJoin("ConnectionPoint_HallwayConnector_01", new[] { hallDoorway });
            hallJoinRoom.AddJoin("ConnectionPoint_HallwayConnector_02", new[] { hallDoorway });

            //Spawn initial environment
            var g = Instantiate(hallDoubleSilo.Reference);
            currentEnvironment.currentPart = hallDoubleSilo;
            currentEnvironment.currentGameObejct = g.gameObject;
            loadedObjects.Add(currentEnvironment.currentGameObejct);

            //Store current environment
            //Inject to environment spawners
            DiContainer container = new DiContainer();
            container.Bind<EnvironmentManager>().FromInstance(this).AsSingle();
            var hallLeft = container.InstantiatePrefab(hallDoorway.Reference);
            hallLeft.GetComponent<EnvironmentPrefab>().connectedTo = "Hallway-DoubleSilo(Clone)";

            var hallRight = container.InstantiatePrefab(hallDoorway.Reference);
            hallRight.GetComponent<EnvironmentPrefab>().connectedTo = "Hallway-DoubleSilo(Clone)";

            loadedObjects.Add(hallRight);
            loadedObjects.Add(hallLeft);

            container.InjectGameObject(currentEnvironment.currentGameObejct);
            //container.Bind<List<GameObject>>().FromInstance(loadedObjects).AsSingle();
            //container.InjectGameObject(hallLeft);
            //container.InjectGameObject(hallRight);
            //var hall = Instantiate(hallDoorway.Reference);

            hallRight.GetComponent<EnvironmentPrefab>().JoinTo("ConnectionPoint_HallwayConnector_02", g.Joins["ConnectionPoint_HallwayConnector_02"]);

            //hall.JoinTo("ConnectionPoint_HallwayConnector_02", g.Joins["ConnectionPoint_HallwayConnector_02"]);
            //container.InjectGameObject(hall.gameObject);
        }

        public void spawnPart(string myEnvironmentConnector, int environmentPrefabIndex, string to_Connect_to, EnvironmentPrefab myConnectors, Collider otherCollider)
        {
            interactedDoor = myConnectors;

            foreach (var obj in loadedObjects)
            {
                if(obj.GetInstanceID() != myConnectors.gameObject.GetInstanceID())
                    objectsToUnload.Add(obj);

            }

            DiContainer container = new DiContainer();
            //Next Main Room spawn
            var roomPart = currentEnvironment.currentPart.MyJoins[myEnvironmentConnector][environmentPrefabIndex].MyJoins[myEnvironmentConnector][nextRoomIndex(myConnectors)];
            currentEnvironment nextRoom = new();
            nextRoom.currentPart = roomPart;
            container.Bind<Collider>().FromInstance(otherCollider).AsSingle();
            var nR = Instantiate(roomPart.Reference);
            nextRoom.currentGameObejct = nR.gameObject;

            loadedObjects.Add(nextRoom.currentGameObejct);
            //Reposition new room
            nR.GetComponent<EnvironmentPrefab>().JoinTo(myEnvironmentConnector, myConnectors.Joins[myEnvironmentConnector]);

            container.Bind<EnvironmentManager>().FromInstance(this).AsSingle();

            var connectorDoor = container.InstantiatePrefab(currentEnvironment.currentPart.MyJoins[myEnvironmentConnector][environmentPrefabIndex].Reference);
            loadedObjects.Add(connectorDoor.gameObject);

            container.InjectGameObject(nextRoom.currentGameObejct);

            //Reposition door & set connection point
            connectorDoor.GetComponent<EnvironmentPrefab>().JoinTo(myEnvironmentConnector, nR.GetComponent<EnvironmentPrefab>().Joins[to_Connect_to]);
            connectorDoor.GetComponent<EnvironmentPrefab>().connectedTo = nR.gameObject.name;

            //update what current doors connected to
            interactedDoor.connectedTo = nR.gameObject.name;

            //Save for unload on going back to current room
            newlyLoadedObjects.Add(connectorDoor);
            newlyLoadedObjects.Add(nextRoom.currentGameObejct);
        }
        private int nextRoomIndex(EnvironmentPrefab myConnectors)
        {
            if (myConnectors.connectedTo == "Hallway-DoubleSilo(Clone)")
                return 1;

            return 0;
        }

        public void unloadAssets()
        {
            if (roomChanged == true)
            {
                foreach (var obj in objectsToUnload)
                {
                    loadedObjects.Remove(obj);
                    Destroy(obj);
                }
                objectsToUnload.Clear();
                newlyLoadedObjects.Clear();
                roomChanged = false;
            }
            else
            {
                if(interactedDoor)
                    interactedDoor.connectedTo = currentEnvironment.currentGameObejct.name;
                
                foreach (var obj in newlyLoadedObjects)
                {
                    loadedObjects.Remove(obj);
                    Destroy(obj);
                }
                newlyLoadedObjects.Clear();
                objectsToUnload.Clear();
            }
        }

        public void setCurrentEnvironment(GameObject gameObject)
        {
            if (gameObject.name == "Hallway-SmallStraightJoin(Clone)" && gameObject.transform.position != Vector3.zero)
                _repositionSignalHandler.saveRelativePosition();

            if (currentEnvironment.currentGameObejct != gameObject)
            {
                currentEnvironment.currentGameObejct = gameObject;
                roomChanged = true;
            }
        }
    }
}