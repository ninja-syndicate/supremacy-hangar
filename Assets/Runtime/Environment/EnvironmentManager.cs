using SupremacyHangar.Runtime.Reposition;
using SupremacyHangar.Runtime.Silo;
using SupremacyHangar.Runtime.Scriptable;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System;

namespace SupremacyHangar.Runtime.Environment
{
    public class EnvironmentManager : MonoInstaller
    {
        [Inject] private DiContainer _container;
        
        [Inject]
        private RepositionSignalHandler _repositionSignalHandler;

        [Inject]
        private SupremacyGameObject _playerInventory;

        [SerializeField] private List<EnvironmentPart> parts;

        private readonly Dictionary<string, EnvironmentPart> prefabsByName = new();

        public currentEnvironment currentEnvironment = new();

        public List<GameObject> loadedObjects = new();
        public List<GameObject> objectsToUnload = new();
        public List<GameObject> newlyLoadedObjects = new();
        private bool roomChanged = false;

        private EnvironmentPrefab interactedDoor = null;

        private SiloPositioner _currentSilo;
        private EnvironmentPrefab newDoor;
        public int SiloOffset { get; private set; } = 0;

        public int MaxSiloOffset { get; private set; }
        private bool forward = true;
        private currentEnvironment nextRoom;

        public bool SiloExists { get; private set; }

        public override void InstallBindings()
        {
            MaxSiloOffset = _playerInventory.silos.Length - 1;

            if (MaxSiloOffset < 0)
                MaxSiloOffset++;

            Container.Bind<EnvironmentManager>().FromInstance(this);

            Container.Bind<SiloContent[]>().FromInstance(getCurrentSiloInfo()).AsCached();
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
            currentEnvironment.currentGameObject = g.gameObject;
            loadedObjects.Add(currentEnvironment.currentGameObject);

            //Store current environment
            //Inject to environment spawners
            var hallLeft = _container.InstantiatePrefab(hallDoorway.Reference);
            var hallLeftEnvironmentPrefab = hallLeft.GetComponent<EnvironmentPrefab>();
            hallLeftEnvironmentPrefab.connectedTo = currentEnvironment.currentGameObject;
            hallLeftEnvironmentPrefab.JoinTo("ConnectionPoint_HallwayConnector_01", g.Joins["ConnectionPoint_HallwayConnector_01"]);

            //Disable door colliders
            toggleDoor(hallLeftEnvironmentPrefab);

            var hallRight = _container.InstantiatePrefab(hallDoorway.Reference);
            var hallRightEnvironmentPrefab = hallRight.GetComponent<EnvironmentPrefab>();
            hallRightEnvironmentPrefab.connectedTo = currentEnvironment.currentGameObject;
            hallRightEnvironmentPrefab.JoinTo("ConnectionPoint_HallwayConnector_02", g.Joins["ConnectionPoint_HallwayConnector_02"]);

            if(MaxSiloOffset <= 1)
                toggleDoor(hallRightEnvironmentPrefab);

            loadedObjects.Add(hallRight);
            loadedObjects.Add(hallLeft);

            _container.InjectGameObject(currentEnvironment.currentGameObject);
        }

        public void toggleDoor(EnvironmentPrefab door)
        {
            foreach (Collider c in door.ColliderList)
                c.enabled = !c.enabled;
        }

        public void spawnPart(string myEnvironmentConnector, string to_Connect_to, EnvironmentPrefab myConnectors, Animator doorAnimator)
        {
            interactedDoor = myConnectors; 

            foreach (var obj in loadedObjects)
            {
                if(obj.GetInstanceID() != myConnectors.gameObject.GetInstanceID())
                    objectsToUnload.Add(obj);

            }
            _container.Bind<Animator>().FromInstance(doorAnimator);

            //Next Main Room spawn
            var roomPart = currentEnvironment.currentPart.MyJoins[myEnvironmentConnector][0].MyJoins[myEnvironmentConnector][NextRoomIndex(myConnectors)];
            nextRoom = new()
            {
                currentPart = roomPart,
                currentGameObject = _container.InstantiatePrefab(roomPart.Reference).gameObject,
            };

            interactedDoor.wasConnected = true;
            var nextRoomEnvironmentPrefabRef = nextRoom.currentGameObject.GetComponent<EnvironmentPrefab>();

            loadedObjects.Add(nextRoom.currentGameObject);
            //Reposition new room
            nextRoomEnvironmentPrefabRef.JoinTo(myEnvironmentConnector, myConnectors.Joins[myEnvironmentConnector]);

            var connectorDoor = _container.InstantiatePrefab(currentEnvironment.currentPart.MyJoins[myEnvironmentConnector][0].Reference);
            loadedObjects.Add(connectorDoor.gameObject);

            newDoor = connectorDoor.GetComponent<EnvironmentPrefab>();
            //Reposition door & set connection point
            newDoor.JoinTo(myEnvironmentConnector, nextRoomEnvironmentPrefabRef.Joins[to_Connect_to]);
            newDoor.connectedTo = nextRoom.currentGameObject;

            if(SiloOffset == 0 || SiloOffset >= MaxSiloOffset)
                toggleDoor(newDoor);

            //update what current doors connected to
            interactedDoor.connectedTo = nextRoom.currentGameObject;

            //Save for unload on going back to current room (MAY NEED TO MOVE BACK UP)
            newlyLoadedObjects.Add(connectorDoor);
            newlyLoadedObjects.Add(nextRoom.currentGameObject);

            //Injects animations for closing into active room
            _container.InjectGameObject(currentEnvironment.currentGameObject);
            _container.Unbind<Animator>();
        }

