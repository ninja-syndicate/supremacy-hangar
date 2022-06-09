using SupremacyHangar.Runtime.Silo;
using SupremacyHangar.Runtime.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System;
using SupremacyHangar.Runtime.Types;
using SupremacyHangar.Runtime.Environment.Types;
using UnityEngine.AddressableAssets;
using SupremacyHangar.Runtime.ContentLoader.Types;

namespace SupremacyHangar.Runtime.Environment
{
    public class EnvironmentManager : MonoInstaller
    {
        [Inject] private DiContainer _container;
        
        [Inject]
        private RepositionSignalHandler _repositionSignalHandler;

        [Inject]
        private SupremacyGameObject _playerInventory;

        [Inject]
        private SupremacyDictionary _supremacyDictionary;

        private EnvironmentConnectivity _connectivityGraph;

        [SerializeField] private List<EnvironmentPart> parts;

        private readonly Dictionary<string, EnvironmentPart> prefabsByName = new();

        public CurrentEnvironment currentEnvironment = new();

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
        private CurrentEnvironment nextRoom;

        public bool SiloExists { get; private set; }

        public override void InstallBindings()
        {
            MaxSiloOffset = _playerInventory.Silos.Length - 1;

            if (MaxSiloOffset < 0)
                MaxSiloOffset++;

            Container.Bind<EnvironmentManager>().FromInstance(this);

            Container.Bind<SiloContent[]>().FromInstance(GetCurrentSiloInfo()).AsCached();
        }

        private void Awake()
        {
            AssetReference connectivityGraph = _supremacyDictionary.FactionDictionary[_playerInventory.faction];

            connectivityGraph.LoadAssetAsync<EnvironmentConnectivity>().Completed += (obj) =>
            {
                _connectivityGraph = obj.Result;
                SpawnInitialHallway();
            };
        }

        private bool initLoadComplete = false;
        private void SpawnInitialHallway()
        {
            //Spawn initial environment
            currentEnvironment.CurrentPrefabAsset = _connectivityGraph.MyJoins["Hallway-DoubleSilo"];
            _connectivityGraph.MyJoins["Hallway-DoubleSilo"].Reference.InstantiateAsync().Completed += (room) =>
            {
                currentEnvironment.CurrentGameObject = room.Result;
                loadedObjects.Add(currentEnvironment.CurrentGameObject);
                
                _connectivityGraph.MyJoins["Hallway-Connector"].Reference.InstantiateAsync().Completed += (door) =>
                {
                    var hallLeft = door.Result;
                    var hallLeftEnvironmentPrefab = hallLeft.GetComponent<EnvironmentPrefab>();
                    hallLeftEnvironmentPrefab.connectedTo = currentEnvironment.CurrentGameObject;//Todo Part.Joins is NULL yet private joins is full
                    hallLeftEnvironmentPrefab.JoinTo("ConnectionPoint_HallwayConnector_01", currentEnvironment.CurrentPrefabAsset.Part.Joins["ConnectionPoint_HallwayConnector_01"]);

                    loadedObjects.Add(hallLeft);
                    //Disable door colliders
                    ToggleDoor(hallLeftEnvironmentPrefab);
                };

                _connectivityGraph.MyJoins["Hallway-Connector"].Reference.InstantiateAsync().Completed += (door) =>
                {
                    var hallRight = door.Result;
                    var hallRightEnvironmentPrefab = hallRight.GetComponent<EnvironmentPrefab>();
                    hallRightEnvironmentPrefab.connectedTo = currentEnvironment.CurrentGameObject;
                    hallRightEnvironmentPrefab.JoinTo("ConnectionPoint_HallwayConnector_01", currentEnvironment.CurrentPrefabAsset.Part.Joins["ConnectionPoint_HallwayConnector_02"]);

                    loadedObjects.Add(hallRight);

                    if (MaxSiloOffset <= 1)
                        ToggleDoor(hallRightEnvironmentPrefab);
                };

                _container.InjectGameObject(currentEnvironment.CurrentGameObject);
            };            
            //Store current environment
            //Inject to environment spawners
        }

        //public void Awake()
        //{
        //    foreach (var part in parts) prefabsByName[part.ReferenceName] = part;

        //    //TODO: replace this with json loader code and editor.
        //    var hallDoorway = prefabsByName["Hallway-Connector"];
        //    var hallDoubleSilo = prefabsByName["Hallway-DoubleSilo"];
        //    var hallJoinRoom = prefabsByName["Hallway-SmallStraightJoin"];
        //    var siloRoomLeft = prefabsByName["Hallway - Silo_Left"];
        //    var siloRoomRight = prefabsByName["Hallway - Silo_Right"];

