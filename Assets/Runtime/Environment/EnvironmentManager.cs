using SupremacyHangar.Runtime.Silo;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using SupremacyHangar.Runtime.Types;
using SupremacyHangar.Runtime.Environment.Types;
using UnityEngine.AddressableAssets;
using SupremacyHangar.Runtime.Environment.Connections;
using UnityEngine.ResourceManagement.AsyncOperations;
using SupremacyHangar.Runtime.ContentLoader.Types;
using SupremacyHangar.Runtime.ContentLoader;

namespace SupremacyHangar.Runtime.Environment
{
    public class EnvironmentManager : MonoInstaller
    {
        [Inject] private DiContainer _container;
        
        [Inject]
        private RepositionSignalHandler _repositionSignalHandler;

        [Inject]
        private HangarData _playerInventory;

        [Inject]
        private SiloSignalHandler _siloSignalHandler;
                
        private SignalBus _bus;
        private LoadingProgressContext loadingProgressContext = new();

        private EnvironmentConnectivity _connectivityGraph;

        EnvironmentPrefab nextRoomEnvironmentPrefabRef;
        public CurrentEnvironment currentEnvironment = new();

        private List<GameObject> loadedObjects = new();
        private List<GameObject> objectsToUnload = new();
        private List<GameObject> newlyLoadedObjects = new();
        private bool roomChanged = false;

        private EnvironmentPrefab interactedDoor = null;

        private SiloSpawner _currentSilo;
        private EnvironmentPrefab newDoorEnvironmentPrefab;
        public int SiloOffset { get; private set; } = 0;
        public IReadOnlyList<SiloItem> SiloItems => _playerInventory?.Silos;

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
        private bool _subscribed;

        private GameObject loadedSilo;

        [Inject]
        public void Construct(SignalBus bus)
        {
            _bus = bus;
        }
        
        private void OnEnable()
        {
            SubscribeToSignal();
            _container.Inject(loadingProgressContext);
        }

        private void OnDisable()
        {
            if (!_subscribed) return;
            _bus.Unsubscribe<SiloUnloadedSignal>(UnloadAssetsAfterSiloClosed);
            _bus.Unsubscribe<InventoryLoadedSignal>(LoadFactionGraph);

            _subscribed = false;
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<SiloUnloadedSignal>(UnloadAssetsAfterSiloClosed);
            _bus.Subscribe<InventoryLoadedSignal>(LoadFactionGraph);

            _subscribed = true;
        }

        public override void InstallBindings()
        {
            Container.Bind<EnvironmentManager>().FromInstance(this);
        }

        private void LoadFactionGraph()
        {
            MaxSiloOffset = _playerInventory.Silos.Count;
            Container.Bind<SiloItem[]>().FromInstance(GetCurrentSiloInfo()).AsCached();
            //Todo: run from signal instead
            AssetReferenceEnvironmentConnectivity connectivityGraph = _playerInventory.factionGraph;

            var connectivityGraphOp = connectivityGraph.LoadAssetAsync<EnvironmentConnectivity>();

            StartCoroutine(loadingProgressContext.LoadingAssetProgress(connectivityGraphOp, "Loading Faction Layout"));
            connectivityGraphOp.Completed += (obj) =>
            {
                _connectivityGraph = obj.Result;
                SpawnInitialHallway();
            };

		}

        private void SpawnInitialHallway()
        {
            //Spawn initial environment
            currentEnvironment.CurrentPrefabAsset = _connectivityGraph.GetInitialSection();

            var hallwayOp = currentEnvironment.CurrentPrefabAsset.Reference.InstantiateAsync();
            StartCoroutine(loadingProgressContext.LoadingAssetProgress(hallwayOp, "Loading Hallway"));
            hallwayOp.Completed += DefaultRoomSpawn;
        }

