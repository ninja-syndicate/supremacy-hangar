using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using Zenject;

namespace SupremacyHangar.Runtime.Environment
{
    public class EnvironmentManager : MonoInstaller
    {
        [Inject] private DiContainer _container;
        
        [Inject]
        private RepositionSignalHandler _repositionSignalHandler;

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
            var hallLeft = _container.InstantiatePrefab(hallDoorway.Reference);
            var hallLeftEnvironmentPrefab = hallLeft.GetComponent<EnvironmentPrefab>();
            hallLeftEnvironmentPrefab.connectedTo = "Hallway-DoubleSilo(Clone)";
            hallLeftEnvironmentPrefab.JoinTo("ConnectionPoint_HallwayConnector_01", g.Joins["ConnectionPoint_HallwayConnector_01"]);

            var hallRight = _container.InstantiatePrefab(hallDoorway.Reference);
            var hallRightEnvironmentPrefab = hallRight.GetComponent<EnvironmentPrefab>();
            hallRightEnvironmentPrefab.connectedTo = "Hallway-DoubleSilo(Clone)";
            hallRightEnvironmentPrefab.JoinTo("ConnectionPoint_HallwayConnector_02", g.Joins["ConnectionPoint_HallwayConnector_02"]);

            loadedObjects.Add(hallRight);
            loadedObjects.Add(hallLeft);

            _container.InjectGameObject(currentEnvironment.currentGameObejct);
            //container.Bind<List<GameObject>>().FromInstance(loadedObjects).AsSingle();
            //container.InjectGameObject(hallLeft);
            //container.InjectGameObject(hallRight);
            //var hall = Instantiate(hallDoorway.Reference);


            //hall.JoinTo("ConnectionPoint_HallwayConnector_02", g.Joins["ConnectionPoint_HallwayConnector_02"]);
            //container.InjectGameObject(hall.gameObject);
        }
        private bool firstLoad = true;
        public void spawnPart(string myEnvironmentConnector, int environmentPrefabIndex, string to_Connect_to, EnvironmentPrefab myConnectors, Collider otherCollider, Animator[] doorAnimators)
        {
            interactedDoor = myConnectors;

            foreach (var obj in loadedObjects)
            {
                if(obj.GetInstanceID() != myConnectors.gameObject.GetInstanceID())
                    objectsToUnload.Add(obj);

            }

            _container.Bind<Animator[]>().FromInstance(doorAnimators);
            _container.Bind<Collider>().FromInstance(otherCollider);

            //Next Main Room spawn
            var roomPart = currentEnvironment.currentPart.MyJoins[myEnvironmentConnector][environmentPrefabIndex].MyJoins[myEnvironmentConnector][nextRoomIndex(myConnectors)];
            currentEnvironment nextRoom = new()
            {
                currentPart = roomPart,
                currentGameObejct = _container.InstantiatePrefab(roomPart.Reference).gameObject,
            };

            var nextRoomEnvironmentPrefabRef = nextRoom.currentGameObejct.GetComponent<EnvironmentPrefab>();

            loadedObjects.Add(nextRoom.currentGameObejct);
            //Reposition new room
            nextRoomEnvironmentPrefabRef.JoinTo(myEnvironmentConnector, myConnectors.Joins[myEnvironmentConnector]);
            
            var connectorDoor = _container.InstantiatePrefab(currentEnvironment.currentPart.MyJoins[myEnvironmentConnector][environmentPrefabIndex].Reference);
            loadedObjects.Add(connectorDoor.gameObject);

            //_container.InjectGameObject(nextRoom.currentGameObejct);

            //Reposition door & set connection point
            connectorDoor.GetComponent<EnvironmentPrefab>().JoinTo(myEnvironmentConnector, nextRoomEnvironmentPrefabRef.Joins[to_Connect_to]);
            connectorDoor.GetComponent<EnvironmentPrefab>().connectedTo = nextRoom.currentGameObejct.name;

            //update what current doors connected to
            interactedDoor.connectedTo = nextRoom.currentGameObejct.name;

            //Save for unload on going back to current room
            newlyLoadedObjects.Add(connectorDoor);
            newlyLoadedObjects.Add(nextRoom.currentGameObejct);

            if(firstLoad)
                _container.InjectGameObject(currentEnvironment.currentGameObejct);
            
            _container.Unbind<Collider>();
            _container.Unbind<Animator[]>();

            firstLoad = false;
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
                _repositionSignalHandler.repositionObject(gameObject.transform.position);

            if (currentEnvironment.currentGameObejct != gameObject)
            {
                currentEnvironment.currentGameObejct = gameObject;
                roomChanged = true;
            }
        }
    }
}