        //    //Initialize public connector references
        //    hallDoorway.Reference.Initialize();
        //    hallDoubleSilo.Reference.Initialize();
        //    hallJoinRoom.Reference.Initialize();
        //    siloRoomLeft.Reference.Initialize();
        //    siloRoomRight.Reference.Initialize();

        //    //Create Joins (connectivity graph)
        //    siloRoomRight.AddJoin("ConnectionPoint_Silo_01", new[] { hallDoubleSilo });
        //    siloRoomLeft.AddJoin("ConnectionPoint_Silo_02", new[] { hallDoubleSilo });

        //    hallDoorway.AddJoin("ConnectionPoint_HallwayConnector_01", new[] { hallDoubleSilo, hallJoinRoom });
        //    hallDoorway.AddJoin("ConnectionPoint_HallwayConnector_02", new[] { hallDoubleSilo, hallJoinRoom });

        //    hallDoubleSilo.AddJoin("ConnectionPoint_HallwayConnector_01", new[] { hallDoorway });
        //    hallDoubleSilo.AddJoin("ConnectionPoint_HallwayConnector_02", new[] { hallDoorway });
        //    hallDoubleSilo.AddJoin("ConnectionPoint_Silo_02", new[] { siloRoomLeft });
        //    hallDoubleSilo.AddJoin("ConnectionPoint_Silo_01", new[] { siloRoomRight });

        //    hallJoinRoom.AddJoin("ConnectionPoint_HallwayConnector_01", new[] { hallDoorway });
        //    hallJoinRoom.AddJoin("ConnectionPoint_HallwayConnector_02", new[] { hallDoorway });

        //    //Spawn initial environment
        //    var g = Instantiate(hallDoubleSilo.Reference);
        //    currentEnvironment.CurrentPart = hallDoubleSilo;
        //    currentEnvironment.CurrentGameObject = g.gameObject;
        //    loadedObjects.Add(currentEnvironment.CurrentGameObject);

        //    //Store current environment
        //    //Inject to environment spawners
        //    var hallLeft = _container.InstantiatePrefab(hallDoorway.Reference);
        //    var hallLeftEnvironmentPrefab = hallLeft.GetComponent<EnvironmentPrefab>();
        //    hallLeftEnvironmentPrefab.connectedTo = currentEnvironment.CurrentGameObject;
        //    hallLeftEnvironmentPrefab.JoinTo("ConnectionPoint_HallwayConnector_01", g.Joins["ConnectionPoint_HallwayConnector_01"]);

        //    //Disable door colliders
        //    ToggleDoor(hallLeftEnvironmentPrefab);

        //    var hallRight = _container.InstantiatePrefab(hallDoorway.Reference);
        //    var hallRightEnvironmentPrefab = hallRight.GetComponent<EnvironmentPrefab>();
        //    hallRightEnvironmentPrefab.connectedTo = currentEnvironment.CurrentGameObject;
        //    hallRightEnvironmentPrefab.JoinTo("ConnectionPoint_HallwayConnector_02", g.Joins["ConnectionPoint_HallwayConnector_02"]);

        //    if (MaxSiloOffset <= 1)
        //        ToggleDoor(hallRightEnvironmentPrefab);

        //    loadedObjects.Add(hallRight);
        //    loadedObjects.Add(hallLeft);

        //    _container.InjectGameObject(currentEnvironment.CurrentGameObject);
        //}

        public void ToggleDoor(EnvironmentPrefab door)
        {
            foreach (Collider c in door.ColliderList)
                c.enabled = !c.enabled;
        }

