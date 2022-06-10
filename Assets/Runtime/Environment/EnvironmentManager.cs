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
using UnityEngine.ResourceManagement.AsyncOperations;

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

        public bool SiloExists { get; private set; } = false;

        [SerializeField]
        private AssetReferenceGameObject _playerObject;

        public override void InstallBindings()
        {
            MaxSiloOffset = _playerInventory.Silos.Count - 1;

            if (MaxSiloOffset < 0)
                MaxSiloOffset++;

            Container.Bind<EnvironmentManager>().FromInstance(this);

            Container.Bind<SiloItem[]>().FromInstance(GetCurrentSiloInfo()).AsCached();
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
                    hallLeftEnvironmentPrefab.connectedTo = currentEnvironment.CurrentGameObject;
                    hallLeftEnvironmentPrefab.JoinTo("ConnectionPoint_HallwayConnector_01", currentEnvironment.CurrentPrefabAsset.Part.Joins["ConnectionPoint_HallwayConnector_01"]);

                    _container.InjectGameObject(hallLeft);
                    //Disable door colliders
                    ToggleDoor(hallLeftEnvironmentPrefab);

                    loadedObjects.Add(hallLeft);
                };

                _connectivityGraph.MyJoins["Hallway-Connector"].Reference.InstantiateAsync().Completed += (door) =>
                {
                    var hallRight = door.Result;
                    var hallRightEnvironmentPrefab = hallRight.GetComponent<EnvironmentPrefab>();
                    hallRightEnvironmentPrefab.connectedTo = currentEnvironment.CurrentGameObject;
                    hallRightEnvironmentPrefab.JoinTo("ConnectionPoint_HallwayConnector_01", currentEnvironment.CurrentPrefabAsset.Part.Joins["ConnectionPoint_HallwayConnector_02"]);

                    _container.InjectGameObject(hallRight);

                    if (MaxSiloOffset <= 1)
                        ToggleDoor(hallRightEnvironmentPrefab);

                    loadedObjects.Add(hallRight);
                    loadPlayer();
                };

                _container.InjectGameObject(currentEnvironment.CurrentGameObject);
            };
        }

        private void loadPlayer()
        {
            _playerObject.InstantiateAsync().Completed += (player) =>
            {
                _container.InjectGameObject(player.Result);
                player.Result.SetActive(true);
            };
        }

        public void ToggleDoor(EnvironmentPrefab door)
        {
            foreach (Collider c in door.ColliderList)
                c.enabled = !c.enabled;
        }

        private void DoorOpened()
        {
            foreach(var obj in objectsToUnload)
            {
                ToggleDoor(obj.GetComponent<EnvironmentPrefab>());
            }
        }

        public void SpawnPart(string myEnvironmentConnector, string to_Connect_to, EnvironmentPrefab myConnectors)
        {            
            if (interactedDoor != myConnectors)
            {
                if(interactedDoor) interactedDoor.wasConnected = false;
                
                interactedDoor = myConnectors;
            }
         
            foreach (var obj in loadedObjects)
            {
                if(obj.GetInstanceID() != myConnectors.gameObject.GetInstanceID())
                    objectsToUnload.Add(obj);
            }

            DoorOpened();

            string nextRoomName = _connectivityGraph.MyJoins[myConnectors.PrefabName].MyJoinsByConnector[to_Connect_to].Destinations[NextRoomIndex(myConnectors)].PrefabName;

            currentEnvironment.CurrentPrefabAsset = _connectivityGraph.MyJoins[nextRoomName];
            nextRoom = new()
            {
                CurrentPrefabAsset = _connectivityGraph.MyJoins[nextRoomName]
            };

            _connectivityGraph.MyJoins[nextRoomName].Reference.InstantiateAsync().Completed += (room) =>
            {
                nextRoom.CurrentGameObject = room.Result;

                _container.InjectGameObject(nextRoom.CurrentGameObject);

                interactedDoor.wasConnected = true;
                var nextRoomEnvironmentPrefabRef = nextRoom.CurrentGameObject.GetComponent<EnvironmentPrefab>();
                //Reposition new room
                nextRoomEnvironmentPrefabRef.JoinTo(myEnvironmentConnector, myConnectors.Joins[to_Connect_to]);

                loadedObjects.Add(nextRoom.CurrentGameObject);
                newlyLoadedObjects.Add(nextRoom.CurrentGameObject);
                
                _connectivityGraph.MyJoins[myConnectors.PrefabName].Reference.InstantiateAsync().Completed += (door) =>
                {
                    var connectedDoor = door.Result;
                    newDoor = connectedDoor.GetComponent<EnvironmentPrefab>();
                    newDoor.connectedTo = nextRoom.CurrentGameObject;
                    newDoor.JoinTo(myEnvironmentConnector, nextRoomEnvironmentPrefabRef.Joins[to_Connect_to]);

                    _container.InjectGameObject(connectedDoor);
                    //Disable door colliders
                    ToggleDoor(newDoor);

                    loadedObjects.Add(connectedDoor);
                    //update what current doors connected to
                    interactedDoor.connectedTo = nextRoom.CurrentGameObject;

                    //Save for unload on going back to current room
                    newlyLoadedObjects.Add(connectedDoor);
                };
            };
        }

        public void SpawnSilo(string prefabName, string to_Connect_to, SiloPositioner currentSilo)
        {
            _currentSilo = currentSilo;
            SiloExists = true;

            var nextRoomEnvironmentPrefabRef = currentEnvironment.CurrentGameObject.GetComponent<EnvironmentPrefab>();

            _connectivityGraph.MyJoins[prefabName].Reference.InstantiateAsync().Completed += (silo) =>
            {
                var newSilo = silo.Result;
                _container.InjectGameObject(newSilo);
                //Reposition door & set connection point
                newSilo.GetComponent<EnvironmentPrefab>().JoinTo(to_Connect_to, nextRoomEnvironmentPrefabRef.Joins[to_Connect_to]);

                loadedObjects.Add(newSilo);
                newlyLoadedObjects.Add(newSilo);
            };
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

        private SiloItem[] GetCurrentSiloInfo()
        {
            //ToDo try remove the creation of new SiloItems
            if (MaxSiloOffset == 0)
                return new SiloItem[] { new Mech(), new Mech() };

            if (SiloOffset + 1 >= MaxSiloOffset)
                return new[] { _playerInventory.Silos[SiloOffset], new Mech() };

            return new[] { _playerInventory.Silos[SiloOffset], _playerInventory.Silos[SiloOffset + 1] };
        }

        private void ReBindSiloInfo()
        {
            Container.Rebind<SiloItem[]>().FromInstance(GetCurrentSiloInfo()).AsCached();
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
            //Enable doors again
            DoorOpened();

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
                    Addressables.ReleaseInstance(obj);
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
                    Addressables.ReleaseInstance(obj);
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