        private void DefaultRoomSpawn(AsyncOperationHandle<GameObject> handle1)
        {
            currentEnvironment.CurrentGameObject = handle1.Result;
            nextRoomEnvironmentPrefabRef = currentEnvironment.CurrentGameObject.GetComponent<EnvironmentPrefab>();
            loadedObjects.Add(currentEnvironment.CurrentGameObject);
            
            currentEnvironment.CurrentGameObject.SetActive(true);

            foreach (ConnectivityJoin join in _connectivityGraph.RequiredJoins)
            {
                var partForJoin = currentEnvironment.CurrentPrefabAsset.MyJoinsByConnector[join];
                var nodeForJoin = partForJoin.Destinations[0];

                var joinOp = _connectivityGraph.MyJoins[nodeForJoin].Reference.InstantiateAsync();
                StartCoroutine(loadingProgressContext.LoadingAssetProgress(joinOp, "Loading Doors"));
                operationsForJoins.Add(joinOp, join);
            }

            foreach (var operation in operationsForJoins.Keys)
                operation.Completed += InitializeDefaultDoor;

            var playerOp = _playerObject.InstantiateAsync();
            StartCoroutine(loadingProgressContext.LoadingAssetProgress(playerOp, "Loading Player"));
            playerOp.Completed += PlayerLoaded;
            _container.InjectGameObject(currentEnvironment.CurrentGameObject);
        }

        private void InitializeDefaultDoor(AsyncOperationHandle<GameObject> handle2)
        {
            var join = operationsForJoins[handle2];
            var newConnector = handle2.Result;
            var newSectionEnvironmentPrefab = newConnector.GetComponent<EnvironmentPrefab>();
            newSectionEnvironmentPrefab.connectedTo = currentEnvironment.CurrentGameObject;

            newSectionEnvironmentPrefab.JoinTo(join, nextRoomEnvironmentPrefabRef.Joins[join]);

            _container.InjectGameObject(newConnector);

            newConnector.SetActive(true);

            //Disable door colliders
            if (doorCounter == 0)
                newSectionEnvironmentPrefab.ToggleDoor();
            else if (MaxSiloOffset <= 2)
                newSectionEnvironmentPrefab.ToggleDoor();

            ++doorCounter;
            loadedObjects.Add(newConnector);
            operationsForJoins.Remove(handle2);
        }

        private void PlayerLoaded(AsyncOperationHandle<GameObject> handle)
        {
            var result = handle.Result;
            if (nextRoomEnvironmentPrefabRef.SpawnPointValid)
            {
                var spawnPoint = nextRoomEnvironmentPrefabRef.SpawnPoint;
                result.transform.position = spawnPoint.position;
                result.transform.rotation = spawnPoint.rotation;
            }

            _container.InjectGameObject(result);
            GameObject.FindGameObjectWithTag("Loading").SetActive(false);
            result.SetActive(true);
        }

        public void UpdatePlayerInventory(int siloIndex, SiloItem newContent)
        {
            _playerInventory.Silos[siloIndex] = newContent;
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

            nextRoom.CurrentGameObject.SetActive(true);

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
            newDoorEnvironmentPrefab = connectedDoor.GetComponent<EnvironmentPrefab>();
            newDoorEnvironmentPrefab.JoinTo(Door2.ToConnectTo, nextRoomEnvironmentPrefabRef.Joins[Door2.ToConnectTo]);
            newDoorEnvironmentPrefab.connectedTo = nextRoom.CurrentGameObject;

            _container.InjectGameObject(connectedDoor);
            
            //Disable door colliders
            newDoorEnvironmentPrefab.ToggleDoor();

            loadedObjects.Add(connectedDoor);

            //update what current doors connected to
            interactedDoor.connectedTo = nextRoom.CurrentGameObject;

            connectedDoor.SetActive(true);
            
            //Save for unload on going back to current room
            newlyLoadedObjects.Add(connectedDoor);

            if (newlyLoadedObjects.Count > 1)
            {
                Debug.Log("Door open as alls loaded");
                Door2.OpenDoor();
            }
            operationsForNewDoor.Remove(doorHandler);
        }

        public void SpawnSilo(SiloSpawner currentSilo)
        {
            _currentSilo = currentSilo;
            SiloExists = true;

            nextRoomEnvironmentPrefabRef = currentEnvironment.CurrentGameObject.GetComponent<EnvironmentPrefab>();

            var partForJoin = currentEnvironment.CurrentPrefabAsset.MyJoinsByConnector[currentSilo.ToConnectTo];
            var nodeForJoin = partForJoin.Destinations[0];
            var siloOperationHandle = _connectivityGraph.MyJoins[nodeForJoin].Reference.InstantiateAsync();
            StartCoroutine(loadingProgressContext.LoadingAssetProgress(siloOperationHandle));
            siloOperationHandle.Completed += InstantiateSilo;
        }

