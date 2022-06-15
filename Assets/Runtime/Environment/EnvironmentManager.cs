using SupremacyHangar.Runtime.Silo;
using SupremacyHangar.Runtime.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System;
using SupremacyHangar.Runtime.Types;
using SupremacyHangar.Runtime.Environment.Types;
using UnityEngine.AddressableAssets;
using SupremacyHangar.Runtime.Environment.Connections;
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

        public CurrentEnvironment currentEnvironment = new();

        private List<GameObject> loadedObjects = new();
        private List<GameObject> objectsToUnload = new();
        private List<GameObject> newlyLoadedObjects = new();
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

        int doorCounter = 0;
        private Dictionary<AsyncOperationHandle<GameObject>, ConnectivityJoin> operationsForJoins = new Dictionary<AsyncOperationHandle<GameObject>, ConnectivityJoin>();
        private Dictionary<AsyncOperationHandle<GameObject>, DoorTuple> operationsForNewRoom = new Dictionary<AsyncOperationHandle<GameObject>, DoorTuple>();
        private Dictionary<AsyncOperationHandle<GameObject>, EnvironmentSpawner> operationsForNewDoor = new Dictionary<AsyncOperationHandle<GameObject>, EnvironmentSpawner>();

        public override void InstallBindings()
        {
            MaxSiloOffset = _playerInventory.Silos.Count;

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
        EnvironmentPrefab nextRoomEnvironmentPrefabRef;

        private void SpawnInitialHallway()
        {
            //Spawn initial environment
            currentEnvironment.CurrentPrefabAsset = _connectivityGraph.GetInitialSection();

            currentEnvironment.CurrentPrefabAsset.Reference.InstantiateAsync().Completed += DefaultRoomSpawn;
        }

        private void DefaultRoomSpawn(AsyncOperationHandle<GameObject> handle1)
        {
            currentEnvironment.CurrentGameObject = handle1.Result;
            nextRoomEnvironmentPrefabRef = currentEnvironment.CurrentGameObject.GetComponent<EnvironmentPrefab>();
            loadedObjects.Add(currentEnvironment.CurrentGameObject);

            foreach (ConnectivityJoin join in _connectivityGraph.RequiredJoins)
            {
                var partForJoin = currentEnvironment.CurrentPrefabAsset.MyJoinsByConnector[join];
                var nodeForJoin = partForJoin.Destinations[0];

                operationsForJoins.Add(_connectivityGraph.MyJoins[nodeForJoin].Reference.InstantiateAsync(), join);
            }

            foreach (var operation in operationsForJoins.Keys)
                operation.Completed += InitializeDefaultDoor;

            loadPlayer();
            _container.InjectGameObject(currentEnvironment.CurrentGameObject);
        }
                
        private void InitializeDefaultDoor(AsyncOperationHandle<GameObject> handle2)
        {
            var join = operationsForJoins[handle2];
            var newSection = handle2.Result;
            var newSectionEnvironmentPrefab = newSection.GetComponent<EnvironmentPrefab>();
            newSectionEnvironmentPrefab.connectedTo = currentEnvironment.CurrentGameObject;

            newSectionEnvironmentPrefab.JoinTo(join, nextRoomEnvironmentPrefabRef.Joins[join]);

            _container.InjectGameObject(newSection);

            //Disable door colliders
            if (doorCounter == 0)
                newSectionEnvironmentPrefab.ToggleDoor();
            else if (MaxSiloOffset <= 1)
                newSectionEnvironmentPrefab.ToggleDoor();

            ++doorCounter;
            loadedObjects.Add(newSection);
            operationsForJoins.Remove(handle2);
        }

        private void loadPlayer()
        {
            _playerObject.InstantiateAsync().Completed += (player) =>
            {
                _container.InjectGameObject(player.Result);
                player.Result.SetActive(true);
            };
        }
        
        private void DoorOpened()
        {
            foreach (var obj in objectsToUnload)
            {
                obj.GetComponent<EnvironmentPrefab>().ToggleDoor();
            }
        }

        public void SpawnPart(EnvironmentSpawner Door1, EnvironmentSpawner Door2)
        {       
            if (interactedDoor != Door2.MyConnectors)
            {
                if(interactedDoor) interactedDoor.wasConnected = false;
                
                interactedDoor = Door2.MyConnectors;
            }
         
            foreach (var obj in loadedObjects)
            {
                if (obj.GetInstanceID() != Door2.MyConnectors.gameObject.GetInstanceID())
                    objectsToUnload.Add(obj);
            }

            DoorOpened();

            var partForJoin = _connectivityGraph.MyJoins[Door2.MyConnectors.PrefabName];
            var nodeForJoin = partForJoin.MyJoinsByConnector[Door1.ToConnectTo];
            var nextRoomToJoinTo = nodeForJoin.Destinations[NextRoomIndex(Door2.MyConnectors)];

            currentEnvironment.CurrentPrefabAsset = _connectivityGraph.MyJoins[nextRoomToJoinTo];
            nextRoom = new()
            {
                CurrentPrefabAsset = _connectivityGraph.MyJoins[nextRoomToJoinTo]
            };

            operationsForNewRoom.Add(_connectivityGraph.MyJoins[nextRoomToJoinTo].Reference.InstantiateAsync(), new DoorTuple { Door2 = Door2, Door1 = Door1});
            
            foreach (var operation in operationsForNewRoom.Keys)
                operation.Completed += InitializeNewRoom;
        }

        private void InitializeNewRoom(AsyncOperationHandle<GameObject> roomHandler)
        {
            var Door1 = operationsForNewRoom[roomHandler].Door1;
            var Door2 = operationsForNewRoom[roomHandler].Door2;
            
            nextRoom.CurrentGameObject = roomHandler.Result;

            interactedDoor.wasConnected = true;
            nextRoomEnvironmentPrefabRef = nextRoom.CurrentGameObject.GetComponent<EnvironmentPrefab>();

            //Reposition new room
            nextRoomEnvironmentPrefabRef.JoinTo(Door1.ToConnectTo, Door2.MyConnectors.Joins[Door1.ToConnectTo]);
            _container.InjectGameObject(nextRoom.CurrentGameObject);

            loadedObjects.Add(nextRoom.CurrentGameObject);
            newlyLoadedObjects.Add(nextRoom.CurrentGameObject);

            operationsForNewDoor.Add(_connectivityGraph.MyJoins[Door2.MyConnectors.PrefabName].Reference.InstantiateAsync(), Door2);

            operationsForNewRoom.Remove(roomHandler);
            foreach (var operation in operationsForNewDoor.Keys)
                operation.Completed += InitializeNewDoor;
        }

        private void InitializeNewDoor(AsyncOperationHandle<GameObject> doorHandler)
        {
            var Door2 = operationsForNewDoor[doorHandler];

            var connectedDoor = doorHandler.Result;
            newDoor = connectedDoor.GetComponent<EnvironmentPrefab>();
            newDoor.JoinTo(Door2.ToConnectTo, nextRoomEnvironmentPrefabRef.Joins[Door2.ToConnectTo]);
            newDoor.connectedTo = nextRoom.CurrentGameObject;

            _container.InjectGameObject(connectedDoor);
            //Disable door colliders
            newDoor.ToggleDoor();

            loadedObjects.Add(connectedDoor);
            //update what current doors connected to
            interactedDoor.connectedTo = nextRoom.CurrentGameObject;

            //Save for unload on going back to current room
            newlyLoadedObjects.Add(connectedDoor);

            operationsForNewDoor.Remove(doorHandler);
        }

        public void SpawnSilo(SiloPositioner currentSilo)
        {
            _currentSilo = currentSilo;
            SiloExists = true;

            var nextRoomEnvironmentPrefabRef = currentEnvironment.CurrentGameObject.GetComponent<EnvironmentPrefab>();

            var partForJoin = currentEnvironment.CurrentPrefabAsset.MyJoinsByConnector[currentSilo.ToConnectTo];
            var nodeForJoin = partForJoin.Destinations[0];
            _connectivityGraph.MyJoins[nodeForJoin].Reference.InstantiateAsync().Completed += InstantiateSilo;
        }

        private void InstantiateSilo(AsyncOperationHandle<GameObject> operationHandler)
        {
            var newSilo = operationHandler.Result;
            _container.InjectGameObject(newSilo);
            //Reposition door & set connection point
            newSilo.GetComponent<EnvironmentPrefab>().JoinTo(_currentSilo.ToConnectTo, nextRoomEnvironmentPrefabRef.Joins[_currentSilo.ToConnectTo]);

            loadedObjects.Add(newSilo);
            newlyLoadedObjects.Add(newSilo);
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
                return new SiloItem[] { new(), new() };

            if (SiloOffset + 1 >= MaxSiloOffset)
                return new[] { _playerInventory.Silos[SiloOffset], new() };

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