        public void SpawnPart(string myEnvironmentConnector, string to_Connect_to, EnvironmentPrefab myConnectors, Animator doorAnimator)
        {
            interactedDoor = myConnectors; 

            foreach (var obj in loadedObjects)
            {
                if(obj.GetInstanceID() != myConnectors.gameObject.GetInstanceID())
                    objectsToUnload.Add(obj);

            }
            _container.Bind<Animator>().FromInstance(doorAnimator);

            //Next Main Room spawn
            var roomPart = currentEnvironment.CurrentPart.MyJoins[myEnvironmentConnector][0].MyJoins[myEnvironmentConnector][NextRoomIndex(myConnectors)];
            nextRoom = new()
            {
                CurrentPart = roomPart,
                CurrentGameObject = _container.InstantiatePrefab(roomPart.Reference).gameObject,
            };

            interactedDoor.wasConnected = true;
            var nextRoomEnvironmentPrefabRef = nextRoom.CurrentGameObject.GetComponent<EnvironmentPrefab>();

            loadedObjects.Add(nextRoom.CurrentGameObject);
            //Reposition new room
            nextRoomEnvironmentPrefabRef.JoinTo(myEnvironmentConnector, myConnectors.Joins[myEnvironmentConnector]);

            var connectorDoor = _container.InstantiatePrefab(currentEnvironment.CurrentPart.MyJoins[myEnvironmentConnector][0].Reference);
            loadedObjects.Add(connectorDoor.gameObject);

            newDoor = connectorDoor.GetComponent<EnvironmentPrefab>();
            //Reposition door & set connection point
            newDoor.JoinTo(myEnvironmentConnector, nextRoomEnvironmentPrefabRef.Joins[to_Connect_to]);
            newDoor.connectedTo = nextRoom.CurrentGameObject;

            if(SiloOffset == 0 || SiloOffset >= MaxSiloOffset)
                ToggleDoor(newDoor);

            //update what current doors connected to
            interactedDoor.connectedTo = nextRoom.CurrentGameObject;

            //Save for unload on going back to current room (MAY NEED TO MOVE BACK UP)
            newlyLoadedObjects.Add(connectorDoor);
            newlyLoadedObjects.Add(nextRoom.CurrentGameObject);

            //Injects animations for closing into active room
            _container.InjectGameObject(currentEnvironment.CurrentGameObject);
            _container.Unbind<Animator>();
        }

        public void SpawnSilo(string myEnvironmentConnector, int environmentPrefabIndex, SiloPositioner currentSilo)
        {
            _currentSilo = currentSilo;
            SiloExists = true;

            var nextRoomEnvironmentPrefabRef = currentEnvironment.CurrentGameObject.GetComponent<EnvironmentPrefab>();

            var silo = _container.InstantiatePrefab(currentEnvironment.CurrentPart.MyJoins[myEnvironmentConnector][environmentPrefabIndex].Reference);
            loadedObjects.Add(silo.gameObject);
            newlyLoadedObjects.Add(silo.gameObject);

            //Reposition door & set connection point
            silo.GetComponent<EnvironmentPrefab>().JoinTo(myEnvironmentConnector, nextRoomEnvironmentPrefabRef.Joins[myEnvironmentConnector]);
        }

        public void ChangeDirection(bool direction)
        {
            forward = direction;
        }

        private void ChangeSiloOffset()
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
            ReBindSiloInfo();
        }

        private SiloContent[] GetCurrentSiloInfo()
        {
            if (MaxSiloOffset == 0)
                return new SiloContent[] { new(), new() };

            if (SiloOffset + 1 >= MaxSiloOffset)
                return new[] { _playerInventory.Silos[SiloOffset], new() };

            return new[] { _playerInventory.Silos[SiloOffset], _playerInventory.Silos[SiloOffset + 1] };
        }

        private void ReBindSiloInfo()
        {
            Container.Rebind<SiloContent[]>().FromInstance(GetCurrentSiloInfo()).AsCached();
        }

        private int NextRoomIndex(EnvironmentPrefab myConnectors)
        {
            if (myConnectors.connectedTo.name == "Hallway-DoubleSilo(Clone)")
            {
                return 1;
            }

            ChangeSiloOffset();
            return 0;
        }

        public void UnloadAssets()
        {

            if (_currentSilo)
                _currentSilo.SiloSpawned = false;

            if (newDoor)
            {
                if (newDoor.connectedTo.name != "Hallway-SmallStraightJoin(Clone)" && SiloOffset > 0 && SiloOffset < MaxSiloOffset)
                    ToggleDoor(newDoor);
                else if (newDoor.connectedTo.name == "Hallway-SmallStraightJoin(Clone)")
                    ToggleDoor(newDoor);
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
                    interactedDoor.connectedTo = currentEnvironment.CurrentGameObject;
                
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
                if (interactedDoor.connectedTo == currentEnvironment.CurrentGameObject)
                    interactedDoor.connectedTo = nextRoom.CurrentGameObject;
                else if (commited == false)
                    interactedDoor.connectedTo = currentEnvironment.CurrentGameObject;
            }
        }

        public void setCurrentEnvironment(GameObject gameObject)
        {
            if (gameObject.name == "Hallway-SmallStraightJoin(Clone)" && gameObject.transform.position != Vector3.zero)
                _repositionSignalHandler.RepositionObject(gameObject.transform.position);

            if (currentEnvironment.CurrentGameObject != gameObject)
            {
                currentEnvironment.CurrentGameObject = gameObject;
                roomChanged = true;
            }
        }
    }
}