        private void InstantiateSilo(AsyncOperationHandle<GameObject> operationHandler)
        {
            var newSilo = operationHandler.Result;
            _currentSilo.InjectGameObject(newSilo);
            
            //Reposition door & set connection point
            newSilo.GetComponent<EnvironmentPrefab>().JoinTo(_currentSilo.ToConnectTo, nextRoomEnvironmentPrefabRef.Joins[_currentSilo.ToConnectTo]);

            newSilo.SetActive(true);

            loadingProgressContext.ProgressSignalHandler.FinishedLoading(newSilo);

            loadedSilo = newSilo;
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
                return new SiloItem[] { new EmptySilo(), new EmptySilo() };

            if (SiloOffset + 1 >= MaxSiloOffset)
                return new[] { _playerInventory.Silos[SiloOffset], new EmptySilo() };

            return new[] { _playerInventory.Silos[SiloOffset], _playerInventory.Silos[SiloOffset + 1] };
        }

        private void ReBindSiloInfo()
        {
            Container.Rebind<SiloItem[]>().FromInstance(GetCurrentSiloInfo()).AsCached();
        }

        private int NextRoomIndex(EnvironmentPrefab myConnectors)
        {
            if (myConnectors.connectedTo.name.StartsWith("Hallway-DoubleSilo"))
            {
                return 1;
            }

            ChangeSiloOffset();
            return 0;
        }

        public void UnloadSilo(bool waitOnWindow = true)
        {
            if (_currentSilo && waitOnWindow)
            {
                _siloSignalHandler.CloseSilo();
                _currentSilo.SiloSpawned = false;
                _currentSilo = null;
            }
            else if (!waitOnWindow && loadedSilo)
                UnloadAssetsAfterSiloClosed();
        }

        public void UnloadAssets()
        {
            DoorOpened();

            if (newDoorEnvironmentPrefab)
            {
                if (SiloOffset > 0 && SiloOffset < MaxSiloOffset - 2)
                    newDoorEnvironmentPrefab.ToggleDoor();
                else if (newDoorEnvironmentPrefab.connectedTo.name.StartsWith("Hallway-SmallStraightJoin"))
                    newDoorEnvironmentPrefab.ToggleDoor();
            }

            if (roomChanged)
            {
                foreach (var obj in objectsToUnload)
                {
                    loadedObjects.Remove(obj);
                    UnityEngine.AddressableAssets.Addressables.ReleaseInstance(obj);
                }
                objectsToUnload.Clear();
                newlyLoadedObjects.Clear();
                roomChanged = false;
            }
            else
            {
                if (interactedDoor)
                    interactedDoor.connectedTo = currentEnvironment.CurrentGameObject;

                foreach (var obj in newlyLoadedObjects)
                {
                    loadedObjects.Remove(obj);
                    UnityEngine.AddressableAssets.Addressables.ReleaseInstance(obj);
                }
                newlyLoadedObjects.Clear();
                objectsToUnload.Clear();
            }
            SiloExists = false;
        }

        private void UnloadAssetsAfterSiloClosed()
        {
            UnityEngine.AddressableAssets.Addressables.ReleaseInstance(loadedSilo);
        }

        public void resetConnection(bool commited = false)
        {
            if (interactedDoor)
            {
                if (interactedDoor.connectedTo == currentEnvironment.CurrentGameObject)
                {
                    if(nextRoom.CurrentGameObject) interactedDoor.connectedTo = nextRoom.CurrentGameObject;
                }
                else if (commited == false)
                    interactedDoor.connectedTo = currentEnvironment.CurrentGameObject;
            }
        }

        public void setCurrentEnvironment(GameObject gameObject)
        {
            if (gameObject.name.StartsWith("Hallway-SmallStraightJoin") && gameObject.transform.position != Vector3.zero)
                _repositionSignalHandler.RepositionObject(gameObject.transform.position);

            if (currentEnvironment.CurrentGameObject != gameObject)
            {
                currentEnvironment.CurrentGameObject = gameObject;
                roomChanged = true;
            }
        }
    }
}