        public void spawnSilo(string myEnvironmentConnector, int environmentPrefabIndex, SiloPositioner currentSilo)
        {
            _currentSilo = currentSilo;
            SiloExists = true;

            var nextRoomEnvironmentPrefabRef = currentEnvironment.currentGameObject.GetComponent<EnvironmentPrefab>();

            var silo = _container.InstantiatePrefab(currentEnvironment.currentPart.MyJoins[myEnvironmentConnector][environmentPrefabIndex].Reference);
            loadedObjects.Add(silo.gameObject);
            newlyLoadedObjects.Add(silo.gameObject);

            //Reposition door & set connection point
            silo.GetComponent<EnvironmentPrefab>().JoinTo(myEnvironmentConnector, nextRoomEnvironmentPrefabRef.Joins[myEnvironmentConnector]);
        }

        public void ChangeDirection(bool direction)
        {
            forward = direction;
        }

        private void changeSiloOffset()
        {
            if (interactedDoor.wasConnected == false)
            {
                if (forward)
                {
                    SiloOffset += 2;
                }
                else
                {
                    SiloOffset -= 2;
                }
            }
            reBindSiloInfo();
        }

        private SiloContent[] getCurrentSiloInfo()
        {
            if (MaxSiloOffset == 0)
                return new SiloContent[] { new(), new() };

            if (SiloOffset + 1 >= MaxSiloOffset)
                return new[] { _playerInventory.silos[SiloOffset], new() };

            return new[] { _playerInventory.silos[SiloOffset], _playerInventory.silos[SiloOffset + 1] };
        }

        private void reBindSiloInfo()
        {
            Container.Rebind<SiloContent[]>().FromInstance(getCurrentSiloInfo()).AsCached();
        }

        private int NextRoomIndex(EnvironmentPrefab myConnectors)
        {
            if (myConnectors.connectedTo.name == "Hallway-DoubleSilo(Clone)")
            {
                return 1;
            }

            changeSiloOffset();
            return 0;
        }

        public void unloadAssets()
        {

            if (_currentSilo)
                _currentSilo.SiloSpawned = false;

            if (newDoor)
            {
                if (newDoor.connectedTo.name != "Hallway-SmallStraightJoin(Clone)" && SiloOffset > 0 && SiloOffset < MaxSiloOffset)
                    toggleDoor(newDoor);
                else if (newDoor.connectedTo.name == "Hallway-SmallStraightJoin(Clone)")
                    toggleDoor(newDoor);
            }

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
                    interactedDoor.connectedTo = currentEnvironment.currentGameObject;
                
                foreach (var obj in newlyLoadedObjects)
                {
                    loadedObjects.Remove(obj);
                    Destroy(obj);
                }
                newlyLoadedObjects.Clear();
                objectsToUnload.Clear();
            }
            SiloExists = false;
        }

        public void resetConnection(bool commited = false)
        {
            if (interactedDoor)
            {
                //Debug.Log("Reset connection");
                if (interactedDoor.connectedTo == currentEnvironment.currentGameObject)
                    interactedDoor.connectedTo = nextRoom.currentGameObject;
                else if (commited == false)
                    interactedDoor.connectedTo = currentEnvironment.currentGameObject;
            }
        }

        public void setCurrentEnvironment(GameObject gameObject)
        {
            if (gameObject.name == "Hallway-SmallStraightJoin(Clone)" && gameObject.transform.position != Vector3.zero)
                _repositionSignalHandler.repositionObject(gameObject.transform.position);

            if (currentEnvironment.currentGameObject != gameObject)
            {
                currentEnvironment.currentGameObject = gameObject;
                roomChanged = true;
            }
        }
    }
}