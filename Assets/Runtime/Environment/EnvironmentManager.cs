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
            MaxSiloOffset = _playerInventory.Silos.Length - 1;

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
            currentEnvironment.CurrentPrefabAsset = _connectivityGraph.GetInitialSection();

            currentEnvironment.CurrentPrefabAsset.Reference.InstantiateAsync().Completed += (operationHandle) =>
            {
                currentEnvironment.CurrentGameObject = operationHandle.Result;
                var currentEnvironmentPrefab = currentEnvironment.CurrentGameObject.GetComponent<EnvironmentPrefab>();
                loadedObjects.Add(currentEnvironment.CurrentGameObject);

                int i = 0;
                foreach (ConnectivityJoin join in _connectivityGraph.RequiredJoins)
                {
                    _connectivityGraph.MyJoins[currentEnvironment.CurrentPrefabAsset.MyJoinsByConnector[join].Destinations[0]].Reference.InstantiateAsync().Completed += (operationHandle) =>
                    {
                        var newSection = operationHandle.Result;
                        var newSectionEnvironmentPrefab = newSection.GetComponent<EnvironmentPrefab>();
                        newSectionEnvironmentPrefab.connectedTo = currentEnvironment.CurrentGameObject;
                        newSectionEnvironmentPrefab.JoinTo(join, currentEnvironmentPrefab.Joins[join]);

                        _container.InjectGameObject(newSection);

                        //Disable door colliders
                        if(i == 0)
                            newSectionEnvironmentPrefab.ToggleDoor();
                        else if (MaxSiloOffset <= 1)
                            newSectionEnvironmentPrefab.ToggleDoor();

                        ++i;
                        loadedObjects.Add(newSection);
                    };
                }

                _container.InjectGameObject(currentEnvironment.CurrentGameObject);
            };
        }

        public void SpawnPart(EnvironmentSpawner otherSpawner, EnvironmentSpawner mySpawner)
        {       
            if (interactedDoor != mySpawner.MyConnectors)
            {
                if(interactedDoor) interactedDoor.wasConnected = false;
                
                interactedDoor = mySpawner.MyConnectors;
            }
         
            foreach (var obj in loadedObjects)
            {
                if (obj.GetInstanceID() != mySpawner.MyConnectors.gameObject.GetInstanceID())
                    objectsToUnload.Add(obj);
            }
			
            ConnectivityJoin nextRoomToJoinTo = _connectivityGraph.MyJoins[mySpawner.MyConnectors.PrefabName].MyJoinsByConnector[otherSpawner.ToConnectTo].Destinations[NextRoomIndex(mySpawner.MyConnectors)];

            currentEnvironment.CurrentPrefabAsset = _connectivityGraph.MyJoins[nextRoomToJoinTo];
            nextRoom = new()
            {
                CurrentPrefabAsset = _connectivityGraph.MyJoins[nextRoomToJoinTo]
            };

            _connectivityGraph.MyJoins[nextRoomToJoinTo].Reference.InstantiateAsync().Completed += (room) =>
            {
                nextRoom.CurrentGameObject = room.Result;

                _container.InjectGameObject(nextRoom.CurrentGameObject);

                interactedDoor.wasConnected = true;
                var nextRoomEnvironmentPrefabRef = nextRoom.CurrentGameObject.GetComponent<EnvironmentPrefab>();
                //Reposition new room

                nextRoomEnvironmentPrefabRef.JoinTo(otherSpawner.ToConnectTo, mySpawner.MyConnectors.Joins[otherSpawner.ToConnectTo]);

                loadedObjects.Add(nextRoom.CurrentGameObject);
                newlyLoadedObjects.Add(nextRoom.CurrentGameObject);

                _connectivityGraph.MyJoins[mySpawner.MyConnectors.PrefabName].Reference.InstantiateAsync().Completed += (door) =>
                {
                    var connectedDoor = door.Result;
                    newDoor = connectedDoor.GetComponent<EnvironmentPrefab>();
                    newDoor.connectedTo = nextRoom.CurrentGameObject;
                    newDoor.JoinTo(mySpawner.ToConnectTo, nextRoomEnvironmentPrefabRef.Joins[mySpawner.ToConnectTo]);

                    _container.InjectGameObject(connectedDoor);
                    //Disable door colliders
                    newDoor.ToggleDoor();

                    loadedObjects.Add(connectedDoor);
                    //update what current doors connected to
                    interactedDoor.connectedTo = nextRoom.CurrentGameObject;

                    //Save for unload on going back to current room
                    newlyLoadedObjects.Add(connectedDoor);
                };
            };
        }

        public void SpawnSilo(SiloPositioner currentSilo)
        {
            _currentSilo = currentSilo;
            SiloExists = true;

            var nextRoomEnvironmentPrefabRef = currentEnvironment.CurrentGameObject.GetComponent<EnvironmentPrefab>();

            _connectivityGraph.MyJoins[currentEnvironment.CurrentPrefabAsset.MyJoinsByConnector[currentSilo.ToConnectTo].Destinations[0]].Reference.InstantiateAsync().Completed += (operationHandler) =>
            {
                var newSilo = operationHandler.Result;
                _container.InjectGameObject(newSilo);
                //Reposition door & set connection point
                newSilo.GetComponent<EnvironmentPrefab>().JoinTo(currentSilo.ToConnectTo, nextRoomEnvironmentPrefabRef.Joins[currentSilo.ToConnectTo]);

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
                    newDoor.ToggleDoor();
                else if (newDoor.connectedTo.name == "Hallway-SmallStraightJoin(Clone)")
                    newDoor.ToggleDoor();
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